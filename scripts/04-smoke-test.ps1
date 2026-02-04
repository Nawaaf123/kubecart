Write-Host "Testing ingress routes (requires kubecart.local mapped in hosts)..."

curl http://kubecart.local/api/auth/ping
curl http://kubecart.local/api/catalog/ping
curl http://kubecart.local/api/orders/ping

Write-Host "Done."
