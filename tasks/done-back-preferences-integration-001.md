# todo-back-preferences-integration-001.md -- Integration cross-module IPreferenceChecker

**Module** : Preferences + Notifications + AI (integration)
**Dependances** : todo-back-preferences-api-001
**Priorite** : MOYENNE
**Skills a lire** : `ardalis-modular-monolith`, `ardalis-result`

---

## Objectif

Integrer `IPreferenceChecker` dans les modules Notifications et AI pour respecter les opt-in/opt-out avant execution.

## Spec de reference

- `docs/PREFERENCES-STUDY.md` section 4 (impact modules existants)
- `docs/PO-POST-MVP-DECISIONS.md` section 2.2 (granularity rules)

## Decisions PO a respecter

- Drug interaction checks : **jamais desactivables**. Meme si `IPreferenceChecker.IsEnabledAsync(AIDrugInteractions)` retourne false (ce qui ne devrait pas arriver grace a la validation API), le handler drug interactions doit **ignorer** cette preference et toujours executer.
- Preferences s'appliquent au **staff clinique** (Users) uniquement, pas aux owners (proprietaires d'animaux). Les owners n'ont pas de compte User dans le systeme actuel.
- Si un consumer Notifications recoit un event avec un `OwnerEmail` (pas un UserId), il **envoie toujours** -- pas de check preference.

## Implementation

### 1. Module Notifications

**Fichiers a modifier :**

- `Vetolib.Notifications/Consumers/AppointmentReminderConsumer.cs`
  - Injecter `IPreferenceChecker`
  - Avant envoi : `IsEnabledAsync(userId, clinicId, PreferenceKey.NotificationAppointmentReminder)`
  - Si l'event ne porte pas de `UserId` (owner-only) : envoyer sans check
  - Log `"User {UserId} opted out of appointment reminders"` si opt-out

- `Vetolib.Notifications/Consumers/UserInvitedConsumer.cs`
  - Injecter `IPreferenceChecker`
  - Check `PreferenceKey.NotificationEmail`
  - Note : l'invitation est envoyee a un user qui n'a peut-etre pas encore de preferences (nouveau user). Dans ce cas, utiliser les system defaults (opt-in par defaut).

- `Vetolib.Notifications/Consumers/InvoiceSentConsumer.cs`
  - Injecter `IPreferenceChecker`
  - Check `PreferenceKey.NotificationInvoice`
  - Meme logique owner vs user que AppointmentReminder

- `Vetolib.Notifications/Vetolib.Notifications.csproj`
  - Ajouter `<ProjectReference Include="...Vetolib.Preferences.Contracts.csproj" />`

### 2. Module AI

**Fichiers a modifier :**

- `Vetolib.AI/Application/Commands/TriageSymptoms/TriageSymptomsHandler.cs` (si existant)
  - Injecter `IPreferenceChecker`
  - Check `PreferenceKey.AITriage` au niveau clinic (utiliser le UserId du vet qui demande le triage)
  - Si desactive : `return Result<TriageSuggestionDto>.Error("AI triage is disabled for this clinic")`

- `Vetolib.AI/Application/Commands/PredictNoShow/PredictNoShowHandler.cs` (si existant)
  - Check `PreferenceKey.AINoShowPrediction`
  - Si desactive : `return Result.Error("No-show prediction is disabled for this clinic")`

- Drug interaction handler (s'il existe dans AI) :
  - **NE PAS ajouter de check IPreferenceChecker**. Le handler doit toujours executer. Ajouter un commentaire explicite :
  ```csharp
  // SAFETY: Drug interaction checks are ALWAYS enabled regardless of preferences.
  // See PO-POST-MVP-DECISIONS.md section 2.2: "Drug interaction alerts: ALWAYS ON"
  ```

- `Vetolib.AI/Vetolib.AI.csproj` (si le module AI existe)
  - Ajouter `<ProjectReference Include="...Vetolib.Preferences.Contracts.csproj" />`

### 3. Cache IPreferenceChecker

Le `PreferenceChecker` (dans le runtime Preferences) doit utiliser `IMemoryCache` :
- TTL : 5 minutes
- Cache key : `pref:{clinicId}:{userId}:{key}`
- Invalidation : via `PreferenceChangedIntegrationEvent` consumer dans le module Preferences lui-meme

Cela evite N+1 queries quand Notifications envoie un batch de messages.

### Tests BDD

Ajouter a `features/Preferences/Preferences.feature` :

```gherkin
  Scenario: Opted-out user does not receive appointment reminder email
    Given a user has set preference "NotificationAppointmentReminder" to "false"
    When an appointment reminder event is published for this user
    Then no email is sent to the user

  Scenario: Opted-in user receives appointment reminder email
    Given a user has default preferences (all notifications ON)
    When an appointment reminder event is published for this user
    Then an email is sent to the user

  Scenario: Owner without User account always receives reminder
    When an appointment reminder event is published for an owner email without UserId
    Then an email is sent to the owner email

  Scenario: AI triage respects clinic preference
    Given the clinic has set "AITriage" to "false"
    When a vet requests AI triage
    Then the response indicates AI triage is disabled

  Scenario: Drug interaction check ignores preferences
    Given the clinic has set "AIDrugInteractions" to "false" via direct DB manipulation
    When a prescription with drug interactions is checked
    Then the drug interaction alert fires regardless
```

### Tests unitaires

- `PreferenceChecker` : resolution cascade (user > clinic > system) avec mock DbContext
- `PreferenceChecker` : cache hit/miss verification
- Chaque consumer modifie : test avec `IPreferenceChecker` retournant true et false

## Critere

```
[] Vetolib.Notifications.csproj reference Vetolib.Preferences.Contracts
[] AppointmentReminderConsumer check opt-in avant envoi
[] UserInvitedConsumer check opt-in avant envoi
[] InvoiceSentConsumer check opt-in avant envoi
[] Consumers envoient toujours si pas de UserId (owner-only)
[] Vetolib.AI.csproj reference Vetolib.Preferences.Contracts (si module AI existe)
[] TriageSymptomsHandler check AITriage preference
[] PredictNoShowHandler check AINoShowPrediction preference
[] Drug interaction handler : aucun check preference, commentaire SAFETY
[] PreferenceChecker utilise IMemoryCache (TTL 5 min)
[] Invalidation cache via PreferenceChangedIntegrationEvent
[] Scenarios BDD ajoutes et verts
[] Tests unitaires consumers et PreferenceChecker verts
[] Renommer en done
```
