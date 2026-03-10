# Etude architecturale : Preference Management (opt-in/opt-out)

**Date** : 2026-03-10
**Auteur** : Agent architecte
**Statut** : Draft -- en attente validation PO + architecte humain

---

## Table des matieres

1. [Recommandation architecture module](#1-recommandation-architecture-module)
2. [Data model propose](#2-data-model-propose)
3. [API endpoints](#3-api-endpoints)
4. [Impact sur les modules existants](#4-impact-sur-les-modules-existants)
5. [Conformite reglementaire](#5-conformite-reglementaire)
6. [Frontend](#6-frontend)
7. [Tasks techniques decoupees](#7-tasks-techniques-decoupees)
8. [Risques](#8-risques)

---

## 1. Recommandation architecture module

### Question : nouveau module `Vetolib.Preferences` ou extension du module Auth ?

**Recommandation : nouveau module `Vetolib.Preferences`.**

### Justification

| Critere | Extension Auth | Nouveau module Preferences |
|---|---|---|
| Cohesion fonctionnelle | Faible -- Auth gere identite et authentification, pas les choix utilisateur | Forte -- un seul domaine : les preferences/consentements |
| Consommateurs | Tous les modules (Notifications, AI, Analytics, Messaging) | Idem, mais via `.Contracts` (propre) |
| Schema DB | Pollution du schema `auth` avec des tables sans rapport | Schema `preferences` isole |
| Taille de Auth | Auth est deja charge (users, clinics, tokens, RBAC, password) | Auth ne grossit plus |
| Conformite CLAUDE.md | Enfreint le principe "un module = un domaine" | Conforme |
| Deploiement independant | Les changements de preferences declenchent des tests Auth inutiles | Cycle de dev independant |

Le seul argument pour Auth serait la proximite avec `User` (les preferences sont liees a un UserId). Mais ce lien se fait naturellement via un Guid -- pas besoin de couplage runtime.

### Structure module

```
Modules/
  Preferences/
    Vetolib.Preferences.Contracts/        <-- PUBLIC
      PreferenceDto.cs
      PreferenceCategoryDto.cs
      ConsentAuditDto.cs
      IPreferenceChecker.cs               <-- interface cross-module
      PreferenceChangedIntegrationEvent.cs <-- event MassTransit
      PreferenceCategory.cs               <-- enum
      PreferenceKey.cs                     <-- enum
      Vetolib.Preferences.Contracts.csproj
    Vetolib.Preferences/                  <-- INTERNAL
      Application/
        Commands/
          UpdatePreference/
            UpdatePreferenceCommand.cs
            UpdatePreferenceHandler.cs
            UpdatePreferenceValidator.cs
          BulkUpdatePreferences/
            BulkUpdatePreferencesCommand.cs
            BulkUpdatePreferencesHandler.cs
          RevokeConsent/
            RevokeConsentCommand.cs
            RevokeConsentHandler.cs
        Queries/
          GetUserPreferences/
            GetUserPreferencesQuery.cs
            GetUserPreferencesHandler.cs
          GetClinicDefaults/
            GetClinicDefaultsQuery.cs
            GetClinicDefaultsHandler.cs
          CheckPreference/
            CheckPreferenceQuery.cs
            CheckPreferenceHandler.cs
        Services/
          PreferenceChecker.cs            <-- implements IPreferenceChecker
        Domain/
          UserPreference.cs
          ClinicPreferenceDefault.cs
          ConsentAuditEntry.cs
      Infrastructure/
        PreferencesDbContext.cs
        UserPreferenceConfiguration.cs
        ClinicPreferenceDefaultConfiguration.cs
        ConsentAuditConfiguration.cs
      Api/
        PreferenceEndpoints.cs
      ModuleServiceRegistrar.cs
      Vetolib.Preferences.csproj
```

### Granularite : User + Clinic (hierarchique)

Les preferences fonctionnent en cascade :

```
Clinic Default (admin definit)
    |
    v
User Override (l'utilisateur surcharge)
    |
    v
Effective Preference = User Override ?? Clinic Default ?? System Default
```

**Justification** :
- L'admin de clinique doit pouvoir definir des defaults (ex: "dans notre clinique, le triage AI est desactive par defaut")
- Chaque utilisateur peut surcharger les defaults de sa clinique
- Le systeme a des defaults raisonnables si ni la clinique ni l'user n'ont configure

---

## 2. Data model propose

### 2.1 Enumerations (dans Contracts)

```csharp
// Vetolib.Preferences.Contracts/PreferenceCategory.cs
public enum PreferenceCategory
{
    Notifications,    // Email, Push, SMS opt-in/out
    Analytics,        // PostHog tracking, usage analytics
    AIFeatures,       // Triage, no-show prediction, drug interactions
    Communication,    // Messaging hours, preferred channels
    Privacy           // Data sharing, marketing consent
}

// Vetolib.Preferences.Contracts/PreferenceKey.cs
public enum PreferenceKey
{
    // --- Notifications ---
    NotificationEmail,              // bool: receive email notifications
    NotificationPush,               // bool: receive push notifications
    NotificationSms,                // bool: receive SMS notifications
    NotificationAppointmentReminder,// bool: receive appointment reminders
    NotificationInvoice,            // bool: receive invoice notifications

    // --- Analytics ---
    AnalyticsPosthog,               // bool: allow PostHog tracking
    AnalyticsUsageData,             // bool: allow anonymous usage data collection

    // --- AI Features ---
    AITriage,                       // bool: enable AI triage suggestions
    AINoShowPrediction,             // bool: enable no-show prediction
    AIDrugInteractions,             // bool: enable drug interaction checks
    AIMessaging,                    // bool: enable AI message triage/suggestions

    // --- Communication ---
    CommunicationQuietHoursStart,   // string: "22:00" (HH:mm)
    CommunicationQuietHoursEnd,     // string: "07:00" (HH:mm)
    CommunicationPreferredChannel,  // string: "Email" | "Sms" | "Push"
    CommunicationLanguage,          // string: "en" | "ar"

    // --- Privacy ---
    PrivacyDataSharing,             // bool: allow cross-clinic data sharing
    PrivacyMarketingEmails          // bool: allow marketing communications
}
```

### 2.2 Entites domain (internal)

```csharp
// User-level preference override
internal class UserPreference : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid UserId { get; private set; }
    public PreferenceCategory Category { get; private set; }
    public PreferenceKey Key { get; private set; }
    public string Value { get; private set; } = string.Empty;  // "true", "false", "22:00", "Email"

    private UserPreference() { }

    public static Result<UserPreference> Create(
        Guid clinicId, Guid userId,
        PreferenceCategory category, PreferenceKey key, string value)
    {
        if (clinicId == Guid.Empty)
            return Result<UserPreference>.Invalid(
                new ValidationError(nameof(clinicId), "ClinicId is required"));
        if (userId == Guid.Empty)
            return Result<UserPreference>.Invalid(
                new ValidationError(nameof(userId), "UserId is required"));
        if (string.IsNullOrWhiteSpace(value))
            return Result<UserPreference>.Invalid(
                new ValidationError(nameof(value), "Value is required"));

        return new UserPreference
        {
            ClinicId = clinicId,
            UserId = userId,
            Category = category,
            Key = key,
            Value = value
        };
    }

    public Result Update(string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
            return Result.Invalid(
                new ValidationError(nameof(newValue), "Value is required"));

        Value = newValue;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
```

```csharp
// Clinic-level defaults (set by Admin)
internal class ClinicPreferenceDefault : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public PreferenceCategory Category { get; private set; }
    public PreferenceKey Key { get; private set; }
    public string Value { get; private set; } = string.Empty;

    // Same factory pattern as UserPreference
}
```

```csharp
// Audit trail for consent changes (IMMUTABLE -- append-only)
internal class ConsentAuditEntry : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid UserId { get; private set; }
    public PreferenceCategory Category { get; private set; }
    public PreferenceKey Key { get; private set; }
    public string PreviousValue { get; private set; } = string.Empty;
    public string NewValue { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty; // "user_action", "admin_default", "api", "cookie_banner"
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    // Factory only -- no Update method (immutable)
}
```

### 2.3 Schema SQL

```sql
CREATE SCHEMA preferences;

CREATE TABLE preferences.user_preferences (
    id UUID PRIMARY KEY,
    clinic_id UUID NOT NULL,
    user_id UUID NOT NULL,
    category VARCHAR(50) NOT NULL,
    key VARCHAR(100) NOT NULL,
    value VARCHAR(500) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (clinic_id, user_id, key)
);

CREATE INDEX ix_user_preferences_user ON preferences.user_preferences (user_id, clinic_id);

CREATE TABLE preferences.clinic_preference_defaults (
    id UUID PRIMARY KEY,
    clinic_id UUID NOT NULL,
    category VARCHAR(50) NOT NULL,
    key VARCHAR(100) NOT NULL,
    value VARCHAR(500) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (clinic_id, key)
);

CREATE TABLE preferences.consent_audit (
    id UUID PRIMARY KEY,
    clinic_id UUID NOT NULL,
    user_id UUID NOT NULL,
    category VARCHAR(50) NOT NULL,
    key VARCHAR(100) NOT NULL,
    previous_value VARCHAR(500) NOT NULL,
    new_value VARCHAR(500) NOT NULL,
    source VARCHAR(50) NOT NULL,
    ip_address VARCHAR(45) NULL,
    user_agent VARCHAR(500) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_consent_audit_user ON preferences.consent_audit (user_id, created_at DESC);
```

### 2.4 System defaults (code, pas DB)

```csharp
internal static class SystemDefaults
{
    public static readonly Dictionary<PreferenceKey, string> Values = new()
    {
        // Notifications: opt-in par defaut (UAE: pas de reglementation opt-in stricte)
        [PreferenceKey.NotificationEmail] = "true",
        [PreferenceKey.NotificationPush] = "true",
        [PreferenceKey.NotificationSms] = "false",  // SMS = cout, opt-in explicite
        [PreferenceKey.NotificationAppointmentReminder] = "true",
        [PreferenceKey.NotificationInvoice] = "true",

        // Analytics: opt-OUT par defaut (GDPR-ready pour expansion EU)
        [PreferenceKey.AnalyticsPosthog] = "false",
        [PreferenceKey.AnalyticsUsageData] = "false",

        // AI: opt-in par defaut (valeur ajoutee produit)
        [PreferenceKey.AITriage] = "true",
        [PreferenceKey.AINoShowPrediction] = "true",
        [PreferenceKey.AIDrugInteractions] = "true",
        [PreferenceKey.AIMessaging] = "true",

        // Communication
        [PreferenceKey.CommunicationQuietHoursStart] = "22:00",
        [PreferenceKey.CommunicationQuietHoursEnd] = "07:00",
        [PreferenceKey.CommunicationPreferredChannel] = "Email",
        [PreferenceKey.CommunicationLanguage] = "en",

        // Privacy: opt-out par defaut (privacy-first)
        [PreferenceKey.PrivacyDataSharing] = "false",
        [PreferenceKey.PrivacyMarketingEmails] = "false",
    };
}
```

---

## 3. API endpoints

### 3.1 User preferences

```
GET    /api/preferences                           -> PreferenceDto[]
  Returns effective preferences for the authenticated user (merged: system < clinic < user)
  Auth: RequireAuthorization (any authenticated user)

GET    /api/preferences/{category}                 -> PreferenceDto[]
  Returns effective preferences for a specific category
  Auth: RequireAuthorization

PUT    /api/preferences                            -> Result (204)
  Bulk update user preferences
  Auth: RequireAuthorization
  Request: { preferences: [{ key: string, value: string }] }

PUT    /api/preferences/{key}                      -> Result (204)
  Update a single preference
  Auth: RequireAuthorization
  Request: { value: string }

POST   /api/preferences/consent/revoke             -> Result (204)
  Revoke consent for a category (sets all keys in category to "false")
  Auth: RequireAuthorization
  Request: { category: string }
```

### 3.2 Clinic defaults (Admin only)

```
GET    /api/clinics/preferences                    -> ClinicPreferenceDefaultDto[]
  Returns clinic-level defaults
  Auth: AdminOnly

PUT    /api/clinics/preferences                    -> Result (204)
  Bulk update clinic defaults
  Auth: AdminOnly
  Request: { defaults: [{ key: string, value: string }] }
```

### 3.3 Consent audit (Admin only)

```
GET    /api/preferences/audit                      -> ConsentAuditDto[] (paged)
  Returns consent change history for the clinic
  Auth: AdminOnly
  Query: ?userId={id}&category={cat}&from={date}&to={date}&page=1&pageSize=20
```

### 3.4 Cross-module query (internal, via IPreferenceChecker)

```csharp
// Vetolib.Preferences.Contracts/IPreferenceChecker.cs
public interface IPreferenceChecker
{
    /// <summary>
    /// Checks if a specific preference is enabled for a user.
    /// Resolves the effective value: user override > clinic default > system default.
    /// </summary>
    Task<bool> IsEnabledAsync(Guid userId, Guid clinicId, PreferenceKey key, CancellationToken ct);

    /// <summary>
    /// Gets the effective string value of a preference.
    /// </summary>
    Task<string> GetValueAsync(Guid userId, Guid clinicId, PreferenceKey key, CancellationToken ct);
}
```

Cette interface est dans `.Contracts` et implementee dans le runtime `Vetolib.Preferences`. Les autres modules (Notifications, AI) referencent uniquement `.Contracts` et injectent `IPreferenceChecker` via DI.

### 3.5 Integration event (MassTransit)

```csharp
// Vetolib.Preferences.Contracts/PreferenceChangedIntegrationEvent.cs
public record PreferenceChangedIntegrationEvent(
    Guid ClinicId,
    Guid UserId,
    PreferenceCategory Category,
    PreferenceKey Key,
    string OldValue,
    string NewValue,
    DateTime ChangedAt);
```

Les modules consommateurs peuvent reagir au changement. Exemples :
- Notifications : desabonner l'utilisateur d'un channel quand `NotificationEmail` passe a `false`
- Frontend (via SSE ou polling) : mettre a jour l'UI quand un admin change les defaults

### 3.6 DTOs

```csharp
// Vetolib.Preferences.Contracts/PreferenceDto.cs
public record PreferenceDto(
    PreferenceCategory Category,
    PreferenceKey Key,
    string Value,
    string Source);  // "system_default", "clinic_default", "user_override"

// Vetolib.Preferences.Contracts/PreferenceCategoryDto.cs
public record PreferenceCategoryDto(
    PreferenceCategory Category,
    string DisplayName,
    string Description,
    PreferenceDto[] Preferences);

// Vetolib.Preferences.Contracts/ConsentAuditDto.cs
public record ConsentAuditDto(
    Guid Id,
    Guid UserId,
    PreferenceCategory Category,
    PreferenceKey Key,
    string PreviousValue,
    string NewValue,
    string Source,
    DateTime CreatedAt);
```

---

## 4. Impact sur les modules existants

### 4.1 Notifications

**Impact : MOYEN**

Le module Notifications doit consulter les preferences avant d'envoyer. Actuellement, les consumers (ex: `AppointmentReminderConsumer`) envoient sans verifier l'opt-in.

Modification requise :

```csharp
// Avant (actuel)
public async Task Consume(ConsumeContext<AppointmentReminderDueIntegrationEvent> context)
{
    // Envoie directement -- pas de check opt-in
    await _emailSender.SendAsync(message, context.CancellationToken);
}

// Apres (avec preferences)
public async Task Consume(ConsumeContext<AppointmentReminderDueIntegrationEvent> context)
{
    var isOptedIn = await _preferenceChecker.IsEnabledAsync(
        evt.UserId, evt.ClinicId,
        PreferenceKey.NotificationAppointmentReminder,
        context.CancellationToken);

    if (!isOptedIn)
    {
        _logger.LogInformation("User {UserId} opted out of appointment reminders", evt.UserId);
        return;
    }

    await _emailSender.SendAsync(message, context.CancellationToken);
}
```

Fichiers concernes :
- `Vetolib.Notifications/Consumers/AppointmentReminderConsumer.cs`
- `Vetolib.Notifications/Consumers/UserInvitedConsumer.cs`
- `Vetolib.Notifications/Consumers/InvoiceSentConsumer.cs`
- `Vetolib.Notifications.csproj` -- ajouter reference `Vetolib.Preferences.Contracts`

**Probleme a resoudre** : les events actuels ne portent pas toujours un `UserId`. Par exemple, `AppointmentReminderDueIntegrationEvent` a un `OwnerEmail` mais pas un `UserId` (le proprietaire n'est pas forcement un User du systeme). Il faudra trancher :
- Option A : les preferences ne s'appliquent qu'aux Users (staff clinique), pas aux owners (proprietaires d'animaux)
- Option B : creer un concept de "contact preferences" pour les owners (sans User account)

**Recommandation** : Option A pour la v1. Les owners ne sont pas des Users dans le systeme actuel. Les preferences de notification pour les owners (rappels RDV) sont un sujet separe qui necessite un modele "contact" ou "owner preferences" -- a traiter dans une iteration ulterieure.

### 4.2 AI Module

**Impact : FAIBLE**

Le module AI doit verifier si les features AI sont activees avant d'executer un triage ou une prediction.

```csharp
// Dans TriageSymptomsHandler
var isTriageEnabled = await _preferenceChecker.IsEnabledAsync(
    command.UserId, clinicId,
    PreferenceKey.AITriage, ct);

if (!isTriageEnabled)
    return Result<TriageSuggestionDto>.Error("AI_TRIAGE_DISABLED");
```

Fichiers concernes :
- `Vetolib.AI/Application/Commands/TriageSymptoms/TriageSymptomsHandler.cs`
- `Vetolib.AI/Application/Commands/PredictNoShow/PredictNoShowHandler.cs`
- `Vetolib.AI.csproj` -- ajouter reference `Vetolib.Preferences.Contracts`

**Note** : les preferences AI fonctionnent a deux niveaux :
- Clinic-level : l'admin peut desactiver le triage AI pour toute la clinique (clinic default `AITriage = false`)
- User-level : un vet peut desactiver le no-show prediction pour lui-meme

### 4.3 Analytics (Frontend)

**Impact : MOYEN**

Le frontend utilise actuellement `gtag` (Google Analytics) via `src/lib/analytics.ts`. Pas de PostHog installe. L'integration analytics doit respecter le consent.

Modification requise :

```typescript
// src/lib/analytics.ts -- apres integration preferences
import { getPreference } from '@/lib/api/preferences';

let analyticsInitialized = false;

export async function initAnalytics(): Promise<void> {
  const isOptedIn = await getPreference('AnalyticsPosthog');
  if (isOptedIn === 'true' && !analyticsInitialized) {
    // Initialize PostHog / gtag
    analyticsInitialized = true;
  }
}

export function trackEvent(name: string, properties?: Record<string, string>): void {
  if (!analyticsInitialized) return; // Silent no-op si pas de consent
  // ... tracking code
}
```

### 4.4 Auth

**Impact : NEGLIGEABLE**

Auth n'est pas modifie structurellement. La seule interaction est que les preferences sont liees par `UserId` (Guid opaque). Pas de reference croisee au runtime Auth.

Le JWT pourrait optionnellement porter un claim leger (ex: `prefs_version: 3`) pour permettre au frontend de cacher les preferences localement et invalider le cache quand la version change. Mais ce n'est pas obligatoire pour la v1.

### 4.5 Messaging

**Impact : FAIBLE**

Le module Messaging (Phase 4) consultera `CommunicationQuietHoursStart/End` pour savoir si un message doit etre envoye immediatement ou mis en queue pour livraison ulterieure.

```csharp
var quietStart = await _preferenceChecker.GetValueAsync(
    userId, clinicId, PreferenceKey.CommunicationQuietHoursStart, ct);
var quietEnd = await _preferenceChecker.GetValueAsync(
    userId, clinicId, PreferenceKey.CommunicationQuietHoursEnd, ct);

// Si l'heure actuelle est dans la plage quiet hours -> queue pour plus tard
```

### 4.6 Recapitulatif des impacts

| Module | Impact | Modification | Ref Contracts |
|---|---|---|---|
| Notifications | Moyen | Check opt-in avant envoi dans chaque consumer | Oui |
| AI | Faible | Check feature flag avant execution dans handlers | Oui |
| Analytics (FE) | Moyen | Conditionner init PostHog/gtag au consent | Via API |
| Auth | Negligeable | Aucune modification | Non |
| Messaging | Faible | Respect quiet hours (quand le module existera) | Oui |
| Agenda | Aucun | - | Non |
| Billing | Aucun | - | Non |
| MedicalRecords | Aucun | - | Non |

---

## 5. Conformite reglementaire

### 5.1 UAE PDPL (Personal Data Protection Law -- Federal Decree-Law No. 45/2021)

Entree en vigueur : janvier 2022, mise en application progressive.

| Obligation | Statut Vetolib | Impact Preferences module |
|---|---|---|
| Consentement explicite pour traitement donnees personnelles | Partiellement couvert (login = consent implicite pour le service) | Le module formalise le consentement pour le tracking analytics et les communications marketing |
| Droit d'acces aux donnees | A implementer | `GET /api/preferences` + `GET /api/preferences/audit` couvrent les preferences |
| Droit de retrait du consentement | A implementer | `POST /api/preferences/consent/revoke` |
| Registre des traitements | L'audit trail (`consent_audit`) documente les changements | ConsentAuditEntry couvre ce besoin |
| Notification en cas de breach | Hors scope preferences | - |
| DPO (Data Protection Officer) | Organisationnel, pas technique | - |

**Specifite UAE** : la PDPL est moins stricte que le GDPR sur le consentement pre-collecte. Le modele opt-OUT par defaut pour analytics est conservateur mais prepare l'expansion EU.

### 5.2 GDPR (si expansion Europe)

| Obligation GDPR | Implementation dans Preferences |
|---|---|
| Art. 6 -- Base legale du traitement | Consentement explicite pour analytics et marketing. Interet legitime pour les fonctionnalites core (notifications RDV) |
| Art. 7 -- Conditions du consentement | Consent banner frontend, granulaire par categorie, revocable |
| Art. 13 -- Information a la collecte | Page "Privacy" avec description de chaque preference |
| Art. 17 -- Droit a l'effacement | Supprimer UserPreferences + anonymiser ConsentAudit quand un user demande l'effacement |
| Art. 20 -- Portabilite | `GET /api/preferences` retourne toutes les prefs en JSON (format machine-readable) |
| Art. 21 -- Droit d'opposition | `POST /api/preferences/consent/revoke` par categorie |
| Art. 30 -- Registre des traitements | `consent_audit` table = registre automatique |

**Decision cle pour GDPR** : les system defaults de `AnalyticsPosthog` et `AnalyticsUsageData` sont `false` (opt-out par defaut). C'est le seul choix legal en EU. Les defaults actuels (UAE-first) sont deja configures ainsi.

### 5.3 CCPA (si expansion US -- Californie)

| Obligation CCPA | Implementation dans Preferences |
|---|---|
| Right to know | `GET /api/preferences` + audit trail |
| Right to delete | Meme que GDPR art. 17 |
| Right to opt-out of sale | `PrivacyDataSharing = false` (Vetolib ne vend pas de donnees, mais le mecanisme est pret) |
| Non-discrimination | L'application fonctionne identiquement quel que soit le choix de preferences |
| Notice at collection | Cookie consent banner + preference page |

### 5.4 Consent audit trail -- implementation

Chaque modification de preference genere un `ConsentAuditEntry` immutable :

```
UserPreference changed:
  userId: 123
  key: AnalyticsPosthog
  previousValue: "false"
  newValue: "true"
  source: "cookie_banner"
  ipAddress: "192.168.1.1"
  userAgent: "Mozilla/5.0..."
  createdAt: 2026-03-10T14:30:00Z
```

L'audit trail n'est jamais supprime (meme si l'utilisateur demande l'effacement -- le consentement log est une obligation legale). Les champs `userId` et `ipAddress` sont anonymises apres effacement du compte (remplace par un hash).

### 5.5 Retention des donnees

| Donnee | Retention | Justification |
|---|---|---|
| UserPreference | Duree de vie du compte | Supprime avec le compte |
| ClinicPreferenceDefault | Duree de vie de la clinique | Supprime avec la clinique |
| ConsentAuditEntry | 5 ans apres derniere modification | Obligation legale UAE PDPL + GDPR |

---

## 6. Frontend

### 6.1 Page Settings/Preferences

Nouvelle page : `/[locale]/(dashboard)/settings/preferences/page.tsx`

Structure UI :

```
Settings > Preferences
+-----------------------------------------------+
| Notifications                            [v]   |
|  +------------------------------------------+ |
|  | Email notifications        [toggle: ON]  | |
|  | Push notifications         [toggle: ON]  | |
|  | SMS notifications          [toggle: OFF] | |
|  | Appointment reminders      [toggle: ON]  | |
|  | Invoice notifications      [toggle: ON]  | |
|  +------------------------------------------+ |
|                                                |
| AI Features                             [v]   |
|  +------------------------------------------+ |
|  | AI Triage suggestions      [toggle: ON]  | |
|  | No-show predictions        [toggle: ON]  | |
|  | Drug interaction checks    [toggle: ON]  | |
|  | AI messaging assistance    [toggle: ON]  | |
|  +------------------------------------------+ |
|                                                |
| Privacy & Analytics                      [v]   |
|  +------------------------------------------+ |
|  | Analytics tracking         [toggle: OFF] | |
|  | Usage data collection      [toggle: OFF] | |
|  | Cross-clinic data sharing  [toggle: OFF] | |
|  | Marketing emails           [toggle: OFF] | |
|  +------------------------------------------+ |
|                                                |
| Communication                            [v]   |
|  +------------------------------------------+ |
|  | Quiet hours: [22:00] to [07:00]          | |
|  | Preferred channel: [Email v]             | |
|  | Language: [English v]                    | |
|  +------------------------------------------+ |
|                                                |
| [Revoke all analytics consent]  [Save changes] |
+-----------------------------------------------+
```

Chaque toggle a un `data-testid` :
- `data-testid="pref-toggle-NotificationEmail"`
- `data-testid="pref-toggle-AnalyticsPosthog"`
- etc.

### 6.2 Cookie consent banner

Composant : `src/components/features/consent/CookieBanner.tsx`

Apparait au premier chargement si `AnalyticsPosthog` n'a pas encore de valeur user_override.

```
+-----------------------------------------------+
| We use cookies and analytics to improve your   |
| experience. You can manage your preferences    |
| at any time in Settings.                       |
|                                                |
| [Accept all]  [Reject all]  [Manage settings]  |
+-----------------------------------------------+
```

- "Accept all" : met `AnalyticsPosthog = true`, `AnalyticsUsageData = true`
- "Reject all" : met `AnalyticsPosthog = false`, `AnalyticsUsageData = false`
- "Manage settings" : redirige vers `/settings/preferences`

`data-testid` :
- `data-testid="cookie-banner"`
- `data-testid="cookie-accept-all"`
- `data-testid="cookie-reject-all"`
- `data-testid="cookie-manage"`

### 6.3 Integration PostHog

```typescript
// src/lib/analytics.ts (updated)
import posthog from 'posthog-js';

let initialized = false;

export async function initAnalytics(isOptedIn: boolean): void {
  if (isOptedIn && !initialized) {
    posthog.init(process.env.NEXT_PUBLIC_POSTHOG_KEY!, {
      api_host: process.env.NEXT_PUBLIC_POSTHOG_HOST,
      loaded: (ph) => {
        if (process.env.NODE_ENV === 'development') ph.debug();
      },
    });
    initialized = true;
  } else if (!isOptedIn && initialized) {
    posthog.opt_out_capturing();
    initialized = false;
  }
}
```

### 6.4 Clinic defaults page (Admin only)

Nouvelle page : `/[locale]/(dashboard)/settings/clinic-preferences/page.tsx`

Visible uniquement par les Admins. Permet de definir les defaults de la clinique.

### 6.5 MSW handlers (dev)

```typescript
// src/mocks/handlers/preferences.ts
import { http, HttpResponse } from 'msw';

const userPreferences: Record<string, string> = {};
const clinicDefaults: Record<string, string> = {};

export const preferencesHandlers = [
  http.get('/api/preferences', () => {
    // Return merged preferences (system < clinic < user)
    return HttpResponse.json([...]);
  }),
  http.put('/api/preferences/:key', async ({ params, request }) => {
    const { value } = await request.json();
    userPreferences[params.key as string] = value;
    return new HttpResponse(null, { status: 204 });
  }),
  // ... etc
];
```

---

## 7. Tasks techniques decoupees

### Phase 1 : Scaffolding module (S)

```markdown
# todo-back-preferences-001 -- Scaffold module Preferences
- Creer Vetolib.Preferences.Contracts/ avec enums, DTOs, IPreferenceChecker
- Creer Vetolib.Preferences/ avec structure Clean Architecture
- PreferencesDbContext + migrations (schema "preferences")
- ModuleServiceRegistrar.cs
- Enregistrer dans Vetolib.Api/Program.cs
- Tests unit : domain factories (Create, Update)
Critere : dotnet build passe, migration appliquee, tests verts
```

### Phase 2 : API CRUD (M)

```markdown
# todo-back-preferences-002 -- API Preferences CRUD
- GET /api/preferences (merged effective prefs)
- PUT /api/preferences/{key}
- PUT /api/preferences (bulk)
- GET /api/clinics/preferences (AdminOnly)
- PUT /api/clinics/preferences (AdminOnly)
- FluentValidation sur les inputs
- Tests BDD : feature file Preferences.feature
Critere : tous les endpoints fonctionnels, tests BDD verts
```

### Phase 3 : Consent et audit (M)

```markdown
# todo-back-preferences-003 -- Consent audit trail
- ConsentAuditEntry entity + configuration
- POST /api/preferences/consent/revoke
- GET /api/preferences/audit (paged, AdminOnly)
- Chaque modification de preference cree un ConsentAuditEntry
- PreferenceChangedIntegrationEvent publie via MassTransit
- Tests BDD : scenarios consent revoke + audit trail
Critere : audit trail complet, event publie, tests verts
```

### Phase 4 : IPreferenceChecker + integration Notifications (M)

```markdown
# todo-back-preferences-004 -- Integration cross-module
- Implementer PreferenceChecker (IPreferenceChecker)
- Ajouter ref Vetolib.Preferences.Contracts dans Vetolib.Notifications
- Modifier AppointmentReminderConsumer, UserInvitedConsumer, InvoiceSentConsumer
- Check opt-in avant chaque envoi
- Tests BDD : scenario "user opted out does not receive email"
Critere : consumers respectent les preferences, tests verts
```

### Phase 5 : Frontend preferences page (M)

```markdown
# todo-front-preferences-001 -- Settings Preferences page
[MSW: oui]
- Page /settings/preferences avec toggles par categorie
- Hooks: usePreferences(), useUpdatePreference()
- MSW handlers
- data-testid sur tous les toggles
- Tests Playwright : toggle on/off, save, reload
Critere : page fonctionnelle, tests Playwright verts
```

### Phase 6 : Cookie consent banner (S)

```markdown
# todo-front-preferences-002 -- Cookie consent banner
[MSW: oui]
- Composant CookieBanner.tsx
- Logique : afficher si AnalyticsPosthog pas encore configure
- Accept all / Reject all / Manage settings
- Persistance locale (localStorage) + sync API au login
- data-testid
- Tests Playwright
Critere : banner apparait, choix persiste, tests Playwright verts
```

### Phase 7 : Integration AI module (S)

```markdown
# todo-back-preferences-005 -- Integration AI preferences
- Ajouter ref Vetolib.Preferences.Contracts dans Vetolib.AI
- Check AITriage, AINoShowPrediction, AIDrugInteractions avant execution
- Retourner Result.Error("AI_FEATURE_DISABLED") si opt-out
- Tests BDD
Critere : handlers AI respectent les preferences
```

### Phase 8 : Clinic defaults admin page (S)

```markdown
# todo-front-preferences-003 -- Admin clinic defaults page
[MSW: oui]
- Page /settings/clinic-preferences (AdminOnly)
- Toggles pour definir les defaults clinique
- data-testid
- Tests Playwright
Critere : page admin fonctionnelle
```

### Phase 9 : Wire (S)

```markdown
# todo-wire-preferences-001 -- Wire frontend/backend
- Supprimer MSW handlers preferences
- Playwright contre vrai backend
- Verifier consent audit trail dans la DB
Critere : tous les tests Playwright verts contre le backend reel
```

### Estimation totale

| Phase | Taille | Dependances |
|---|---|---|
| 1 - Scaffold | S | Aucune |
| 2 - API CRUD | M | Phase 1 |
| 3 - Consent/Audit | M | Phase 2 |
| 4 - Integration Notifications | M | Phase 3 |
| 5 - Frontend preferences | M | Phase 2 (MSW) |
| 6 - Cookie banner | S | Phase 5 |
| 7 - Integration AI | S | Phase 3, module AI existant |
| 8 - Admin clinic defaults | S | Phase 5 |
| 9 - Wire | S | Phases 1-8 |
| **Total** | **L (Large)** | ~3-4 semaines |

---

## 8. Risques

| # | Risque | Probabilite | Impact | Mitigation |
|---|---|---|---|---|
| R1 | Performance : `IPreferenceChecker` appele a chaque notification = N+1 queries | Elevee | Moyen | Cache in-memory (IMemoryCache) avec invalidation via PreferenceChangedIntegrationEvent. TTL 5 min. |
| R2 | Owner vs User : les proprietaires d'animaux n'ont pas de compte User | Certaine (v1) | Moyen | v1 = preferences staff only. v2 = modele "contact preferences" pour owners (sujet separe) |
| R3 | Incoherence temporelle : preference changee pendant un envoi en cours | Faible | Faible | Acceptable. Le check est "at send time". Pas de transaction distribuee. |
| R4 | Explosion du nombre de preferences si chaque module ajoute les siennes | Moyenne | Moyen | L'enum PreferenceKey est centralise dans Contracts. Ajout = PR review obligatoire. |
| R5 | Cookie banner ne bloque pas le tracking avant consentement (GDPR strict) | Moyenne si expansion EU | Eleve | PostHog initialise UNIQUEMENT apres consentement explicite (pas de tracking "anonyme" avant) |
| R6 | Admin ecrase les preferences individuelles | Faible | Moyen | Les clinic defaults ne remplacent pas les user overrides. Cascade claire : system < clinic < user. |
| R7 | Migration Shared/ necessaire | Nulle | - | Le module Preferences est autonome. Aucune modification de Shared/Kernel ou Shared/Infrastructure. |
| R8 | Latence ajoutee par le check IPreferenceChecker dans les consumers Notifications | Moyenne | Faible | Cache in-memory (R1). En cas de miss : 1 query simple sur index unique (user_id, key). < 1ms. |

---

## Annexe : Decisions de design explicites

### Pourquoi des enums et pas des strings libres pour les keys ?

Les enums (`PreferenceKey`, `PreferenceCategory`) garantissent :
1. Pas de typo (`"NotificaitonEmail"` impossible)
2. IntelliSense et refactoring
3. L'ajout d'une preference est un changement de code (PR review), pas une insertion en DB
4. Le frontend peut generer les forms automatiquement a partir de l'enum

Trade-off : ajouter une preference necessite un changement dans Contracts + redeploiement. Acceptable car les preferences ne changent pas quotidiennement.

### Pourquoi pas un key-value store (Redis) ?

Les preferences sont relationnelles (user + clinic + category + key). Le volume est faible (20 keys x N users). PostgreSQL avec cache in-memory est largement suffisant. Redis ajouterait une dependance infra sans benefice mesurable.

### Pourquoi pas EAV (Entity-Attribute-Value) pur ?

Le modele propose est un EAV contraint (enum keys). Un EAV pur (string keys) serait plus flexible mais perdrait la type-safety et la documentation inline. La flexibilite n'est pas necessaire ici -- les categories de preferences sont connues a l'avance.
