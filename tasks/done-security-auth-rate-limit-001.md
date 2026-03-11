# todo-security-auth-rate-limit-001 — Rate limiting manquant sur login, refresh et change-password

**Module** : Auth
**Priorité** : HAUTE
**Skills à lire** : `aspnet-minimal-api`

---

## Problème détecté

Le scan de sécurité a révélé que trois endpoints critiques n'ont pas de `RequireRateLimiting` :

- `POST /api/v1/auth/login` — AllowAnonymous, aucune limitation
- `POST /api/v1/auth/refresh` — AllowAnonymous, aucune limitation
- `POST /api/v1/auth/change-password` — authentifié, mais aucune limitation

Seul `/api/v1/clinic/register` a `RequireRateLimiting("signup")`. Ces trois endpoints sont les
cibles prioritaires d'attaques par force brute et de credential stuffing.

Fichier concerné : `src/backend/Modules/Auth/Vetolib.Auth/Api/AuthEndpoints.cs`

## Règle métier

- `login` et `refresh` : appliquer une policy `"auth"` dédiée (ex: 10 requêtes/minute par IP)
- `change-password` : appliquer la même policy `"auth"` (l'attaquant peut tenter de changer le
  mot de passe en devinant l'ancien)

La policy `"auth"` doit être enregistrée dans `AuthModuleServiceRegistrar` ou dans le host
`Program.cs` selon là où les autres policies sont définies.

## Critères de complétion

```
□ Policy "auth" créée (10 req/min par IP, sliding window)
□ .RequireRateLimiting("auth") ajouté sur MapPost("/login")
□ .RequireRateLimiting("auth") ajouté sur MapPost("/refresh")
□ .RequireRateLimiting("auth") ajouté sur MapPost("/change-password")
□ dotnet build → 0 erreur
□ Test d'intégration existant toujours GREEN
```
