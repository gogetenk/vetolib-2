# todo-infra-secrets-001.md — Gestion des secrets et configuration sécurisée

**Module** : Infrastructure / Backend
**Dépendances** : done-back-migrations-001
**Skills à lire** : `dotnet-aspire`

---

## Contexte

Le JWT secret est en dur dans `appsettings.json`. La connection string PostgreSQL est en clair. Aucune validation des variables d'environnement au démarrage. C'est un risque de sécurité et un bloquant pour la production.

## Périmètre exact

### 1. Externaliser tous les secrets

Supprimer les secrets de `appsettings.json` et `appsettings.Development.json`. Les remplacer par des variables d'environnement :

```json
// appsettings.json — plus de secrets
{
  "Jwt": {
    "Issuer": "vetolib",
    "Audience": "vetolib-app",
    "ExpirationMinutes": 15,
    "RefreshExpirationDays": 7
    // Secret: retiré — doit venir de l'environnement
  }
}
```

Configuration via env vars :
```
Jwt__Secret=<min 32 chars>
ConnectionStrings__vetolibdb=Host=...;...
```

### 2. Validation au démarrage (fail-fast)

Ajouter une validation dans `Program.cs` qui crash immédiatement si un secret manque :

```csharp
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is required. Set via environment variable Jwt__Secret");

if (jwtSecret.Length < 32)
    throw new InvalidOperationException("Jwt:Secret must be at least 32 characters");
```

Même chose pour la connection string PostgreSQL.

### 3. User Secrets pour le développement local

Configurer .NET User Secrets pour le dev :
```bash
dotnet user-secrets init --project src/backend/Vetolib.Api
dotnet user-secrets set "Jwt:Secret" "dev-secret-minimum-32-characters-long"
dotnet user-secrets set "ConnectionStrings:vetolibdb" "Host=localhost;..."
```

Documenter dans le README comment configurer.

### 4. .gitignore vérifié

S'assurer que `.env`, `.env.local`, `appsettings.*.local.json` sont dans `.gitignore`. Scanner le repo pour tout secret committé accidentellement.

### 5. Frontend — valider les env vars

Ajouter dans `next.config.ts` ou un fichier `env.ts` :
```typescript
// src/frontend/src/lib/env.ts
export const env = {
  apiUrl: process.env.NEXT_PUBLIC_API_URL ?? '',
} as const
```

Pas de validation stricte côté frontend (MSW mode = pas d'API_URL, c'est OK).

## Critère de complétion

```
□ Aucun secret dans appsettings.json ni dans le code source
□ Validation fail-fast au démarrage si secret manquant
□ User Secrets configuré pour le dev local
□ .gitignore couvre .env, secrets, credentials
□ Scan du repo : 0 secret committé
□ docker-compose.yml utilise des variables d'env (pas de valeurs en dur)
□ README section "Configuration" documentée
□ Renommer en done-infra-secrets-001.md
```
