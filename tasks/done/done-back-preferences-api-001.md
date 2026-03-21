# todo-back-preferences-api-001.md -- API Preferences CRUD + Consent audit

**Module** : Preferences
**Dependances** : todo-back-preferences-domain-001
**Priorite** : MOYENNE
**Skills a lire** : `aspnet-minimal-api`, `ardalis-result`, `cqrs-mediatr`

---

## Objectif

Implementer tous les endpoints Minimal API du module Preferences : CRUD user preferences, clinic defaults (admin), consent revoke, audit trail.

## Spec de reference

- `docs/PREFERENCES-STUDY.md` sections 3 (API endpoints) et 5 (conformite reglementaire)
- `docs/PO-POST-MVP-DECISIONS.md` section 2

## Decisions PO a respecter

- `PUT /api/preferences/{key}` avec `key = AIDrugInteractions` et `value = false` : **doit retourner 400** avec message "Drug interaction alerts cannot be disabled"
- Clinic-level settings (AI toggles, analytics) : **AdminOnly** via RBAC
- User-level settings (notifications email, language) : **any authenticated user** modifie les siennes
- Chaque modification de preference genere un `ConsentAuditEntry` automatiquement
- L'audit trail n'est jamais supprime

## Implementation

### Handlers MediatR (CQRS)

1. **GetUserPreferencesQuery** -> `Result<List<PreferenceCategoryDto>>`
   - Resout les preferences effectives : system < clinic default < user override
   - Chaque preference indique sa `Source` ("system_default", "clinic_default", "user_override")

2. **GetUserPreferencesByCategoryQuery(category)** -> `Result<List<PreferenceDto>>`

3. **UpdatePreferenceCommand(key, value)** -> `Result`
   - Valide que `key` n'est pas `AIDrugInteractions` avec `value = false`
   - Cree ou met a jour la `UserPreference`
   - Cree un `ConsentAuditEntry` avec source "user_action"
   - Publie `PreferenceChangedIntegrationEvent` via MassTransit

4. **BulkUpdatePreferencesCommand(preferences[])** -> `Result`
   - Meme logique que UpdatePreference, en batch
   - Un seul `ConsentAuditEntry` par preference modifiee

5. **RevokeConsentCommand(category)** -> `Result`
   - Passe toutes les preferences bool de la categorie a "false"
   - `ConsentAuditEntry` par preference modifiee, source "consent_revoke"
   - Exception : `AIDrugInteractions` n'est pas affecte par un revoke de la categorie AIFeatures

6. **GetClinicDefaultsQuery** -> `Result<List<PreferenceCategoryDto>>` (AdminOnly)

7. **UpdateClinicDefaultsCommand(defaults[])** -> `Result` (AdminOnly)
   - Cree un `ConsentAuditEntry` par default modifie, source "admin_default"

8. **GetConsentAuditQuery(userId?, category?, from?, to?, page, pageSize)** -> `Result<PagedResult<ConsentAuditDto>>` (AdminOnly)

### Endpoints Minimal API

```
GET    /api/preferences                    -> GetUserPreferencesQuery
GET    /api/preferences/{category}         -> GetUserPreferencesByCategoryQuery
PUT    /api/preferences/{key}              -> UpdatePreferenceCommand
PUT    /api/preferences                    -> BulkUpdatePreferencesCommand
POST   /api/preferences/consent/revoke     -> RevokeConsentCommand
GET    /api/clinics/preferences            -> GetClinicDefaultsQuery (AdminOnly)
PUT    /api/clinics/preferences            -> UpdateClinicDefaultsCommand (AdminOnly)
GET    /api/preferences/audit              -> GetConsentAuditQuery (AdminOnly)
```

Tous les endpoints utilisent `.ToMinimalApiResult()`.

### Validators (FluentValidation)

- `UpdatePreferenceValidator` : key doit etre un PreferenceKey valide, value non vide
- `BulkUpdatePreferencesValidator` : liste non vide, max 50 items, chaque item valide
- `RevokeConsentValidator` : category doit etre un PreferenceCategory valide
- `UpdateClinicDefaultsValidator` : idem bulk

### ConsentAuditEntry automatique

Chaque handler qui modifie une preference doit :
1. Lire la valeur precedente (ou "NOT_SET" si premiere modification)
2. Creer un `ConsentAuditEntry` avec `PreviousValue`, `NewValue`, `Source`, `IpAddress`, `UserAgent`
3. `IpAddress` et `UserAgent` extraits de `HttpContext` via un service `IAuditContextProvider`

### Tests BDD

Creer `features/Preferences/Preferences.feature` avec les scenarios :

```gherkin
Feature: Preference Management

  Scenario: User retrieves effective preferences
    Given a clinic with default AI triage set to "false"
    And a user with no preference overrides
    When the user retrieves their preferences
    Then the preference "AITriage" has value "false" with source "clinic_default"
    And the preference "NotificationEmail" has value "true" with source "system_default"

  Scenario: User updates a preference
    When the user sets preference "NotificationEmail" to "false"
    Then the preference "NotificationEmail" has value "false" with source "user_override"
    And a consent audit entry is created for "NotificationEmail"

  Scenario: User cannot disable drug interaction alerts
    When the user sets preference "AIDrugInteractions" to "false"
    Then the response status is 400
    And the preference "AIDrugInteractions" remains "true"

  Scenario: User revokes analytics consent
    Given the user has opted in to analytics
    When the user revokes consent for category "Analytics"
    Then all analytics preferences are set to "false"
    And consent audit entries are created for each analytics preference

  Scenario: Admin updates clinic defaults
    Given the user has role "Admin"
    When the admin sets clinic default "AITriage" to "false"
    Then all users without an override see AITriage as "false"

  Scenario: Non-admin cannot update clinic defaults
    Given the user has role "Vet"
    When the user tries to update clinic defaults
    Then the response status is 403

  Scenario: Admin views consent audit trail
    Given several preference changes have been recorded
    When the admin retrieves the consent audit
    Then the audit entries are returned in chronological order
```

## Critere

```
[] 8 endpoints Minimal API fonctionnels avec ToMinimalApiResult()
[] Resolution cascade system < clinic < user implementee
[] AIDrugInteractions refuse de passer a false (400)
[] ConsentAuditEntry cree automatiquement a chaque modification
[] IpAddress et UserAgent captures dans l'audit
[] RBAC : clinic defaults et audit = AdminOnly
[] FluentValidation sur tous les commands
[] PreferenceChangedIntegrationEvent publie via MassTransit
[] Feature file Preferences.feature cree
[] Step definitions implementees
[] Tous les scenarios BDD verts
[] Renommer en done
```
