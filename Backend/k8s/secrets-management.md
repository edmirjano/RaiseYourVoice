# RaiseYourVoice Kubernetes Secrets Management

This document outlines how sensitive configuration values are managed in the RaiseYourVoice Kubernetes deployment.

## Secret Management Strategy

All sensitive configuration values should be stored in Kubernetes Secrets and injected as environment variables into containers.

### Environment Variable Naming

All environment variables for the RaiseYourVoice backend should be prefixed with `RYV_` followed by the configuration path with double underscores as separators.

For example:
- `JwtSettings:SecretKey` becomes `RYV_JwtSettings__SecretKey`
- `MongoDbSettings:ConnectionString` becomes `RYV_MongoDbSettings__ConnectionString`
- `ConnectionStrings:RedisConnection` becomes `RYV_ConnectionStrings__RedisConnection`

### Creating Kubernetes Secrets

```bash
# Create a secret for database credentials
kubectl create secret generic ryv-db-credentials \
  --from-literal=RYV_MongoDbSettings__ConnectionString="mongodb://username:password@mongodb-service:27017" \
  --from-literal=RYV_ConnectionStrings__RedisConnection="redis-service:6379,password=yourpassword"

# Create a secret for JWT and API authentication
kubectl create secret generic ryv-auth-secrets \
  --from-literal=RYV_JwtSettings__SecretKey="your-production-jwt-secret-key" \
  --from-literal=RYV_JwtSettings__Issuer="api.raiseyourvoice.al" \
  --from-literal=RYV_JwtSettings__Audience="raiseyourvoice.al"

# Create a secret for encryption keys
kubectl create secret generic ryv-encryption-keys \
  --from-literal=RYV_EncryptionSettings__Key="your-production-encryption-key" \
  --from-literal=RYV_EncryptionSettings__IV="your-production-encryption-iv"

# Create a secret for payment processing
kubectl create secret generic ryv-payment-secrets \
  --from-literal=RYV_Stripe__SecretKey="your-production-stripe-secret-key" \
  --from-literal=RYV_Stripe__WebhookSecret="your-production-stripe-webhook-secret"
```

### Mounting Secrets in Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ryv-backend
spec:
  replicas: 3
  selector:
    matchLabels:
      app: ryv-backend
  template:
    metadata:
      labels:
        app: ryv-backend
    spec:
      containers:
      - name: api
        image: raiseyourvoice/backend:latest
        ports:
        - containerPort: 80
        envFrom:
        - secretRef:
            name: ryv-db-credentials
        - secretRef:
            name: ryv-auth-secrets
        - secretRef:
            name: ryv-encryption-keys
        - secretRef:
            name: ryv-payment-secrets
```

## Local Development

For local development, you should use .NET's User Secrets feature:

```bash
cd RaiseYourVoice.Api
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "your-dev-jwt-secret"
dotnet user-secrets set "MongoDbSettings:ConnectionString" "mongodb://localhost:27017"
```

## Secret Rotation

Secrets should be rotated periodically following this process:

1. Create new secrets with updated values
2. Update deployments to use the new secrets
3. Roll out the update with zero downtime
4. Verify the application is working with the new secrets
5. Delete the old secrets

Example secret rotation script:

```bash
# Create new secret
kubectl create secret generic ryv-auth-secrets-new \
  --from-literal=RYV_JwtSettings__SecretKey="your-new-jwt-secret-key" \
  --from-literal=RYV_JwtSettings__Issuer="api.raiseyourvoice.al" \
  --from-literal=RYV_JwtSettings__Audience="raiseyourvoice.al"

# Update deployment to use new secret
kubectl patch deployment ryv-backend -p '{"spec":{"template":{"spec":{"containers":[{"name":"api","envFrom":[{"secretRef":{"name":"ryv-auth-secrets-new"}}]}]}}}}'

# Wait for rollout to complete
kubectl rollout status deployment/ryv-backend

# Verify application is working, then delete old secret
kubectl delete secret ryv-auth-secrets

# Rename new secret to standard name
kubectl get secret ryv-auth-secrets-new -o yaml | sed 's/ryv-auth-secrets-new/ryv-auth-secrets/' | kubectl apply -f -
kubectl delete secret ryv-auth-secrets-new
```

## Security Best Practices

1. Never commit secrets to source control
2. Use short TTL (Time-To-Live) for secrets where possible
3. Implement least-privilege access to secrets
4. Audit secret access regularly
5. Rotate secrets after team member departures
6. Consider using a dedicated secret manager like HashiCorp Vault for advanced use cases