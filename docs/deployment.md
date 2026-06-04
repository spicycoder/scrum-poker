# Deployment

Helm chart in `k8s/`. Same images (`ghcr.io`) and same deploy command for local and production.

No passwords anywhere in the repo. Secrets created manually on the cluster before deploying.

---

## Prerequisites (both environments)

- **kubectl** — k8s CLI
- **helm** — package manager for k8s
- **Docker or podman** — container runtime
- **dotnet SDK 10.0** — for API image build (or use CI)
- **ghcr.io access** — push for build, pull for cluster
- **k8s cluster** — minikube (local), K3s (VPS), or cloud K8s
- **nginx ingress controller** — installed on cluster (minikube: `minikube addons enable ingress`)

---

## Build and push images

Same commands for local and prod. Replace `<tag>` with version (or use `latest`).

CI builds and pushes on every push (any branch) — see `.github/workflows/ci.yml`.
Images tagged with: `{sha}`, `{branch-name}`, and `latest` (main only).

To build manually:

**API:**
```powershell
dotnet publish src/ScrumPoker.API `
  /t:PublishContainer `
  -p ContainerImageName=ghcr.io/spicycoder/scrumpoker-api `
  -p ContainerImageTag=<tag> `
  -p ContainerRuntimeIdentifier=linux-x64
podman push ghcr.io/spicycoder/scrumpoker-api:<tag>
```

**Web:**
```powershell
podman build -f k8s/web.Dockerfile -t ghcr.io/spicycoder/scrumpoker-web:<tag> web/
podman push ghcr.io/spicycoder/scrumpoker-web:<tag>
```

For ARM64 (Oracle VPS), add `--platform linux/arm64` to `podman build` and use `linux-arm64` for `ContainerRuntimeIdentifier`.

Update image tags in `k8s/values.yaml` before deploying.

---

## K8s secrets management

Passwords never go in files. Created directly on cluster via `kubectl`.

### Create secrets (one-time, before first deploy)

One password shared between Redis and API. Use same `<password>` in both commands.

```powershell
kubectl create secret generic scrum-poker-redis-secret `
  --from-literal=REDIS_PASSWORD="<password>" `
  --namespace scrum-poker

kubectl create secret generic scrum-poker-api-secret `
  --from-literal=ConnectionStrings__redis="redis-service:6379,password=<password>" `
  --from-literal=REDIS_PASSWORD="<password>" `
  --from-literal=REDIS_URI="redis://:<password>@redis-service:6379" `
  --namespace scrum-poker
```

### View secrets (verify contents)

```powershell
# List all keys
kubectl get secrets -n scrum-poker -o yaml

# Decode a specific key
kubectl get secret scrum-poker-api-secret -n scrum-poker `
  -o jsonpath="{.data.ConnectionStrings__redis}" | `
  python -c "import sys,base64; print(base64.b64decode(sys.stdin.read()).decode())"
```

### Update / rotate password

```powershell
# Delete both secrets
kubectl delete secret scrum-poker-redis-secret scrum-poker-api-secret -n scrum-poker

# Re-run Create commands above with new password
kubectl rollout restart deployment -n scrum-poker
```

### Delete secrets (cleanup)

```powershell
kubectl delete secret scrum-poker-redis-secret scrum-poker-api-secret -n scrum-poker
```

---

## ghcr.io authentication (imagePullSecret)

Helm chart expects `ghcr-pull-secret` for pulling images from ghcr.io. Must be created before deploy.

```powershell
# Create docker-registry secret with your GitHub token
# Token needs `read:packages` scope
kubectl create secret docker-registry ghcr-pull-secret `
  --docker-server=ghcr.io `
  --docker-username=<your-github-username> `
  --docker-password=<github-token-with-packages-read> `
  --namespace scrum-poker
```

For local minikube with `minikube image load` (see below), you can skip this — set `imagePullPolicy: IfNotPresent` in deployment (default).

---

## Local (Minikube)

### 1. Start minikube + enable ingress

```powershell
minikube start
minikube addons enable ingress
```

### 2. Create namespace

```powershell
kubectl create namespace scrum-poker
```

### 3. Create K8s secrets + ghcr pull secret

See [K8s secrets management](#k8s-secrets-management) and [ghcr.io authentication](#ghcrio-authentication-imagepullsecret) above.
Use any password for local — only needs to be consistent between the two secrets.

### 4. Get images onto minikube

Two options:

**Option A — Pull from ghcr.io (requires imagePullSecret + internet):**
```powershell
# Ensure ghcr-pull-secret exists (see ghcr auth section above)
helm install scrum-poker ./k8s `
  --namespace scrum-poker `
  -f k8s/values.yaml
```

**Option B — Build + load locally (faster for dev, no auth needed):**
```powershell
# Build API image (.NET SDK container publishing)
dotnet publish src/ScrumPoker.API `
  /t:PublishContainer `
  -c Release `
  -p ContainerImageName=ghcr.io/spicycoder/scrumpoker-api `
  -p ContainerImageTag=latest `
  -p ContainerRuntimeIdentifier=linux-x64

# Build web image
docker build -f k8s/web.Dockerfile -t ghcr.io/spicycoder/scrumpoker-web:latest web/

# Load both into minikube
minikube image load ghcr.io/spicycoder/scrumpoker-api:latest
minikube image load ghcr.io/spicycoder/scrumpoker-web:latest

# Deploy — ImagePullPolicy defaults to IfNotPresent, finds local images first
helm install scrum-poker ./k8s `
  --namespace scrum-poker `
  -f k8s/values.yaml
```

### 6. Verify

```powershell
kubectl get pods -n scrum-poker
kubectl get ingress -n scrum-poker
```

### 7. Access

```powershell
minikube tunnel   # keep running in a separate terminal
```

App available at: `http://localhost`

### Upgrade after code changes

Rebuild + push images, then:
```powershell
helm upgrade scrum-poker ./k8s `
  --namespace scrum-poker `
  -f k8s/values.yaml
```

### Scale API replicas

```powershell
kubectl scale deployment scrumpoker-api-deployment --replicas=3 -n scrum-poker
```

### Teardown

```powershell
helm uninstall scrum-poker --namespace scrum-poker
kubectl delete namespace scrum-poker
```

---

## Production (K3s / K8s)

### 1. Create namespace

```powershell
kubectl create namespace scrum-poker
```

### 2. Create secrets + ghcr pull secret

See [K8s secrets management](#k8s-secrets-management) and [ghcr.io authentication](#ghcrio-authentication-imagepullsecret).
Use strong password for production.

### 3. Deploy

```powershell
helm install scrum-poker ./k8s `
  --namespace scrum-poker `
  -f k8s/values.yaml
```

### 4. Verify

```powershell
kubectl get pods -n scrum-poker
kubectl get ingress -n scrum-poker
```

### Notes

- nginx ingress controller must be installed on the cluster
- Ingress sticky session cookie `.ScrumPoker.Affinity` required for SignalR WS affinity
- Redis backplane handles cross-replica SignalR message routing
- `/health` = readiness probe (pod pulled from rotation if Redis unreachable)
- `/alive` = liveness probe (self-check, independent of Redis)
- `k8s/templates/k8s-dashboard/` = Aspire dashboard (OTLP collector) — delete if not needed

---

## Troubleshooting

### Pods stuck in ImagePullBackOff

```powershell
kubectl describe pod <pod-name> -n scrum-poker
```

Likely causes:
- No `ghcr-pull-secret` → `kubectl create secret docker-registry ghcr-pull-secret ...`
- Wrong registry URL in values.yaml → check `parameters.scrumpoker_api.scrumpoker_api_image`
- Token expired → regenerate GitHub token with `read:packages`

### API pod CrashLoopBackOff (Redis unreachable)

Check Redis pod is running first:
```powershell
kubectl logs -n scrum-poker redis-statefulset-0
kubectl logs -n scrum-poker <api-pod-name>
```

Common: Redis takes ~5s to start; API fails readiness probe → restart. Wait 30s, then `kubectl get pods -n scrum-poker`.

### Web pod CrashLoopBackOff (YARP config)

Web pod uses `mcr.microsoft.com/dotnet/nightly/yarp:2.3-preview`. If appsettings.json missing or misconfigured, pod starts but returns 503.

Check:
```powershell
kubectl logs -n scrum-poker web-deployment-<pod-id>
kubectl exec -n scrum-poker web-deployment-<pod-id> -- cat /app/appsettings.json
```

### Ingress not routing

```powershell
kubectl describe ingress -n scrum-poker
minikube tunnel  # must be running in separate terminal
```

Verify ingress controller addon:
```powershell
minikube addons enable ingress
kubectl get pods -n ingress-nginx
```
