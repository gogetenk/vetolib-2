# Predictive Health Alerts -- Architecture Specification

> Phase 1: Rules Engine (no ML). Target: half-day implementation per task.
> PO-validated feature. See study: docs/studies/PREDICTIVE-HEALTH-INNOVATION-2026.md
> BDD spec: tests/Vetolib.Tests.Acceptance/Features/AI/PredictiveHealthAlerts.feature

---

## 1. Module Placement

**Predictive Health Alerts lives in the existing `Vetolib.AI` module.**

Rationale:
- The AI module already owns triage, no-show prediction, SOAP notes, and drug interactions
- Health alerts are another form of clinical decision support -- same domain
- The AI module already references `MedicalRecords.Contracts` for patient data
- No new module needed -- avoids assembly proliferation

New code goes into:
```
Modules/AI/
  Vetolib.AI.Contracts/
    HealthAlertDto.cs                    (public DTO)
    HealthAlertSeverity.cs               (public enum)
    HealthAlertStatus.cs                 (public enum)
    HealthAlertType.cs                   (public enum)
    IHealthAlertReader.cs                (public contract for other modules)
  Vetolib.AI/
    Application/
      Domain/
        HealthAlert.cs                   (entity)
        BreedRiskProfile.cs              (static rule data, NOT an EF entity)
        HealthAlertRule.cs               (rule abstraction)
      Rules/
        IHealthAlertRule.cs              (interface)
        CatRenalScreeningRule.cs
        CardiacBreedRule.cs
        SeniorWellnessRule.cs
        WeightTrendRule.cs
        VaccinationOverdueRule.cs
        BrachycephalicAirwayRule.cs
        (+ 4 more rules)
        BreedRiskData.cs                 (static breed risk dictionary)
      Commands/
        DismissHealthAlert/
        AcknowledgeHealthAlert/
        ConvertAlertToAppointment/
        GenerateHealthAlerts/            (manual trigger + background job)
      Queries/
        GetHealthAlerts/
        GetPatientHealthAlerts/
    Infrastructure/
      HealthAlertConfiguration.cs        (EF config)
    Api/
      HealthAlertEndpoints.cs            (Minimal API)
```

## 2. Domain Model

### HealthAlert Entity

```csharp
internal class HealthAlert : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public HealthAlertType AlertType { get; private set; }
    public HealthAlertSeverity Severity { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string? RecommendedAction { get; private set; }
    public string RuleId { get; private set; }           // e.g. "BREED-RENAL-001"
    public int RiskScore { get; private set; }            // 0-100
    public HealthAlertStatus Status { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public DateTime? DismissedAt { get; private set; }
    public string? DismissedReason { get; private set; }
    public string? DismissedByName { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public Guid? ConvertedToAppointmentId { get; private set; }

    // Factory: Result<HealthAlert> Create(...)
    // Methods: Result Dismiss(...), Result Acknowledge(), Result MarkScheduled(appointmentId)
}
```

Key design decisions:
- `DismissedReason` + `DismissedByName` per the .feature ("Owner declined screening" + vet name)
- `ConvertedToAppointmentId` links to Agenda without coupling (just a Guid, no FK)
- `RuleId` traces which rule generated the alert (auditable)
- `Status` enum: `New`, `Acknowledged`, `Scheduled`, `Dismissed`
- Multi-tenant via `ClinicId` + global query filter (automatic)

### BreedRiskData (Static, Not an Entity)

A static C# class with dictionaries. NOT stored in DB for Phase 1 -- keeps implementation to half a day. Can be promoted to a seeded DB entity in Phase 2.

```csharp
internal static class BreedRiskData
{
    // Dictionary<(Species, string breed), List<BreedRisk>>
    // BreedRisk = record(string ConditionName, string Category, int BaseRiskScore,
    //                     int OnsetAgeMonths, int ScreeningIntervalMonths,
    //                     string ScreeningProcedure, string Description)
}
```

Top 10 dog breeds + top 5 cat breeds covered in Phase 1 (matches study section 4).

## 3. Rules Engine

### Interface

```csharp
internal interface IHealthAlertRule
{
    string RuleId { get; }
    Task<IReadOnlyList<HealthAlert>> EvaluateAsync(
        PatientAlertContext patient,
        IReadOnlyList<HealthAlert> existingAlerts,
        CancellationToken ct);
}
```

### PatientAlertContext (read model)

```csharp
internal record PatientAlertContext(
    Guid PatientId,
    Guid ClinicId,
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    decimal? WeightKg,
    IReadOnlyList<MedicalRecordSummary> RecentRecords,
    IReadOnlyList<WeightEntry> WeightHistory);

internal record MedicalRecordSummary(
    string Diagnosis,
    string Treatment,
    DateTime ExaminedAt);

internal record WeightEntry(decimal WeightKg, DateTime RecordedAt);
```

### 10 Rules for Phase 1

| # | RuleId | Trigger | Alert Title | Severity |
|---|---|---|---|---|
| 1 | `BREED-RENAL-001` | Cat >= 7y, no renal panel in 12+ months | "Annual renal screening overdue" | High |
| 2 | `BREED-CARDIAC-001` | CKCS/Doberman/Boxer >= 5y, no cardiac exam in 12+ months | "Annual cardiac screening recommended" | Medium |
| 3 | `SENIOR-WELLNESS-001` | Dog >= 8y or Cat >= 10y, no wellness exam in 6+ months | "Senior wellness exam recommended" | Medium |
| 4 | `WEIGHT-TREND-001` | Weight increase > 15% over last 3 visits | "Weight gain trend detected" | High |
| 5 | `VACCINE-GAP-001` | Core vaccination overdue by 30+ days | "Core vaccination overdue" | High |
| 6 | `BRACHY-RESP-001` | Brachycephalic breed + respiratory diagnosis in history | "Annual airway assessment recommended" | Medium |
| 7 | `BREED-HIP-001` | Golden/Lab/GSD >= 2y, no hip X-ray in records | "Hip dysplasia screening recommended" | Medium |
| 8 | `DIABETES-RISK-001` | Cat, overweight, age >= 5 | "Obesity screening recommended" | Medium |
| 9 | `DENTAL-001` | Dog/Cat >= 3y, no dental cleaning in 24+ months | "Dental prophylaxis recommended" | Low |
| 10 | `ARTHRITIS-001` | Large breed dog >= 7y, arthritis in history, no follow-up in 4+ months | "Arthritis management review due" | Medium |

### Rule evaluation strategy

Rules scan medical record free-text fields (`Diagnosis`, `Treatment`) for keywords:
- "renal panel", "kidney", "BUN", "creatinine" -> renal screening done
- "cardiac", "heart", "echocardiogram", "murmur" -> cardiac exam done
- "vaccine", "vaccination", "DHPP", "rabies" -> vaccination done
- "respiratory", "breathing", "BOAS", "airway" -> respiratory diagnosis
- "hip", "radiograph", "x-ray" -> hip screening done
- "dental", "prophylaxis", "teeth", "scaling" -> dental done
- "arthritis", "NSAID", "mobility", "joint" -> arthritis history

This is Phase 1 keyword matching. Phase 2 replaces with NLP/LLM extraction.

### Deduplication

Before persisting a new alert, check if an active (non-dismissed) alert with the same `RuleId` + `PatientId` already exists. If so, skip. This prevents duplicate alerts on consecutive nightly runs.

## 4. Inter-Module Communication

### Reading patient data

The AI module reads from `MedicalRecords.Contracts.IPatientReader` (already exists).

**New contract needed** in `MedicalRecords.Contracts`:

```csharp
public interface IPatientAlertDataReader
{
    Task<Result<IReadOnlyList<PatientAlertDataDto>>> GetAllActivePatientsWithRecordsAsync(
        CancellationToken ct);
}

public record PatientAlertDataDto(
    Guid PatientId,
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    decimal? WeightKg,
    IReadOnlyList<MedicalRecordSummaryDto> RecentRecords,
    IReadOnlyList<WeightEntryDto> WeightHistory);

public record WeightEntryDto(decimal WeightKg, DateTime RecordedAt);
```

Implementation lives in `MedicalRecords` runtime, registered via DI. The AI module only sees the contract.

### Converting alert to appointment

When the vet clicks "Schedule appointment from alert", the handler:
1. Creates a pre-filled appointment request via `Agenda.Contracts` (if an interface exists), OR
2. Returns the data needed for the frontend to navigate to the appointment creation form with pre-filled fields

For Phase 1: option 2 (simpler). The endpoint returns appointment pre-fill data, and the frontend navigates. No direct Agenda coupling.

## 5. Background Job

### Strategy: MediatR command triggered by BackgroundService

```csharp
internal class HealthAlertGeneratorJob : BackgroundService
{
    // Runs once daily at 06:00 local time
    // Sends GenerateHealthAlertsCommand via MediatR
    // The handler:
    //   1. Calls IPatientAlertDataReader.GetAllActivePatientsWithRecordsAsync()
    //   2. For each patient, runs all IHealthAlertRule implementations
    //   3. Deduplicates against existing alerts
    //   4. Persists new HealthAlerts to AIDbContext
}
```

Manual trigger endpoint also available: `POST /api/v1/ai/health-alerts/generate` (admin only).

## 6. API Endpoints

All under `/api/v1/ai/health-alerts`, added to existing `AIEndpoints.cs`.

| Method | Path | Handler | Auth |
|---|---|---|---|
| GET | `/api/v1/ai/health-alerts` | GetHealthAlertsQuery | ClinicStaff |
| GET | `/api/v1/ai/health-alerts/patient/{patientId}` | GetPatientHealthAlertsQuery | ClinicStaff |
| PATCH | `/api/v1/ai/health-alerts/{id}/dismiss` | DismissHealthAlertCommand | VetOrAdmin |
| PATCH | `/api/v1/ai/health-alerts/{id}/acknowledge` | AcknowledgeHealthAlertCommand | VetOrAdmin |
| POST | `/api/v1/ai/health-alerts/{id}/convert-to-appointment` | ConvertAlertToAppointmentCommand | VetOrAdmin |
| POST | `/api/v1/ai/health-alerts/generate` | GenerateHealthAlertsCommand | VetOrAdmin |

## 7. EF Configuration

New table `ai.HealthAlerts` with:
- Standard BaseEntity columns (Id, CreatedAt, UpdatedAt)
- ClinicId (tenant filter, automatic)
- Index on (ClinicId, PatientId, RuleId, Status) for dedup queries
- Index on (ClinicId, Status, Severity) for dashboard list queries

Migration added to AI module's DbContext.

## 8. Task Breakdown

See `tasks/todo-back-predictive-health-*.md` files:

1. **todo-back-predictive-health-001.md** -- Domain + Rules Engine + IPatientAlertDataReader contract
2. **todo-back-predictive-health-002.md** -- IPatientAlertDataReader implementation in MedicalRecords
3. **todo-back-predictive-health-003.md** -- Endpoints + Handlers (CRUD on alerts)
4. **todo-back-predictive-health-004.md** -- Background job + GenerateHealthAlerts handler
5. **todo-front-predictive-health-001.md** -- Dashboard alert panel + patient alert tab (MSW)

Tasks 1 and 2 can run in parallel. Task 3 depends on 1. Task 4 depends on 1+2.
Task 5 (frontend) can start immediately with MSW.

## 9. Test Strategy (Hourglass)

- **TU**: Rule evaluation logic (10 rules x edge cases), HealthAlert domain factory, deduplication logic
- **TI**: 1 per endpoint (6 endpoints), EF migration applies, multi-tenancy isolation
- **TF**: Already written in PredictiveHealthAlerts.feature (12 scenarios)
