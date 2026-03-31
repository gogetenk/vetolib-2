# QA Night Audit -- Dead Code & Dead Wiring

**Date**: 2026-04-01
**Branch**: develop
**Scope**: Full backend + frontend scan post-120 PRs

---

## 1. Dead Endpoints

**Verdict: CLEAN** -- All endpoints registered in `Program.cs` are wired and reachable.

Each `Map*Endpoints()` call in `Program.cs` (lines 353-365) delegates to a module registrar
which in turn calls sub-endpoint mappers. Every sub-mapper defines handler methods. Full chain verified:

| Program.cs call | Registrar | Sub-endpoints wired |
|---|---|---|
| `MapAuthEndpoints` | `AuthModuleServiceRegistrar` | Auth, User, Clinic, ClinicGroup, Onboarding, Referral, Portal |
| `MapAgendaEndpoints` | `ModuleServiceRegistrar` | Appointment, ConsultationType, Feedback, Waitlist |
| `MapMedicalRecordsEndpoints` | `ModuleServiceRegistrar` | Patient, Owner, MedicalRecord, DrugCatalog, Weight, Template, SharedRecord, OwnerPortal |
| `MapBillingEndpoints` | `ModuleServiceRegistrar` | Invoice, EReporting |
| `MapAIEndpoints` | `ModuleServiceRegistrar` | AI (includes HealthAlert internally) |
| `MapMessagingEndpoints` | `MessagingModuleServiceRegistrar` | Messaging, Portal, SSE, WhatsApp |
| `MapStockEndpoints` | `StockModuleServiceRegistrar` | Stock |
| `MapBreedingEndpoints` | `BreedingModuleServiceRegistrar` | Litter, Pregnancy, HeatCycle, Lineage |
| `MapPreferencesEndpoints` | `ModuleServiceRegistrar` | WorkingHours |
| `MapNotificationsEndpoints` | `NotificationsModuleServiceRegistrar` | Reminder |
| `MapAuditApiEndpoints` | `AuditEndpoints` | Audit log |
| `MapDashboardApiEndpoints` | `DashboardEndpoints` | Stats, TodayAppointments, RecentActivity, Analytics, RevenueTrend, ConsultationBreakdown, SpeciesDistribution, VetWorkload |

No empty handlers found. All endpoint methods (`MapGet`, `MapPost`, etc.) reference existing private static handler methods.

---

## 2. Dead Middleware / Add-Use Pairing

**Verdict: CLEAN** -- All `Add*`/`Use*` pairs are matched.

| Add (services) | Use (pipeline) | Status |
|---|---|---|
| `AddCors` | `UseCors` | OK |
| `AddRateLimiter` | `UseRateLimiter` | OK |
| `AddAuthentication` (in AuthModule) | `UseAuthentication` | OK |
| `AddAuthorization` (in AuthModule) | `UseAuthorization` | OK |
| `AddOutputCache` | `UseOutputCache` | OK |
| `UseSerilog` (Host) | N/A (Host-level) | OK |
| `UseSentry` (WebHost) | N/A (WebHost-level) | OK |
| `AddOpenApi` | `MapOpenApi` (dev only) | OK |
| `UseHttpsRedirection` | No Add needed | OK |
| `UseExceptionHandler` | No Add needed | OK |

No orphaned middleware found.

---

## 3. Unregistered / Always-Null Services

**FOUND: 2 issues (LOW severity)**

### 3a. `IMessageTriageService` -- no implementation, no DI registration

- **Defined in**: `Modules/AI/Vetolib.AI.Contracts/IMessageTriageService.cs`
- **Injected as nullable in**:
  - `Messaging/Application/Services/TriageOrchestrator.cs` (line 45: `IMessageTriageService? triageService = null`)
  - `Messaging/Application/Queries/GetConversationById/GetConversationByIdHandler.cs` (line 34)
- **Impact**: The triage feature in Messaging always falls back to the null-check path. The interface exists but no class implements it and no DI registration exists anywhere.
- **Risk**: LOW -- intentionally optional (nullable parameter with `= null` default). But the interface and `MessageTriageResult` DTO are dead code until implemented.

### 3b. `IConversationSummaryService` -- no implementation, no DI registration

- **Defined in**: `Modules/AI/Vetolib.AI.Contracts/IConversationSummaryService.cs`
- **Injected as nullable in**:
  - `Messaging/Application/Queries/GetConversationSummary/GetConversationSummaryHandler.cs` (line 25)
- **Impact**: Conversation summary always returns the fallback. Same pattern as above.
- **Risk**: LOW -- intentionally optional. Dead until AI module implements it.

### Action recommended
These are forward-declared contracts waiting for AI implementation. No bug, but they should be tracked as TODOs. If not planned for the next milestone, consider removing to reduce dead code.

---

## 4. Orphaned Files

### 4a. Backend -- Orphaned Contract Types

| File | Last referenced by | Status |
|---|---|---|
| `Agenda.Contracts/UpdateAppointmentStatusRequest.cs` | **Nothing** (0 references outside its own file) | DEAD -- replaced by `TransitionAppointmentRequest` |
| `Agenda.Contracts/ListAppointmentsRequest.cs` | **Nothing** (0 references outside its own file) | DEAD -- never wired to any endpoint or handler |

### 4b. Backend -- Events Published But Never Consumed

| Event | Published by | Consumer | Status |
|---|---|---|---|
| `StockInsufficientForPrescriptionEvent` | `Stock/PrescriptionCreatedConsumer` | **None** | DEAD -- fire-and-forget with no listener |
| `StockExpiringEvent` | `Stock/GetStockAlertsHandler` | **None** | DEAD -- fire-and-forget with no listener |

These events are MediatR notifications (`INotification`) that are published but have no `INotificationHandler<T>` registered. They silently do nothing at runtime.

### 4c. Frontend

**Verdict: CLEAN** -- All frontend files are imported/referenced:
- All hooks (`use-aha-moment`, `use-direction`, `use-form-shake`, `use-push-notifications`, `use-scroll-animation`, `useServiceWorker`) are imported by components.
- All MSW handlers are registered in `mocks/handlers/index.ts`.
- All API client files (`lib/api/*.ts`) are imported by components or MSW handlers.
- `feature-flags.ts` is imported by `PricingPageClient.tsx`.
- `posthog.ts` is imported by `analytics.ts` and `PostHogProvider.tsx`.

---

## 5. DbContext Pending Model Changes

**Verdict: ALL CLEAN** -- 11/11 contexts have no pending changes.

| Context | Project | Result |
|---|---|---|
| `AuthDbContext` | Auth | No pending changes |
| `AgendaDbContext` | Agenda | No pending changes |
| `MedicalRecordsDbContext` | MedicalRecords | No pending changes |
| `BillingDbContext` | Billing | No pending changes |
| `MessagingDbContext` | Messaging | No pending changes |
| `NotificationsDbContext` | Notifications | No pending changes |
| `AIDbContext` | AI | No pending changes |
| `StockDbContext` | Stock | No pending changes |
| `PreferencesDbContext` | Preferences | No pending changes |
| `BreedingDbContext` | Breeding | No pending changes |
| `AuditDbContext` | Shared.Infrastructure | No pending changes |

---

## 6. Additional Observations

### 6a. MassTransit Consumer Registration Gap

Only 2 assemblies are scanned for MassTransit consumers in `Program.cs`:
- `NotificationsModuleServiceRegistrar.Assembly`
- `Preferences.ModuleServiceRegistrar.Assembly`

This is correct because only these two modules have MassTransit `IConsumer<T>` implementations. Other modules (Stock, MedicalRecords, etc.) use MediatR `INotificationHandler<T>` for internal event handling, which is auto-registered by `AddMediatR()`.

### 6b. MassTransit Outbox Coverage

Outbox is configured for: Auth, Agenda, Billing, Notifications, Messaging.

Modules that publish via MassTransit `IPublishEndpoint`: Auth, Agenda, Messaging, Notifications.
Billing's outbox exists but no `IPublishEndpoint.Publish()` calls were found in Billing code -- the outbox registration is proactive/preventive. Acceptable.

Stock, AI, Breeding, Preferences, MedicalRecords do NOT use MassTransit `IPublishEndpoint` -- they only use MediatR `IPublisher`. No missing outbox.

### 6c. Exception Throws in Module Code

Found `throw new InvalidOperationException` / `ArgumentException` in:
- `MessagingModuleServiceRegistrar.cs` (config validation at startup) -- acceptable
- `LocalFileStorage.cs` (security guard) -- acceptable
- `AesTokenEncryptor.cs` (parameter validation) -- acceptable
- `CheckInHmacService.cs` (config validation) -- acceptable
- Notification consumers (email send failures) -- these re-throw for MassTransit retry. Acceptable.

No violations of the "Ardalis.Result PARTOUT" rule for business logic flow control.

---

## Summary

| Category | Findings | Severity |
|---|---|---|
| Dead endpoints | 0 | -- |
| Dead middleware | 0 | -- |
| Unregistered services (always null) | 2 (`IMessageTriageService`, `IConversationSummaryService`) | LOW |
| Orphaned contract files | 2 (`UpdateAppointmentStatusRequest`, `ListAppointmentsRequest`) | LOW |
| Events with no consumers | 2 (`StockInsufficientForPrescriptionEvent`, `StockExpiringEvent`) | LOW |
| Pending DB model changes | 0/11 | -- |
| Frontend orphaned files | 0 | -- |

**Total dead code items: 6** (all LOW severity)

### Recommended Cleanups (non-blocking)

1. **Delete** `Agenda.Contracts/UpdateAppointmentStatusRequest.cs` -- replaced by `TransitionAppointmentRequest`
2. **Delete** `Agenda.Contracts/ListAppointmentsRequest.cs` -- never used
3. **Add consumers** for `StockInsufficientForPrescriptionEvent` and `StockExpiringEvent` (e.g., notification handlers), OR delete the publish calls if notifications are not planned
4. **Track** `IMessageTriageService` / `IConversationSummaryService` as TODOs for AI module implementation, or remove if not on roadmap
