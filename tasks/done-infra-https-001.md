# todo-infra-https-001.md — HTTPS enforcement + HSTS

**Module** : Infra
**Dépendances** : aucune
**Priorité** : CRITIQUE (pre-prod)

---

## Objectif

Forcer HTTPS en production et configurer HSTS.

## Implémentation

### 1. Program.cs — middleware HTTPS

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

Placer `UseHttpsRedirection` avant `UseRouting`.

### 2. Docker / reverse proxy

Dans `docker-compose.yml`, ajouter un service Caddy (reverse proxy TLS automatique via Let's Encrypt) devant le backend :

```yaml
caddy:
  image: caddy:2-alpine
  ports:
    - "80:80"
    - "443:443"
  volumes:
    - ./Caddyfile:/etc/caddy/Caddyfile
    - caddy_data:/data
```

Créer `Caddyfile` :
```
api.vetolib.ae {
    reverse_proxy backend:8080
}
app.vetolib.ae {
    reverse_proxy frontend:3000
}
```

### 3. Frontend next.config.ts

Ajouter les security headers :
```typescript
headers: [
  { key: 'Strict-Transport-Security', value: 'max-age=63072000; includeSubDomains; preload' },
  { key: 'X-Content-Type-Options', value: 'nosniff' },
  { key: 'X-Frame-Options', value: 'DENY' },
  { key: 'Referrer-Policy', value: 'strict-origin-when-cross-origin' },
]
```

## Critère

```
□ UseHttpsRedirection + UseHsts dans Program.cs
□ Caddy reverse proxy dans docker-compose
□ Security headers dans next.config.ts
□ Renommer en done
```
