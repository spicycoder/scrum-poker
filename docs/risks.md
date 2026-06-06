# Risks

Risks identified during scale audit and initial deployment. Address once deployment is stable.

---

## 1. Room ID Collision (Race Condition)

**File:** `src/ScrumPoker.Persistence/RoomRepository.cs` — `GenerateIdAsync()`

**Problem:** Method generates random 4-digit ID (1000–9999), checks if key exists in Redis, then saves. Two pods can generate same ID simultaneously → second write silently overwrites first.

```
Pod A: KeyExists(1234) → false → SET 1234 → saved
Pod B: KeyExists(1234) → false → SET 1234 → overwrites A's room
```

**Impact:** Room creation collision under concurrent writes. Low probability (9000 values) but non-zero.

**Suggested fixes:**
- Use Redis `INCR` with a counter key for deterministic sequential IDs
- Switch to GUID/UUID for room IDs (breaking change to frontend URL format)
- Use `SETNX` (atomic check-and-set) instead of check-then-set
- Expand ID space to 6+ digits

**Priority:** Low. Acceptable for now.

---

## 2. No Graceful SignalR Drain on Pod Shutdown

**Files:** `k8s/api/deployment.yaml` (no preStop hook)

**Problem:** K8s sends SIGTERM → pod stops instantly → active WebSocket connections are cut. Clients reconnect to another pod (auto-reconnect logic in `web/src/lib/signalr.ts` handles this), but ~1-2s blip during rollouts.

**Impact:** Brief disruption during rolling updates or pod terminations. No data loss — clients reconnect via `onreconnected` handler which re-joins the SignalR group.

**Suggested fixes:**
- Add `preStop` lifecycle hook to API deployment:
  ```yaml
  lifecycle:
    preStop:
      exec:
        command: ["sleep", "5"]
  ```
  This gives in-flight WebSocket messages time to complete before SIGTERM.

**Priority:** Medium. Affects UX during deployments.

---

## 3. Wolverine In-Memory Bus (Non-Durable Events)

**File:** `src/ScrumPoker.Application/Features/Commands/*/` — all handlers follow same pattern:

```csharp
await repository.SaveAsync(room, ct);   // Save to Redis
await bus.PublishAsync(@event);          // Fire SignalR notification
```

**Problem:** No transactional outbox. If step 1 succeeds but step 2 fails:
- Room saved in Redis → data persists
- SignalR event never fires → connected clients don't update
- User sees stale state until page refresh

If step 1 fails but step 2 fires (unlikely with in-memory bus, but theoretically possible with certain failure modes): phantom event.

**Impact:** Stale clients under transient Redis or network failures. No data corruption — state is always consistent in Redis.

**Suggested fixes:**
- Add Wolverine transactional outbox with Redis backing
- Accept and let clients refresh on missed updates (current behavior)
- Retry event publish on failure

**Priority:** Low for a real-time game. Acceptable trade-off.

---

## 4. Redis Single Point of Failure

**Files:** `k8s/redis/statefulset.yaml`

**Problem:**
- Single replica Redis StatefulSet (`replicas: 1`)
- No RDB or AOF persistence configured in `redis-server` args
- PVC is mounted but no explicit save policy
- No Redis Sentinel or Cluster for high availability

**Failure scenarios:**
- Redis pod restarts → empty state (unless Redis saves to disk via its default config)
- Redis pod fails → all game rooms lost
- Redis is slow → all API pods fail readiness probes → complete outage

**Current mitigation:** PVC provides disk persistence. Redis 8.6 defaults to RDB saves which should persist to the mounted volume. Not explicitly verified.

**Suggested fixes:**
- Add explicit `redis-server --save 60 1000 --appendonly yes` for RDB + AOF
- Document Redis recovery procedure
- For production: use managed Redis or add Redis Sentinel

**Priority:** High. Must address before any real use. Single Redis = single point of failure for entire app — rooms, game state, and SignalR backplane all depend on it.

---

## 5. Podman Image Prefix Quirk (Resolved)

**Files:** `k8s/api/deployment.yaml`, `k8s/web/deployment.yaml`, `deploy.ps1`

**Problem:** podman automatically prefixes locally-built images with `localhost/` when no registry is specified. Caused image name mismatch with deployment YAMLs.

**Resolution:** Switched image names to `ghcr.io/spicycoder/scrumpoker-api:latest` format. podman only adds `localhost/` prefix when no registry hostname is in the image name — `ghcr.io/` prevents it. Now works with both podman and Docker.

**Priority:** Resolved.

---

## 6. Web Deployment: No Readiness Probe

**File:** `k8s/web/deployment.yaml`

**Problem:** Web deployment has a TCP liveness probe but no readiness probe. If nginx is slow to start, K8s has no signal that the pod isn't ready for traffic.

**Impact:** Brief window where traffic could reach a pod that isn't serving. Unlikely with nginx (starts in <1s).

**Suggested fix:** Add TCP readiness probe matching the liveness probe:
```yaml
readinessProbe:
  tcpSocket:
    port: 5000
  initialDelaySeconds: 5
  periodSeconds: 10
  failureThreshold: 3
```

**Priority:** Low.

---

## 7. Rolling Update Strategy Not on Web

**File:** `k8s/web/deployment.yaml`

**Problem:** Web deployment has no explicit `strategy` block. Default K8s rolling update uses `maxUnavailable: 25%`, which with 1 replica means 0 pods available briefly during updates.

**Impact:** Brief downtime window on web deployments.

**Note:** API deployment already has `maxSurge: 1, maxUnavailable: 0` strategy applied.

**Suggested fix:** Add same strategy to web deployment.

**Priority:** Low for single-replica web. Matters at scale.

---

## 8. No TLS on Ingress

**File:** `k8s/ingress.yaml`

**Problem:** Ingress serves HTTP on port 80 only. No HTTPS, no TLS termination. Traffic between browser and cluster is unencrypted.

**Impact:** All traffic in plaintext. Fine for local minikube testing. Production requires TLS cert (e.g., Let's Encrypt via cert-manager).

**Suggested fixes:**
- Add TLS block in ingress with cluster issuer for Let's Encrypt
- For local: `minikube addons enable ingress` already provides an option for TLS

**Priority:** Low (local) / Critical (production)

---

## 9. No Resource Limits on Pods

**Files:** All deployment/statefulset YAMLs

**Problem:** No container has CPU/memory `requests` or `limits`. Pod can consume all node resources, starving other pods (including K8s system components).

**Impact:** Unpredictable performance under load. K8s scheduler has no resource data for placement decisions.

**Suggested fixes:**
- Add sensible requests/limits:
  ```yaml
  resources:
    requests:
      cpu: "100m"
      memory: "128Mi"
    limits:
      cpu: "500m"
      memory: "256Mi"
  ```
- Adjust based on actual usage metrics

**Priority:** Medium. Won't cause failures at low traffic.

---

## 10. CORS AllowAnyOrigin

**File:** `src/ScrumPoker.API/Program.cs` — `.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()`

**Problem:** CORS policy allows any origin. Works because SPA talks to same origin via ingress. But unusual pattern — typically the API would restrict to the SPA's origin.

**Impact:** Any website can make API calls from a browser. No credential/CSRF protection.

**Suggested fix:** Restrict to the SPA origin:
```csharp
.WithOrigins("http://localhost")
.AllowAnyHeader()
.AllowAnyMethod();
```

**Priority:** Low for local. Medium for production.

---

## 11. Sticky Session Cookie SameSite

**File:** `k8s/ingress.yaml`

**Problem:** Ingress sets `.ScrumPoker.Affinity` cookie for sticky sessions but does not set `SameSite` attribute. Modern browsers (Chrome, Safari) default cross-site cookies to `Lax` or `Strict`, which can interfere with the SignalR WebSocket upgrade handshake.

**Impact:** Intermittent WebSocket connection failures depending on browser and how the user navigates to the app.

**Suggested fix:** Add annotation:
```yaml
nginx.ingress.kubernetes.io/session-cookie-samesite: "Lax"
```

**Priority:** Medium. Will hit users randomly based on browser defaults.

---

## 12. Redis Key TTL at Scale

**File:** `src/ScrumPoker.API/appsettings.json` — `Game:ExpirationSeconds: 5400`

**Problem:** Rooms expire after 1.5 hours. Redis uses passive expiry (checked on access) + active expiry (sampled periodically). Under high room creation rates, stale keys can accumulate between active expiry sweeps.

**Impact:** Redis memory pressure if rooms are created rapidly without being accessed. Won't cause correctness issues — expired keys are skipped on access.

**Suggested fixes:**
- Monitor Redis memory usage (`kubectl exec` into Redis and run `INFO memory`)
- Lower TTL if appropriate
- Acceptable behavior for local testing

**Priority:** Low. Only matters at very high room creation rates.

---

## Summary

| # | Risk | Impact | Priority |
|---|------|--------|----------|
| 1 | Room ID collision | Data loss (room overwrite) | Low |
| 2 | No graceful SignalR drain | UX blip during rollouts | Medium |
| 3 | Non-durable event bus | Stale clients | Low |
| 4 | Redis SPOF | Complete data loss. Rooms, game state, SignalR backplane all die | **High** |
| 5 | Podman image prefix | Build tooling coupling | Low |
| 6 | No web readiness probe | Brief traffic routing issue | Low |
| 7 | Web rolling update default | Brief deploy downtime | Low |
| 8 | No TLS on ingress | Traffic unencrypted | Low (local) / Critical (prod) |
| 9 | No resource limits | Unpredictable performance | Medium |
| 10 | CORS AllowAnyOrigin | No origin restriction | Low |
| 11 | Sticky session cookie SameSite | Intermittent WS failures | Medium |
| 12 | Redis key TTL at scale | Memory pressure | Low |
