# KubeCart — Capstone Microservices Application (Minikube)

KubeCart is a **production-style microservice e-commerce application** built as a capstone project using modern backend, frontend, and DevOps practices.

The system is designed to closely resemble **real-world microservice architecture**, including strict service boundaries, independent databases, and Kubernetes deployment.

---

## Tech Stack

- **Backend**: .NET 8 Web APIs (Microservices)
- **Frontend**: React + Vite
- **Database**: SQL Server (external to Kubernetes)
- **Containerization**: Docker
- **Orchestration**: Kubernetes (Minikube)
- **Routing**: Kubernetes Ingress (path-based)
- **Authentication**: JWT (Role-based: Admin / Customer)

---

## Architecture Overview

### Services

- **Identity Service**
  - User registration & login
  - JWT token issuance
  - Role-based claims (Admin / Customer)
  - `/api/auth/*`

- **Catalog Service**
  - Product listing & search
  - Product details
  - Admin product management (create, update, delete)
  - `/api/catalog/*`

- **Orders Service**
  - Cart management
  - Checkout flow
  - Order history
  - Admin order status updates
  - `/api/orders/*`
  - Communicates with Catalog service via HTTP

- **UI (Frontend)**
  - React SPA
  - Customer and Admin views
  - Served via Nginx in Kubernetes

---

## Databases (External to Kubernetes)

Each microservice owns **its own database** (no shared schemas):

- `KubeCart_Identity`
- `KubeCart_Catalog`
- `KubeCart_Orders`

> ⚠️ Databases are hosted **outside Kubernetes** (local SQL Server).  
> Kubernetes pods connect using `host.minikube.internal`.

---

## Ingress Routing

All services are accessed under a **single domain**:

| Path | Service |
|-----|--------|
| `/` | UI |
| `/api/auth` | Identity API |
| `/api/catalog` | Catalog API |
| `/api/orders` | Orders API |

Example:
http://kubecart.local


---

## Repository Structure

```txt
kubecart/
│
├── docs/
│   ├── architecture.md
│   ├── api-contract.md
│   └── troubleshooting.md
│
├── scripts/
│   ├── 00-check-prereqs.ps1
│   ├── 01-start-minikube.ps1
│   ├── 02-build-images.ps1
│   ├── 03-deploy-k8s.ps1
│   ├── 04-smoke-test.ps1
│   └── 99-cleanup.ps1
│
├── k8s/
│   ├── namespace.yaml
│   ├── ingress.yaml
│   ├── identity/
│   ├── catalog/
│   ├── orders/
│   └── ui/
│
├── services/
│   ├── identity/
│   ├── catalog/
│   └── orders/
│
├── ui/
│   ├── src/
│   └── Dockerfile
│
└── README.md
Prerequisites
Install the following on your machine:

Docker Desktop

kubectl

Minikube

SQL Server (local) + SSMS

Node.js 18+

.NET SDK 8+

Git

Step 1 — Initialize Databases (External SQL Server)
This project uses SQL Server outside Kubernetes.

Open PowerShell in repo root:

cd C:\Projects\kubecart
Option A — Windows Authentication (Local Dev)
.\scripts\sql\00-create-dbs.ps1
Option B — SQL Authentication (Required for Kubernetes)
$env:SQL_SERVER="localhost"
$env:SQL_USER="sa"
$env:SQL_PASSWORD="YOUR_PASSWORD"

.\scripts\sql\00-create-dbs.ps1
This script creates:

All required databases

Tables

Initial seed data (Catalog categories)

Step 2 — Prepare Kubernetes Secrets
Example secret templates are provided:

k8s/secrets/
  identity-secrets.example.yaml
  catalog-secrets.example.yaml
  orders-secrets.example.yaml
Create real secrets (DO NOT COMMIT):

identity-secrets.example.yaml → identity-secrets.yaml
catalog-secrets.example.yaml  → catalog-secrets.yaml
orders-secrets.example.yaml   → orders-secrets.yaml
Edit the real files and set:

DB_HOST

DB_USER

DB_PASSWORD

JWT_SIGNING_KEY (same for all services)

APP_ENCRYPTION_KEY (Identity only)

Secrets are ignored by Git via .gitignore.

Step 3 — Start Minikube
.\scripts\00-check-prereqs.ps1
.\scripts\01-start-minikube.ps1
Note the Minikube IP shown in output.

Step 4 — Update Hosts File
Edit as Administrator:

C:\Windows\System32\drivers\etc\hosts
Add:

<MINIKUBE_IP> kubecart.local
Step 5 — Build Docker Images (Inside Minikube)
.\scripts\02-build-images.ps1
Images built:

kubecart-identity:local

kubecart-catalog:local

kubecart-orders:local

kubecart-ui:local

Step 6 — Deploy to Kubernetes
.\scripts\03-deploy-k8s.ps1
Watch pods:

kubectl -n demo get pods -w
Step 7 — Smoke Test
.\scripts\04-smoke-test.ps1
Open in browser:

http://kubecart.local
Health Endpoints
Each service exposes:

/health/live → process alive

/health/ready → DB reachable

Used by Kubernetes liveness & readiness probes.

Authentication & Roles
JWT-based authentication

Roles embedded as claims

Customer

Admin

Admin-only endpoints enforced via [Authorize(Roles = "Admin")]

Cleanup
To remove all Kubernetes resources:

.\scripts\99-cleanup.ps1
Notes
SQL Server must allow TCP connections

SQL Authentication is required for Kubernetes

UI communicates with APIs using relative paths (/api/...)

Orders service communicates with Catalog via internal service DNS

Documentation
See /docs for:

architecture.md

api-contract.md

troubleshooting.md

