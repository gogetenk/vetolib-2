# todo-back-preferences-domain-001.md -- Scaffold module Vetolib.Preferences

**Module** : Preferences (nouveau)
**Dependances** : aucune
**Priorite** : MOYENNE
**Skills a lire** : `ardalis-result`, `ardalis-modular-monolith`, `multitenant-efcore`
MODIF_GELE: autorise (AppHost/Program.cs, Vetolib.Api/Program.cs)

---

## Objectif

Creer le module Preferences avec la structure 2 assemblies (Contracts + runtime).
Ce module porte le domaine des preferences utilisateur/clinique et du consentement.

## Spec de reference

- `docs/PREFERENCES-STUDY.md` (etude architecturale complete)
- `docs/PO-POST-MVP-DECISIONS.md` section 2 (decisions PO)

## Decisions PO a respecter

- Drug interaction alerts (`AIDrugInteractions`) : **NOT configurable, ALWAYS ON**. Le `PreferenceKey` existe dans l'enum mais le handler doit refuser toute tentative de le passer a `false`.
- AI features (triage, no-show, messaging) : **per-clinic** (Admin only), pas per-user en v1.
- Analytics tracking : **per-clinic** (Admin opt-out).
- Notifications email : **per-user** (choix individuel).
- SMS/Push : **OFF par defaut** (pas encore implemente).

## Implementation

### Structure

```
Modules/Preferences/
  Vetolib.Preferences.Contracts/
    Vetolib.Preferences.Contracts.csproj
    PreferenceCategory.cs          (enum: Notifications, Analytics, AIFeatures, Communication, Privacy)
    PreferenceKey.cs               (enum: NotificationEmail, AITriage, AnalyticsPosthog, etc.)
    PreferenceDto.cs               (record: Category, Key, Value, Source)
    PreferenceCategoryDto.cs       (record: Category, DisplayName, Description, Preferences[])
    ConsentAuditDto.cs             (record: Id, UserId, Category, Key, PreviousValue, NewValue, Source, CreatedAt)
    IPreferenceChecker.cs          (interface cross-module)
    PreferenceChangedIntegrationEvent.cs  (record MassTransit event)
  Vetolib.Preferences/
    Vetolib.Preferences.csproj
    Application/
      Domain/
        UserPreference.cs           (BaseEntity, IMultiTenant)
        ClinicPreferenceDefault.cs  (BaseEntity, IMultiTenant)
        ConsentAuditEntry.cs        (BaseEntity, IMultiTenant -- immutable, append-only)
        SystemDefaults.cs           (static class -- defaults hardcodes en code)
      Services/
        PreferenceChecker.cs        (implements IPreferenceChecker)
    Infrastructure/
      PreferencesDbContext.cs       (herite MultiTenantDbContext, schema "preferences")
      UserPreferenceConfiguration.cs
      ClinicPreferenceDefaultConfiguration.cs
      ConsentAuditConfiguration.cs
      Migrations/                   (migration initiale)
    ModuleServiceRegistrar.cs       (seule classe public)
```

### Entites domain (Ardalis.Result obligatoire)

1. **UserPreference** : `Create(clinicId, userId, category, key, value) -> Result<UserPreference>`, `Update(newValue) -> Result`
2. **ClinicPreferenceDefault** : meme pattern factory
3. **ConsentAuditEntry** : factory `Create(...)` uniquement, pas de methode Update (immutable)

### Contraintes cles

- UserPreference a un index unique `(ClinicId, UserId, Key)`
- ClinicPreferenceDefault a un index unique `(ClinicId, Key)`
- ConsentAuditEntry : jamais de UPDATE ni DELETE (append-only)
- PreferencesDbContext utilise le schema PostgreSQL `preferences`
- Le global query filter multi-tenant s'applique automatiquement (heriter MultiTenantDbContext)

### SystemDefaults (hardcodes)

- Notifications : email ON, push OFF, sms OFF, appointment reminders ON, invoice ON
- Analytics : PostHog OFF, usage data OFF (GDPR-ready)
- AI : triage ON, no-show ON, drug interactions ON (non modifiable), messaging ON
- Communication : quiet hours 22:00-07:00, preferred channel Email, language EN
- Privacy : data sharing OFF, marketing OFF

### Fichiers geles modifies

- `AppHost/Program.cs` : reference projet Preferences
- `Vetolib.Api/Program.cs` : `AddPreferencesModule()` (pas de MapEndpoints dans cette tache)

### Tests unitaires

- Domain factories : `UserPreference.Create` avec cas valides et invalides
- Domain factories : `ClinicPreferenceDefault.Create` avec cas valides et invalides
- Domain : `ConsentAuditEntry.Create` -- immutabilite verifiee
- Domain : `UserPreference.Update` -- validation du newValue
- `SystemDefaults` : toutes les PreferenceKey ont un default

## Critere

```
[] Structure 2 assemblies creee (Contracts + runtime)
[] Enums PreferenceCategory et PreferenceKey dans Contracts
[] IPreferenceChecker interface dans Contracts
[] PreferenceChangedIntegrationEvent dans Contracts
[] DTOs (PreferenceDto, PreferenceCategoryDto, ConsentAuditDto) dans Contracts
[] Entites UserPreference, ClinicPreferenceDefault, ConsentAuditEntry avec Result<T>
[] PreferencesDbContext herite MultiTenantDbContext, schema "preferences"
[] EF Core configurations avec index uniques
[] Migration initiale generee
[] SystemDefaults couvre toutes les PreferenceKey
[] ModuleServiceRegistrar seule classe public du runtime
[] PreferenceChecker implemente IPreferenceChecker (resolution cascade : user > clinic > system)
[] Enregistre dans AppHost + Vetolib.Api
[] dotnet build passe
[] Tests unitaires domain verts
[] Renommer en done
```
