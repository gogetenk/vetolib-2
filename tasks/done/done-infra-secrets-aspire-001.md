# todo-infra-secrets-aspire-001.md — Secrets management Aspire-compatible

**Module** : Infra
**Dépendances** : aucune
**Priorité** : HAUTE (pre-prod)

---

## Objectif

Sécuriser les secrets (JWT key, DB password) avec une approche Aspire-native, simple et gratuite.

## Approche

Utiliser les **Aspire Parameters** avec `secret: true` — c'est le mécanisme natif d'Aspire pour les secrets. En dev, les valeurs viennent de `user-secrets`. En prod, des variables d'environnement.

## Implémentation

### 1. AppHost/Program.cs — Aspire Parameters

```csharp
var jwtKey = builder.AddParameter("jwt-key", secret: true);
var dbPassword = builder.AddParameter("db-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: dbPassword)
    .WithDataVolume();

var api = builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(postgres)
    .WithEnvironment("Jwt__Key", jwtKey);
```

### 2. Dev — dotnet user-secrets

```bash
cd src/backend/AppHost
dotnet user-secrets init
dotnet user-secrets set "Parameters:jwt-key" "dev-super-secret-key-at-least-32-chars"
dotnet user-secrets set "Parameters:db-password" "devpassword123"
```

### 3. Prod — variables d'environnement

Aspire Parameters sont automatiquement résolus depuis les env vars en production :
- `Parameters__jwt-key` → résolu par Aspire
- `Parameters__db-password` → résolu par Aspire

### 4. Supprimer les secrets hardcodés

- Retirer tout secret en clair de `appsettings.json` et `appsettings.Development.json`
- Vérifier que `docker-compose.yml` utilise `${VAR}` avec `.env` (pas de valeurs en clair)

### 5. .gitignore

Vérifier que ces fichiers sont ignorés :
```
*.secrets.json
.env
!.env.example
```

## Critère

```
□ Aspire Parameters secret:true pour JWT key et DB password
□ dotnet user-secrets configuré pour le dev
□ Zéro secret en clair dans les fichiers versionnés
□ .env.example documenté
□ Renommer en done
```
