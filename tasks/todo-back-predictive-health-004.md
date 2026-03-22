# todo-back-predictive-health-004 -- Background Job + Alert Generation Handler

**Module** : AI
**Dependencies** : todo-back-predictive-health-001 + 002 (rules + patient reader)
**Priority** : HIGH
**Estimated** : 2 hours

## Context

Nightly background job scans all patients per clinic and generates health alerts using the
rules engine. Also exposed as a manual trigger endpoint for admins.

## Skills to read

- `skills/cqrs-mediatr/SKILL.md`
- `skills/multitenant-efcore/SKILL.md`

## Spec

`docs/specs/PREDICTIVE-HEALTH-ALERTS-SPEC.md` (section 5)

## Scope

### 1. GenerateHealthAlertsCommand + Handler

```csharp
internal record GenerateHealthAlertsCommand : IRequest<Result<int>>;
// Returns count of new alerts generated
```

Handler logic:
1. Call `IPatientAlertDataReader.GetAllActivePatientsWithRecordsAsync()`
2. Load existing non-dismissed alerts from AIDbContext
3. For each patient, build `PatientAlertContext` from DTO
4. Run all `IHealthAlertRule` implementations (injected via `IEnumerable<IHealthAlertRule>`)
5. Collect new alerts, deduplicate against existing
6. Persist to AIDbContext
7. Return count

### 2. HealthAlertGeneratorJob (BackgroundService)

```csharp
internal class HealthAlertGeneratorJob : BackgroundService
{
    // Uses IServiceScopeFactory to create scoped MediatR sender
    // Runs daily at 06:00 UTC (configurable via appsettings)
    // Logs start/end + alert count
    // Catches and logs errors (never crashes the host)
}
```

Important: The background job must set `IClinicContext` for multi-tenancy.
Strategy: query all clinic IDs from the database, iterate, set context per clinic.
This requires a way to set ClinicId on the scoped IClinicContext.

Alternative (simpler for Phase 1): The manual trigger endpoint already has ClinicContext
from the HTTP request. The background job calls the same handler but iterates clinics.

### 3. Manual trigger endpoint

Already defined in task 003: `POST /api/v1/ai/health-alerts/generate`
This task implements the handler that the endpoint calls.

### 4. Rule DI registration

Register all 10 `IHealthAlertRule` implementations in AI's `ModuleServiceRegistrar`:
```csharp
services.AddTransient<IHealthAlertRule, CatRenalScreeningRule>();
services.AddTransient<IHealthAlertRule, CardiacBreedRule>();
// ... etc
```

### 5. Tests

- TU: GenerateHealthAlertsHandler with mocked IPatientAlertDataReader and rules
- TU: Deduplication logic (existing alert with same RuleId + PatientId -> skip)
- TI: End-to-end generation via manual trigger endpoint

## Completion criteria

- [ ] GenerateHealthAlertsCommand + Handler implemented
- [ ] HealthAlertGeneratorJob BackgroundService
- [ ] All 10 rules registered in DI
- [ ] Deduplication working (no duplicate alerts on re-run)
- [ ] Unit tests for handler (mock reader, verify alert creation)
- [ ] Integration test (manual trigger creates alerts)
- [ ] `dotnet build` GREEN
- [ ] `dotnet test` GREEN
