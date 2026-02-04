Required

Namespace: demo

Ingress host: kubecart.local

Paths:

/api/auth → identity-service

/api/catalog → catalog-service

/api/orders → orders-service

Health:

/health/live

/health/ready

Ping endpoints:

/api/auth/ping

/api/catalog/ping

/api/orders/ping

Container port: 8080 (targetPort 8080, service port 80)

Env vars (names only; no secrets committed)