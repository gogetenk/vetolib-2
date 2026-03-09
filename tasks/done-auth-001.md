# todo-auth-001.md — Auth : Login JWT + Refresh Token

**Module** : Auth
**Dépendances** : scaffold-000 (solution doit compiler)
**Gherkins** : `features/auth/login.feature`
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `ardalis-modular-monolith`, `multitenant-efcore`, `aspnet-minimal-api`, `reqnroll-bindings`

---

## Périmètre exact

Implémenter dans `Modules/Auth/Vetolib.Auth/` :

**Entités Domain :**
- `User` : factory `Create()` → `Result<User>`, méthode `RecordFailedLogin()` → `Result`, `Unlock()` → `Result`
- `RefreshToken` : Value Object, méthode `Revoke()` → `Result`

**Commands/Handlers :**
- `LoginCommand` → `Result<AuthTokenDto>`
- `RefreshTokenCommand` → `Result<AuthTokenDto>`

**Validators :**
- `LoginValidator` : Email non vide, Password non vide

**Infrastructure :**
- `AuthDbContext` : hérite `MultiTenantDbContext`, DbSet User + RefreshToken
- Migration initiale Auth

**Endpoints** (dans `Api/AuthEndpoints.cs`) :
- `POST /api/auth/login`
- `POST /api/auth/refresh`

**Contracts** (dans `Vetolib.Auth.Contracts/`) :
- `LoginRequest`, `AuthTokenDto`, `UserDto`, `UserRole` enum

**ModuleServiceRegistrar** : `AddAuthModule()` + `MapAuthEndpoints()`

---

## Règles métier à implémenter

- JWT access token : 15 min. Refresh token : 7 jours
- Refresh token rotation : chaque refresh invalide le précédent, émet une nouvelle paire
- 5 tentatives échouées → compte bloqué 15 min
- Mot de passe : BCrypt hash
- Claim JWT : `sub`, `email`, `clinic_id`, `role`

---

## Critère de complétion

```
□ Tous les scénarios features/auth/login.feature sont VERTS
□ dotnet build → 0 erreurs
□ Renommer en done-auth-001.md
```
