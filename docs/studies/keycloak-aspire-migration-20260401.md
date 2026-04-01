# Keycloak + Aspire + OTel — Deep Dive Technique

> **Statut** : ETUDE COMPLEMENTAIRE — Fait suite a `keycloak-migration-20260401.md`
> **Pattern valide** : C (Organizations, Keycloak 26+)
> **Date** : 2026-04-01

---

## 1. Integration .NET Aspire

### 1.1. Composant Aspire officiel

Depuis Aspire 9.1, le package `Aspire.Hosting.Keycloak` fournit `AddKeycloak()`.
Le package client cote API est `Aspire.Keycloak.Authentication`.

```xml
<!-- AppHost/AppHost.csproj -->
<PackageReference Include="Aspire.Hosting.Keycloak" Version="9.1.*" />

<!-- Vetolib.Api/Vetolib.Api.csproj (ou Auth module) -->
<PackageReference Include="Aspire.Keycloak.Authentication" Version="9.1.*" />
```

### 1.2. AppHost/Program.cs (apres migration)

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var db = postgres.AddDatabase("vetolibdb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// Keycloak — dev container with realm auto-import
var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume("keycloak-data")
    .WithRealmImport("../infra/keycloak/vetolib-realm.json")
    .WithEnvironment("KC_FEATURES", "organization")  // Enable Organizations feature
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_METRICS_ENABLED", "true");

var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithEndpoint(targetPort: 1025, scheme: "tcp", name: "smtp")
    .WithHttpEndpoint(targetPort: 8025, name: "ui");

builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(keycloak)      // Injects ConnectionStrings__keycloak
    .WaitFor(keycloak)            // Wait for Keycloak healthy before starting API
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithEnvironment("Email__Provider", "smtp")
    .WithEnvironment("Email__Smtp__Host", mailhog.GetEndpoint("smtp"))
    .WithEnvironment("Email__Smtp__Port", "1025")
    .WithEnvironment("Email__Smtp__EnableSsl", "false");

builder.Build().Run();
```

**Ce que `AddKeycloak("keycloak")` fait concretement** :

1. Lance `quay.io/keycloak/keycloak:26.0` en mode `start-dev`
2. Expose le port 8080 (HTTP console admin + OIDC endpoints)
3. Cree un admin user par defaut (`admin/admin` en dev)
4. `WithRealmImport` copie le JSON dans `/opt/keycloak/data/import/` et ajoute `--import-realm` au demarrage
5. `WithDataVolume` persiste le H2 entre les restarts (pas de perte de config)
6. `WithReference(keycloak)` cote API injecte :
   - `ConnectionStrings__keycloak` = `http://localhost:{port}`
   - Et/ou les variables Keycloak-specifiques selon le package client

**Variables d'environnement injectees dans Vetolib.Api** :

| Variable | Valeur (dev) | Usage |
|---|---|---|
| `ConnectionStrings__keycloak` | `http://localhost:18080` | Base URL Keycloak |
| `Keycloak__Realm` | `vetolib` | Configurable dans le realm import |
| `Keycloak__ClientId` | `vetolib-api` | Audience pour la validation JWT |
| `Keycloak__ClientSecret` | `(genere)` | Pour le client confidential (Admin API) |

### 1.3. Configuration des Organizations programmatiquement

Les Organizations ne se configurent pas via le realm import JSON de maniere complete (limitation Keycloak 26). La creation d'Organizations se fait via l'Admin REST API au runtime.

**Pattern recommande pour le dev** : un `IHostedService` de seeding qui s'execute au demarrage en dev uniquement.

```csharp
// Infrastructure/KeycloakSeedService.cs (dev only)
internal class KeycloakSeedService : BackgroundService
{
    private readonly HttpClient _adminClient;  // Authenticated via service account

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Wait for Keycloak to be ready
        await WaitForKeycloakReadyAsync(ct);

        // Seed organizations (clinics) for dev
        var clinics = new[]
        {
            ("org-demo-clinic-1", "Desert Paws Veterinary Clinic"),
            ("org-demo-clinic-2", "Abu Dhabi Pet Hospital"),
        };

        foreach (var (id, name) in clinics)
        {
            await CreateOrganizationIfNotExistsAsync(id, name, ct);
        }

        // Seed demo users in organizations
        await SeedDemoUsersAsync(ct);
    }
}
```

**Pour la prod** : les Organizations sont creees par `RegisterClinicHandler` via l'Admin API Keycloak au moment de l'inscription.

### 1.4. Realm JSON minimal (infra/keycloak/vetolib-realm.json)

```json
{
  "realm": "vetolib",
  "enabled": true,
  "organizationsEnabled": true,
  "roles": {
    "realm": [
      { "name": "Admin", "description": "Clinic administrator" },
      { "name": "Vet", "description": "Veterinarian" },
      { "name": "Receptionist", "description": "Front desk staff" },
      { "name": "Assistant", "description": "Veterinary assistant" }
    ]
  },
  "clients": [
    {
      "clientId": "vetolib-api",
      "name": "Vetolib API",
      "enabled": true,
      "clientAuthenticatorType": "client-secret",
      "secret": "dev-secret-change-in-prod",
      "serviceAccountsEnabled": true,
      "directAccessGrantsEnabled": false,
      "standardFlowEnabled": false,
      "protocolMappers": [
        {
          "name": "clinic_id_mapper",
          "protocol": "openid-connect",
          "protocolMapper": "oidc-organization-membership-mapper",
          "config": {
            "id.token.claim": "true",
            "access.token.claim": "true",
            "claim.name": "clinic_id",
            "jsonType.label": "String"
          }
        },
        {
          "name": "realm_role_mapper",
          "protocol": "openid-connect",
          "protocolMapper": "oidc-usermodel-realm-role-mapper",
          "config": {
            "claim.name": "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            "access.token.claim": "true",
            "id.token.claim": "true",
            "multivalued": "true"
          }
        }
      ]
    },
    {
      "clientId": "vetolib-web",
      "name": "Vetolib Web Frontend",
      "enabled": true,
      "publicClient": true,
      "directAccessGrantsEnabled": false,
      "standardFlowEnabled": true,
      "redirectUris": ["http://localhost:3000/*"],
      "webOrigins": ["http://localhost:3000"],
      "attributes": {
        "pkce.code.challenge.method": "S256"
      }
    },
    {
      "clientId": "vetolib-owner-portal",
      "name": "Vetolib Owner Portal",
      "enabled": true,
      "publicClient": true,
      "directAccessGrantsEnabled": false,
      "standardFlowEnabled": true,
      "redirectUris": ["http://localhost:3000/portal/*"],
      "webOrigins": ["http://localhost:3000"]
    }
  ],
  "browserFlow": "browser",
  "passwordPolicy": "length(8) and upperCase(1) and lowerCase(1) and digits(1) and specialChars(1)",
  "bruteForceProtected": true,
  "maxFailureWaitSeconds": 900,
  "failureFactor": 5,
  "verifyEmail": true,
  "smtpServer": {
    "host": "mailhog",
    "port": "1025",
    "from": "noreply@vetolib.com",
    "fromDisplayName": "Vetolib",
    "ssl": "false",
    "starttls": "false"
  }
}
```

**Points cles** :
- `organizationsEnabled: true` active la feature Organizations
- Le Protocol Mapper `oidc-organization-membership-mapper` emet le claim `clinic_id` a partir de l'Organization active de l'utilisateur
- Le Role Mapper est configure pour emettre dans `ClaimTypes.Role` (.NET standard) ce qui preserve la compatibilite avec `RequireRole()`
- La password policy replique exactement les regles actuelles de `User.ValidatePassword()`
- Le brute force protection replique `AuthSecurityOptions` (5 attempts, 15 min lockout)

---

## 2. OpenTelemetry pour Keycloak

### 2.1. Activation des traces Keycloak

Keycloak 25+ integre le SDK OpenTelemetry Java. Configuration via variables d'environnement dans Aspire :

```csharp
// AppHost/Program.cs — ajout OTel sur le container Keycloak
var keycloak = builder.AddKeycloak("keycloak")
    .WithRealmImport("../infra/keycloak/vetolib-realm.json")
    .WithEnvironment("KC_FEATURES", "organization")
    .WithEnvironment("KC_TRACING_ENABLED", "true")
    .WithEnvironment("KC_TRACING_ENDPOINT", "http://host.docker.internal:4317")
    .WithEnvironment("KC_TRACING_PROTOCOL", "grpc")
    .WithEnvironment("KC_TRACING_RESOURCE_ATTRIBUTES",
        "service.name=keycloak,service.namespace=vetolib")
    .WithEnvironment("KC_TRACING_JDBC_ENABLED", "false")  // Trop verbeux en dev
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_METRICS_ENABLED", "true");
```

**Ce qui apparait dans l'Aspire Dashboard** :

| Span | Description | Attributs |
|---|---|---|
| `POST /realms/vetolib/protocol/openid-connect/token` | Token issuance | `user.id`, `client_id`, `grant_type` |
| `POST /realms/vetolib/protocol/openid-connect/token` (refresh) | Token refresh | `user.id`, `grant_type=refresh_token` |
| `GET /realms/vetolib/protocol/openid-connect/certs` | JWKS fetch (par l'API .NET) | `realm` |
| `POST /admin/realms/vetolib/users` | User creation via Admin API | `user.email` |
| `POST /admin/realms/vetolib/organizations` | Organization creation | `org.name` |

**Trace distribuee typique (login flow)** :
```
[Frontend] POST /api/v1/auth/login-redirect
  -> [Vetolib.Api] redirect to Keycloak
    -> [Keycloak] POST /realms/vetolib/protocol/openid-connect/auth
    -> [Keycloak] POST /realms/vetolib/protocol/openid-connect/token
  -> [Vetolib.Api] validate JWT (JWKS cached)
  -> [Vetolib.Api] query user data from DB
```

### 2.2. Metriques Keycloak

Keycloak expose des metriques Prometheus-compatibles sur `/metrics` (quand `KC_METRICS_ENABLED=true`).

Metriques cles pour le monitoring Vetolib :

| Metrique | Description | Alerte suggeree |
|---|---|---|
| `keycloak_login_total{result="success"}` | Logins reussis | - |
| `keycloak_login_total{result="error"}` | Logins echoues | > 50/min = brute force potentiel |
| `keycloak_token_refresh_total` | Token refreshes | - |
| `keycloak_active_sessions` | Sessions actives | > 10000 = verifier la memoire |
| `keycloak_user_registrations_total` | Nouveaux comptes | - |
| `keycloak_request_duration_seconds` | Latence des requetes | p99 > 500ms = probleme |

Pour les remonter dans Aspire Dashboard, ajouter un scraper Prometheus -> OTLP dans le pipeline OTel (ou utiliser le Prometheus exporter d'Aspire).

### 2.3. Health checks Keycloak dans Aspire

Le composant Aspire `AddKeycloak()` ajoute automatiquement un health check sur `GET /health/ready`. Il apparait dans l'Aspire Dashboard comme un resource avec son statut.

Pour un health check plus fin cote API (verifier que le JWKS est accessible) :

```csharp
// Dans AuthModuleServiceRegistrar ou ServiceDefaults
services.AddHealthChecks()
    .AddUrlGroup(
        new Uri($"{keycloakAuthority}/.well-known/openid-configuration"),
        name: "keycloak-oidc-discovery",
        tags: ["ready"]);
```

---

## 3. Inventaire complet des handlers Auth — impact Keycloak

### 3.1. Handlers SUPPRIMES (auth flow delegue a Keycloak)

| Handler | Raison de suppression | Remplace par |
|---|---|---|
| `LoginHandler` | Login = OIDC redirect vers Keycloak | Frontend OIDC flow (Authorization Code + PKCE) |
| `RefreshTokenHandler` | Refresh = grant_type=refresh_token vers Keycloak | Lib OIDC frontend (`oidc-client-ts` ou `next-auth`) |
| `LogoutHandler` | Logout = end_session_endpoint Keycloak + revocation | Frontend OIDC logout + Keycloak back-channel logout |
| `ChangePasswordHandler` | Password change = Keycloak Account Management API | Frontend redirect vers Keycloak Account page ou Admin API call |
| `VerifyEmailHandler` | Email verification = geree nativement par Keycloak | Config realm `verifyEmail: true` |
| `OwnerPortalLoginHandler` | Login owner = OIDC flow avec client `vetolib-owner-portal` | Frontend OIDC |
| `RegisterOwnerAccountHandler` | Registration owner = Keycloak self-registration flow | Config realm + custom theme |

**Total : 7 handlers supprimes**

### 3.2. Handlers MODIFIES (logique business preservee, credentials deleguees)

| Handler | Modification | Detail |
|---|---|---|
| `RegisterClinicHandler` | Password et User.Create() → Admin API Keycloak | Creer l'Organization dans Keycloak, creer le user via Admin API, lier le user a l'org. Conserver la creation de `Clinic` entity en DB locale. Le JWT n'est plus genere ici — le frontend fera un login OIDC apres l'inscription. |
| `CreateUserHandler` | User.Create() → Admin API Keycloak | Creer le user dans Keycloak, l'ajouter a l'Organization courante. Conserver la creation en DB locale pour les champs metier (VetLicenseNumber, etc.). |
| `InviteUserHandler` | User.Invite() → Admin API Keycloak | Creer le user dans Keycloak avec `requiredActions: ["UPDATE_PASSWORD"]`, envoyer l'invitation via Keycloak (ou conserver le domain event). |
| `SwitchClinicHandler` | JWT custom → delegation au frontend | Le handler peut etre supprime ou transforme en simple validation d'autorisation. Le switch reel se fait cote frontend en demandant un nouveau token avec `organization={target_clinic_id}` au token endpoint Keycloak. |
| `DeactivateUserHandler` | Ajouter : desactiver le user dans Keycloak | Apres `user.Deactivate()` en DB locale, appeler `PUT /admin/realms/vetolib/users/{id}` avec `{ "enabled": false }`. |
| `ChangeUserRoleHandler` | Ajouter : mettre a jour les roles dans Keycloak | Apres le changement en DB locale, synchroniser les realm roles dans Keycloak via l'Admin API. |

**Total : 6 handlers modifies**

### 3.3. Handlers CONSERVES TEL QUEL

| Handler | Raison |
|---|---|
| `GetCurrentUserHandler` | Lit les infos user depuis la DB locale — pas d'impact |
| `ListUsersHandler` | Query sur la DB locale |
| `CompleteOnboardingStepHandler` | Logique onboarding, pas liee a l'auth |
| `DismissChecklistHandler` | Idem |
| `DismissWelcomeBannerHandler` | Idem |
| `GetOnboardingStateHandler` | Idem |
| `CreateClinicGroupHandler` | Logique clinic groups, pas liee a l'auth |
| `AddClinicToGroupHandler` | Idem |
| `RemoveClinicFromGroupHandler` | Idem |
| `ListGroupClinicsHandler` | Query |
| `GetGroupDashboardStatsHandler` | Query |
| `GetGroupClinicStatsHandler` | Query |
| `GetGroupRevenueComparisonHandler` | Query |
| `GetOrCreateReferralCodeHandler` | Logique referral, pas liee a l'auth |
| `SearchClinicsHandler` | Query publique |
| `RegisterWebhookHandler` | Logique webhook, pas liee a l'auth |
| `ListWebhooksHandler` | Query |
| `DeactivateWebhookHandler` | Logique webhook |
| `ReceiveWebhookHandler` | Logique webhook |
| `LinkOwnerByMicrochipHandler` | Logique owner portal, pas liee aux credentials |
| `InviteVetHandler` | Logique virale, pas liee aux credentials |

**Total : 21 handlers conserves**

### 3.4. Entites Domain — impact

| Entite | Impact | Detail |
|---|---|---|
| `User` | **MODIFIE** | Supprimer : `PasswordHash`, `VerifyPassword()`, `ChangePassword()`, `FailedLoginAttempts`, `IsLocked`, `LockedUntil`, `RecordFailedLogin()`, `Unlock()`, `IsCurrentlyLocked()`, `EmailVerified`, `EmailVerificationToken`, `EmailVerificationExpiry`, `VerifyEmail()`, `RegenerateVerificationToken()`, `ValidatePassword()`. Ajouter : `KeycloakUserId` (string). Conserver : `ClinicId`, `Email`, `FullName`, `Role`, `VetLicenseNumber`, `IsActive`, `MustChangePassword` (flag local), `ReferredByUserId`. |
| `RefreshToken` | **SUPPRIME** | Table et entite supprimees. Keycloak gere les refresh tokens. |
| `OwnerAccount` | **SUPPRIME** | Les owner accounts vivent dans Keycloak. Si des champs metier sont necessaires (Phone, IsVerified local), creer une entite legere `OwnerProfile` qui reference le `KeycloakUserId`. |
| `Clinic` | **MODIFIE** | Ajouter : `KeycloakOrganizationId` (string). Reste inchange sinon. |
| `ClinicGroup` | Conserve | Pas d'impact. |
| `ClinicGroupMember` | Conserve | Pas d'impact. |
| `OnboardingState` | Conserve | Pas d'impact. |
| `ReferralCode` | Conserve | Pas d'impact. |
| `WebhookRegistration` | Conserve | Pas d'impact. |
| `WebhookLog` | Conserve | Pas d'impact. |
| `VetInvitationLog` | Conserve | Pas d'impact. |

### 3.5. Services — impact

| Service | Impact | Detail |
|---|---|---|
| `JwtTokenService` | **SUPPRIME** | Keycloak genere les JWT. |
| `OwnerPortalJwtService` | **SUPPRIME** | Idem. |
| `IJwtTokenService` | **SUPPRIME** | Plus de consommateur. |
| `IOwnerPortalJwtService` | **SUPPRIME** | Idem. |
| `ClinicVetReader` | Conserve | Cross-module reader, pas lie a l'auth. |
| `SubscriptionChecker` | Conserve | Logique metier. |
| `AuthSecurityOptions` | **MODIFIE** | Supprimer : `TokenExpirationMinutes`, `MaxFailedLoginAttempts`, `LockoutMinutes` (configures dans Keycloak). Conserver : `TrialDays`. |

### 3.6. Endpoints — impact

| Endpoint | Impact | Detail |
|---|---|---|
| `POST /api/v1/auth/login` | **SUPPRIME** | Login = OIDC flow. |
| `POST /api/v1/auth/refresh` | **SUPPRIME** | Refresh = OIDC. |
| `POST /api/v1/auth/verify-email` | **SUPPRIME** | Keycloak natif. |
| `POST /api/v1/auth/logout` | **SUPPRIME** | OIDC end_session. |
| `POST /api/v1/auth/change-password` | **SUPPRIME ou redirect** | Keycloak Account Management. |
| `GET /api/v1/auth/me` | Conserve | |
| `POST /api/v1/auth/switch-clinic` | **MODIFIE** | Simplifie ou supprime (frontend-side). |
| `POST /api/v1/clinics/register` | **MODIFIE** | Appel Admin API Keycloak. |
| `GET /api/v1/clinics/search` | Conserve | |
| `POST /api/v1/users/*` | **MODIFIE** | Sync Keycloak. |
| `GET /api/v1/users` | Conserve | |
| `POST /api/v1/portal/register` | **SUPPRIME** | Keycloak self-registration. |
| `POST /api/v1/portal/login` | **SUPPRIME** | OIDC flow. |
| `POST /api/v1/portal/link-microchip` | Conserve | |
| `POST /api/v1/portal/invite-vet` | Conserve | |
| Onboarding endpoints (4) | Conserve | |
| Clinic Group endpoints (7) | Conserve | |
| Referral endpoint (1) | Conserve | |
| Webhook endpoints (4) | Conserve | |

---

## 4. Migration des passwords BCrypt

### 4.1. Le probleme

Les passwords sont stockes comme BCrypt hashes dans la colonne `User.PasswordHash`. Keycloak utilise ses propres algorithmes (pbkdf2-sha256 par defaut). Il n'est pas possible d'importer directement des BCrypt hashes via l'Admin API standard.

### 4.2. Option A — Force password reset (simple)

1. Creer les users dans Keycloak via Admin API avec `"requiredActions": ["UPDATE_PASSWORD"]`
2. Envoyer un email a tous les users : "Nous avons ameliore notre systeme de connexion. Merci de definir un nouveau mot de passe."
3. L'utilisateur est redirige vers le formulaire Keycloak au premier login

**Avantage** : Zero code custom dans Keycloak.
**Inconvenient** : Friction pour tous les users existants. Risque de perte d'engagement.

### 4.3. Option B — Lazy migration via User Storage SPI (recommande)

Keycloak supporte les **User Storage SPIs** (anciennement User Federation). Un SPI custom peut :

1. Au login, verifier le password contre le BCrypt hash en DB Vetolib
2. Si OK, laisser Keycloak re-hasher le password avec son algorithme
3. Marquer le user comme "migre" et desactiver le SPI pour cet user

```java
// keycloak-bcrypt-migration-spi/src/main/java/...
public class BcryptMigrationProvider implements CredentialInputValidator {

    @Override
    public boolean isValid(RealmModel realm, UserModel user, CredentialInput input) {
        // 1. Check if user has a "legacy_bcrypt_hash" attribute
        String bcryptHash = user.getFirstAttribute("legacy_bcrypt_hash");
        if (bcryptHash == null) return false;  // Already migrated

        // 2. Verify against BCrypt
        if (BCrypt.checkpw(input.getChallengeResponse(), bcryptHash)) {
            // 3. Remove the legacy attribute (Keycloak will store new hash)
            user.removeAttribute("legacy_bcrypt_hash");
            return true;
        }
        return false;
    }
}
```

**Script de migration batch** :
```
Pour chaque User en DB Vetolib :
  1. POST /admin/realms/vetolib/users  →  creer le user dans Keycloak
  2. Stocker le BCrypt hash comme user attribute "legacy_bcrypt_hash"
  3. POST /admin/realms/vetolib/organizations/{org-id}/members  →  lier a la clinic
  4. Assigner les realm roles correspondants
```

**Avantage** : Migration transparente, zero friction.
**Inconvenient** : Necessite un SPI Java custom a maintenir jusqu'a ce que tous les users se soient connectes au moins une fois.

### 4.4. Option C — Dual-stack avec migration progressive

Pendant la phase dual-stack (Phase 1), quand un user se connecte via l'ancien endpoint Legacy :
1. Verifier le BCrypt hash (ancien flow)
2. Si OK, creer/mettre a jour le user dans Keycloak via Admin API avec le password en clair (on l'a a ce moment)
3. Retourner le token Legacy pour cette session
4. A la prochaine connexion, le user peut utiliser le flow OIDC Keycloak

**Avantage** : Pas de SPI Java, migration dans le code .NET existant.
**Inconvenient** : Le password transite temporairement en memoire dans l'API pour le transfert vers Keycloak (acceptable car c'est deja le cas au login).

**Recommandation : Option C** — elle est la plus simple a implementer dans notre stack .NET et ne necessite pas de developper/deployer un SPI Java custom.

### 4.5. Dual-stack JWT (pendant la migration)

```csharp
// AuthModuleServiceRegistrar.cs — Phase 1 (dual-stack)
services.AddAuthentication("MultiScheme")
    .AddJwtBearer("Legacy", options =>
    {
        // Ancien systeme HMAC-SHA256
        var jwtKey = config["Jwt:Key"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"] ?? "Vetolib",
            ValidAudience = config["Jwt:Audience"] ?? "Vetolib",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "sub",
            RoleClaimType = ClaimTypes.Role
        };
    })
    .AddJwtBearer("Keycloak", options =>
    {
        // Nouveau systeme RSA via JWKS
        options.Authority = config["Keycloak:Authority"];
        options.Audience = "vetolib-api";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            NameClaimType = "sub",
            RoleClaimType = ClaimTypes.Role
        };
    })
    .AddPolicyScheme("MultiScheme", "Route to Legacy or Keycloak", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return "Keycloak";  // Default to new system

            var token = authHeader["Bearer ".Length..];

            // Keycloak JWT has 3 parts and header contains "typ":"JWT" with RSA alg
            // Legacy JWT uses HMAC-SHA256 (HS256), Keycloak uses RS256
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return jwt.Header.Alg == "HS256" ? "Legacy" : "Keycloak";
            }
            catch
            {
                return "Keycloak";
            }
        };
    });
```

---

## 5. Plan de tasks detaille

### Phase 0 — PoC (3 jours)

| # | Task | Description | Estimation |
|---|---|---|---|
| 0.1 | `todo-poc-keycloak-aspire-setup` | Ajouter `Aspire.Hosting.Keycloak` dans AppHost, creer le realm JSON minimal, verifier que Keycloak demarre avec `dotnet run` dans AppHost | 0.5j |
| 0.2 | `todo-poc-keycloak-organizations` | Configurer Organizations dans le realm, creer 2 orgs de test via Admin API, verifier le claim `clinic_id` dans le JWT emis | 1j |
| 0.3 | `todo-poc-keycloak-jwt-validation` | Configurer `AddJwtBearer` avec le JWKS endpoint Keycloak, valider que les TI Auth existants passent avec un JWT Keycloak (ou identifier les delta) | 1j |
| 0.4 | `todo-poc-keycloak-go-nogo` | Documenter les resultats, decision Go/No-Go | 0.5j |

**Critere Go/No-Go** : Les claims `sub`, `clinic_id`, `role` dans le JWT Keycloak sont lisibles par `ClinicContext` et les policies `ClinicStaff`/`VetOrAdmin` sans modifier le code GELE.

### Phase 1 — Infrastructure Keycloak + Aspire (1 semaine)

| # | Task | Description | Estimation |
|---|---|---|---|
| 1.1 | `todo-infra-keycloak-realm-config` | Realm JSON complet : clients, roles, Protocol Mappers, password policy, brute force, SMTP (Mailhog) | 1j |
| 1.2 | `todo-infra-keycloak-otel` | Activer OTel tracing + metriques sur Keycloak, verifier dans Aspire Dashboard | 0.5j |
| 1.3 | `todo-infra-keycloak-admin-api-client` | Creer `IKeycloakAdminService` : wrapper HttpClient pour les operations Admin API (CRUD users, orgs, roles) | 1.5j |
| 1.4 | `todo-infra-keycloak-health-checks` | Health check OIDC discovery + readiness dans ServiceDefaults | 0.5j |
| 1.5 | `todo-infra-keycloak-dev-seed` | `KeycloakSeedService` pour creer les orgs et users de demo au demarrage | 0.5j |

### Phase 2 — Migration handlers Auth (2 semaines)

| # | Task | Description | Estimation |
|---|---|---|---|
| 2.1 | `todo-back-auth-dual-stack` | Multi-scheme JWT (Legacy + Keycloak) dans `AuthModuleServiceRegistrar` | 1j |
| 2.2 | `todo-back-auth-register-keycloak` | Modifier `RegisterClinicHandler` : creer Organization + user dans Keycloak | 1.5j |
| 2.3 | `todo-back-auth-create-user-keycloak` | Modifier `CreateUserHandler` : creer user dans Keycloak + ajouter a l'org | 1j |
| 2.4 | `todo-back-auth-invite-keycloak` | Modifier `InviteUserHandler` : creer user dans Keycloak avec `UPDATE_PASSWORD` required action | 1j |
| 2.5 | `todo-back-auth-deactivate-sync` | Modifier `DeactivateUserHandler` : desactiver dans Keycloak | 0.5j |
| 2.6 | `todo-back-auth-role-sync` | Modifier `ChangeUserRoleHandler` : sync roles dans Keycloak | 0.5j |
| 2.7 | `todo-back-auth-switch-clinic-oidc` | Simplifier `SwitchClinicHandler` ou le supprimer (frontend-driven) | 0.5j |
| 2.8 | `todo-back-auth-user-entity-cleanup` | Retirer les champs credentials de `User`, ajouter `KeycloakUserId` | 1j |
| 2.9 | `todo-back-auth-owner-migration` | Migrer `OwnerAccount` vers Keycloak ou creer `OwnerProfile` | 1j |
| 2.10 | `todo-test-auth-keycloak-ti` | Adapter les TI Auth pour valider les JWT Keycloak (Testcontainers Keycloak) | 1.5j |

### Phase 3 — Dual-stack + migration users (1 semaine)

| # | Task | Description | Estimation |
|---|---|---|---|
| 3.1 | `todo-back-auth-lazy-password-migration` | Dans `LoginHandler` (temporaire) : si login Legacy OK, creer/update le user dans Keycloak avec le password courant | 1.5j |
| 3.2 | `todo-back-auth-migration-script` | Script batch pour creer les users existants dans Keycloak (sans password) avec `requiredActions: ["UPDATE_PASSWORD"]` comme fallback | 1j |
| 3.3 | `todo-front-auth-oidc` | Integrer `next-auth` v5 avec Keycloak provider, PKCE, refresh token rotation | 2j |
| 3.4 | `todo-front-owner-portal-oidc` | Client OIDC dedie pour le owner portal | 1j |

### Phase 4 — Cutover + cleanup (3 jours)

| # | Task | Description | Estimation |
|---|---|---|---|
| 4.1 | `todo-back-auth-cutover` | Retirer le scheme Legacy, supprimer les endpoints login/refresh/verify-email/logout | 0.5j |
| 4.2 | `todo-back-auth-cleanup-code` | Supprimer : `JwtTokenService`, `OwnerPortalJwtService`, `RefreshToken` entity, `OwnerAccount` entity (si OwnerProfile cree), `LoginHandler`, `RefreshTokenHandler`, `LogoutHandler`, `ChangePasswordHandler`, `VerifyEmailHandler`, `OwnerPortalLoginHandler`, `RegisterOwnerAccountHandler` | 1j |
| 4.3 | `todo-back-auth-cleanup-migration` | Migration EF : supprimer tables `refresh_tokens`, colonnes `password_hash`/`failed_login_attempts`/`is_locked`/`locked_until`/`email_verification_*` de `users` | 0.5j |
| 4.4 | `todo-test-auth-cleanup` | Mettre a jour TU/TF, supprimer les tests des handlers supprimes, ajouter TU pour `IKeycloakAdminService` | 1j |
| 4.5 | `todo-infra-keycloak-prod` | Docker compose prod, backup strategy, TLS config | (hors scope, post-migration) |

**Total : ~25 taches, estim 5-6 semaines agent temps plein.**

---

## 6. Risques techniques

| # | Risque | Probabilite | Impact | Mitigation |
|---|---|---|---|---|
| R1 | Le Protocol Mapper `oidc-organization-membership-mapper` n'emet pas le claim `clinic_id` au format attendu | Moyenne | Critique | Valide en Phase 0. Fallback : custom Protocol Mapper ou renommage dans un ClaimsTransformation middleware .NET. |
| R2 | `ClinicContext` (GELE) ne peut pas lire le claim Keycloak | Faible | Critique | Le mapper Keycloak DOIT emettre exactement `clinic_id`. Si impossible, demander un arbitrage humain pour modifier `ClinicContext`. |
| R3 | Latence Keycloak pour la validation JWT | Faible | Moyenne | .NET cache le JWKS automatiquement (5 min par defaut). Premiere requete lente (~100ms), suivantes < 1ms. |
| R4 | Keycloak Organizations instable en production | Moyenne | Elevee | PoC de 3 jours. Si instable, fallback Pattern B (groupes Keycloak). |
| R5 | Migration BCrypt echoue pour certains users | Faible | Moyenne | Fallback : force reset pour les users non migres apres 30 jours. |
| R6 | `next-auth` v5 instable avec Keycloak Organizations | Moyenne | Moyenne | Alternative : `oidc-client-ts` + custom React hooks. |
| R7 | Testcontainers Keycloak lent en CI | Certaine | Faible | Keycloak container = ~15-20s de startup. Acceptable pour TI, pas pour TU. Les TU restent mockes. |
| R8 | Double-write (DB locale + Keycloak Admin API) = inconsistance | Moyenne | Elevee | Toujours ecrire dans Keycloak en premier. Si Keycloak echoue, ne pas persister en DB locale. Compenser les divergences avec un reconciliation job periodique. |
| R9 | Taille du realm import JSON depasse les limites Aspire | Faible | Faible | Splitter le realm config : base dans le JSON, Organizations/users dans le seed service. |
| R10 | CORS issues entre le frontend et Keycloak en dev | Moyenne | Faible | Configurer `webOrigins` dans le client Keycloak. Aspire reverse proxy peut aussi aider. |

---

## 7. Architecture cible (post-migration)

```
Frontend (Next.js)
  |
  |-- OIDC Authorization Code + PKCE --> Keycloak (vetolib-web client)
  |-- API calls with Bearer JWT -------> Vetolib.Api

Vetolib.Api
  |
  |-- Validate JWT (JWKS from Keycloak, cached)
  |-- Extract clinic_id from "organization" claim (via Protocol Mapper → "clinic_id")
  |-- ClinicContext (FROZEN) reads "clinic_id" → EF Core global query filter
  |
  |-- Auth Module (reduced):
  |     |-- User entity (no credentials, has KeycloakUserId)
  |     |-- Clinic entity (has KeycloakOrganizationId)
  |     |-- IKeycloakAdminService for CRUD sync
  |     |-- ClinicGroup, Onboarding, Referral, Webhook (unchanged)
  |
  |-- Other Modules (unchanged): Agenda, MedicalRecords, Billing, etc.

Keycloak 26+
  |
  |-- Realm: vetolib
  |-- Organizations = Clinics (multi-tenant)
  |-- Users with org membership + realm roles
  |-- Password management, MFA, email verification
  |-- OpenTelemetry traces → Aspire Dashboard
  |-- Prometheus metrics → monitoring
```

---

## 8. Prochaine etape

Le fondateur valide les questions ouvertes listees dans `keycloak-migration-20260401.md` section 9, puis :

1. Creer les tasks Phase 0 (PoC) dans `tasks/todo/`
2. Dispatcher un agent sur le PoC (3 jours max)
3. Go/No-Go basee sur les resultats du PoC
4. Si Go : creer les tasks Phase 1-4 en mode pipeline
