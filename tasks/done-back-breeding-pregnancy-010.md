# todo-back-breeding-pregnancy-010.md -- Pregnancy tracking entity + endpoints

**Module** : Breeding
**Priority** : Haute
**Dependencies** : todo-back-breeding-scaffold-007
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`, `aspnet-minimal-api`

## Context

Track pregnancies from mating to delivery. Critical for equine breeding (340-day gestation), canine (63 days), feline (65 days), and falcon (31-33 days). Vets schedule follow-up exams at appropriate intervals. Expected due date is auto-calculated based on species gestation period.

## Scope

### 1. Domain entities

#### Pregnancy (AggregateRoot)

Properties:
- `Id`, `ClinicId` (IMultiTenant)
- `PatientId` (Guid, the mother)
- `FatherPatientId` (Guid?, optional)
- `MatingDate` (DateOnly)
- `MatingMethod` (enum: Natural, ArtificialInsemination, EmbryoTransfer)
- `ExpectedDueDate` (DateOnly, auto-calculated)
- `ActualDeliveryDate` (DateOnly?)
- `Outcome` (enum?: LiveBirth, Stillbirth, Miscarriage, Abortion, Unknown)
- `OffspringCount` (int?)
- `Status` (enum: Active, Completed, Lost)
- `Notes` (string?)
- `ScheduledChecks` (List<PregnancyCheck>)

Factory `Create()` returning `Result<Pregnancy>`:
- Patient must be Female (checked via IPatientReader in handler)
- Patient must NOT be SpayedFemale
- MatingDate not in future
- Auto-calculate ExpectedDueDate using species gestation table
- No overlapping active pregnancy for same patient

#### PregnancyCheck

Properties:
- `Id`, `ClinicId` (IMultiTenant)
- `PregnancyId` (Guid)
- `ScheduledDate` (DateOnly)
- `CheckType` (enum: Ultrasound, BloodTest, PhysicalExam, Other)
- `Note` (string?)
- `CompletedAt` (DateTime?)
- `Result` (string?)

### 2. Species gestation periods (constants)

```csharp
internal static class GestationPeriods
{
    public static int GetDefaultDays(Species species) => species switch
    {
        Species.Dog => 63,
        Species.Cat => 65,
        Species.Horse => 340,
        Species.Camel => 390,
        Species.Falcon => 32,
        Species.Rabbit => 31,
        _ => 60  // default fallback
    };
}
```

### 3. Enums in Contracts

```csharp
public enum MatingMethod { Natural, ArtificialInsemination, EmbryoTransfer }
public enum PregnancyOutcome { LiveBirth, Stillbirth, Miscarriage, Abortion, Unknown }
public enum PregnancyStatus { Active, Completed, Lost }
public enum PregnancyCheckType { Ultrasound, BloodTest, PhysicalExam, Other }
```

### 4. DTOs in Contracts

```csharp
public record CreatePregnancyRequest(
    Guid PatientId, Guid? FatherPatientId,
    DateOnly MatingDate, MatingMethod Method, string? Notes);

public record RecordDeliveryRequest(
    DateOnly DeliveryDate, PregnancyOutcome Outcome, int? OffspringCount);

public record ScheduleCheckRequest(
    DateOnly ScheduledDate, PregnancyCheckType CheckType, string? Note);

public record CompleteCheckRequest(string? Result);

public record PregnancyDto(
    Guid Id, Guid PatientId, string PatientName,
    Guid? FatherPatientId, string? FatherName,
    DateOnly MatingDate, MatingMethod Method,
    DateOnly ExpectedDueDate, DateOnly? ActualDeliveryDate,
    PregnancyOutcome? Outcome, int? OffspringCount,
    PregnancyStatus Status, string? Notes,
    IReadOnlyList<PregnancyCheckDto> ScheduledChecks);

public record PregnancyCheckDto(
    Guid Id, DateOnly ScheduledDate, PregnancyCheckType CheckType,
    string? Note, DateTime? CompletedAt, string? Result);
```

### 5. Commands + Handlers

- `CreatePregnancyCommand` + handler (validate sex, no overlap, auto-calculate due date)
- `RecordDeliveryCommand` + handler (set ActualDeliveryDate, Outcome, Status=Completed)
- `RecordLossCommand` + handler (set Outcome=Miscarriage/Abortion, Status=Lost)
- `ScheduleCheckCommand` + handler
- `CompleteCheckCommand` + handler

### 6. Queries

- `GetPregnancyByIdQuery` -> `Result<PregnancyDto>`
- `GetPregnanciesByPatientQuery` -> `Result<IReadOnlyList<PregnancyDto>>`
- `GetActivePregnanciesQuery` -> `Result<IReadOnlyList<PregnancyDto>>` (sorted by ExpectedDueDate)

### 7. Endpoints

```
POST   /api/v1/pregnancies                       — VetOrAdmin
GET    /api/v1/pregnancies/{id}                  — authenticated
GET    /api/v1/patients/{id}/pregnancies         — authenticated
GET    /api/v1/pregnancies/active                — authenticated
PUT    /api/v1/pregnancies/{id}/delivery         — VetOrAdmin
PUT    /api/v1/pregnancies/{id}/loss             — VetOrAdmin
POST   /api/v1/pregnancies/{id}/checks           — VetOrAdmin
PUT    /api/v1/pregnancies/{id}/checks/{checkId} — VetOrAdmin
```

### 8. Unit tests

- Pregnancy.Create() with valid data + auto-calculated due date
- Pregnancy.Create() for different species -> correct gestation days
- Pregnancy.Create() with future mating date -> Invalid
- RecordDelivery: sets status to Completed
- RecordDelivery: delivery date < mating date -> Invalid
- RecordLoss: sets status to Lost
- ScheduleCheck: adds check to list
- CompleteCheck: sets CompletedAt and Result
- Handler: male patient -> rejected
- Handler: spayed patient -> rejected
- Handler: overlapping active pregnancy -> rejected

## BDD

`tests/Vetolib.Tests.Acceptance/Features/Breeding/Pregnancy.feature`
- 11 scenarios covering mating methods, due date calculation, delivery, loss, checks, authorization

## Completion criteria

- [ ] Pregnancy + PregnancyCheck entities with Result<T>
- [ ] Species-specific gestation calculation
- [ ] All 5 commands with handlers
- [ ] 3 queries with handlers
- [ ] 8 endpoints registered
- [ ] Validation: sex, spay, overlap, dates
- [ ] Unit tests for all validation paths
- [ ] `dotnet build` + `dotnet test` GREEN
