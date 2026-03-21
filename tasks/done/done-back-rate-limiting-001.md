# todo-back-rate-limiting-001.md — Rate limiting sur les endpoints sensibles

**Module** : Infrastructure / Auth
**Dépendances** : aucune
**Priorité** : HAUTE (Security audit H-01)

---

## Contexte

Aucun rate limiting sur `/api/auth/login` et `/api/auth/refresh`. Vulnérable au credential stuffing et brute force au-delà du lockout par compte.

## Périmètre

Installer `AspNetCoreRateLimit` ou utiliser le rate limiter built-in de .NET 8+ :

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});
```

Appliquer :
- `/api/auth/login` → "auth" (10 req/min par IP)
- `/api/auth/refresh` → "auth"
- Tous les autres endpoints → "api" (100 req/min par IP)
- Retourner 429 Too Many Requests avec header Retry-After

## Critère
```
□ Rate limiter configuré
□ Login : 10 req/min par IP
□ API générale : 100 req/min par IP
□ 429 retourné avec Retry-After header
□ Test unitaire
□ Renommer en done
```
