# DevOps Handoff — KubeCart (Minikube Deployment)

This document is for the DevOps teammate to deploy KubeCart into Minikube Kubernetes.

---

## 1) What this repo contains

Microservices:
- Identity API (.NET 8)  → /api/auth/*
- Catalog API (.NET 8)   → /api/catalog/*
- Orders API (.NET 8)    → /api/orders/*
- UI (React/Vite → Nginx) → /

Kubernetes namespace:
- demo

Ingress host:
- kubecart.local

Ingress routes:
- /            → ui-service
- /api/auth    → identity-service
- /api/catalog → catalog-service
- /api/orders  → orders-service

---

## 2) External SQL Server requirement (NOT inside Kubernetes)

SQL Server is hosted outside Kubernetes.

Databases:
- KubeCart_Identity
- KubeCart_Catalog
- KubeCart_Orders

Minikube DB host:
- host.minikube.internal

SQL must allow TCP + SQL auth recommended.

---

## 3) Environment variables contract

Identity (ConfigMap):
- DB_HOST
- DB_NAME=KubeCart_Identity
- DB_USER
Identity (Secrets):
- DB_PASSWORD
- JWT_SIGNING_KEY
- APP_ENCRYPTION_KEY

Catalog (ConfigMap):
- DB_HOST
- DB_NAME=KubeCart_Catalog
- DB_USER
Catalog (Secrets):
- DB_PASSWORD
- JWT_SIGNING_KEY (same as Identity)

Orders (ConfigMap):
- DB_HOST
- DB_NAME=KubeCart_Orders
- DB_USER
- CATALOG_SERVICE_URL=http://catalog-service.demo.svc.cluster.local
Orders (Secrets):
- DB_PASSWORD
- JWT_SIGNING_KEY (same as Identity)

UI:
- Uses relative calls /api/... (no env required)

---

## 4) Health endpoints (Kubernetes probes)

All APIs expose:
- GET /health/live
- GET /health/ready (DB check)

---

## 5) Secrets creation workflow

Example templates exist:
- k8s/secrets/*-secrets.example.yaml

DevOps must create:
- k8s/secrets/identity-secrets.yaml
- k8s/secrets/catalog-secrets.yaml
- k8s/secrets/orders-secrets.yaml

JWT_SIGNING_KEY must be SAME across all services.

---

## 6) Docker images

Expected images:
- kubecart-identity:local
- kubecart-catalog:local
- kubecart-orders:local
- kubecart-ui:local

---

## 7) Smoke test URLs

After deployment:
- http://kubecart.local
- http://kubecart.local/api/auth/ping
- http://kubecart.local/api/catalog/ping
- http://kubecart.local/api/orders/ping

---

## 8) Debug commands

kubectl -n demo get all
kubectl -n demo get pods -o wide
kubectl -n demo describe pod <podname>
kubectl -n demo logs deploy/identity-api
kubectl -n demo logs deploy/catalog-api
kubectl -n demo logs deploy/orders-api
kubectl -n demo get ingress
kubectl -n demo describe ingress kubecart-ingress
kubectl -n demo get endpoints

Port forward UI if ingress fails:
kubectl -n demo port-forward svc/ui-service 8085:80
Open http://localhost:8085
