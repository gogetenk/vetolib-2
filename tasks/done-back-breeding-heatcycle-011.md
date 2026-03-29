# todo-back-breeding-heatcycle-011.md -- HeatCycle tracking + prediction

**Module** : Breeding
**Priority** : Moyenne
**Dependencies** : todo-back-breeding-scaffold-007
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`, `aspnet-minimal-api`

## Context

Recording heat cycles allows breeders to predict optimal breeding windows. Particularly important for dogs (estrus every ~6 months), cats (seasonal polyestrus), and horses (seasonal). Simple statistical prediction based on historical interval averages.

## Scope

### 1. Domain entity: HeatCycle

File: `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Domain/HeatCycle.cs`

Properties:
- `Id`, `ClinicId` (IMultiTenant)
- `PatientId` (Guid)
- `StartDate` (DateOnly)
- `EndDate` (DateOnly?)
- `Notes` (string?)

Factory `Create()` returning `Result<HeatCycle>`:
- EndDate > StartDate (if EndDate provided)
- No overlapping cycles for same patient (checked in handler)

### 2. Prediction logic

File: `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Domain/HeatCyclePrediction.cs`

```csharp
internal static class HeatCyclePrediction
{
    public static Result<HeatPredictionDto> Predict(IReadOnlyList<HeatCycle> cycles)
    {
        if (cycles.Count < 2)
            return Result<HeatPredictionDto>.Error("At least 2 recorded cycles are needed for prediction");

        var ordered = cycles.OrderBy(c => c.StartDate).ToList();
        var intervals = new List<int>();
        for (int i = 1; i < ordered.Count; i++)
            intervals.Add(ordered[i].StartDate.DayNumber - ordered[i-1].StartDate.DayNumber);

        var avgInterval = (int)Math.Round(intervals.Average());
        var predictedNext = ordered.Last().StartDate.AddDays(avgInterval);
        var hasConfidence = cycles.Count >= 3;

        return Result<HeatPredictionDto>.Success(
            new HeatPredictionDto(predictedNext, avgInterval, hasConfidence));
    }
}
```

### 3. DTOs in Contracts

```csharp
public record RecordHeatCycleRequest(
    DateOnly StartDate, DateOnly? EndDate, string? Notes);

public record HeatCycleDto(
    Guid Id, DateOnly StartDate, DateOnly? EndDate,
    int? DurationDays, string? Notes);

public record HeatPredictionDto(
    DateOnly PredictedNextStart, int AverageIntervalDays, bool HasConfidence);
```

### 4. Commands

- `RecordHeatCycleCommand` + handler
  - Validate patient is Female (not SpayedFemale) via IPatientReader
  - Check no overlapping cycles
  - Save

### 5. Queries

- `GetHeatCyclesQuery(Guid patientId)` -> `Result<IReadOnlyList<HeatCycleDto>>`
- `PredictNextHeatQuery(Guid patientId)` -> `Result<HeatPredictionDto>`

### 6. Endpoints

```
POST /api/v1/patients/{id}/heat-cycles              — VetOrAdmin
GET  /api/v1/patients/{id}/heat-cycles              — VetOrAdmin (ASSISTANT excluded)
GET  /api/v1/patients/{id}/heat-cycles/prediction   — VetOrAdmin
```

**Authorization note**: ASSISTANT has NO access to heat cycle data (sensitive breeding information). Only VET and ADMIN.

### 7. Unit tests

- HeatCycle.Create() with valid data
- HeatCycle.Create() with EndDate < StartDate -> Invalid
- Prediction with 2 cycles -> returns prediction without confidence
- Prediction with 3+ cycles -> returns prediction with confidence
- Prediction with < 2 cycles -> Error
- Handler: male patient -> rejected
- Handler: spayed patient -> rejected
- Handler: overlapping cycles -> rejected

## BDD

`tests/Vetolib.Tests.Acceptance/Features/Breeding/HeatCycle.feature`
- 8 scenarios covering recording, prediction, validation, authorization

## Completion criteria

- [ ] HeatCycle entity with Result<T> factory
- [ ] Prediction algorithm (average interval)
- [ ] RecordHeatCycle command with sex/overlap validation
- [ ] 2 queries (history + prediction)
- [ ] 3 endpoints with VetOrAdmin authorization (no ASSISTANT)
- [ ] Unit tests for domain + prediction + handler paths
- [ ] `dotnet build` + `dotnet test` GREEN
