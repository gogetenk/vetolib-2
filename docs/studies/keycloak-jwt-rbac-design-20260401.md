# Keycloak JWT & RBAC Multi-Cabinet Design — 2026-04-01

> **Statut** : ETUDE TECHNIQUE — Detaille le design JWT/RBAC pour le Pattern C (Organizations)
> **Prerequis** : `keycloak-migration-20260401.md` (Pattern C valide par le fondateur)
> **Auteur** : Agent Architect
> **Modules impactes** : Auth (majeur), Shared.Infrastructure/ClinicContext (lecture seule, FROZEN)

---

## 1. Structure JWT recommandee

### 1.1. Approche retenue : Token scope au cabinet courant (pas de mega-token)

**Decision** : Le JWT est scope a UN SEUL cabinet courant. Il ne contient PAS la liste exhaustive de toutes les organizations + roles de l'utilisateur.

**Pourquoi pas un mega-token avec toutes les orgs ?**

La structure proposee dans la demande :
```json
{
  "organizations": {
    "clinic-uuid-1": { "roles": ["Admin", "Vet"], "name": "Dubai Pet Clinic" },
    "clinic-uuid-2": { "roles": ["Vet"], "name": "Abu Dhabi Animal Hospital" }
  }
}
```

...pose plusieurs problemes :
1. **Taille du token** : un vet travaillant dans 10 clinics aurait un JWT de plusieurs KB. Les headers HTTP ont une limite pratique (~8KB). Chaque requete transporte ce poids.
2. **Securite** : le token contient les permissions de TOUTES les clinics. Un token vole donne acces a tout.
3. **Invalidation** : si on revoque un role dans une clinic, il faut invalider le token entier (pas juste un scope).
4. **ClinicContext** : le code FROZEN (`ClinicContext.cs`) lit UN SEUL `clinic_id`. Il faudrait le modifier pour choisir parmi N orgs — interdit.
5. **Global Query Filter** : le `MultiTenantDbContext` filtre par UN clinicId. Pas de support multi-tenant simultane.

**Le pattern Keycloak Organizations confirme cette approche** : quand un user s'authentifie dans le contexte d'une organization, Keycloak emet un token scope a CETTE organization. Le switch se fait via un re-token (token exchange ou grant avec parametre `organization`).

### 1.2. Structure JWT cible

```json
{
  "iss": "https://keycloak.vetolib.com/realms/vetolib",
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "aud": "vetolib-api",
  "exp": 1711929600,
  "iat": 1711926000,
  "email": "dr.ahmed@example.com",
  "name": "Dr. Ahmed Al-Rashid",

  "clinic_id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",

  "organization": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "name": "Dubai Pet Clinic",
    "roles": ["Admin", "Vet"]
  },

  "realm_access": {
    "roles": ["default-roles-vetolib"]
  },

  "resource_access": {
    "vetolib-api": {
      "roles": ["Admin", "Vet"]
    }
  },

  "vetLicense": "UAE-VET-2024-1234",
  "clinic_group_id": "group-uuid-if-applicable",

  "jti": "unique-token-id"
}
```

### 1.3. Claims essentiels — contrat avec le code existant

| Claim | Source | Consommateur | Obligatoire |
|---|---|---|---|
| `sub` | Keycloak natif (user ID) | Endpoints qui lisent `ClaimTypes.NameIdentifier` | Oui |
| `clinic_id` | Protocol Mapper custom OU alias de `organization.id` | `ClinicContext.cs` (FROZEN) — `FindFirst("clinic_id")` | **CRITIQUE** |
| `role` (ClaimTypes.Role) | Organization Role Mapper | Policies `ClinicStaff`, `VetOrAdmin`, `RequireRole("Admin")` | Oui |
| `email` | Keycloak natif | Divers endpoints, `User.ToDto()` | Oui |
| `vetLicense` | User Attribute Mapper | Endpoints medical records | Si Vet |
| `jti` | Keycloak natif | Token revocation | Oui |

**Point critique** : Le claim `clinic_id` DOIT etre un claim de premier niveau (pas nested). `ClinicContext.cs` fait `FindFirst("clinic_id")` — il ne sait pas naviguer dans un objet JSON nested. Keycloak peut emettre un claim `clinic_id` flat via un Protocol Mapper meme si l'organization est un objet structure.

### 1.4. Protocol Mappers Keycloak necessaires

```
Mapper 1 — clinic_id (flat claim)
  Type: Organization Membership Mapper (ou Script Mapper)
  Token Claim Name: clinic_id
  Source: organization.id du contexte d'authentification courant
  Add to ID token: yes
  Add to access token: yes

Mapper 2 — clinic_name (informatif)
  Type: Organization Attribute Mapper
  Token Claim Name: clinicName
  Source: organization.name
  Add to access token: yes

Mapper 3 — vetLicense (user attribute)
  Type: User Attribute Mapper
  User Attribute: vetLicenseNumber
  Token Claim Name: vetLicense
  Add to access token: yes

Mapper 4 — clinic_group_id (optionnel)
  Type: User Attribute Mapper
  User Attribute: clinicGroupId
  Token Claim Name: clinicGroupId
  Add to access token: yes
  (Note: necessaire pour le ClinicSwitcher frontend qui lit parseJwtClaim("clinicGroupId"))
```

---

## 2. Analyse du systeme actuel

### 2.1. Chemin complet : Login -> Token -> API Call -> ClinicContext

```
1. Frontend POST /api/v1/auth/login { email, password }
2. LoginHandler verifie BCrypt, genere JWT via JwtTokenService
3. JwtTokenService.GenerateAccessToken(user) :
   - claim "sub" = user.Id
   - claim "email" = user.Email
   - claim "clinic_id" = user.ClinicId     <-- LE PIVOT
   - claim ClaimTypes.Role = user.Role
   - claim "vetLicense" = user.VetLicenseNumber (optionnel)
   - signing: HMAC-SHA256 avec cle symetrique
   - expiration: configurable via AuthSecurityOptions.TokenExpirationMinutes
4. Frontend stocke access_token + refresh_token dans localStorage
5. Chaque requete API : header Authorization: Bearer {access_token}
6. ASP.NET JWT middleware valide le token, popule HttpContext.User (ClaimsPrincipal)
7. ClinicContext (scoped) : lit FindFirst("clinic_id") depuis HttpContext.User
8. MultiTenantDbContext : injecte ClinicContext, applique WHERE ClinicId = @clinicId
```

### 2.2. Chemin Switch Clinic

```
1. Frontend POST /api/v1/auth/switch-clinic { clinicId: "target-uuid" }
2. SwitchClinicHandler :
   a. Lookup user par ID (IgnoreQueryFilters — cross-tenant)
   b. Verifie autorisation :
      - isOwnClinic (user.ClinicId == targetClinicId)
      - ownsGroupWithClinic (user est owner d'un ClinicGroup contenant la target)
      - isStaffAtClinic (un User record avec le meme email existe dans la target clinic)
   c. GenerateAccessTokenForClinic(user, targetClinicId) — meme user, clinic_id different
   d. Nouveau refresh token
3. Frontend recoit nouveau access_token, le stocke, reload la page
4. Toutes les requetes suivantes portent le nouveau clinic_id
```

### 2.3. Points d'attention pour la migration

**`ClinicContext.cs` (FROZEN)** :
```csharp
var claim = _accessor.HttpContext?.User.FindFirst("clinic_id");
```
- Lit le claim `clinic_id` (string exacte)
- Retourne `Guid.Empty` si absent ou invalide
- Aucun fallback, aucun header, aucun cookie

**Consequences** :
- Le JWT Keycloak DOIT emettre un claim flat `clinic_id` (pas `organization.id`, pas nested)
- Si le claim est absent, le global query filter utilise `Guid.Empty` — AUCUNE donnee retournee (safe by default)
- Pas besoin de modifier `ClinicContext` si le Protocol Mapper est correct

**Frontend `ClinicSwitcher.tsx`** :
- Lit `parseJwtClaim("clinicId")` — attention: camelCase `clinicId`, PAS `clinic_id`
- Lit aussi `parseJwtClaim("clinicName")` et `parseJwtClaim("clinicGroupId")`
- Le backend utilise `clinic_id` (snake_case) dans le JWT

**Discrepance detectee** : Le backend JWT emet `clinic_id` (snake_case). Le frontend lit `clinicId` (camelCase). Soit le frontend parse les deux, soit il y a un mapping quelque part. A verifier — c'est peut-etre un bug latent ou le MSW mock utilise camelCase.

---

## 3. Header x-current-clinic : analyse

### 3.1. Etat actuel

**Aucun header `x-current-clinic` ou similaire n'existe dans le codebase.** La recherche `x-current-clinic`, `x-clinic`, `X-Clinic`, `x-current-cabinet` ne retourne aucun resultat.

Le `clinic_id` est extrait **exclusivement du JWT claim** par `ClinicContext.cs`.

### 3.2. Comparaison des 3 approaches

| Approche | Mecanisme | Avantages | Inconvenients |
|---|---|---|---|
| **A. Claim JWT** (actuel) | `clinic_id` dans le token | Token self-contained, pas de requete supplementaire pour valider. Le middleware JWT standard suffit. | Le switch de clinic necessite un nouveau token. |
| **B. Header HTTP** | `x-current-clinic: {uuid}` | Switch instantane sans re-token. Flexible. | Le backend doit valider que le user a acces a cette clinic (sinon spoofing). Requete DB a chaque appel OU cache. |
| **C. Token exchange** (Keycloak natif) | Parametre `organization` sur le token endpoint | Natif Keycloak, token scope automatiquement. Pas de validation custom. | Necessite un round-trip vers Keycloak pour chaque switch. |

### 3.3. Recommandation : Approche C (token exchange) avec l'approche B en complement

**Pour le switch de clinic** : Utiliser le mecanisme natif de Keycloak Organizations.

```
POST /realms/vetolib/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&refresh_token=eyJ...
&client_id=vetolib-web
&organization=a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

Keycloak retourne un nouveau access_token scope a l'organization demandee, avec le claim `clinic_id` mis a jour. Zero code backend custom.

**Pour les requetes normales** : Le `clinic_id` reste dans le JWT (approche A). `ClinicContext.cs` continue de le lire. Zero modification du code FROZEN.

**L'approche B (header) est a eviter** car :
1. Elle requiert un middleware custom de validation (l'user a-t-il acces a cette clinic ?)
2. Elle cree une surface d'attaque (header spoofable si le middleware a un bug)
3. Elle duplique la logique d'autorisation qui est deja dans Keycloak
4. Elle n'est pas necessaire avec le token exchange natif

---

## 4. Design du middleware .NET

### 4.1. Ce qui change vs l'existant

Aujourd'hui, la chaine est :
```
UseAuthentication() → UseAuthorization() → Endpoint handler
```

Avec Keycloak, la chaine reste identique. Le middleware JWT Bearer valide le token contre le JWKS endpoint de Keycloak (cle publique RSA) au lieu d'une cle symetrique. Le `ClaimsPrincipal` est popule de la meme facon.

**Pas de nouveau middleware necessaire** si le Protocol Mapper emet les bons claims.

### 4.2. Adaptation du JWT Bearer

```csharp
// AuthModuleServiceRegistrar.cs — APRES migration
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keycloak OIDC discovery — charge automatiquement les cles publiques
        options.Authority = config["Keycloak:Authority"];
        // ex: "https://keycloak.vetolib.com/realms/vetolib"
        options.Audience = "vetolib-api";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,

            // Mapping des claims pour compatibilite avec le code existant
            NameClaimType = "sub",
            RoleClaimType = ClaimTypes.Role
        };

        // Transformer les claims Keycloak en claims .NET standards
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity == null) return Task.CompletedTask;

                // Keycloak emet les roles dans resource_access.vetolib-api.roles
                // OU dans organization.roles (Organizations feature)
                // Le Protocol Mapper DOIT les emettre aussi en tant que claim "role" flat
                // pour compatibilite avec RequireRole() existant.

                // Verifier que clinic_id est present
                var clinicId = identity.FindFirst("clinic_id");
                if (clinicId == null)
                {
                    // Fallback: essayer le claim "organization" nested
                    // (seulement si le Protocol Mapper n'a pas emis clinic_id flat)
                    var orgClaim = identity.FindFirst("organization");
                    // Note: ce fallback est une securite, le Protocol Mapper
                    // DEVRAIT toujours emettre clinic_id flat
                }

                return Task.CompletedTask;
            }
        };
    });
```

### 4.3. Validation des roles per-organization

Keycloak Organizations supporte des roles specifiques par organization. Quand un user est "Admin" dans la clinic 1 et "Vet" dans la clinic 2, le token scope a la clinic 1 contient `roles: ["Admin"]` et celui scope a la clinic 2 contient `roles: ["Vet"]`.

**C'est exactement le comportement attendu.** Les policies existantes fonctionnent sans modification :

```csharp
// Ces policies continuent de fonctionner tel quel
options.AddPolicy("ClinicStaff", policy =>
    policy.RequireRole("Vet", "Receptionist", "Admin"));

options.AddPolicy("VetOrAdmin", policy =>
    policy.RequireRole("Vet", "Admin"));
```

Le role dans le token est celui de l'organization courante, pas un role global.

### 4.4. Cas du Owner Portal

Le Owner Portal utilise un client OIDC separe (`vetolib-owner-portal`). Les owners ne sont PAS membres d'organizations — ils sont des users du realm avec un role specifique (`owner`).

Le JWT du owner portal ne contient PAS de `clinic_id`. Le `ClinicContext` retourne `Guid.Empty`, ce qui est le comportement actuel correct (les owners accedent a leurs donnees via des endpoints specifiques qui ne dependent pas du global query filter).

---

## 5. Flow complet : Login -> Switch Clinic -> API Call

### 5.1. Premier login

```
Frontend                          Keycloak                         Backend API
   |                                 |                                 |
   |  1. Redirect to /auth?          |                                 |
   |     client_id=vetolib-web       |                                 |
   |     &response_type=code         |                                 |
   |     &scope=openid+organization  |                                 |
   |     &organization=clinic-uuid   |                                 |
   |  =============================> |                                 |
   |                                 |                                 |
   |  2. Login page (email/password  |                                 |
   |     ou Social Login)            |                                 |
   |  <============================> |                                 |
   |                                 |                                 |
   |  3. Authorization code          |                                 |
   |  <============================= |                                 |
   |                                 |                                 |
   |  4. Exchange code for tokens    |                                 |
   |  =============================> |                                 |
   |                                 |                                 |
   |  5. access_token + refresh_token|                                 |
   |     (scope a l'organization)    |                                 |
   |  <============================= |                                 |
   |                                 |                                 |
   |  6. GET /api/v1/appointments                                      |
   |     Authorization: Bearer {access_token}                          |
   |  ================================================================>|
   |                                 |                                 |
   |                                 |  7. JWT middleware valide token  |
   |                                 |     (JWKS endpoint)             |
   |                                 |  8. ClinicContext lit clinic_id  |
   |                                 |  9. EF Core filtre par clinic   |
   |                                 |                                 |
   |  10. Response (donnees de la clinic courante)                     |
   |  <================================================================|
```

### 5.2. Switch clinic

```
Frontend                          Keycloak                         Backend API
   |                                 |                                 |
   |  1. User clique "Abu Dhabi      |                                 |
   |     Animal Hospital" dans le    |                                 |
   |     ClinicSwitcher              |                                 |
   |                                 |                                 |
   |  2. POST /token                 |                                 |
   |     grant_type=refresh_token    |                                 |
   |     &refresh_token=eyJ...       |                                 |
   |     &organization=clinic-uuid-2 |                                 |
   |  =============================> |                                 |
   |                                 |                                 |
   |  3. Keycloak verifie :          |                                 |
   |     - refresh_token valide      |                                 |
   |     - user est membre de        |                                 |
   |       l'organization demandee   |                                 |
   |     - emet nouveau token avec   |                                 |
   |       clinic_id = clinic-uuid-2 |                                 |
   |       roles = roles dans cette  |                                 |
   |       organization              |                                 |
   |                                 |                                 |
   |  4. Nouveau access_token +      |                                 |
   |     refresh_token               |                                 |
   |  <============================= |                                 |
   |                                 |                                 |
   |  5. Frontend stocke les         |                                 |
   |     nouveaux tokens             |                                 |
   |  6. window.location.reload()    |                                 |
   |                                 |                                 |
   |  7. Toutes les requetes API     |                                 |
   |     utilisent le nouveau token  |                                 |
   |     avec clinic_id = clinic-2   |                                 |
```

### 5.3. Frontend : adaptation du ClinicSwitcher

Le `ClinicSwitcher.tsx` actuel appelle `POST /api/v1/auth/switch-clinic` (endpoint backend custom).

Apres migration, le switch se fait directement vers Keycloak (token exchange). Le backend n'est plus implique :

```typescript
// lib/api/clinic-group.ts — APRES migration
export async function switchClinic(clinicId: string): Promise<void> {
  const refreshToken = localStorage.getItem("refresh_token");

  // Token exchange directement avec Keycloak
  const response = await fetch(`${KEYCLOAK_URL}/protocol/openid-connect/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: new URLSearchParams({
      grant_type: "refresh_token",
      refresh_token: refreshToken!,
      client_id: "vetolib-web",
      organization: clinicId,  // Le parametre natif Keycloak Organizations
    }),
  });

  const tokens = await response.json();
  storeTokens(tokens);
}
```

**Alternative avec next-auth v5** : si on utilise `next-auth` pour gerer le flow OIDC, le switch se fait via un re-signin avec le parametre `organization` dans les `authorizationParams`. `next-auth` gere le refresh automatiquement.

### 5.4. Obtenir la liste des organizations d'un user

Pour que le `ClinicSwitcher` affiche la liste des cliniques accessibles, deux options :

**Option A** : Endpoint Keycloak Admin API (via un proxy backend)
```
GET /admin/realms/vetolib/users/{userId}/orgs
→ [{ id, name, alias, ... }, ...]
```

**Option B** : Endpoint backend custom qui lit depuis la DB locale
Le `ClinicGroup` et `ClinicGroupMember` existants peuvent servir. Mais avec Keycloak Organizations, la source de verite est Keycloak.

**Recommandation** : Option A. La liste des orgs vient de Keycloak. Le backend expose un proxy endpoint `/api/v1/auth/my-clinics` qui appelle l'Admin API Keycloak. Ca evite d'exposer l'Admin API au frontend.

---

## 6. Mapping Keycloak Organizations -> Claims

### 6.1. Configuration Keycloak

```json
{
  "realm": "vetolib",
  "organizationsEnabled": true,
  "clients": [
    {
      "clientId": "vetolib-api",
      "publicClient": false,
      "standardFlowEnabled": false,
      "serviceAccountsEnabled": true,
      "directAccessGrantsEnabled": false,
      "protocolMappers": [
        {
          "name": "clinic_id_mapper",
          "protocol": "openid-connect",
          "protocolMapper": "oidc-organization-membership-mapper",
          "config": {
            "claim.name": "clinic_id",
            "id.token.claim": "true",
            "access.token.claim": "true",
            "jsonType.label": "String"
          }
        },
        {
          "name": "org_roles_mapper",
          "protocol": "openid-connect",
          "protocolMapper": "oidc-organization-role-mapper",
          "config": {
            "claim.name": "roles",
            "multivalued": "true",
            "id.token.claim": "true",
            "access.token.claim": "true"
          }
        },
        {
          "name": "vet_license_mapper",
          "protocol": "openid-connect",
          "protocolMapper": "oidc-usermodel-attribute-mapper",
          "config": {
            "user.attribute": "vetLicenseNumber",
            "claim.name": "vetLicense",
            "id.token.claim": "true",
            "access.token.claim": "true",
            "jsonType.label": "String"
          }
        }
      ]
    },
    {
      "clientId": "vetolib-web",
      "publicClient": true,
      "standardFlowEnabled": true,
      "directAccessGrantsEnabled": false,
      "redirectUris": ["http://localhost:3000/*", "https://app.vetolib.com/*"],
      "webOrigins": ["http://localhost:3000", "https://app.vetolib.com"]
    }
  ],
  "organizations": [
    {
      "name": "Dubai Pet Clinic",
      "alias": "dubai-pet-clinic",
      "domains": [],
      "attributes": {
        "plan": ["professional"],
        "address": ["Dubai Healthcare City, Building 47"]
      }
    }
  ],
  "roles": {
    "realm": [
      { "name": "Admin" },
      { "name": "Vet" },
      { "name": "Receptionist" },
      { "name": "Assistant" }
    ]
  }
}
```

### 6.2. Roles par organization vs realm roles

Keycloak Organizations supporte deux niveaux de roles :
- **Realm roles** : globaux, assignes a un user dans tout le realm
- **Organization roles** : specifiques a une organization, assignes a un user pour cette org

**Pour Vetolib, les roles DOIVENT etre per-organization.** Un vet peut etre Admin dans sa propre clinic et simple Vet dans une clinic partenaire.

Les Organization Roles sont configures via l'Admin API :
```
POST /admin/realms/vetolib/organizations/{orgId}/roles
{ "name": "Admin" }
{ "name": "Vet" }
{ "name": "Receptionist" }
{ "name": "Assistant" }

POST /admin/realms/vetolib/organizations/{orgId}/members/{userId}/roles
[{ "name": "Vet" }]
```

Le `oidc-organization-role-mapper` injecte ces roles dans le token quand l'user s'authentifie dans le contexte de cette organization.

---

## 7. Impact sur le code existant

### 7.1. Fichiers qui changent

| Fichier | Nature du changement |
|---|---|
| `Auth/AuthModuleServiceRegistrar.cs` | Remplacer `AddJwtBearer` HMAC par Keycloak JWKS. Garder les policies. |
| `Auth/Application/Services/JwtTokenService.cs` | **SUPPRIME** — Keycloak genere les JWT |
| `Auth/Application/Services/IJwtTokenService.cs` | **SUPPRIME** |
| `Auth/Application/Commands/Login/LoginHandler.cs` | **SUPPRIME** — flow OIDC |
| `Auth/Application/Commands/RefreshToken/RefreshTokenHandler.cs` | **SUPPRIME** — Keycloak gere |
| `Auth/Application/Commands/SwitchClinic/SwitchClinicHandler.cs` | **SUPPRIME** ou transforme en proxy vers Keycloak Admin API |
| `Auth/Application/Domain/User.cs` | Supprimer `PasswordHash`, `VerifyPassword()`, `ChangePassword()`, `EmailVerificationToken`, `RecordFailedLogin()`, `IsLocked`, `LockedUntil` |
| `Auth/Application/Domain/RefreshToken.cs` | **SUPPRIME** |
| `Auth/Api/AuthEndpoints.cs` | Supprimer `/login`, `/refresh`, `/register`. Ajouter `/my-clinics` (proxy Keycloak). |
| `Auth/Api/ClinicGroupEndpoints.cs` | L'endpoint `/switch-clinic` est supprime. Le reste (ClinicGroup CRUD, dashboard) est conserve. |
| `Frontend/src/lib/api/clinic-group.ts` | `switchClinic()` appelle Keycloak directement |
| `Frontend/src/components/features/shell/ClinicSwitcher.tsx` | Lire les claims depuis le token OIDC (via next-auth session). Adapter les noms de claims. |
| `Frontend/src/lib/api/auth.ts` | Supprimer login/refresh custom. Deleguer a next-auth. |

### 7.2. Fichiers qui NE changent PAS

| Fichier | Raison |
|---|---|
| `Shared/Vetolib.Shared.Kernel/IClinicContext.cs` | FROZEN — interface inchangee |
| `Shared/Vetolib.Shared.Infrastructure/ClinicContext.cs` | FROZEN — lit `clinic_id`, le Protocol Mapper l'emet |
| `Shared/Vetolib.Shared.Infrastructure/MultiTenantDbContext.cs` | FROZEN — injecte ClinicContext, aucun changement |
| Tous les modules (Agenda, MedicalRecords, Billing, etc.) | Zero changement si les claims `clinic_id`, `role`, `sub` sont preserves |
| `Vetolib.Api/Program.cs` | La registration `AddScoped<IClinicContext, ClinicContext>()` reste. L'ajout de Keycloak Aspire se fait dans `AppHost/Program.cs`. |

### 7.3. Nouveau code a creer

| Fichier | Description |
|---|---|
| `Auth/Application/Services/IKeycloakAdminClient.cs` | Interface pour les appels Admin API Keycloak |
| `Auth/Infrastructure/KeycloakAdminClient.cs` | Implementation HttpClient vers l'Admin API |
| `Auth/Application/Queries/ListMyOrganizations/` | Query handler pour `/my-clinics` (proxy Admin API) |
| `Auth/Application/Commands/CreateOrganization/` | Cree une org Keycloak quand une clinic est creee |
| `Auth/Application/Commands/AddMemberToOrg/` | Ajoute un user a une org (invite) |
| `infra/keycloak/vetolib-realm.json` | Realm export pour import Aspire (dev) |
| `infra/keycloak/vetolib-realm-test.json` | Realm export simplifie pour Testcontainers |

---

## 8. Risques et edge cases

### 8.1. Claim name mismatch

**Risque** : Keycloak Organizations emet le claim `organization` (objet nested), pas `clinic_id` (string flat).

**Mitigation** : Le Protocol Mapper `oidc-organization-membership-mapper` DOIT etre configure pour emettre un claim flat `clinic_id`. A valider pendant le PoC (Phase 0).

**Si impossible** : Il faudrait un `OnTokenValidated` event dans le middleware JWT pour extraire `organization.id` et l'ajouter comme claim `clinic_id` au `ClaimsIdentity`. Cela evite de toucher a `ClinicContext.cs` (FROZEN).

```csharp
options.Events = new JwtBearerEvents
{
    OnTokenValidated = context =>
    {
        var identity = context.Principal?.Identity as ClaimsIdentity;
        if (identity?.FindFirst("clinic_id") == null)
        {
            // Fallback: extraire depuis le claim nested "organization"
            var orgJson = identity?.FindFirst("organization")?.Value;
            if (orgJson != null)
            {
                var org = JsonDocument.Parse(orgJson).RootElement;
                if (org.TryGetProperty("id", out var orgId))
                {
                    identity!.AddClaim(new Claim("clinic_id", orgId.GetString()!));
                }
            }
        }
        return Task.CompletedTask;
    }
};
```

### 8.2. Role claim format

**Risque** : Keycloak emet les roles dans `realm_access.roles` (array) au lieu de claims `role` multiples. Le `RequireRole()` de ASP.NET attend des claims de type `ClaimTypes.Role`.

**Mitigation** : Configurer `RoleClaimType` dans `TokenValidationParameters` OU utiliser un `ClaimsTransformation` pour mapper les roles Keycloak vers des claims .NET standard.

Avec les Organization Roles et le bon mapper, les roles devraient etre emis directement comme claims flat. A valider pendant le PoC.

### 8.3. User sans organization (premier login)

**Scenario** : Un user vient de s'inscrire (RegisterClinic). L'organization Keycloak n'est pas encore creee, ou le user n'est pas encore membre.

**Mitigation** : Le flow `RegisterClinic` DOIT :
1. Creer l'organization dans Keycloak via l'Admin API
2. Ajouter le user comme membre avec le role `Admin`
3. Puis rediriger vers le login OIDC avec le parametre `organization`

Si l'organization est creee mais le user pas encore membre, Keycloak refuse le login dans le contexte de cette org. C'est le comportement desire.

### 8.4. Latence du token exchange (switch clinic)

**Scenario** : Le switch de clinic necessite un round-trip vers Keycloak (token exchange). Si Keycloak est lent, le switch est lent.

**Mitigation** :
- Keycloak en mode clustering pour la HA
- Le token exchange est une operation legere (pas de DB lookup complexe)
- Le JWKS est cache cote backend (.NET le fait nativement, cache de 24h par defaut)
- Si la latence est inacceptable : pre-fetch les tokens des orgs les plus recentes cote frontend

### 8.5. Discrepance frontend claim names

**Constat** : Le frontend `ClinicSwitcher.tsx` lit `parseJwtClaim("clinicId")` (camelCase) mais le backend emet `clinic_id` (snake_case).

**Avec Keycloak** : Le Protocol Mapper controlera exactement le nom du claim. On peut choisir :
- `clinic_id` (snake_case) pour la compatibilite backend
- `clinicId` (camelCase) pour la compatibilite frontend
- Emettre les DEUX (deux Protocol Mappers)

**Recommandation** : Emettre `clinic_id` (snake_case) pour le backend. Le frontend utilise la session next-auth qui expose les claims de maniere structuree, pas de parsing JWT manuel.

### 8.6. Testcontainers Keycloak pour les TI

Les tests d'integration actuels utilisent un JWT genere par `JwtTokenService` (code custom). Apres migration, il faut :
- Soit utiliser `Testcontainers.Keycloak` (conteneur Keycloak reel en test)
- Soit generer des JWT de test signe avec une cle RSA de test (pas Keycloak)

**Recommandation** : Pour les TI, generer des JWT de test signes avec une cle RSA de test. Le backend est configure pour accepter cette cle en mode test. Pas besoin de Keycloak reel pour les TI (rapide, deterministe). Keycloak reel uniquement pour les TF (acceptance) et les tests E2E.

---

## 9. Resume des decisions

| Decision | Choix | Justification |
|---|---|---|
| Structure JWT | Token scope a 1 clinic | ClinicContext FROZEN, Global Query Filter mono-tenant |
| Switch clinic | Token exchange Keycloak natif (parametre `organization`) | Zero code custom backend, Keycloak valide l'appartenance |
| Header x-current-clinic | NON | Surface d'attaque inutile, le token exchange suffit |
| Claim `clinic_id` | Protocol Mapper flat | Compatibilite `ClinicContext.FindFirst("clinic_id")` sans modifier le code FROZEN |
| Roles | Per-organization (Organization Roles) | Un user peut avoir des roles differents par clinic |
| Owner Portal | User hors organization, client OIDC dedie | Pas de `clinic_id` dans le token, comportement actuel preserve |
| Frontend auth lib | next-auth v5 avec Keycloak provider | OIDC standard, gere le refresh, le token exchange |
| TI avec Keycloak | JWT de test signe RSA (pas de conteneur Keycloak) | Rapidite, determinisme |
| TF/E2E avec Keycloak | Testcontainers.Keycloak (conteneur reel) | Validation end-to-end du flow OIDC |

---

## 10. Prochaine etape

Cette etude est le complement technique de `keycloak-migration-20260401.md`. Elle detaille le design JWT/RBAC necessaire pour implementer les taches de la Phase 0 (PoC).

**Taches a creer en priorite** :
1. `todo-infra-keycloak-aspire` — Setup Keycloak dans AppHost, realm import avec Organizations
2. `todo-back-auth-keycloak-jwt-validation` — Remplacer HMAC par JWKS, configurer les Protocol Mappers
3. `todo-back-auth-claim-mapping-poc` — Valider que `clinic_id` flat est emis correctement par le mapper
4. `todo-front-auth-nextauth-keycloak` — Integrer next-auth v5, remplacer le login custom
5. `todo-front-clinic-switcher-oidc` — Adapter le ClinicSwitcher au token exchange Keycloak
