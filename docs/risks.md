# Risks

Risks identified during scale audit and initial deployment. Each risk includes severity and decision.

---

## 1. Room ID Collision (Race Condition)

**File:** `src/ScrumPoker.Persistence/RoomRepository.cs` — `GenerateIdAsync()`

**Problem:** Method generates random 4-digit ID (1000–9999), checks if key exists in Redis, then saves. Two pods can generate same ID simultaneously → second write silently overwrites first.

**Verdict:** Accept. Very rare edge case (9000 values). Room creation isn't high-frequency.

**Priority:** Low.

---

## 2. No Graceful SignalR Drain on Pod Shutdown

**Files:** `k8s/api/deployment.yaml` (no preStop hook)

**Problem:** K8s sends SIGTERM → pod stops instantly → active WebSocket connections cut. Clients reconnect via auto-reconnect (~1-2s blip).

**Verdict:** Accept. Only affects active games during deployments. Deployments happen on weekends when nobody plays. Client reconnection worked in testing.

**Priority:** Low.

---

## 3. Wolverine In-Memory Bus (Non-Durable Events)

**File:** `src/ScrumPoker.Application/Features/Commands/*/`

**Problem:** No transactional outbox. If Redis save succeeds but SignalR publish fails, clients go stale. If Redis save fails but event publishes (unlikely), phantom event.

**Verdict:** Intentional. Using Redis as Wolverine transport is not worth the complexity for this context. Stale clients can refresh. No data corruption.

**Priority:** Low.

---

## 4. Redis Single Point of Failure

**Files:** `k8s/redis/statefulset.yaml`

**Problem:** Single Redis pod. If it crashes, all rooms, game state, and SignalR backplane are lost. Entire app goes down.

**Verdict:** To be addressed — Redis cluster next. The current setup works for single-node testing but cannot survive a Redis pod failure.

**Priority:** **High.** Next focus.

---

## 5. Web Deployment: No Readiness Probe

**File:** `k8s/web/deployment.yaml`

**Problem:** Web deployment has a TCP liveness probe but no readiness probe. If nginx is slow to start, traffic could briefly hit a pod that isn't ready.

**Verdict:** Accept. nginx starts in <1s. Negligible impact.

**Priority:** Low.

---

## 6. Rolling Update Strategy Not on Web

**File:** `k8s/web/deployment.yaml`

**Problem:** Default rolling update (`maxUnavailable: 25%`) with 1 replica means 0 pods available briefly during web updates. Short downtime window.

**Verdict:** Accept. Deployments happen on weekends when nobody plays. No impact on active users.

**Note:** API deployment already has `maxSurge: 1, maxUnavailable: 0` applied.

**Priority:** Low.

---

## 7. No TLS on Ingress

**File:** `k8s/ingress.yaml`

**Problem:** Ingress serves HTTP on port 80 only. No HTTPS. Traffic unencrypted.

**Verdict:** Not needed for local minikube. Production concern — address when deploying to Oracle VPS (use Let's Encrypt via cert-manager).

**Priority:** Low (local) / Medium (production).

---

## 8. No Resource Limits on Pods

**File:** All deployment/statefulset YAMLs

**Problem:** No CPU/memory requests or limits. Pod can consume all node resources.

**Verdict:** Accept. Expected traffic is low (dozens of teams, each <12 players). Oracle VPS has 24GB RAM / 200GB storage — plenty of headroom. If usage grows, add sensible limits.

**Priority:** Low.

---

## 9. CORS AllowAnyOrigin

**File:** `src/ScrumPoker.API/Program.cs`

**Problem:** CORS allows any origin. Unnecessary — SPA and API are always same-origin (Vite proxy in dev, ingress in production). The CORS middleware never actually fires for same-origin requests.

**Verdict:** Dead code, not a real risk. Can be removed or kept. If removed, use `if (env.IsDevelopment())` for any dev-only CORS config.

**Priority:** None (doesn't affect anything).

---

## 10. Sticky Session Cookie SameSite

**File:** `k8s/ingress.yaml`

**Problem:** Ingress sets `.ScrumPoker.Affinity` cookie for sticky sessions but doesn't set `SameSite` attribute.

**Why this matters:** When a browser opens a WebSocket to `/api/hub`, it needs to send this cookie so the ingress routes them to the right pod. Modern browsers (Chrome, Safari) default to `SameSite=Lax` for cookies, which may block sending the cookie during WebSocket upgrade. This can cause intermittent SignalR reconnection failures or users jumping between pods.

**Fix:** Add annotation:
```yaml
nginx.ingress.kubernetes.io/session-cookie-samesite: "Lax"
```

**Verdict:** Easy fix but low impact. Users would need to refresh the page to recover. Can address alongside other ingress changes.

**Priority:** Low.

---

## 11. Redis Key TTL at Scale

**File:** `src/ScrumPoker.API/appsettings.json` — `Game:ExpirationSeconds: 5400`

**Problem:** Rooms expire after 1.5 hours. Under high creation rates, stale keys accumulate in Redis memory between expiry sweeps.

**Verdict:** Not significant at expected traffic levels. Redis on an Oracle VPS can handle far more keys than this app will ever create.

**Priority:** Low.

---

## 12. No Production Observability

**Files:** `src/ScrumPoker.API/Program.cs`, `src/ScrumPoker.ServiceDefaults/Extensions.cs`

**Problem:** Application has OpenTelemetry wired (via Aspire) but no production-grade observability stack. Currently:
- OTLP exporter configured but no receiver (Aspire dashboard only works locally)
- No log aggregation (Loki, Elastic, etc.)
- No metrics dashboards (Prometheus + Grafana)
- No alerting (who gets paged when Redis dies?)

**Suggested stack (keep it simple):**
- **Uptime Kuma** — synthetic monitoring, pings the app every minute, alerts via email/Telegram/Discord if down
- **Loki + Promtail** — lightweight log aggregation from K8s pods
- **Grafana** — dashboard for logs + basic metrics

Or even simpler for a start: just Uptime Kuma for health alerts + `kubectl logs` for ad-hoc debugging.

**Verdict:** To be addressed. Can start minimal and grow.

**Priority:** Medium.

---

## 13. No Rate Limiting

**Files:** `k8s/ingress.yaml`, `src/ScrumPoker.API/Program.cs`

**Problem:** No rate limiting at any layer. A malicious user could spam the API with requests (create rooms, vote rapidly, etc.) and degrade the experience for others.

**Options (from edge to app):**
1. **Cloudflare** — reverse proxy with built-in DDoS protection + rate limiting rules. Simplest if DNS goes through Cloudflare
2. **nginx ingress annotations** — `nginx.ingress.kubernetes.io/limit-rps: "10"` — rate limit per IP at the ingress level
3. **API middleware** — library like `AspNetCoreRateLimit` for app-level rate limiting (more flexible, per-endpoint rules)

**Suggested approach:** Start with nginx ingress rate limiting (zero code change, one annotation). Realistic human limit: ~100 req/min per IP. A whole game session (create, join, vote × few rounds) is ~20 requests.

**Verdict:** Not urgent at expected traffic levels, but easy to add basic protection.

**Priority:** Low (add before going public).

---

## Summary

| # | Risk | Impact | Priority |
|---|------|--------|----------|
| 1 | Room ID collision | Room overwrite (rare) | Low |
| 2 | No graceful SignalR drain | Brief WS blip on deploy | Low |
| 3 | Non-durable event bus | Stale clients (refresh fixes) | Low |
| 4 | Redis SPOF | Complete app outage | **High** |
| 5 | No web readiness probe | None (nginx starts instantly) | Low |
| 6 | Web rolling update default | Brief downtime (weekend deploys) | Low |
| 7 | No TLS on ingress | Local OK, prod needs HTTPS | Low (local) / Medium (prod) |
| 8 | No resource limits | Fine at expected traffic | Low |
| 9 | CORS AllowAnyOrigin | Dead code, never fires | None |
| 10 | Sticky session SameSite | Intermittent WS reconnect issues | Low |
| 11 | Redis key TTL at scale | Memory pressure at scale | Low |
| 12 | No production observability | Blind in production | Medium |
| 13 | No rate limiting | Abuse possible | Low |
