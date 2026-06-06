# Local Minikube Deployment

PowerShell commands to deploy to local minikube. No Helm — raw `kubectl apply`.

---

## Prerequisites

- minikube, kubectl
- podman
- .NET 10 SDK

---

## First-time Deploy

### 1. Start minikube

```powershell
minikube start
minikube addons enable ingress
```

Keep minikube running. If you stop it later (e.g., restart PC), run the above again.

### 2. Deploy everything

```powershell
.\deploy.ps1 -RedisPassword "devpass"
```

This does all of the following automatically:
1. Builds API image (dotnet publish as container)
2. Builds web image (podman multi-stage)
3. Loads both into minikube
4. Creates namespace `scrum-poker`
5. Creates Redis + API secrets (password: `devpass`)
6. Applies all YAML manifests (Redis, API, Web, Ingress)
7. Waits for all pods to be ready
8. Restarts deployments to pick up fresh images

Use any password you like. Just keep it consistent.

### 3. Access the app

Open a **new terminal** and run:

```powershell
minikube tunnel
```

This stays running. Then open `http://localhost` in your browser.

---

## Redeploy (code changes)

When you change code, rebuild images and restart pods:

```powershell
dotnet publish src/ScrumPoker.API -t:PublishContainer -c Release -p ContainerRepository=ghcr.io/spicycoder/scrumpoker-api -p ContainerImageTag=latest -p ContainerRuntimeIdentifier=linux-x64

podman build -f k8s/web.Dockerfile -t ghcr.io/spicycoder/scrumpoker-web:latest web/

minikube image load ghcr.io/spicycoder/scrumpoker-api:latest ghcr.io/spicycoder/scrumpoker-web:latest

kubectl rollout restart deployment -n scrum-poker
```

Or run `.\deploy.ps1 -RedisPassword "devpass"` again (does full cycle).

---

## Scale API replicas

```powershell
kubectl scale deployment scrumpoker-api --replicas=3 -n scrum-poker
```

Check they're all ready:

```powershell
kubectl get pods -n scrum-poker -w
```

### Check traffic distribution across pods

After scaling to 3, see if requests spread across pods or stick to one:

```powershell
# Tail logs from all API pods
kubectl logs -n scrum-poker -l component=scrumpoker-api --tail=5 -f
```

Make requests from the browser (vote, create room, etc.). Each log line shows which pod handled the request. With sticky sessions, each browser stays on the same pod, but different browsers may land on different pods. The Redis backplane ensures events reach everyone regardless.

To see which pod a specific browser is on, check the ingress cookie:

```powershell
# Open browser dev tools > Application > Cookies > localhost
# Look for .ScrumPoker.Affinity cookie — the encoded value maps to a pod
```

Or just kill one pod and watch which browser disconnects:

```powershell
# Pick a pod name from 'kubectl get pods -n scrum-poker'
kubectl delete pod <pod-name> -n scrum-poker
```

---

## Useful Commands

| What | Command |
|------|---------|
| View pods | `kubectl get pods -n scrum-poker` |
| View logs | `kubectl logs deployment/scrumpoker-api -n scrum-poker` |
| Tail all API logs | `kubectl logs -n scrum-poker -l component=scrumpoker-api --tail=5 -f` |
| View ingress | `kubectl get ingress -n scrum-poker` |
| Restart all pods | `kubectl rollout restart deployment -n scrum-poker` |
| Delete a pod | `kubectl delete pod <name> -n scrum-poker` |
| Delete everything | `kubectl delete namespace scrum-poker` |
| Stop minikube | `minikube stop` |

---

## Notes

- Image names use `ghcr.io/spicycoder/...` format (registry-ready). For local dev, images are built and loaded directly into minikube's container runtime — no registry needed.
- Secrets use `--dry-run=client -o yaml | kubectl apply -f -` pattern — idempotent, safe to re-run.
- `imagePullPolicy: IfNotPresent` — uses local images first. Falls back to pulling from ghcr.io if not found locally.
