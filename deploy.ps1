param(
  [Parameter(Mandatory)]
  [string]$RedisPassword
)

$ErrorActionPreference = "Stop"

$ns = "scrum-poker"
$apiImg = "ghcr.io/spicycoder/scrumpoker-api:latest"
$webImg = "ghcr.io/spicycoder/scrumpoker-web:latest"

# ---- Preflight ----
$minikubeOk = & minikube status 2>$null
if ($LASTEXITCODE -ne 0) {
  Write-Error "Minikube not running. Run: minikube start && minikube addons enable ingress"
  exit 1
}

# ---- 1. Build images ----
Write-Host "`nBuilding API image..." -ForegroundColor Cyan
dotnet publish src/ScrumPoker.API -t:PublishContainer -c Release `
  -p ContainerRepository=ghcr.io/spicycoder/scrumpoker-api `
  -p ContainerImageTag=latest `
  -p ContainerRuntimeIdentifier=linux-x64

Write-Host "Building web image..." -ForegroundColor Cyan
podman build -f k8s/web.Dockerfile -t $webImg web/

# ---- 2. Load into minikube ----
Write-Host "Loading images into minikube..." -ForegroundColor Cyan
minikube image load $apiImg $webImg

# ---- 3. Namespace ----
kubectl create namespace $ns --dry-run=client -o yaml | kubectl apply -f -

# ---- 4. Secrets ----
Write-Host "Creating secrets..." -ForegroundColor Cyan
kubectl create secret generic scrum-poker-redis-secret `
  --from-literal=REDIS_PASSWORD="$RedisPassword" `
  --dry-run=client -o yaml -n $ns | kubectl apply -f -

kubectl create secret generic scrum-poker-api-secret `
  --from-literal=ConnectionStrings__redis="redis-service:6379,password=$RedisPassword" `
  --dry-run=client -o yaml -n $ns | kubectl apply -f -

# ---- 5. Deploy manifests ----
Write-Host "Deploying manifests..." -ForegroundColor Cyan
kubectl apply -f k8s/redis/
kubectl apply -f k8s/api/
kubectl apply -f k8s/web/
kubectl apply -f k8s/ingress.yaml

# ---- 6. Wait for pods ----
Write-Host "Waiting for pods..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l component=redis -n $ns --timeout=60s
kubectl wait --for=condition=ready pod -l component=scrumpoker-api -n $ns --timeout=120s
kubectl wait --for=condition=ready pod -l component=web -n $ns --timeout=60s

# ---- 7. Restart pods to pick up fresh images ----
kubectl rollout restart deployment -n $ns

kubectl get pods -n $ns

Write-Host "`nDone!" -ForegroundColor Green
Write-Host "Run 'minikube tunnel' in a separate terminal, then open http://localhost" -ForegroundColor Yellow
