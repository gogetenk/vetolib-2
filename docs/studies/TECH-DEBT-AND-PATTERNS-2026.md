# Technical Debt Assessment & Advanced Patterns Study

**Date**: 2026-03-21
**Author**: Architect Agent (Opus)
**Scope**: Full backend codebase (`src/backend/Modules/`), 9 modules, ~611 source files

---

## PARTIE 1 -- DETTE TECHNIQUE

### 1.1 Code Smells

#### 1.1.1 French strings in production code (Severite: IMPORTANTE | Effort: faible)

`SlotScoringService.cs` (271 lines) contains 9 hardcoded French strings used as slot reasoning text. The product targets UAE (English-first), then France and Poland. These strings are user-facing and should be externalized or at minimum switched to English.

**Fichiers concernes:**
- `Modules/Agenda/Vetolib.Agenda/Application/Services/SlotScoringService.cs` (lines 131, 151, 156, 168, 187, 221, 233, 254, 256)
- `Modules/Agenda/Vetolib.Agenda/Application/Commands/CreateAppointment/CreateAppointmentHandler.cs` (line 25: "Impossible de creer un rendez-vous dans le passe")

#### 1.1.2 Large handlers with multiple responsibilities (Severite: MOYENNE | Effort: moyen)

Several handlers exceed the 50-line threshold and mix concerns:

| Handler | Lines | Issue |
|---------|-------|-------|
| `CheckInteractionsHandler.cs` | 236 | 5 private methods -- well-structured but could extract a `DrugInteractionChecker` domain service |
| `TriageSymptomsHandler.cs` | 234 | Mixes LLM orchestration, JSON parsing, persistence, DTO mapping |
| `ImportPatientsHandler.cs` | 226 | Batch processing with CSV parsing, validation, owner dedup -- classic "God handler" |
| `CreateOwnerConversationHandler.cs` | 158 | 8-step workflow (consent, limits, triage, business hours, events) in one method |
| `SendReplyHandler.cs` | 143 | Mixes reply creation, attachment handling, audit, event publishing, SSE broadcast |
| `SlotScoringService.cs` | 271 | Acceptable for a scoring algorithm but scoring criteria are not extensible |

#### 1.1.3 Duplicate domain methods (Severite: MINEURE | Effort: faible)

`Conversation.cs` has both `ChangeCategory()` and `UpdateCategory()` -- functionally near-identical except `UpdateCategory` resets `IsTriageUncertain`. This is confusing and error-prone. Should be unified into one method with an optional `resetUncertainty` parameter.

#### 1.1.4 Inconsistent void methods on domain entities (Severite: MINEURE | Effort: faible)

`Patient.cs` has `SetWeight()` and `AddOwner()` that return `void` instead of `Result`. This breaks the "Result everywhere" rule from CLAUDE.md. Compare with `Appointment.cs` where every mutation returns `Result`.

**Fichiers concernes:**
- `Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Domain/Patient.cs` (lines 84-92)

### 1.2 Couplage Inter-Modules

#### 1.2.1 Module dependency graph

```
Auth -> [Agenda.Contracts, Billing.Contracts, MedicalRecords.Contracts]
AI -> [Agenda.Contracts, MedicalRecords.Contracts, Preferences.Contracts]
Messaging -> [AI.Contracts, Agenda.Contracts, MedicalRecords.Contracts]
Notifications -> [Auth.Contracts, Agenda.Contracts, Billing.Contracts, Messaging.Contracts, Preferences.Contracts]
MedicalRecords -> [Stock.Contracts]
Stock -> [MedicalRecords.Contracts]
Billing -> (standalone)
Preferences -> (standalone)
Agenda -> [Auth.Contracts]
```

**Constats:**
- **Notifications** est un "star receiver" -- depend de 5 modules. Acceptable car c'est son role (reagir aux events de tous les modules). Pas de violation.
- **Auth** depend de 3 Contracts pour l'onboarding state checker. C'est borderline -- l'onboarding pourrait etre un module a part.
- **Stock <-> MedicalRecords** : dependance circulaire via Contracts. Stock reference MedicalRecords.Contracts ET MedicalRecords reference Stock.Contracts. Ce n'est pas une violation car c'est via Contracts (public), mais c'est un signe de couplage trop fort.
- **Aucune violation critique** : aucun module ne reference le runtime d'un autre module. Toutes les references passent par `.Contracts`.

#### 1.2.2 Messaging.Contracts reference MedicalRecords.Contracts (Severite: IMPORTANTE | Effort: moyen)

Le projet Contracts de Messaging reference MedicalRecords.Contracts. Les Contracts sont censes etre legers et auto-suffisants. Cela cree un transitive dependency : tout module qui reference Messaging.Contracts tire aussi MedicalRecords.Contracts.

### 1.3 EF Core

#### 1.3.1 IgnoreQueryFilters -- usage excessif dans Messaging (Severite: IMPORTANTE | Effort: moyen)

L'audit identifie **47 occurrences** de `IgnoreQueryFilters()` dans le codebase :
- **Legitimes (seeds/migrations/cross-tenant background services)** : DbInitializer, DrugCatalogSeedData, ReminderSchedulerService, EmergencyEscalationBackgroundService, PendingUploadCleanupService -- ces usages sont documentes et necessaires.
- **Auth (pre-tenant)** : Login, RegisterClinic, RefreshToken, ChangePassword, ClinicVetReader, SwitchClinic, SubscriptionChecker -- legitimes car ces endpoints operent avant ou en dehors du contexte tenant.
- **Messaging portal handlers** : 15+ occurrences dans CreateOwnerConversation, SendOwnerMessage, AcceptConsent, GetOwnerConversationById, ListOwnerConversations, ExportOwnerConversations, BusinessHoursChecker, MagicLinkEndpointFilter -- tous portent le commentaire "portal auth bypasses JWT tenant context".

Le pattern Messaging est problematique : au lieu de `IgnoreQueryFilters()` partout, il faudrait un `PortalContext` qui fournit le ClinicId via le magic link token, permettant au global query filter de fonctionner normalement.

#### 1.3.2 Queries sans AsNoTracking sur les lectures (Severite: MINEURE | Effort: faible)

Bien que 43 fichiers utilisent `AsNoTracking()`, de nombreuses queries de lecture ne l'utilisent pas. Les queries `List*`, `Get*ById`, analytics, et counts beneficieraient de `AsNoTracking()` pour eviter le tracking overhead.

#### 1.3.3 Pas de lazy loading accidentel (OK)

Aucune propriete `virtual ICollection<T>` detectee. Toutes les collections utilisent `private readonly List<T>` avec getter `IReadOnlyList<T>`. Pas de risque N+1 par lazy loading.

#### 1.3.4 Include sur commandes (Severite: MINEURE | Effort: faible)

Certains handlers de commandes utilisent `.Include()` pour charger des collections entieres alors qu'ils n'ont besoin que de l'aggregate root. Exemple : `ChangeConversationStatusHandler` fait `.Include(c => c.Messages)` mais n'utilise probablement pas tous les messages.

### 1.4 Domain Model

#### 1.4.1 Qualite globale : BONNE

Les entites du domaine suivent un pattern coherent :
- Constructeur prive + factory `Create()` retournant `Result<T>` : Appointment, Invoice, Conversation, Patient, User, StockItem, TriageResult
- Mutations via methodes nommees retournant `Result` : CheckIn, Complete, Cancel, UpdateStatus, ApplyMovement
- State machines explicites : Appointment (Scheduled -> CheckedIn -> InProgress -> Completed), Invoice (Draft -> Sent -> Paid), Conversation (Open -> InProgress -> Resolved -> Closed)
- Value Objects : InvoiceNumber (avec factory Result<InvoiceNumber>)

#### 1.4.2 Entites manquant de logique metier (Severite: MINEURE | Effort: moyen)

- `ClinicSchedule` : probablement anemique (a verifier)
- `ReminderConfig`, `ReminderLog`, `ClinicPreferenceDefault` : entites de configuration, acceptables comme anemiques
- `PatientOwner` : junction table, acceptable

#### 1.4.3 ToDto() dans les entites (Severite: MINEURE | Effort: moyen)

Toutes les entites contiennent une methode `ToDto()`. Cela couple le domaine aux DTOs du Contracts. Un mapper externe (ou AutoMapper/Mapster) serait plus propre, mais le pragmatisme du projet rend cela acceptable a ce stade.

### 1.5 Error Handling

#### 1.5.1 Result<T> -- adoption quasi-complete (OK)

Tous les handlers retournent `Result<T>` ou `Result`. Les domain factories retournent `Result<T>`. Les mutations retournent `Result`. C'est conforme a CLAUDE.md.

#### 1.5.2 Exceptions restantes (Severite: IMPORTANTE | Effort: moyen)

10 occurrences de `throw new` dans le code non-infrastructure :

| Fichier | Type | Justification |
|---------|------|---------------|
| `AuthModuleServiceRegistrar.cs` | `InvalidOperationException` | Config manquante -- **LEGITIME** (fail-fast au startup) |
| `JwtTokenService.cs` | `InvalidOperationException` | Config manquante -- **LEGITIME** |
| `MessagingModuleServiceRegistrar.cs` | `InvalidOperationException` | Config manquante -- **LEGITIME** |
| `AesTokenEncryptor.cs` | `ArgumentException` | Clef invalide -- **LEGITIME** (infra) |
| `LocalFileStorage.cs` (x2) | `InvalidOperationException` | Path traversal prevention -- **LEGITIME** |
| `AppointmentReminderConsumer.cs` | `InvalidOperationException` | **INTENTIONNEL** : pour MassTransit retry |
| `UserInvitedConsumer.cs` | `InvalidOperationException` | **INTENTIONNEL** : pour MassTransit retry |
| `SendMagicLinkConsumer.cs` | `InvalidOperationException` | **INTENTIONNEL** : pour MassTransit retry |
| `InvoiceSentConsumer.cs` | `InvalidOperationException` | **INTENTIONNEL** : pour MassTransit retry |

Les 4 consumers MassTransit qui throw sont intentionnels : MassTransit requiert une exception pour declencher son retry policy. Cependant, un pattern plus propre serait d'utiliser `throw new TransientException(...)` avec un type dedie, ou de configurer les retry filters sur le type de result.

#### 1.5.3 Aucun Controller -- CONFORME (OK)

Zero occurrence de `ControllerBase` ou `ApiController`.

### 1.6 Configuration -- Valeurs Hardcodees

| Valeur | Fichier | Impact |
|--------|---------|--------|
| `TaxRate = 0.05m` (UAE VAT 5%) | `InvoiceItem.cs:9` | Si expansion multi-pays, bloquant |
| `DueDate = +30 jours` | `Invoice.cs:106` | Payment terms non configurables |
| `Lock duration = 15 min` | `User.cs:114` | Security policy non configurable |
| `Max failed attempts = 5` | `User.cs:111` | Security policy non configurable |
| `Token expiry = 15 min` | `JwtTokenService.cs:50` | JWT lifetime hardcode |
| `Trial = 14 jours` | `Clinic.cs:36` | Business rule hardcodee |
| `Expiry threshold = 30 jours` | `StockItem.cs:101`, `GetStockAlertsHandler.cs:22`, `ListStockItemsHandler.cs:34` | Repete 3 fois |
| `Slot grid = 15 min` | `SlotScoringService.cs:76`, `CreateAppointmentHandler.cs:94` | Planning resolution hardcodee |
| `Availability grid = 30 min` | `GetAvailabilityHandler.cs:54` | Inconsistant avec slot grid (15 vs 30) |
| `DailyMessageLimit = 5` | `CreateOwnerConversationHandler.cs:18` | Rate limiting hardcode |
| `PendingUpload expiry = 24h` | `PendingUpload.cs:54` | Upload lifecycle hardcode |
| `Portal token expiry = 90 days` | `PortalEndpoints.cs:47` | Security parameter hardcode |
| `SLA estimates` (dict) | `CreateOwnerConversationHandler.cs:22-29` | Business rules hardcodees |
| `ActivePrescriptionWindowDays = 90` | `CheckInteractionsHandler.cs:64` | Au moins celui-ci est dans IConfiguration |

### 1.7 Tests -- Couverture des Handlers

Sur ~85 handlers identifies dans le codebase, **seulement 32 ont un test unitaire** correspondant. **53 handlers n'ont aucun TU**.

**Modules les moins couverts :**
- Messaging : 5/~20 handlers testes (25%)
- Notifications : 0/~6 handlers (0% -- uniquement des domain tests)
- Preferences : 0/~3 handlers (0% -- uniquement des domain tests)
- AI : 1/6 handlers (17%)

**Note :** Certains de ces handlers sont probablement couverts indirectement par les tests d'acceptance (TF) et d'integration (TI), mais le modele en sablier exige que les edge cases et error paths soient couverts en TU.

---

## PARTIE 2 -- SAGA PATTERN (MassTransit)

### 2.1 Usage actuel de MassTransit

Le projet utilise MassTransit principalement pour :
- **Consumers d'integration events** : `AppointmentReminderConsumer`, `UserInvitedConsumer`, `SendMagicLinkConsumer`, `InvoiceSentConsumer`, `OwnerMessageReplyConsumer`, `OutboundConversationCreatedConsumer`, `EmergencyMessageReceivedConsumer`, `EmergencyEscalationConsumer`, `PreferenceChangedConsumer`
- **Publishing** : `IPublishEndpoint` dans SendReplyHandler, CreateOwnerConversationHandler, CreateOutboundConversationHandler
- **Outbox** : migrations MassTransit outbox presentes dans Agenda et Auth (tables `OutboxMessage`, `OutboxState`, `InboxState`)
- **Background services** : `AppointmentReminderService`, `EmergencyEscalationBackgroundService`, `PendingUploadCleanupService`

C'est un pattern fire-and-forget simple. Aucun saga, aucune state machine.

### 2.2 Workflows candidats au Saga

#### 2.2.1 Booking complet

```
CreateAppointment -> ConfirmationEmail -> ScheduleReminder -> UpdateDashboardStats
```

**Analyse :** Ce workflow est actuellement gere par un mix de handler synchrone (creation) + events async (reminder scheduling). Le dashboard utilise des queries live, pas des stats pre-calculees. Un saga apporterait de la complexite sans benefice reel.

**Verdict : PAS DE SAGA NECESSAIRE.** Le pattern actuel (event-driven fire-and-forget) est suffisant.

#### 2.2.2 Invoice lifecycle

```
Draft -> Sent(email) -> PaymentReceived -> UpdateRecords
```

**Analyse :** La transition Invoice est deja gere par une state machine dans le domain (`Invoice.UpdateStatus()`). L'email d'envoi est publie via `InvoiceSentConsumer`. Le paiement n'est pas encore implemente (pas de Stripe/BNPL).

**Verdict : SAGA PERTINENT A TERME** si BNPL (Tabby) est integre. Un saga orchestrerait : CreatePaymentIntent -> WaitForCallback -> MarkPaid -> UpdateInvoice -> SendReceipt. Mais pas avant Phase 3+ du produit.

#### 2.2.3 Patient onboarding

```
CreateOwner -> CreatePatient -> CreateFirstAppointment -> SendWelcomeWhatsApp
```

**Analyse :** Ce workflow n'existe pas comme un flux unifie. La creation owner, patient, et appointment sont des actions independantes dans l'UI. Le "welcome WhatsApp" n'est pas implemente.

**Verdict : PAS DE SAGA NECESSAIRE.** Ce n'est pas un workflow atomique -- les etapes sont independantes et initiees par l'utilisateur.

#### 2.2.4 Emergency triage + escalation

```
OwnerMessage(urgent) -> AITriage -> NotifyOnCallVet -> EscalateIfNoResponse(30min) -> NotifyAdmin
```

**Analyse :** Ce workflow est deja implemente via events (`EmergencyMessageReceivedEvent` -> `EmergencyEscalationBackgroundService` scanne les conversations sans reponse). Cependant, le suivi d'etat (escalation envoyee ? vet a repondu ? admin notifie ?) est stocke directement sur l'entite `Conversation` (`EscalationSentAt`).

**Verdict : SAGA PERTINENT A TERME** pour un suivi d'escalation plus robuste avec timeouts configures, mais la complexite n'est pas justifiee au stade MVP.

### 2.3 Choreography vs Orchestration

**Recommandation : RESTER EN CHOREOGRAPHY pour le moment.**

Arguments :
1. **Le codebase est deja en choreography** (events publies -> consumers reagissent). Migrer vers des sagas orchestres demanderait un refactoring significatif sans gain metier.
2. **Les workflows actuels sont lineaires et sans compensation.** Un saga brille quand il y a des transactions distribuees necessitant des compensations (rollback). Ici, si un email echoue, le retry MassTransit suffit.
3. **Quand passer a l'orchestration ?** Si Tabby/BNPL est integre (payment intent -> callback -> reconciliation), un saga Automatonymous serait justifie car le workflow a des etats d'attente et des compensations.

### 2.4 Plan d'action Saga (Phase 3+, si BNPL)

Si un saga est necessaire dans le futur :

```csharp
// MassTransit Automatonymous state machine
public class InvoicePaymentSaga : MassTransitStateMachine<InvoicePaymentState>
{
    public State PaymentPending { get; private set; }
    public State PaymentConfirmed { get; private set; }
    public State PaymentFailed { get; private set; }

    public Event<PaymentIntentCreated> PaymentIntentCreated { get; private set; }
    public Event<PaymentCallbackReceived> PaymentCallbackReceived { get; private set; }
    public Event<PaymentTimeout> PaymentTimeout { get; private set; }

    public InvoicePaymentSaga()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(PaymentIntentCreated)
                .TransitionTo(PaymentPending)
                .Schedule(PaymentTimeoutSchedule, ctx => new PaymentTimeout(ctx.Saga.CorrelationId)));

        During(PaymentPending,
            When(PaymentCallbackReceived)
                .Unschedule(PaymentTimeoutSchedule)
                .TransitionTo(PaymentConfirmed)
                .Publish(ctx => new InvoiceMarkedPaid(ctx.Saga.InvoiceId)),
            When(PaymentTimeout)
                .TransitionTo(PaymentFailed)
                .Publish(ctx => new PaymentExpired(ctx.Saga.InvoiceId)));
    }
}
```

Impact : 1 nouveau projet `Vetolib.Billing/Sagas/`, tables EF pour saga state, configuration MassTransit saga repository.

---

## PARTIE 3 -- PATTERNS AVANCES

### 3.1 Event Sourcing sur Medical Records

**Pertinence : ELEVEE pour l'audit trail -- FAIBLE pour la complexite**

**Avantages :**
- Audit trail complet et immutable de chaque modification d'un dossier medical
- Replay possible pour reconstruire l'etat a n'importe quel moment
- Conformite reglementaire (veterinary records retention laws)

**Inconvenients :**
- Complexite majeure : event store, projections, snapshots, eventual consistency
- Le codebase utilise deja un `AuditDbContext` avec `AuditSaveChangesInterceptor` qui capture les modifications
- L'equipe n'a aucune experience event sourcing

**Recommandation : NE PAS IMPLEMENTER.** L'intercepteur d'audit existant couvre le besoin reglementaire. Si un audit plus fin est requis, enrichir l'intercepteur avec un event log plutot que refactorer tout le module en event sourcing.

**Alternative pragmatique :** Ajouter un `MedicalRecordEvent` table avec `EntityId`, `EventType`, `Payload (JSON)`, `Timestamp`, `UserId` -- un "pauvre homme event sourcing" sans la complexite des projections.

### 3.2 CQRS avec Read Models Separes

**Pertinence : MOYENNE**

Le projet applique deja CQRS au niveau structurel (Commands/ et Queries/ separes, MediatR handlers distincts). Mais il n'y a pas de read models materialises -- toutes les queries vont contre les memes tables que les commandes.

**Ou serait-ce utile ?**
- **Dashboard stats** : actuellement, chaque widget du dashboard execute une query live (GetAppointmentCount, GetInvoiceCount, GetPatientCount, GetRevenueByMonth, GetUnpaidInvoicesTotal). Avec 100+ cliniques actives, 5 queries par chargement de dashboard = charge significative.
- **Messaging inbox** : `ListConversationsHandler` (109 lignes) fait un `.Include(c => c.Messages)` puis pagine. Avec des milliers de conversations, une vue materialisee serait plus performante.

**Recommandation : IMPLEMENTER les read models pour le dashboard uniquement.**

Approche :
1. Creer un `DashboardReadModel` (table denormalisee ou PostgreSQL materialized view)
2. Mettre a jour via domain events (MediatR notifications) quand un appointment/invoice/patient change
3. Le dashboard query devient un simple `SELECT * FROM dashboard_read_model WHERE clinic_id = @current`

Effort : moyen (1-2 jours par module). Benefice : dashboard de 5 queries O(n) a 1 query O(1).

### 3.3 Outbox Pattern

**Pertinence : DEJA EN PLACE**

Le projet utilise deja le MassTransit Transactional Outbox. Les migrations `AddMassTransitOutbox` sont presentes dans Agenda et Auth. Les tables `OutboxMessage`, `OutboxState`, `InboxState` garantissent la coherence entre la DB et les events publies.

**Gaps identifies :**
- Seuls Agenda et Auth ont les tables outbox. **Messaging et Notifications n'ont PAS de tables outbox** malgre qu'ils publient des events via `IPublishEndpoint`. Cela signifie que si le `SaveChangesAsync()` reussit mais que la publication echoue (ou vice-versa), il y a un risque d'inconsistance.
- Billing devrait aussi avoir un outbox pour `InvoiceSentEvent`.

**Recommandation : Ajouter les tables outbox aux modules Messaging, Notifications, et Billing.** C'est une migration EF + une ligne de configuration MassTransit par module.

### 3.4 Circuit Breaker sur les appels externes

**Pertinence : ELEVEE**

Le codebase fait des appels a :
- **LLM (Claude API)** via `IChatClient` dans `TriageSymptomsHandler` et `ClaudeSoapNotesGenerator`
- **WhatsApp Business API** via `WhatsAppSender`
- **SMTP** via `SmtpEmailSender`
- (Futur) **Stripe/Tabby** pour les paiements

Actuellement, seul le `TriageSymptomsHandler` a un try/catch qui retourne `Result.Unavailable()`. Les autres services (WhatsApp, SMTP) lancent des exceptions sur echec, gerees par MassTransit retry.

**Recommandation : Implementer Polly Circuit Breaker.**

```csharp
// Dans SharedInfrastructureExtensions
services.AddHttpClient<IWhatsAppSender, WhatsAppSender>()
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromMinutes(1)));
```

Ajout de Polly resilience policies :
- **Circuit breaker** : 3 echecs -> ouverture 1 min pour WhatsApp/SMTP
- **Retry with exponential backoff** : pour les appels LLM (deja geres au niveau MassTransit pour les consumers, mais pas pour les appels directs)
- **Timeout** : 30s max pour les appels LLM, 10s pour WhatsApp

Effort : faible (1 jour). Benefice : resilience significativement amelioree.

### 3.5 Feature Flags

**Pertinence : ELEVEE**

Le projet a un module Preferences qui gere des "preference keys" (AITriage, AIDrugInteractions, etc.). C'est deja un systeme de feature flags par clinique/utilisateur. Cependant :

- Les flags sont stockes en DB, pas dans un service de feature flags dedie
- Pas de rollout progressif (percentage-based, canary)
- Pas de kill switch rapide (le changement passe par un handler -> DB -> cache eviction)

**Recommandation : Le systeme actuel est SUFFISANT pour le stade MVP.** Si le besoin de rollout progressif apparait (ex: "deployer AITriage a 10% des cliniques, puis monter progressivement"), migrer vers Microsoft.FeatureManagement ou un service tiers (LaunchDarkly, Unleash).

Pour l'instant, le module Preferences + OutputCache offre un cycle update -> evict -> refresh en quelques secondes, ce qui est acceptable.

---

## RESUME EXECUTIF

### Conformite globale : ATTENTION

| Categorie | Score | Details |
|-----------|-------|---------|
| Architecture modulaire | 9/10 | Isolation respectee, aucune reference cross-runtime |
| Result<T> everywhere | 9/10 | 2 methodes void dans Patient.cs |
| Tests unitaires | 5/10 | 53/85 handlers sans TU |
| IgnoreQueryFilters | 6/10 | Usage excessif dans Messaging (15+) -- pattern PortalContext manquant |
| Hardcoded values | 5/10 | 14+ constantes qui devraient etre configurables |
| French strings | 3/10 | 10 strings FR dans le code backend |
| Domain model | 8/10 | Riche, avec state machines et validations |
| Error handling | 8/10 | Throws limites aux consumers MassTransit (intentionnel) et startup |
| EF Core | 7/10 | Pas de lazy loading, AsNoTracking partiel, outbox incomplet |

### Top 5 actions prioritaires

1. **Remplacer les strings FR par EN** dans SlotScoringService et CreateAppointmentHandler (1h)
2. **Creer un PortalContext** pour eliminer les IgnoreQueryFilters dans Messaging (1-2 jours)
3. **Ajouter les tables outbox MassTransit** a Messaging, Notifications, Billing (0.5 jour)
4. **Extraire les constantes hardcodees** vers appsettings.json (1 jour)
5. **Ajouter les TU manquants** pour les handlers les plus critiques : CreateOwnerConversation, SendReply, ImportPatients, RegisterClinic (2-3 jours)
