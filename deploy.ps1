param(
  [Parameter(Mandatory)]
  [string]$RedisPassword,
  [switch]$Pull  # pull from ghcr instead of using local images
)

$ErrorActionPreference = "Stop"
$PSNativeCommandErrorActionPreference = $true

$ns = "scrum-poker"
$apiImg = "ghcr.io/spicycoder/scrumpoker-api:latest"
$webImg = "ghcr.io/spicycoder/scrumpoker-web:latest"

# Preflight checks
$minikubeStatus = & minikube status 2>$null
if ($LASTEXITCODE -ne 0) {
  Write-Error "Minikube not running. Run: minikube start && minikube addons enable ingress"
  exit 1
}

$ingressPods = & kubectl get pods -n ingress-nginx --no-headers 2>$null
if (-not $ingressPods) {
  Write-Warning "Ingress controller not found. Run: minikube addons enable ingress"
}

# Namespace
kubectl create namespace $ns --dry-run=client -o yaml | kubectl apply -f -

# Secrets (idempotent)
kubectl create secret generic scrum-poker-redis-secret `
  --from-literal=REDIS_PASSWORD="$RedisPassword" `
  --dry-run=client -o yaml -n $ns | kubectl apply -f -

kubectl create secret generic scrum-poker-api-secret `
  --from-literal=ConnectionStrings__redis="redis-service:6379,password=$RedisPassword" `
  --from-literal=REDIS_PASSWORD="$RedisPassword" `
  --from-literal=REDIS_URI="redis://:$RedisPassword@redis-service:6379" `
  --dry-run=client -o yaml -n $ns | kubectl apply -f -

# Load images into minikube (default: local images)
if ($Pull) {
  Write-Host "`nPulling images from ghcr.io..." -ForegroundColor Cyan
} else {
  Write-Host "`nLoading local images into minikube..." -ForegroundColor Cyan
  minikube image load $apiImg
  minikube image load $webImg
}

# Deploy
Write-Host "`nDeploying Helm chart..." -ForegroundColor Cyan
helm upgrade --install scrum-poker ./k8s --namespace $ns -f k8s/values.yaml

# Wait for each component
Write-Host "`nWaiting for Redis..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app.kubernetes.io/component=redis -n $ns --timeout=60s

Write-Host "Waiting for API..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app.kubernetes.io/component=scrumpoker-api -n $ns --timeout=120s

Write-Host "Waiting for web..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app.kubernetes.io/component=web -n $ns --timeout=60s

kubectl get pods -n $ns

Write-Host "`nDone! Run 'minikube tunnel' in a separate terminal, then open http://localhost" -ForegroundColor Green
