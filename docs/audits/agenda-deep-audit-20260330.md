# Agenda Module Deep Audit - 2026-03-30

Scope: every `.cs` file in `Vetolib.Agenda/` and `Vetolib.Agenda.Contracts/` (excluding `obj/` and migration Designer files).

---

## Summary

| Category | Count |
|---|---|
| Hardcoded values | 5 |
| Missing validation / missing validator | 2 |
| Potential null-safety issues | 4 |
| Performance concerns | 4 |
| Result pattern violations | 2 |
| Dead / unreachable code | 3 |
| Semantic / logic bugs | 4 |
| Inconsistent error messages (language) | 2 |
| Missing AsNoTracking | 2 |
| TODO / incomplete features | 3 |

**Total findings: 31**

---

## 1. Hardcoded Values

### H-1. Hardcoded clinic name in reminder service
**File:** `Vetolib.Agenda/Infrastructure/AppointmentReminderService.cs:91`
```csharp
ClinicName = "Desert Paws Veterinary Clinic"
```
This should be fetched from the clinic entity or configuration. Every reminder will show the same clinic name regardless of the actual clinic.

### H-2. Hardcoded business hours in ClinicSchedule.Default
**File:** `Vetolib.Agenda/Application/Domain/ClinicSchedule.cs:5`
```csharp
public static ClinicSchedule Default => new(new TimeOnly(9, 0), new TimeOnly(18, 0));
```
Used by `CreateAppointmentHandler:29`, `GetAvailabilityHandler:31`, and `FindNextAvailableSlots`. Clinics cannot configure their own hours. The `AgendaWorkingHoursOptions` exists in `SlotScoringOptions.cs` but is only used by `SlotScoringService`, not by the main appointment creation/availability logic.

### H-3. Hardcoded 30-minute slot grid
**File:** `Vetolib.Agenda/Application/Queries/GetAvailability/GetAvailabilityHandler.cs:54`
```csharp
current = current.AddMinutes(30); // 30-minute grid
```
Not configurable. Some clinics may need 15-minute or 20-minute grids.

### H-4. Hardcoded 15-minute increment for conflict slot suggestions
**File:** `Vetolib.Agenda/Application/Commands/CreateAppointment/CreateAppointmentHandler.cs:93`
```csharp
candidateTime = candidateTime.AddMinutes(15);
```

### H-5. Hardcoded fallback duration of 30 minutes
**File:** `Vetolib.Agenda/Application/Services/DurationEstimator.cs:60`
```csharp
return 30;
```
And also in `SuggestSlotHandler.cs:77`:
```csharp
durationResult.IsSuccess ? durationResult.Value : 30;
```

---

## 2. Missing Validation / Missing Validators

### V-1. DeactivateConsultationTypeCommand has no FluentValidation validator
**File:** `Vetolib.Agenda/Application/Commands/DeactivateConsultationType/DeactivateConsultationTypeCommand.cs`
No `DeactivateConsultationTypeValidator` exists. Although the handler does a null check, the `Id` parameter is not validated as non-empty before the handler runs. Every other command has a validator.

### V-2. GetAvailabilityQuery has no validator
**File:** `Vetolib.Agenda/Application/Queries/GetAvailability/GetAvailabilityQuery.cs`
No validator for `VeterinarianId` (could be `Guid.Empty`) or `DurationMinutes` (could be 0 or negative). These are passed directly from query string parameters in the endpoint.

---

## 3. Null-Safety Issues

### N-1. Unguarded nullable access on `request.Action`
**File:** `Vetolib.Agenda/Api/AppointmentEndpoints.cs:140`
```csharp
var newStatus = request.Action.ToUpperInvariant() switch
```
`TransitionAppointmentRequest.Action` is `string` (not `string?`) but the request comes from HTTP deserialization and could be null at runtime, causing a `NullReferenceException`.

### N-2. `OwnerEmail` null-forgiving operator after null check
**File:** `Vetolib.Agenda/Infrastructure/AppointmentReminderService.cs:85`
```csharp
OwnerEmail = appointment.OwnerEmail!,
```
The query filters `a.OwnerEmail != null` (line 63), so this is safe at runtime, but the null-forgiving pattern is fragile if the query ever changes.

### N-3. `VeterinarianName` fallback to truncated GUID
**File:** `Vetolib.Agenda/Application/Queries/SuggestSlot/SuggestSlotHandler.cs:52`
```csharp
var vetName = appts.FirstOrDefault()?.VeterinarianName ?? $"Vet {vetId:N}";
```
If a vet has no existing appointments, the name degrades to `Vet <guid>` which is user-visible in the API response.

### N-4. `UpdateAppointmentStatusRawRequest.NewStatus` can be null
**File:** `Vetolib.Agenda/Api/AppointmentEndpoints.cs:118`
```csharp
if (!Enum.TryParse<AppointmentStatus>(request.NewStatus, ignoreCase: true, out var status))
```
`NewStatus` is `string` but could be null from deserialization. `Enum.TryParse` handles null (returns false), but the error message would show `''` instead of a clear null indicator.

---

## 4. Performance Concerns

### P-1. CreateAppointmentHandler loads all vet's daily appointments into memory
**File:** `Vetolib.Agenda/Application/Commands/CreateAppointment/CreateAppointmentHandler.cs:38-41`
```csharp
var conflictingAppointments = await _context.Appointments
    .Where(a => a.VeterinarianId == cmd.VeterinarianId && a.Date == cmd.Date && a.Status != AppointmentStatus.Cancelled)
    .ToListAsync(ct);
```
Materializes all appointments for the day, then does in-memory overlap check. For a busy vet with dozens of appointments, this is wasteful. The overlap check could be done in SQL.

### P-2. DurationEstimator issues one DB query per vet in SuggestSlotHandler
**File:** `Vetolib.Agenda/Application/Queries/SuggestSlot/SuggestSlotHandler.cs:71-78`
The comment at line 66 says "Pre-load duration estimates for all vets in a single batch to avoid N+1 queries" but the loop still calls `_durationEstimator.EstimateAsync` once per vet, each of which issues a separate DB query. This is still N+1.

### P-3. AppointmentReader.GetOwnerHistoryAsync missing AsNoTracking
**File:** `Vetolib.Agenda/Application/Services/AppointmentReader.cs:21`
```csharp
var appointments = await _context.Appointments
    .Where(a => a.AnimalId == ownerId)
```
No `.AsNoTracking()` on a read-only query. Other query handlers correctly use `AsNoTracking`.

### P-4. GetCompletedAppointmentCountAsync missing AsNoTracking
**File:** `Vetolib.Agenda/Application/Services/AppointmentReader.cs:142`
```csharp
return await _context.Appointments
    .CountAsync(a => a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.NoShow, ct);
```
No `.AsNoTracking()`. While `CountAsync` doesn't materialize entities, consistent usage is good practice.

---

## 5. Result Pattern Issues

### R-1. Dubious Result.Map with null-forgiving cast
**File:** `Vetolib.Agenda/Application/Commands/CreateConsultationType/CreateConsultationTypeHandler.cs:35`
```csharp
return createResult.Map(_ => (ConsultationTypeDto)null!);
```
Same pattern at `UpdateConsultationTypeHandler.cs:37`. When the domain factory fails with validation errors, this maps the result but discards the error information. The mapped value `null!` is never used (the result is Invalid), but the pattern is misleading and brittle. Should use `Result<ConsultationTypeDto>.Invalid(createResult.ValidationErrors.ToList())` like `CreateAppointmentHandler` does.

### R-2. MarkReminderSent returns Result but nobody checks it
**File:** `Vetolib.Agenda/Application/Domain/Appointment.cs:132-137`
```csharp
public Result MarkReminderSent()
{
    ReminderSent = true;
    UpdatedAt = DateTime.UtcNow;
    return Result.Success();
}
```
Always returns Success, never fails. The caller in `AppointmentReminderService.cs:94` does not check the result:
```csharp
appointment.MarkReminderSent(); // result discarded
```
Either the method should have a failure path or it should return `void`.

---

## 6. Dead / Unreachable Code

### D-1. `OwnerEmail` property is never set
**File:** `Vetolib.Agenda/Application/Domain/Appointment.cs:15`
```csharp
public string? OwnerEmail { get; private set; }
```
The `Create` factory method does not accept or set `OwnerEmail`. There is no `SetOwnerEmail` method. The only way it gets set is if it exists in the database (loaded by EF). This means newly created appointments will always have `OwnerEmail = null` and will never receive reminders.

### D-2. `Appointment.Notes` parameter accepted but not stored
**File:** `Vetolib.Agenda/Application/Domain/Appointment.cs:147` (Reschedule method)
```csharp
public Result Reschedule(..., string? newNotes)
```
The `newNotes` parameter is accepted but never assigned to any property. The `Appointment` entity has no `Notes` property. The `EditAppointmentCommand` sends `Notes` but it is silently ignored.

### D-3. `CreateAppointmentFromMessageRequest` is defined but never used
**File:** `Vetolib.Agenda.Contracts/CreateAppointmentFromMessageRequest.cs`
No handler, endpoint, or any code references this DTO beyond the file itself. It appears to be scaffolded for a future Messaging module integration that doesn't exist yet.

---

## 7. Semantic / Logic Bugs

### S-1. AppointmentReader maps `ownerId` to `AnimalId`
**File:** `Vetolib.Agenda/Application/Services/AppointmentReader.cs:19-22`
```csharp
// ownerId maps to AnimalId -- the animal (patient) belonging to the owner.
var appointments = await _context.Appointments
    .Where(a => a.AnimalId == ownerId)
```
The interface declares `GetOwnerHistoryAsync(Guid ownerId, ...)` but the implementation filters by `AnimalId`. The comment acknowledges this mapping, but it means an owner with multiple animals only gets history for one. The calling code must pass `animalId` pretending it's an `ownerId`, which is confusing.

### S-2. Analytics NoShowRate includes Cancelled as no-shows
**File:** `Vetolib.Agenda/Application/Queries/GetAppointmentsAnalytics/GetAppointmentsAnalyticsHandler.cs:34-35`
```csharp
var noShowCount = statusGroups
    .Where(g => g.Status == AppointmentStatus.Cancelled || g.Status == AppointmentStatus.NoShow)
    .Sum(g => g.Count);
```
Cancelled appointments (which may be legitimate cancellations) are counted as no-shows, inflating the no-show rate. The DTO comment says "CANCELLED or NOSHOW (non-completed) appointments" but this conflates two different concepts.

### S-3. `IncrementReschedule` sets `OriginalAppointmentId` to the NEW appointment's ID
**File:** `Vetolib.Agenda/Application/Domain/Appointment.cs:175-182`
```csharp
public Result IncrementReschedule(Guid newAppointmentId, int maxReschedules)
{
    ...
    OriginalAppointmentId = newAppointmentId;
    return Result.Success();
}
```
This is semantically confusing. `OriginalAppointmentId` should logically point to the *original* appointment, not the *new* one. The method sets the original's `OriginalAppointmentId` to the new appointment, making it a "next appointment" reference rather than an "original" reference.

### S-4. `EditAppointmentHandler` does conflict check AFTER applying changes
**File:** `Vetolib.Agenda/Application/Commands/EditAppointment/EditAppointmentHandler.cs:34-63`
The handler first calls `appointment.Reschedule(...)` (which mutates the entity), then checks for conflicts. If a conflict is found, it returns `Conflict()` but the entity is already mutated in the change tracker. In the retry loop this is handled by `ChangeTracker.Clear()` only for the unique constraint violation case, but for the in-memory conflict check (line 62-63), the mutated entity remains tracked. On the next retry iteration, the entity is re-fetched which resets it, but this is a fragile pattern.

---

## 8. Inconsistent Error Messages (Language)

### L-1. French validation message in English codebase
**File:** `Vetolib.Agenda/Application/Commands/CreateAppointment/CreateAppointmentValidator.cs:16`
```csharp
.WithMessage("La duree doit etre superieure a 0");
```
All other validators use English messages.

### L-2. French reasoning strings in SlotScoringService
**File:** `Vetolib.Agenda/Application/Services/SlotScoringService.cs` (multiple lines)
Lines 133, 148, 153, 168, 187, 197, 221, 231, 245, 249, 253. All scoring reason strings are in French:
```csharp
reasons.Add("Aucun rendez-vous existant ce jour");
reasons.Add("Creneau adjacent au precedent (pas de gap)");
reasons.Add("Charge inferieure a la moyenne (equilibrage favorise)");
```
These strings are returned in the API response (`SlotSuggestionDto.Reasoning`). The project rules say "Toujours en anglais" for the UAE market.

---

## 9. Missing AsNoTracking (Read-Only Queries)

### AT-1. AppointmentReader.GetOwnerHistoryAsync
**File:** `Vetolib.Agenda/Application/Services/AppointmentReader.cs:21`
Uses `.Select()` projection so EF Core won't track, but explicit `.AsNoTracking()` is missing for consistency.

### AT-2. CreateAppointmentHandler conflict check query
**File:** `Vetolib.Agenda/Application/Commands/CreateAppointment/CreateAppointmentHandler.cs:38`
Loads appointments only for overlap checking but does not use `.AsNoTracking()`. The entities are materialized and tracked unnecessarily.

---

## 10. Incomplete / TODO-Like Features

### T-1. `WasReminderSent` always false in AppointmentReader
**File:** `Vetolib.Agenda/Application/Services/AppointmentReader.cs:32,75,130`
```csharp
false)) // ReminderSent not tracked in current domain model
```
The comment says "not tracked" but `ReminderSent` IS a property on the `Appointment` entity. The `AppointmentReader` simply doesn't use it. Should be `a.ReminderSent` instead of hardcoded `false`.

### T-2. `LeadTimeDays` always 0
**File:** `Vetolib.Agenda/Application/Services/AppointmentReader.cs:79,134`
```csharp
LeadTimeDays: 0); // CreatedAt not tracked in current domain model
```
`CreatedAt` IS tracked (it's on `BaseEntity`). `LeadTimeDays` should be computed from `CreatedAt` and `Date`.

### T-3. `AppointmentReminderService` not registered in DI
**File:** `Vetolib.Agenda/ModuleServiceRegistrar.cs`
The `AppointmentReminderService : BackgroundService` exists in the Infrastructure folder but is never registered via `services.AddHostedService<AppointmentReminderService>()` in `ModuleServiceRegistrar`. The reminder service is dead code at runtime.

---

## 11. Additional Observations

### O-1. `UpdateAppointmentStatusRequest` and `TransitionAppointmentRequest` serve overlapping purposes
Both can transition appointment status. The `UpdateAppointmentStatusRequest` uses `AppointmentStatus` enum directly while `TransitionAppointmentRequest` uses string-based actions. Having two endpoints (`PATCH /{id}/status` and `PATCH /{id}/transition`) for the same operation creates API surface confusion.

### O-2. No pagination on ListAppointments
**File:** `Vetolib.Agenda/Application/Queries/ListAppointments/ListAppointmentsHandler.cs`
Returns all appointments for a date with no pagination. For a multi-vet clinic, a single day could have hundreds of appointments.

### O-3. `EditAppointmentHandler` has `IOutputCacheStore?` as nullable optional dependency
**File:** `Vetolib.Agenda/Application/Commands/EditAppointment/EditAppointmentHandler.cs:16`
```csharp
public EditAppointmentHandler(AgendaDbContext context, IOutputCacheStore? cache = null)
```
Only `EditAppointmentHandler` evicts cache. `CreateAppointmentHandler` and `UpdateAppointmentStatusHandler` do not, meaning the dashboard cache could be stale after creating or status-updating appointments.

### O-4. Missing `OwnerEmail` configuration in AppointmentConfiguration
**File:** `Vetolib.Agenda/Infrastructure/AppointmentConfiguration.cs`
The `OwnerEmail` property has no explicit configuration (max length, etc.). The migration sets `maxLength: 256` but the EF configuration doesn't declare this, relying on convention or the migration alone.

---

## File Inventory (audited)

### Vetolib.Agenda.Contracts (24 files, 0 issues in isolation)
All DTOs, enums, requests, queries, events, and interfaces are clean records/enums. No logic to audit.

### Vetolib.Agenda (37 non-obj non-Designer files)
- Domain: 3 files
- Commands: 12 files (4 commands x 3: command + handler + validator, minus 1 missing validator)
- Queries: 12 files (7 queries, handlers, 1 validator)
- Services: 4 files
- Infrastructure: 7 files (DbContext, factory, 2 configurations, reminder service, on-call reader, migrations)
- API: 2 endpoint files
- Module registration: 1 file
