#!/usr/bin/env bash
set -euo pipefail

# Run this once to create secrets in the scrum-poker namespace.
# Replace "devpass" with any password — just keep it consistent.
# This is safe for local minikube. Never commit real passwords.

PASSWORD="${1:-devpass}"
NS="scrum-poker"

echo "Creating secrets in namespace '$NS' with password: $PASSWORD"

kubectl create secret generic scrum-poker-redis-secret \
  --from-literal=REDIS_PASSWORD="$PASSWORD" \
  --namespace "$NS" \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl create secret generic scrum-poker-api-secret \
  --from-literal=ConnectionStrings__redis="redis-service:6379,password=$PASSWORD" \
  --namespace "$NS" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "Secrets created."
