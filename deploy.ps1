param(
  [Parameter(Mandatory)]
  [string]$RedisPassword,
  [switch]$Build
)

$ErrorActionPreference = "Stop"
$PSNativeCommandErrorActionPreference = $true

$containerCmd = if (Get-Command docker -ErrorAction SilentlyContinue) { "docker" } else { "podman" }
Write-Host "Using $containerCmd for container builds" -ForegroundColor DarkGray

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

# Secrets (idempotent — apply, not delete+create)
kubectl create secret generic scrum-poker-redis-secret `
  --from-literal=REDIS_PASSWORD="$RedisPassword" `
  --dry-run=client -o yaml -n $ns | kubectl apply -f -

kubectl create secret generic scrum-poker-api-secret `
  --from-literal=ConnectionStrings__redis="redis-service:6379,password=$RedisPassword" `
  --from-literal=REDIS_PASSWORD="$RedisPassword" `
  --from-literal=REDIS_URI="redis://:$RedisPassword@redis-service:6379" `
  --dry-run=client -o yaml -n $ns | kubectl apply -f -

# Build or pull images
if ($Build) {
  Write-Host "`nBuilding API image..." -ForegroundColor Cyan
  dotnet publish src/ScrumPoker.API /t:PublishContainer -c Release `
    -p ContainerImageName=ghcr.io/spicycoder/scrumpoker-api `
    -p ContainerImageTag=latest `
    -p ContainerRuntimeIdentifier=linux-x64

  Write-Host "`nBuilding web image..." -ForegroundColor Cyan
  & $containerCmd build -f k8s/web.Dockerfile -t ghcr.io/spicycoder/scrumpoker-web:latest web/

  Write-Host "`nLoading images into minikube..." -ForegroundColor Cyan
  minikube image load $apiImg
  minikube image load $webImg
} else {
  Write-Host "`nPulling images from ghcr.io..." -ForegroundColor Cyan
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
