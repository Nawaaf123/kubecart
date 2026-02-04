kubectl apply -f .\k8s\00-namespace.yaml

kubectl apply -f .\k8s\config\identity-configmap.yaml
kubectl apply -f .\k8s\config\catalog-configmap.yaml
kubectl apply -f .\k8s\config\orders-configmap.yaml

kubectl apply -f .\k8s\secrets\identity-secrets.yaml
kubectl apply -f .\k8s\secrets\catalog-secrets.yaml
kubectl apply -f .\k8s\secrets\orders-secrets.yaml

kubectl apply -f .\k8s\deployments\identity-deployment.yaml
kubectl apply -f .\k8s\deployments\catalog-deployment.yaml
kubectl apply -f .\k8s\deployments\orders-deployment.yaml

kubectl apply -f .\k8s\services\identity-service.yaml
kubectl apply -f .\k8s\services\catalog-service.yaml
kubectl apply -f .\k8s\services\orders-service.yaml

kubectl apply -f .\k8s\ingress\ingress.yaml

Write-Host "Deployed KubeCart APIs + ingress (UI later)."
