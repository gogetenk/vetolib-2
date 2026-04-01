# Keycloak Migration Study — 2026-04-01

> **Statut** : ETUDE — En attente de validation fondateur
> **Auteur** : Agent Architect
> **Modules impactes** : Auth (majeur), tous les autres (mineur)

---

## 1. Analyse de l'existant

### 1.1. Systeme d'authentification actuel

Le module Auth (`Vetolib.Auth`) implemente un systeme JWT custom complet :

| Composant | Implementation | Fichiers cles |
|---|---|---|
| JWT generation | `JwtTokenService` — HMAC-SHA256 avec cle symetrique | `Application/Services/JwtTokenService.cs` |
| Refresh tokens | Entite `RefreshToken` en DB, rotation a chaque usage, revocation | `Application/Domain/RefreshToken.cs` |
| Password hashing | BCrypt.Net (hash + verify dans l'entite `User`) | `Application/Domain/User.cs` |
| Account lockout | 5 tentatives max, lockout 15 min, auto-unlock apres expiration | `User.RecordFailedLogin()`, `AuthSecurityOptions` |
| Email verification | Token aleatoire 32 bytes, expiration 24h, domain event pour envoi | `User.VerifyEmail()`, `EmailVerificationRequestedDomainEvent` |
| Roles | Enum `UserRole` (Vet, Receptionist, Admin, Assistant) — claim `role` dans le JWT | `Contracts/UserRole.cs` |
| Multi-tenancy | Claim `clinic_id` dans le JWT, lu par `ClinicContext` pour le global query filter | `Shared.Infrastructure/ClinicContext.cs` |
| Owner portal | Systeme JWT separe (`OwnerPortalJwtService`) avec claim `account_type=owner_portal` | `Application/Services/OwnerPortalJwtService.cs` |
| Clinic switch | Regeneration de JWT avec un `clinic_id` different (pour multi-clinic groups) | `Commands/SwitchClinic/SwitchClinicHandler.cs` |
| Rate limiting | Sliding window sur `/auth` (10/min), fixed window sur `/signup` (3/h) | `Vetolib.Api/Program.cs` |
| Authorization policies | `ClinicStaff`, `VetOrAdmin` — definies dans `AuthModuleServiceRegistrar` | `AuthModuleServiceRegistrar.cs` |

### 1.2. Points d'integration critiques

**ClinicContext (FROZEN)** : `Shared.Infrastructure/ClinicContext.cs` extrait `clinic_id` du JWT pour alimenter le global query filter EF Core. Ce composant est GELE — toute modification necessite un arbitrage humain.

**Claim `clinic_id`** : Consomme par TOUS les modules via `IClinicContext`. C'est le pivot du multi-tenancy. Changer le format ou le nom de ce claim impacte l'ensemble de la plateforme.

**2 populations d'utilisateurs** :
1. **Staff clinique** : `User` entity, multi-tenant (un user = un clinic), JWT standard avec `clinic_id` + `role`
2. **Propretaires d'animaux** : `OwnerAccount` entity, global (pas `IMultiTenant`), JWT avec `account_type=owner_portal` + `linked_clinic_ids`

### 1.3. Ce qui fonctionne bien (a conserver)

- Ardalis.Result partout (pas d'exceptions pour le flow business)
- Domain events pour les side effects (email verification, user invited)
- Refresh token rotation avec concurrency token
- Rate limiting granulaire
- Separation staff / owner portal

### 1.4. Ce qui justifie la migration

- **Password management custom** : BCrypt dans l'entite User, pas de password policies standardisees (rotation, history, breach detection)
- **Pas d'OAuth2/OIDC** : impossible d'integrer des IdP externes (Google, Apple) pour le owner portal
- **Pas de SSO** : chaque clinic group necessite un switch manuel avec regeneration de token
- **Pas de MFA** : aucun support TOTP/WebAuthn — risque de conformite pour les donnees medicales
- **Session management basique** : pas de revocation de toutes les sessions, pas de device tracking
- **Maintenance lourde** : le module Auth est le plus gros module (28 handlers) et grossit avec chaque feature

---

## 2. Patterns multi-tenant Keycloak — Comparaison

### 2.1. Pattern A — 1 Realm par Clinic (Realm-per-Tenant)

**Principe** : Chaque clinic = un realm Keycloak independant.

**Avantages** :
- Isolation totale des donnees utilisateurs
- Configuration independante par clinic (branding, MFA policy, password policy)
- Un admin clinic ne voit que son realm
- Conforme aux exigences reglementaires strictes (donnees medicales isolees)

**Inconvenients majeurs (show-stopper)** :
- **Scalabilite** : Keycloak est connu pour mal scaler au-dela de ~100-200 realms. Chaque realm charge sa configuration en memoire. Le startup time augmente lineairement. Les caches sont per-realm, multipliant l'empreinte memoire.
- **Operations** : chaque realm necessite sa propre configuration (clients, roles, flows). Automatiser ca pour des centaines de clinics est un cauchemar operationnel.
- **Cross-realm impossible** : un user (ex: vet travaillant dans 2 clinics) ne peut pas exister dans un seul realm. Il faut dupliquer le compte — bye bye SSO.
- **Clinic Groups** : le switch de clinic (feature existante) necessite une authentification dans un autre realm = experience utilisateur degradee.
- **Owner Portal** : un proprietaire lie a 3 clinics aurait besoin de 3 comptes dans 3 realms differents.

**Verdict : REJETE** — Incompatible avec le modele Vetolib (clinic groups, owner portal cross-clinic, vets multi-clinic). Les limitations de scalabilite de Keycloak sur les realms sont documentees et confirmees par la communaute.

### 2.2. Pattern B — 1 Realm Global + Groupes/Attributs (Single-Realm)

**Principe** : Un seul realm `vetolib`. Les clinics sont modelisees comme des groupes Keycloak. Le `clinic_id` est un attribut utilisateur ou un group attribute.

**Avantages** :
- **Scalabilite excellente** : un seul realm, pas de probleme de memoire/startup
- **SSO natif** : un user avec un seul compte peut appartenir a plusieurs groupes (clinics)
- **Owner Portal simple** : les owners sont des users dans le meme realm avec un role/group different
- **Clinic Groups** : le switch de clinic = changer le scope/group actif, pas re-authentifier
- **Administration centralisee** : un seul client, un seul flow, une seule config

**Inconvenients** :
- L'isolation des donnees repose entierement sur les claims JWT + global query filter (c'est deja le cas aujourd'hui)
- Pas d'isolation au niveau IdP — un admin Keycloak voit tous les users
- Password policies uniformes pour toutes les clinics (ou complexite custom)
- Les custom claims necessitent un Protocol Mapper custom dans Keycloak

**Implementation technique** :

```
Realm: vetolib
  |
  +-- Groups (hierarchy)
  |     +-- /clinics
  |     |     +-- /clinics/{clinic-uuid-1}  (attributes: clinic_name, plan, etc.)
  |     |     +-- /clinics/{clinic-uuid-2}
  |     |
  |     +-- /owner-portal  (for pet owner accounts)
  |
  +-- Roles
  |     +-- realm roles: vet, receptionist, admin, assistant, owner
  |
  +-- Clients
  |     +-- vetolib-api (confidential, service account)
  |     +-- vetolib-web (public, PKCE)
  |     +-- vetolib-owner-portal (public, PKCE)
  |
  +-- Protocol Mappers (sur le client vetolib-api)
        +-- clinic_id_mapper: extrait le group attribute "clinic_id" du groupe actif
        +-- role_mapper: mappe le role realm vers le claim "role"
```

**Mapping clinic_id** :
- Chaque user est membre d'un ou plusieurs groupes `/clinics/{uuid}`
- Un Protocol Mapper custom (User Attribute ou Group Membership) injecte le `clinic_id` dans le JWT
- Pour le switch de clinic : le frontend demande un token avec un scope specifique ou utilise le token exchange
- Alternative : stocker le `clinic_id` actif comme user attribute et le changer via l'Admin API au switch

**Verdict : RECOMMANDE avec reserves** (voir section 3)

### 2.3. Pattern C — Hybrid (1 Realm + Organizations)

**Principe** : Utiliser la feature **Organizations** de Keycloak 25+ (GA depuis Keycloak 26). Les Organizations sont le mecanisme natif de Keycloak pour le multi-tenancy B2B.

**Avantages** :
- **Concu pour ce use case exact** : Organizations = tenants dans un SaaS B2B
- Chaque clinic = une Organization
- Les users sont membres d'organisations avec des roles specifiques a l'organisation
- SSO et IdP broker par organisation (utile pour les grandes chaines veterinaires)
- Admin delegue par organisation (le clinic admin gere ses users sans acceder aux autres)
- **Token claims natifs** : `organization` claim dans le JWT, configurable
- Pas besoin de Protocol Mapper custom

**Inconvenients** :
- Feature relativement recente (GA dans Keycloak 26, septembre 2024)
- Documentation encore en evolution
- Quelques limitations connues : pas de hierarchie d'organisations (clinic groups = flat), pas de token exchange entre organisations natif
- Necessite Keycloak >= 26 (pas de LTS avant longtemps)

**Implementation technique** :

```
Realm: vetolib
  |
  +-- Organizations
  |     +-- org-{clinic-uuid-1}  (name: "Dubai Vet Center", domain: ...)
  |     |     +-- Members: user-1 (role: admin), user-2 (role: vet)
  |     |
  |     +-- org-{clinic-uuid-2}
  |           +-- Members: user-1 (role: vet), user-3 (role: receptionist)
  |
  +-- Clients
  |     +-- vetolib-api
  |     +-- vetolib-web
  |
  +-- Token Claims
        +-- "organization" claim = clinic_id equivalent (natif)
```

**Mapping clinic_id** :
- Le claim `organization` est automatiquement injecte dans le JWT quand un user s'authentifie dans le contexte d'une organisation
- Pour le multi-clinic : Keycloak 26 supporte le switch d'organisation via le token endpoint (parametre `organization`)
- Compatible avec le `ClinicContext` existant : il suffit de mapper `organization` -> `clinic_id`

**Verdict : RECOMMANDE** (option preferee)

---

## 3. Recommandation

### Choix : Pattern C — Organizations (Keycloak 26+)

**Justification** :

1. **Alignement exact avec le modele Vetolib** : 1 clinic = 1 organization, multi-membership natif, roles par organisation
2. **Switch de clinic natif** : le parametre `organization` sur le token endpoint correspond exactement au `SwitchClinic` actuel
3. **Owner Portal** : les owners peuvent etre des users hors organisation ou dans une organisation speciale
4. **Clinic Groups** : modelisables comme un "admin" qui est membre de N organisations
5. **MFA, SSO, IdP federation** : tout est natif, zero code custom
6. **Pas de Protocol Mapper custom** : le claim `organization` est natif
7. **Scalabilite** : les organisations sont dans un seul realm, pas de probleme de memoire
8. **Future-proof** : les Organizations sont la direction strategique de Keycloak pour le B2B SaaS

**Reserve principale** : Keycloak 26 est recent (septembre 2024). Il faut verifier :
- Stabilite en production de la feature Organizations a l'echelle prevue (500+ clinics a horizon 12 mois)
- Disponibilite de la feature dans les offres managees (Keycloak Cloud / Bitnami / AWS Marketplace)
- Compatibilite avec l'Aspire integration (voir section 5)

**Fallback** : Si les Organizations s'averent trop immatures en phase de validation, basculer sur le Pattern B (groupes) qui est eprouve mais necessite plus de code custom.

---

## 4. Impact par module

### 4.1. Module Auth (IMPACT MAJEUR)

**Supprime** :
- `JwtTokenService` + `OwnerPortalJwtService` — Keycloak genere les JWT
- `RefreshToken` entity + table — Keycloak gere les refresh tokens
- `User.PasswordHash`, `User.VerifyPassword()`, `User.ChangePassword()` — Keycloak gere les credentials
- `User.FailedLoginAttempts`, `User.IsLocked`, `User.LockedUntil` — Keycloak brute force protection
- `User.EmailVerified`, `User.EmailVerificationToken` — Keycloak email verification
- `LoginHandler`, `RefreshTokenHandler`, `LogoutHandler`, `ChangePasswordHandler`, `VerifyEmailHandler` — remplaces par OIDC flow
- `RegisterClinicHandler` — partiellement, la creation du user passe par l'Admin API Keycloak
- `OwnerPortalLoginHandler`, `RegisterOwnerAccountHandler` — remplaces par OIDC flow avec client dedie
- `OwnerAccount` entity — potentiellement supprimable si tout est dans Keycloak
- `AuthSecurityOptions` (TokenExpirationMinutes, MaxFailedLoginAttempts, LockoutMinutes) — configure dans Keycloak

**Modifie** :
- `AuthModuleServiceRegistrar` : remplacer `AddJwtBearer` avec cle symetrique par validation JWT avec cle RSA/EC publique de Keycloak (JWKS endpoint)
- `User` entity : supprimer tout ce qui concerne les credentials, garder les champs metier (Role, VetLicenseNumber, IsActive)
- `Clinic` entity : ajouter un champ `KeycloakOrganizationId` pour le mapping
- `SwitchClinicHandler` : simplifier — le switch se fait cote frontend via un re-token avec le parametre `organization`
- `InviteUserHandler` : creer le user dans Keycloak via l'Admin API au lieu de `User.Invite()`
- `CreateUserHandler` : idem
- Tous les endpoints publics (`/login`, `/refresh`, `/register`) : supprimer ou transformer en proxy vers Keycloak

**Conserve tel quel** :
- `Clinic` entity (hors ajout KeycloakOrganizationId)
- `ClinicGroup`, `ClinicGroupMember` — logique metier complementaire
- `OnboardingState` — pas lie a l'auth
- `ReferralCode` — pas lie a l'auth
- `WebhookRegistration`, `WebhookLog` — pas lie a l'auth
- `VetInvitationLog` — conserver pour l'historique
- Toutes les queries (`GetCurrentUser`, `ListUsers`, etc.) — continuent de lire depuis la DB locale
- `IClinicVetReader`, `ISubscriptionChecker` — pas d'impact

### 4.2. Module Agenda (IMPACT MINEUR)

- Aucun changement de code si le claim `clinic_id` est preserve dans le JWT Keycloak (via mapping `organization` -> `clinic_id`)
- Les `RequireAuthorization(policy => policy.RequireRole(...))` continuent de fonctionner si les roles Keycloak sont mappes sur les memes noms

### 4.3. Modules MedicalRecords, Billing, Messaging, Stock, AI, Breeding, Preferences, Notifications (IMPACT MINEUR)

- Meme chose que Agenda : zero changement si le contrat JWT (claims `sub`, `clinic_id`, `role`) est preserve
- C'est le point crucial de la migration : **le JWT Keycloak doit emettre exactement les memes claims que le JWT custom actuel**

### 4.4. Shared.Infrastructure — ClinicContext (FROZEN)

**Impact : AUCUN si le mapping est correct.**

`ClinicContext.cs` lit `FindFirst("clinic_id")`. Deux options :
1. Configurer le Protocol Mapper Keycloak pour emettre un claim `clinic_id` (option la plus simple)
2. Modifier `ClinicContext` pour lire le claim `organization` — mais ce fichier est GELE

**Recommandation** : Option 1 — configurer Keycloak pour emettre `clinic_id`. Zero modification du code GELE.

### 4.5. Frontend (IMPACT MODERE)

- Remplacer le login custom (POST `/api/v1/auth/login`) par le flow OIDC Authorization Code + PKCE
- Integrer une lib OIDC (`next-auth` v5 ou `oidc-client-ts`)
- Le refresh token est gere par la lib OIDC, plus par le frontend
- Le switch de clinic = re-demander un token avec le parametre `organization`
- L'owner portal utilise un client OIDC dedie (`vetolib-owner-portal`)
- Les `lib/api/*.ts` gardent le meme pattern `Authorization: Bearer {token}` — pas de changement

---

## 5. Integration .NET Aspire

### 5.1. Composant Aspire Keycloak

.NET Aspire dispose d'un composant officiel pour Keycloak depuis Aspire 9.0 :

```csharp
// AppHost/Program.cs
var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume()                    // Persist data between restarts
    .WithRealmImport("./keycloak-realm-export.json");  // Import realm config

builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(keycloak)             // Injects Keycloak connection info
    .WaitFor(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);
```

**Ce que fait `AddKeycloak`** :
- Lance un container Keycloak (image `quay.io/keycloak/keycloak`) dans le dev environment
- Expose les ports HTTP (8080) et management
- Injecte les variables d'environnement dans les projets dependants (issuer URL, client ID)
- Supporte l'import de realm via JSON (pour reproduire la config en dev)
- Health check integre

**Ce que fait `WithReference(keycloak)` cote API** :
- Injecte `services.AddAuthentication().AddKeycloakJwtBearer()` ou configure les connection strings
- L'API valide les JWT contre le JWKS endpoint de Keycloak (cle publique RSA)

### 5.2. Configuration cote API

```csharp
// AuthModuleServiceRegistrar.cs — APRES migration
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = config["Keycloak:Authority"];  // ex: http://keycloak:8080/realms/vetolib
        options.Audience = "vetolib-api";
        options.RequireHttpsMetadata = false;  // Dev only
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            NameClaimType = "sub",
            RoleClaimType = "realm_access.roles"  // Keycloak-specific
        };
    });
```

### 5.3. Realm Import pour le dev

Creer un fichier `infra/keycloak/vetolib-realm.json` avec :
- Realm `vetolib`
- Clients (`vetolib-api`, `vetolib-web`, `vetolib-owner-portal`)
- Roles (vet, admin, receptionist, assistant, owner)
- Protocol Mappers (clinic_id, role, vetLicense)
- Organizations template

Ce fichier est importe automatiquement par Aspire au demarrage.

---

## 6. Observabilite — OpenTelemetry

### 6.1. Keycloak + OpenTelemetry

Keycloak 25+ supporte nativement OpenTelemetry :

```
# keycloak.conf ou variables d'environnement
KC_TRACING_ENABLED=true
KC_TRACING_ENDPOINT=http://otel-collector:4317
KC_TRACING_PROTOCOL=grpc
KC_TRACING_RESOURCE_ATTRIBUTES=service.name=keycloak
```

**Metriques exposees** :
- Login success/failure rate
- Token generation latency
- Active sessions count
- Realm events (user creation, password change, etc.)

### 6.2. Integration avec Aspire Dashboard

Le composant Aspire Keycloak route automatiquement les traces vers le collecteur OTLP d'Aspire. Les traces Keycloak apparaissent dans l'Aspire Dashboard aux cotes des traces .NET.

**Traces distribuees** : un appel frontend -> API -> Keycloak (token validation) apparait comme une seule trace avec 3 spans.

### 6.3. Sentry

Les events Keycloak peuvent etre routes vers Sentry via le Event Listener SPI de Keycloak ou via l'OTLP collector qui forward vers Sentry.

---

## 7. Risques et migration path

### 7.1. Risques identifies

| Risque | Probabilite | Impact | Mitigation |
|---|---|---|---|
| Organizations Keycloak 26 instables | Moyenne | Elevee | Fallback Pattern B (groupes). PoC de 2 semaines avant commit. |
| Breaking change sur le claim `clinic_id` | Faible | Critique | Protocol Mapper qui emet exactement `clinic_id`. Tests d'integration existants le verifient. |
| Performance Keycloak (latence token validation) | Faible | Moyenne | JWKS caching (.NET le fait nativement), Keycloak en mode embedded H2 pour les tests. |
| Migration des users existants | Certaine | Moyenne | Script de migration via Admin API. BCrypt hashes non importables directement — forcer un password reset. |
| Downtime pendant la migration | Moyenne | Elevee | Migration en phases (voir 7.2). Dual-stack temporaire. |
| Owner Portal migration | Moyenne | Moyenne | Les OwnerAccount n'ont pas de donnees critiques. Fresh start possible. |
| Complexite operationnelle Keycloak | Certaine | Moyenne | Formation equipe, runbooks, monitoring dedie. |

### 7.2. Plan de migration (zero-downtime)

**Phase 0 — PoC (2 semaines)**
- Deployer Keycloak 26 via Aspire
- Configurer le realm `vetolib` avec Organizations
- Valider le mapping `organization` -> `clinic_id` claim
- Executer les tests d'integration existants contre des JWT Keycloak
- **Go/No-Go** : si le PoC echoue sur les Organizations, pivoter vers Pattern B

**Phase 1 — Dual-stack lecture (1 semaine)**
- L'API accepte DEUX types de JWT : custom (ancien) ET Keycloak
- Implementation : `AddJwtBearer` avec deux schemes, `AddPolicyScheme` pour router
- Les nouveaux users sont crees dans Keycloak
- Les users existants continuent avec l'ancien systeme

```csharp
services.AddAuthentication("MultiScheme")
    .AddJwtBearer("Legacy", options => { /* ancien config HMAC-SHA256 */ })
    .AddJwtBearer("Keycloak", options => { /* nouveau config RSA/JWKS */ })
    .AddPolicyScheme("MultiScheme", "MultiScheme", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var token = context.Request.Headers.Authorization.FirstOrDefault();
            // Distinguer par issuer ou par format de token
            return IsKeycloakToken(token) ? "Keycloak" : "Legacy";
        };
    });
```

**Phase 2 — Migration des users (1-2 semaines)**
- Script batch via l'Admin API Keycloak pour creer les users existants
- BCrypt hashes ne sont PAS importables dans Keycloak nativement. Options :
  - a) Forcer un password reset pour tous les users (envoyer un email "Nouveau systeme de connexion")
  - b) Implementer un Custom User Storage SPI qui verifie contre BCrypt au premier login puis migre le hash (lazy migration)
- **Recommandation** : Option b) pour zero friction. Le SPI verifie le BCrypt hash, si OK, Keycloak re-hash avec son propre algorithme et desactive le SPI pour cet user.

**Phase 3 — Cutover (1 jour)**
- Retirer le scheme "Legacy"
- Supprimer les endpoints `/auth/login`, `/auth/refresh`, `/auth/register` (ou les transformer en redirects)
- Supprimer `JwtTokenService`, `RefreshToken` entity, tables associees
- Deployer le frontend avec le flow OIDC

**Phase 4 — Nettoyage (1 semaine)**
- Supprimer le code mort dans le module Auth
- Supprimer les tables `refresh_tokens`, `password_hash` columns
- Migration EF Core pour retirer les colonnes obsoletes
- Mettre a jour les tests (TU, TI, TF)

### 7.3. Backward compatibility

**Pendant la Phase 1-2** :
- Le claim `clinic_id` est present dans les deux types de JWT -> `ClinicContext` fonctionne sans modification
- Le claim `role` est present dans les deux types de JWT -> les policies `ClinicStaff`, `VetOrAdmin` fonctionnent
- Le claim `sub` contient le user ID dans les deux cas -> les endpoints qui lisent `sub` fonctionnent

**Apres la Phase 3** :
- Seuls les JWT Keycloak sont acceptes
- Le claim `clinic_id` est emis par le Protocol Mapper / Organizations
- Le claim `role` est emis par le Role Mapper Keycloak

---

## 8. Estimation effort

| Phase | Taches | Estimation | Dependances |
|---|---|---|---|
| Phase 0 — PoC | Setup Keycloak Aspire, realm config, Organizations test, claim mapping, TI validation | 2 semaines | Aucune |
| Phase 1 — Dual-stack | Multi-scheme auth, Keycloak client config, frontend OIDC lib | 1 semaine | Phase 0 validee |
| Phase 2 — Migration users | Custom SPI BCrypt, script migration batch, emails utilisateurs | 1-2 semaines | Phase 1 |
| Phase 3 — Cutover | Retrait Legacy scheme, suppression endpoints, deploy frontend OIDC | 1 jour | Phase 2 + fenetre maintenance |
| Phase 4 — Nettoyage | Code mort, migration EF, tests | 1 semaine | Phase 3 |
| **Total** | | **5-6 semaines** | |

**Decomposition en taches (a creer apres validation)** :

1. `todo-infra-keycloak-aspire` — Ajouter Keycloak dans AppHost, realm import
2. `todo-infra-keycloak-realm-config` — Configurer realm, clients, roles, Protocol Mappers, Organizations
3. `todo-back-auth-dual-stack` — Multi-scheme JWT validation (Legacy + Keycloak)
4. `todo-back-auth-keycloak-admin-api` — Service wrapper pour l'Admin API Keycloak (CRUD users/orgs)
5. `todo-back-auth-user-migration-spi` — Custom SPI pour lazy BCrypt migration
6. `todo-back-auth-invite-keycloak` — Modifier InviteUser/CreateUser pour creer dans Keycloak
7. `todo-back-auth-register-keycloak` — Modifier RegisterClinic pour creer org + user dans Keycloak
8. `todo-back-auth-switch-clinic-oidc` — Adapter SwitchClinic au flow Organizations
9. `todo-front-auth-oidc` — Integrer `next-auth` v5 avec Keycloak provider, remplacer le login custom
10. `todo-front-owner-portal-oidc` — OIDC pour le owner portal
11. `todo-back-auth-cutover` — Retirer Legacy scheme, supprimer code mort
12. `todo-back-auth-cleanup-migration` — Migration EF pour retirer colonnes/tables obsoletes
13. `todo-test-auth-keycloak` — Adapter TU/TI/TF pour Keycloak (Testcontainers Keycloak)
14. `todo-infra-keycloak-otel` — Configurer OpenTelemetry sur Keycloak, integration Aspire Dashboard
15. `todo-infra-keycloak-prod` — Docker compose / Kubernetes config pour la prod

**Total : ~15 taches, 5-6 semaines pour un agent a temps plein.**

---

## 9. Questions ouvertes pour le fondateur

1. **Password migration** : Preferes-tu un reset force (email "Nouveau systeme") ou une lazy migration (transparente mais necessite un SPI custom) ?
2. **Owner Portal** : Les OwnerAccounts existants doivent-ils etre migres ou on peut repartir a zero (il y en a peu en phase pre-launch) ?
3. **Keycloak manage ou self-hosted** : En prod, utiliser Keycloak Cloud (SaaS), un managed service (AWS/Azure), ou self-hosted ?
4. **Timeline** : La migration est-elle urgente (blocker pour un client) ou planifiee post-MVP ?
5. **MFA** : Doit-on activer le MFA des la migration ou le garder optionnel dans un premier temps ?
6. **Social login** : Le owner portal doit-il supporter Google/Apple login des le jour 1 ?
7. **Fallback Pattern B** : Si le PoC Organizations echoue, le Pattern B (groupes) est-il acceptable comme plan B ?

---

## 10. Decision matrix

| Critere | Custom actuel | Pattern A (Realm/tenant) | Pattern B (Single-realm groups) | Pattern C (Organizations) |
|---|---|---|---|---|
| Effort migration | 0 | Elevee | Moyenne | Moyenne-Elevee |
| Scalabilite | OK | KO (>200 clinics) | OK | OK |
| SSO | Non | Non (cross-realm) | Oui | Oui |
| MFA | Non | Oui | Oui | Oui |
| Social login | Non | Oui | Oui | Oui |
| Multi-clinic | Custom | KO | Custom (Protocol Mapper) | Natif |
| Owner Portal | Custom | KO | Custom | Natif (user hors org) |
| Clinic switch | Custom handler | Re-auth | Custom (attribute switch) | Natif (org parameter) |
| Admin delegue | Non | Oui (realm admin) | Non | Oui (org admin) |
| Maintenabilite | Faible (code custom) | Elevee (ops overhead) | Bonne | Bonne |
| Maturite | Eprouve chez Vetolib | Eprouve | Eprouve | Recente (KC 26) |
| OpenTelemetry | N/A | Oui | Oui | Oui |
| Aspire support | N/A | Oui | Oui | Oui |

---

## 11. Conclusion

La migration vers Keycloak avec le **Pattern C (Organizations)** est la meilleure option a moyen terme. Elle resout les limitations actuelles (MFA, SSO, social login, password management) tout en s'alignant parfaitement avec le modele multi-tenant de Vetolib.

**Cependant**, cette migration n'est pas urgente pour le MVP. Le systeme actuel est fonctionnel et securise. La migration devrait etre planifiee **apres le lancement** et **apres validation du PoC** (Phase 0).

**Prochaine etape** : Le fondateur valide cette etude, puis l'orchestrateur cree les 15 taches dans `tasks/todo/`.
