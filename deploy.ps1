param(
  [Parameter(Mandatory)]
  [string]$RedisPassword
)

$ErrorActionPreference = "Stop"

$ns = "scrum-poker"
$apiImg = "ghcr.io/spicycoder/scrumpoker-api:latest"
$webImg = "ghcr.io/spicycoder/scrumpoker-web:latest"
$redisChartVersion = "27.0.4"

# ---- Preflight ----
$minikubeOk = & minikube status 2>$null
if ($LASTEXITCODE -ne 0) {
  Write-Error "Minikube not running. Run: minikube start && minikube addons enable ingress"
  exit 1
}

# Check Helm installed
if (-not (Get-Command helm -ErrorAction SilentlyContinue)) {
  Write-Error "Helm not found. Install it first: https://helm.sh/docs/intro/install/"
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

# ---- 4. Clean up old single Redis (if present) ----
kubectl delete statefulset redis -n $ns --ignore-not-found 2>$null
kubectl delete service redis-service -n $ns --ignore-not-found 2>$null
kubectl delete secret scrum-poker-redis-secret -n $ns --ignore-not-found 2>$null

# ---- 5. Redis with Sentinel (Helm) ----
Write-Host "Installing/upgrading Redis with Sentinel via Helm..." -ForegroundColor Cyan
helm repo add bitnami https://charts.bitnami.com/bitnami 2>$null
helm upgrade --install redis bitnami/redis --version $redisChartVersion `
  --namespace $ns `
  --set architecture=sentinel `
  --set replica.replicaCount=3 `
  --set auth.password="$RedisPassword" `
  --set auth.sentinelPassword="$RedisPassword" `
  --wait --timeout 5m
if ($LASTEXITCODE -ne 0) {
  Write-Error "Helm install failed. Try running 'helm repo update' first."
  exit 1
}

# ---- 6. Secrets ----
Write-Host "Creating secrets..." -ForegroundColor Cyan
kubectl create secret generic scrum-poker-api-secret `
  --from-literal=ConnectionStrings__redis="redis-redis:26379,serviceName=mymaster,password=$RedisPassword,sentinelPassword=$RedisPassword" `
  --dry-run=client -o yaml -n $ns | kubectl apply -f -

# ---- 7. Deploy app manifests ----
Write-Host "Deploying app manifests..." -ForegroundColor Cyan
kubectl apply -f k8s/api/
kubectl apply -f k8s/web/
kubectl apply -f k8s/ingress.yaml

# ---- 7. Wait for app pods ----
Write-Host "Waiting for app pods..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l component=scrumpoker-api -n $ns --timeout=120s
kubectl wait --for=condition=ready pod -l component=web -n $ns --timeout=60s

# ---- 8. Restart deployments to pick up fresh images ----
kubectl rollout restart deployment -n $ns

kubectl get pods -n $ns

Write-Host "`nDone!" -ForegroundColor Green
Write-Host "Run 'minikube tunnel' in a separate terminal, then open http://localhost" -ForegroundColor Yellow
