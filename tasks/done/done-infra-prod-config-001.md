# todo-infra-prod-config-001.md — appsettings.Production.json + env validation

**Module** : Infra
**Dépendances** : aucune
**Priorité** : CRITIQUE (pre-prod)

---

## Objectif

Créer la configuration de production avec validation fail-fast au démarrage.

## Implémentation

### 1. appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Vetolib": "Information"
    }
  },
  "Cors": {
    "Origins": "__OVERRIDE_VIA_ENV__"
  },
  "ConnectionStrings": {
    "DefaultConnection": "__OVERRIDE_VIA_ENV__"
  }
}
```

Les valeurs sensibles sont injectées via variables d'environnement (pas dans le fichier).

### 2. Validation au démarrage (Program.cs)

```csharp
if (app.Environment.IsProduction())
{
    var requiredVars = new[] { "Jwt__Key", "ConnectionStrings__DefaultConnection", "Cors__Origins" };
    var missing = requiredVars.Where(v => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(v))).ToList();
    if (missing.Any())
        throw new InvalidOperationException($"Missing required environment variables: {string.Join(", ", missing)}");
}
```

### 3. Docker-compose.prod.yml

Créer un override pour la prod :
```yaml
services:
  backend:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Jwt__Key=${JWT_SECRET}
      - ConnectionStrings__DefaultConnection=${DATABASE_URL}
      - Cors__Origins=${CORS_ORIGINS}
      - Email__Provider=console
```

### 4. .env.example

Créer `.env.example` documentant toutes les variables requises (sans valeurs).

## Critère

```
□ appsettings.Production.json créé
□ Validation fail-fast des env vars en prod
□ docker-compose.prod.yml créé
□ .env.example documenté
□ Renommer en done
```
