# todo-back-patient-species-003.md -- Add Falcon and Reptile to Species enum

**Module** : MedicalRecords
**Priority** : Haute
**Dependencies** : aucune (parallelisable avec 001 et 002)
**Skills** : `ardalis-result`, `cqrs-mediatr`

## Context

The current Species enum lacks Falcon (critical for UAE falconry market) and Reptile. These are additive enum values -- no data migration needed. Existing integer values in the database remain valid.

## Scope

### 1. Update Species enum

File: `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts/Species.cs`

```csharp
public enum Species
{
    Dog,      // 0
    Cat,      // 1
    Bird,     // 2
    Rabbit,   // 3
    Horse,    // 4
    Exotic,   // 5
    Camel,    // 6
    Falcon,   // 7 (NEW)
    Reptile   // 8 (NEW)
}
```

**Important**: Falcon and Reptile MUST be appended at the end to preserve existing integer mappings in the database. Do NOT reorder existing values.

### 2. No migration needed

Enum values are stored as integers in PostgreSQL. Adding new values at the end is backward-compatible.

### 3. Update frontend species lists (if any exist)

Check if there are hardcoded species lists in the frontend and update them.

### 4. Unit tests

- Test that `Species.Falcon` has integer value 7
- Test that `Species.Reptile` has integer value 8
- Test Patient.Create() with Falcon species
- Test Patient.Create() with Reptile species

## BDD

`tests/Vetolib.Tests.Acceptance/Features/MedicalRecords/PatientExtendedFields.feature`
- Scenarios: "Register a falcon patient", "Register a reptile patient", "All supported species can be used"

## Completion criteria

- [ ] `Species.Falcon` (value 7) added to enum
- [ ] `Species.Reptile` (value 8) added to enum
- [ ] No migration needed (additive change)
- [ ] Unit tests pass for new species values
- [ ] `dotnet build` + `dotnet test` GREEN
