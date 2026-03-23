# todo-back-predictive-health-001 -- Domain Model + Rules Engine

**Module** : AI
**Dependencies** : none
**Priority** : HIGH
**Estimated** : 2-3 hours

## Context

Predictive Health Alerts Phase 1: deterministic rules engine based on breed/age/medical history.
No ML. Static rules match veterinary guidelines (AAHA/WSAVA).

## Skills to read

- `skills/ardalis-result/SKILL.md`
- `skills/ardalis-modular-monolith/SKILL.md`
- `skills/cqrs-mediatr/SKILL.md`

## Spec

`docs/specs/PREDICTIVE-HEALTH-ALERTS-SPEC.md` (sections 2, 3)

## BDD

`tests/Vetolib.Tests.Acceptance/Features/AI/PredictiveHealthAlerts.feature` (@wip)

## Scope

### 1. HealthAlert entity (in Vetolib.AI/Application/Domain/)

```csharp
internal class HealthAlert : BaseEntity, IMultiTenant
{
    Guid ClinicId
    Guid PatientId
    HealthAlertType AlertType       // enum in Contracts
    HealthAlertSeverity Severity    // enum in Contracts
    string Title
    string Description
    string? RecommendedAction
    string RuleId                   // e.g. "BREED-RENAL-001"
    int RiskScore                   // 0-100
    HealthAlertStatus Status        // enum in Contracts
    DateTime GeneratedAt
    DateTime? DismissedAt
    string? DismissedReason
    string? DismissedByName
    DateTime? AcknowledgedAt
    Guid? ConvertedToAppointmentId
}
```

Factory method: `static Result<HealthAlert> Create(...)` -- validates required fields.
Domain methods:
- `Result Dismiss(string reason, string dismissedByName)` -- sets Status=Dismissed, DismissedAt=now
- `Result Acknowledge()` -- sets Status=Acknowledged, AcknowledgedAt=now
- `Result MarkScheduled(Guid appointmentId)` -- sets Status=Scheduled, ConvertedToAppointmentId

### 2. Enums in AI.Contracts

```csharp
public enum HealthAlertType
{
    BreedSpecificScreening, AgeRelatedScreening, WeightTrend,
    VaccineGap, DentalProphylaxis, ChronicDiseaseFollowUp,
    SeniorWellness, MedicationReview
}

public enum HealthAlertSeverity { Low, Medium, High }

public enum HealthAlertStatus { New, Acknowledged, Scheduled, Dismissed }
```

### 3. HealthAlertDto in AI.Contracts

```csharp
public record HealthAlertDto(
    Guid Id, Guid PatientId, string PatientName,
    HealthAlertType AlertType, HealthAlertSeverity Severity,
    string Title, string Description, string? RecommendedAction,
    string RuleId, int RiskScore, HealthAlertStatus Status,
    DateTime GeneratedAt, DateTime? DismissedAt, DateTime? AcknowledgedAt,
    Guid? ConvertedToAppointmentId);
```

### 4. IPatientAlertDataReader contract (in MedicalRecords.Contracts)

```csharp
public interface IPatientAlertDataReader
{
    Task<Result<IReadOnlyList<PatientAlertDataDto>>> GetAllActivePatientsWithRecordsAsync(
        CancellationToken ct = default);
}

public record PatientAlertDataDto(
    Guid PatientId, string Name, Species Species, string Breed,
    DateOnly BirthDate, decimal? WeightKg,
    IReadOnlyList<MedicalRecordSummaryDto> RecentRecords,
    IReadOnlyList<WeightEntryDto> WeightHistory);

public record WeightEntryDto(decimal WeightKg, DateTime RecordedAt);
```

### 5. Rules Engine

Interface:
```csharp
internal interface IHealthAlertRule
{
    string RuleId { get; }
    IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts);
}
```

PatientAlertContext:
```csharp
internal record PatientAlertContext(
    Guid PatientId, Guid ClinicId, string Name,
    Species Species, string Breed, DateOnly BirthDate,
    decimal? WeightKg,
    IReadOnlyList<MedicalRecordSummary> RecentRecords,
    IReadOnlyList<WeightEntry> WeightHistory);

internal record MedicalRecordSummary(string Diagnosis, string Treatment, DateTime ExaminedAt);
internal record WeightEntry(decimal WeightKg, DateTime RecordedAt);
```

10 rules to implement (see spec section 3 for details):
1. `CatRenalScreeningRule` -- BREED-RENAL-001
2. `CardiacBreedRule` -- BREED-CARDIAC-001
3. `SeniorWellnessRule` -- SENIOR-WELLNESS-001
4. `WeightTrendRule` -- WEIGHT-TREND-001
5. `VaccinationOverdueRule` -- VACCINE-GAP-001
6. `BrachycephalicAirwayRule` -- BRACHY-RESP-001
7. `HipDysplasiaRule` -- BREED-HIP-001
8. `DiabetesRiskRule` -- DIABETES-RISK-001
9. `DentalProphylaxisRule` -- DENTAL-001
10. `ArthritisFollowUpRule` -- ARTHRITIS-001

Each rule:
- Checks species/breed/age/record keywords
- Checks dedup against existingAlerts (same RuleId + PatientId + non-dismissed)
- Returns 0 or 1 HealthAlert

### 6. BreedRiskData (static dictionary)

Static class with breed-to-conditions mapping. NOT an EF entity. Used by rules for breed matching.
Include top 10 dog breeds + top 5 cat breeds from the study.

### 7. EF Configuration

- `HealthAlertConfiguration.cs` in Infrastructure/
- Table: `ai."HealthAlerts"`
- Index on (ClinicId, PatientId, RuleId, Status)
- Index on (ClinicId, Status, Severity)
- Add `DbSet<HealthAlert>` to AIDbContext

### 8. EF Migration

- Generate migration for the new HealthAlerts table

## Completion criteria

- [ ] HealthAlert entity with Create/Dismiss/Acknowledge/MarkScheduled methods returning Result
- [ ] 3 enums + DTO in AI.Contracts
- [ ] IPatientAlertDataReader + DTOs in MedicalRecords.Contracts
- [ ] IHealthAlertRule interface + 10 rule implementations
- [ ] BreedRiskData static class
- [ ] EF configuration + migration
- [ ] Unit tests for all 10 rules (edge cases: age boundaries, keyword matching, dedup)
- [ ] Unit tests for HealthAlert domain methods
- [ ] `dotnet build` GREEN
- [ ] `dotnet test` (unit) GREEN
