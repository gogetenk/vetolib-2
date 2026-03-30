# Dead Wiring Audit -- 2026-03-30

Code that EXISTS but is NOT ACTIVE because middleware/DI/registration is missing.
Each finding looks like it works but silently does nothing at runtime.

---

## CRITICAL -- Output Caching Never Registered

### Finding DW-01: `.CacheOutput()` on 6 endpoints with no `AddOutputCache()` or `UseOutputCache()`

**Files:**
- `src/backend/Vetolib.Api/Dashboard/DashboardEndpoints.cs` lines 29, 35, 47
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Api/PatientEndpoints.cs` lines 38, 44, 50

**What it looks like:** Dashboard stats, today's appointments, analytics, patient list, and patient detail endpoints are cached with named policies (`"Dashboard1min"`, `"Moderate2min"`) or inline configuration.

**Why it is dead:** `Program.cs` never calls `builder.Services.AddOutputCache()` to register the output caching services, and never calls `app.UseOutputCache()` to add the middleware to the pipeline. The `.CacheOutput()` calls on endpoints are metadata annotations that the framework ignores entirely when the middleware is absent. Every request hits the database every time.

**Impact:** Every dashboard and patient list request goes to the database unthrottled. Under load, the dashboard alone fires 3 parallel queries per request with zero caching.

---

## CRITICAL -- Notifications Endpoints Never Mapped

### Finding DW-02: `MapNotificationsEndpoints()` defined but never called in `Program.cs`

**File:** `src/backend/Modules/Notifications/Vetolib.Notifications/NotificationsModuleServiceRegistrar.cs` lines 36-41

**What it looks like:** The Notifications module defines `MapNotificationsEndpoints()` which maps reminder config endpoints at `/api/v1/notifications/reminders` (GET/PUT config, GET logs).

**Why it is dead:** `Program.cs` (lines 329-340) maps endpoints for Auth, Agenda, MedicalRecords, Billing, Audit, Dashboard, AI, Messaging, Stock, Preferences, and Breeding -- but never calls `app.MapNotificationsEndpoints()`. The three reminder endpoints (`ReminderEndpoints.cs`) are unreachable. The `ReminderSchedulerService` background service runs but there is no API to configure it.

**Impact:** Reminder configuration and log viewing APIs are completely inaccessible. Frontend cannot read or update reminder settings.

---

## CRITICAL -- Appointment Reminder Background Service Never Registered

### Finding DW-03: `AppointmentReminderService` exists but is never registered via `AddHostedService`

**File:** `src/backend/Modules/Agenda/Vetolib.Agenda/Infrastructure/AppointmentReminderService.cs` (full file, 107 lines)

**What it looks like:** A `BackgroundService` that runs every hour, scans for appointments in the 23-25h window, publishes `AppointmentReminderDueIntegrationEvent` via MassTransit, and marks reminders as sent.

**Why it is dead:** `ModuleServiceRegistrar.AddAgendaModule()` (lines 18-41) never calls `services.AddHostedService<AppointmentReminderService>()`. The class is compiled into the assembly but never instantiated by the DI container. No appointment reminders are ever sent.

**Impact:** Appointment reminder emails are completely non-functional. The entire reminder pipeline (scan -> publish event -> Notifications consumer -> send email) never fires.

---

## HIGH -- Preferences MassTransit Consumer Not Discovered

### Finding DW-04: `PreferenceChangedConsumer` exists but its assembly is never scanned by `AddConsumers()`

**Files:**
- `src/backend/Modules/Preferences/Vetolib.Preferences/Consumers/PreferenceChangedConsumer.cs` (full file)
- `src/backend/Modules/Preferences/Vetolib.Preferences/ModuleServiceRegistrar.cs` lines 37-39 (comment acknowledges this)
- `src/backend/Vetolib.Api/Program.cs` line 148

**What it looks like:** A MassTransit `IConsumer<PreferenceChangedIntegrationEvent>` that invalidates the `IMemoryCache` entry when a preference changes, ensuring the `PreferenceChecker` reflects updates immediately.

**Why it is dead:** `Program.cs` line 148 only calls `x.AddConsumers(typeof(NotificationsModuleServiceRegistrar).Assembly)` -- scanning only the Notifications assembly. The Preferences assembly is never scanned. The registrar itself contains a comment acknowledging this: "Note: PreferenceChangedConsumer for cache invalidation must be registered in the host via x.AddConsumers(typeof(ModuleServiceRegistrar).Assembly) in Program.cs. Current fallback: cache entries expire after 5 minutes (CacheTtl)."

**Impact:** Preference changes are never immediately invalidated. The cache TTL fallback (5 minutes) partially mitigates this, but users may see stale preference values for up to 5 minutes after changes.

---

## HIGH -- AgendaOptions Never Bound to Configuration

### Finding DW-05: `IOptions<AgendaOptions>` injected in 2 services but `Configure<AgendaOptions>()` never called

**Files:**
- `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Services/SlotScoringService.cs` line 21
- `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Services/DurationEstimator.cs` line 18
- `src/backend/Modules/Agenda/Vetolib.Agenda/ModuleServiceRegistrar.cs` (no `Configure<AgendaOptions>` call)

**What it looks like:** `SlotScoringService` and `DurationEstimator` inject `IOptions<AgendaOptions>` to read slot scoring weights, working hours, and default consultation durations from configuration.

**Why it is dead:** `AddAgendaModule()` never calls `services.Configure<AgendaOptions>(configuration.GetSection(...))`. The DI container will resolve `IOptions<AgendaOptions>` with a default-constructed `AgendaOptions` instance (all default values). No appsettings section for "Agenda" exists either. The scoring weights and working hours use hardcoded defaults silently, and `DefaultDurationByType` is an empty dictionary.

**Impact:** Slot scoring and duration estimation use only hardcoded defaults. Any attempt to customize these via appsettings is silently ignored. `DefaultDurationByType` being empty means the `DurationEstimator` falls back to its internal logic for every consultation type.

---

## HIGH -- MailKitEmailSender Never Registered (Dead Code)

### Finding DW-06: `MailKitEmailSender` class exists but is never registered in DI

**File:** `src/backend/Modules/Notifications/Vetolib.Notifications/Infrastructure/MailKitEmailSender.cs` (full file, 135 lines)

**What it looks like:** A full MailKit-based SMTP email sender with Polly circuit breaker and retry, reading `IOptions<MailKitSmtpOptions>` from the `"Notifications:Smtp"` config section.

**Why it is dead:** The `IEmailSender` registration is done in `Program.cs` line 142 via `builder.Services.AddEmailSender(builder.Configuration)`, which registers `SmtpEmailSender` (from `Shared.Infrastructure`) or `ConsoleEmailSender`. The `NotificationsModuleServiceRegistrar.AddNotificationsModule()` never registers `MailKitEmailSender` as `IEmailSender`. The class exists in the Notifications assembly but is never instantiated.

**Additionally:** `MailKitSmtpOptions` binds to `"Notifications:Smtp"` which does not exist in any appsettings file.

**Impact:** 135 lines of production-grade email code (with circuit breaker, retry, MailKit) are dead. The system uses the simpler `SmtpEmailSender` from Shared infrastructure instead.

---

## HIGH -- EReporting Configuration Section Missing

### Finding DW-07: `EReportingJobOptions` bound to `"EReporting"` section that does not exist

**Files:**
- `src/backend/Modules/Billing/Vetolib.Billing/ModuleServiceRegistrar.cs` line 25
- `src/backend/Modules/Billing/Vetolib.Billing/Infrastructure/Jobs/EReportingJob.cs` lines 86-99

**What it looks like:** `EReportingJobOptions` is bound via `services.Configure<EReportingJobOptions>(configuration.GetSection("EReporting"))`. The job reads `Enabled` (default: `false`) and `IntervalDays` (default: 30).

**Why it is effectively dead:** No `"EReporting"` section exists in any appsettings file (appsettings.json, appsettings.Development.json, appsettings.Production.json). The options resolve with defaults: `Enabled = false`. The `EReportingJob` is registered as a `HostedService` (line 37) but its `ExecuteAsync` likely checks `Enabled` and exits. This is by design (default off) but the configuration section to enable it does not exist, so enabling it requires knowledge of the undocumented section name.

**Impact:** Low immediate impact (defaults to disabled), but impossible to enable without knowing the hidden config key `"EReporting:Enabled"`.

---

## HIGH -- HealthAlertJob Configuration Section Missing

### Finding DW-08: `HealthAlertJobOptions` bound to `"AI:HealthAlertJob"` section that does not exist

**Files:**
- `src/backend/Modules/AI/Vetolib.AI/ModuleServiceRegistrar.cs` lines 134-141
- `src/backend/Modules/AI/Vetolib.AI/Infrastructure/HealthAlertJobOptions.cs`

**What it looks like:** `HealthAlertJobOptions` is bound to `"AI:HealthAlertJob"`. Default is `Enabled = true`, `ScheduledTimeUtc = 06:00`. The registrar reads the options and conditionally registers the hosted service.

**Why it is partially dead:** The `"AI"` section exists in appsettings.json but has no `"HealthAlertJob"` subsection. The options resolve with defaults (`Enabled = true`), so the job IS registered and runs. However, `ScheduledTimeUtc` cannot be configured without knowing the undocumented path `"AI:HealthAlertJob:ScheduledTimeUtc"`.

**Impact:** The job runs at the hardcoded 06:00 UTC default. Configuration is not broken but is not configurable without undocumented knowledge.

---

## MEDIUM -- SubscriptionCheckFilter Never Wired to Any Endpoint

### Finding DW-09: `SubscriptionCheckFilter` and `CheckLimitAttribute` exist but are never applied

**Files:**
- `src/backend/Modules/Auth/Vetolib.Auth/Api/SubscriptionCheckFilter.cs` (full file, 60 lines)
- All `*Endpoints.cs` files in the codebase

**What it looks like:** An `IEndpointFilter` that reads `CheckLimitAttribute` metadata from endpoints, looks up the clinic's subscription plan via `ISubscriptionChecker`, and returns 403 if the plan limit is exceeded.

**Why it is dead:** No endpoint in the entire codebase calls `.AddEndpointFilter<SubscriptionCheckFilter>()`. No method is decorated with `[CheckLimit(...)]`. The filter, attribute, and the `ISubscriptionChecker` service (registered in DI) are all wired up in isolation but never connected to any endpoint.

**Impact:** Subscription plan limits are never enforced at the API layer. A clinic on a free trial plan can create unlimited resources with no gating.

---

## MEDIUM -- Audit Interceptor Missing on 7 of 11 DbContexts

### Finding DW-10: `AddAuditInterceptor<T>()` called for 4 contexts, missing for 7

**File:** `src/backend/Vetolib.Api/Program.cs` lines 136-139

**Covered:**
- `AuthDbContext`
- `AgendaDbContext`
- `MedicalRecordsDbContext`
- `BillingDbContext`

**Not covered:**
- `MessagingDbContext`
- `NotificationsDbContext`
- `AIDbContext`
- `StockDbContext`
- `PreferencesDbContext`
- `BreedingDbContext`
- `AuditDbContext` (self-referential, arguably not needed)

**What it looks like:** The audit trail interceptor captures entity changes (who changed what, when) into the `AuditDbContext`.

**Why the others are dead:** Entity changes in Messaging, Notifications, AI, Stock, Preferences, and Breeding modules are not captured by the audit trail. Any create/update/delete in those modules leaves no audit record.

**Impact:** Audit trail has blind spots. Changes to stock levels, breeding records, preferences, messages, AI health alerts, and notification configs are not audited.

---

## MEDIUM -- MedicalRecords Outbox Not Configured

### Finding DW-11: `MedicalRecordsDbContext` publishes MassTransit events but has no EF Core Outbox

**Files:**
- `src/backend/Vetolib.Api/Program.cs` lines 152-176 (outbox for Auth, Agenda, Billing, Notifications, Messaging -- but not MedicalRecords)
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Commands/AddPrescription/AddPrescriptionHandler.cs`

**What it looks like:** The MedicalRecords module uses `IPublisher` (MediatR) for domain events like `PrescriptionCreatedEvent`. The `MedicalRecordsDbContext` itself injects `IPublisher`. MassTransit outboxes are configured for 5 other contexts.

**Why it is notable:** If any handler in MedicalRecords publishes MassTransit integration events (via `IPublishEndpoint`), those events lack at-least-once delivery guarantees. Currently, the module appears to use MediatR notifications rather than MassTransit, but the inconsistency is a risk if future development adds MassTransit publishing.

**Impact:** Low currently (MediatR notifications are in-process). Risk increases if MassTransit integration events are added to MedicalRecords without configuring the outbox.

---

## LOW -- Database Health Checks Disabled on All Contexts

### Finding DW-12: `DisableHealthChecks = true` on every Aspire-enriched DbContext

**File:** `src/backend/Vetolib.Api/Program.cs` lines 110-235

**What it looks like:** Every `EnrichNpgsqlDbContext` and `AddNpgsqlDbContext` call sets `settings.DisableHealthChecks = true`.

**Why it matters:** The `/health/ready` endpoint (from ServiceDefaults) will not include database connectivity checks. If PostgreSQL goes down, the health endpoint still reports healthy. Load balancers and orchestrators (Kubernetes, Azure Container Apps) will continue routing traffic to an instance that cannot serve requests.

**Impact:** Health checks are not truly dead (the endpoint works), but database health is excluded. This is likely intentional to avoid startup failures, but means database outages are invisible to infrastructure health probes.

---

## Summary Table

| ID | Severity | What | Dead Since |
|---|---|---|---|
| DW-01 | CRITICAL | Output caching never registered (`AddOutputCache`/`UseOutputCache` missing) | Unknown |
| DW-02 | CRITICAL | Notifications endpoints never mapped in `Program.cs` | Unknown |
| DW-03 | CRITICAL | `AppointmentReminderService` never registered as `HostedService` | Unknown |
| DW-04 | HIGH | Preferences `PreferenceChangedConsumer` not discovered by MassTransit | Known (documented in comment) |
| DW-05 | HIGH | `AgendaOptions` never bound to configuration | Unknown |
| DW-06 | HIGH | `MailKitEmailSender` (135 lines) never registered in DI | Unknown |
| DW-07 | HIGH | `EReportingJobOptions` config section does not exist | Unknown |
| DW-08 | HIGH | `HealthAlertJobOptions` config section partially missing | Unknown |
| DW-09 | MEDIUM | `SubscriptionCheckFilter` never wired to any endpoint | Unknown |
| DW-10 | MEDIUM | Audit interceptor missing on 7 of 11 DbContexts | Unknown |
| DW-11 | MEDIUM | MedicalRecords has no MassTransit outbox (inconsistency) | Unknown |
| DW-12 | LOW | Database health checks disabled on all contexts | Intentional? |

---

*Audit performed by Claude Opus 4.6 (1M context) on 2026-03-30.*
*Scope: `src/backend/` -- DI registration, middleware pipeline, MassTransit consumers, configuration binding.*
