# todo-back-weight-history-004.md -- Weight history tracking (WeightEntry entity + endpoints)

**Module** : MedicalRecords
**Priority** : Haute
**Dependencies** : todo-back-patient-sex-001, todo-back-patient-microchip-002, todo-back-patient-species-003 (because Patient entity changes must be merged first to avoid migration conflicts)
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`, `aspnet-minimal-api`

## Context

Currently Patient has a single `WeightKg` snapshot. Breeders and vets need weight history for growth curves, illness detection, and pregnancy weight tracking. WeightEntry is an owned entity (append-only, immutable).

## Scope

### 1. Create WeightEntry entity

File: `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Domain/WeightEntry.cs`

```csharp
internal class WeightEntry : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateOnly RecordedAt { get; private set; }
    public decimal WeightKg { get; private set; }
    public string? Note { get; private set; }
    public string RecordedBy { get; private set; } = string.Empty;

    private WeightEntry() { }

    public static Result<WeightEntry> Create(Guid clinicId, Guid patientId, decimal weightKg,
        DateOnly? recordedAt, string? note, string recordedBy)
    {
        var errors = new List<ValidationError>();

        if (weightKg <= 0)
            errors.Add(new ValidationError(nameof(weightKg), "Weight must be greater than zero"));
        if (weightKg > 10000)
            errors.Add(new ValidationError(nameof(weightKg), "Weight exceeds maximum allowed value"));
        if (string.IsNullOrWhiteSpace(recordedBy))
            errors.Add(new ValidationError(nameof(recordedBy), "Recorder name is required"));

        if (errors.Count > 0)
            return Result<WeightEntry>.Invalid(errors);

        return Result<WeightEntry>.Success(new WeightEntry
        {
            ClinicId = clinicId,
            PatientId = patientId,
            WeightKg = weightKg,
            RecordedAt = recordedAt ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Note = note?.Length > 500 ? note[..500] : note,
            RecordedBy = recordedBy.Trim()
        });
    }
}
```

### 2. Add WeightEntries collection to Patient

```csharp
private readonly List<WeightEntry> _weightEntries = [];
public IReadOnlyList<WeightEntry> WeightEntries => _weightEntries.AsReadOnly();
```

### 3. DTOs in Contracts

File: `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts/WeightEntryDto.cs`

```csharp
public record AddWeightEntryRequest(decimal WeightKg, DateOnly? RecordedAt, string? Note);

public record WeightEntryDto(Guid Id, DateOnly RecordedAt, decimal WeightKg, string? Note, string RecordedBy);

public record WeightCurvePointDto(DateOnly Date, decimal WeightKg);
```

### 4. Commands + Queries

- `AddWeightEntryCommand` + `AddWeightEntryHandler`
  - Validates weight via `WeightEntry.Create()`
  - Saves entry
  - Updates `Patient.WeightKg` to the new value via `Patient.SetWeight()`
  - Returns `Result<WeightEntryDto>`

- `GetWeightHistoryQuery` + `GetWeightHistoryHandler`
  - Returns paginated `Result<IReadOnlyList<WeightEntryDto>>` (most recent first)

- `GetWeightCurveQuery` + `GetWeightCurveHandler`
  - Returns `Result<IReadOnlyList<WeightCurvePointDto>>` (sorted ascending by date)

### 5. Endpoints

File: `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Api/WeightEndpoints.cs`

```
POST /api/v1/patients/{id}/weights     — VetOrAdmin only
GET  /api/v1/patients/{id}/weights     — any authenticated
GET  /api/v1/patients/{id}/weights/curve — any authenticated
```

Register in `MapMedicalRecordsEndpoints()`.

### 6. EF Configuration

Configure `WeightEntry` in `MedicalRecordsDbContext`:
- Table name: `WeightEntries`
- FK to Patient (PatientId)
- Index on PatientId
- ClinicId for multi-tenancy filter

### 7. EF Migration

```sql
CREATE TABLE "WeightEntries" (
    "Id" uuid NOT NULL,
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "RecordedAt" date NOT NULL,
    "WeightKg" numeric(10,2) NOT NULL,
    "Note" varchar(500) NULL,
    "RecordedBy" varchar(200) NOT NULL,
    CONSTRAINT "PK_WeightEntries" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_WeightEntries_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id")
);
CREATE INDEX "IX_WeightEntries_PatientId" ON "WeightEntries" ("PatientId");
```

### 8. Authorization

- `POST weights`: VetOrAdmin only
- `GET weights`, `GET weights/curve`: any authenticated user
- ASSISTANT can view but not record (tested in BDD scenario)

### 9. Unit tests

- WeightEntry.Create() with valid data
- WeightEntry.Create() with weight <= 0 -> Invalid
- WeightEntry.Create() with weight > 10000 -> Invalid
- WeightEntry.Create() with missing recordedBy -> Invalid
- WeightEntry.Create() defaults RecordedAt to today
- Note truncation at 500 chars
- Adding weight entry updates Patient.WeightKg

## BDD

`tests/Vetolib.Tests.Acceptance/Features/MedicalRecords/WeightHistory.feature`
- 8 scenarios covering recording, history, curve, authorization

## Completion criteria

- [ ] `WeightEntry` entity with factory method returning `Result<WeightEntry>`
- [ ] Patient has `WeightEntries` collection
- [ ] Adding weight updates `Patient.WeightKg`
- [ ] 3 endpoints (POST + 2 GETs) with correct authorization
- [ ] DTOs in Contracts
- [ ] EF migration creates `WeightEntries` table
- [ ] Unit tests for validation + edge cases
- [ ] `dotnet build` + `dotnet test` GREEN
