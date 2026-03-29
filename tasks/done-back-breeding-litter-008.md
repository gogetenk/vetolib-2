# todo-back-breeding-litter-008.md -- Litter entity + CQRS handlers + endpoints

**Module** : Breeding
**Priority** : Haute
**Dependencies** : todo-back-breeding-scaffold-007
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`, `aspnet-minimal-api`, `reqnroll-bindings`

## Context

A Litter represents a birth event linking a mother (required Female patient), optional father, and resulting offspring. Each offspring becomes a Patient in MedicalRecords. The Breeding module reads patient data via IPatientReader (Contracts) and never references MedicalRecords runtime.

## Scope

### 1. Domain entities

#### Litter (AggregateRoot)

File: `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Domain/Litter.cs`

Properties:
- `Id`, `ClinicId` (IMultiTenant)
- `MotherPatientId` (Guid, required)
- `FatherPatientId` (Guid?, nullable)
- `ExternalFatherName` (string?, nullable)
- `BirthDate` (DateOnly)
- `BornCount` (int)
- `AliveCount` (int)
- `Notes` (string?)
- `Offspring` (List<LitterOffspring>)

Factory `Create()` returning `Result<Litter>`:
- AliveCount <= BornCount
- BornCount >= 1
- BirthDate not in the future
- Either FatherPatientId or ExternalFatherName, not both

#### LitterOffspring

File: `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Domain/LitterOffspring.cs`

Properties:
- `Id`, `ClinicId` (IMultiTenant)
- `LitterId` (Guid)
- `PatientId` (Guid)
- `BirthOrder` (int?)

### 2. DTOs in Contracts

File: `src/backend/Modules/Breeding/Vetolib.Breeding.Contracts/`

```csharp
public record CreateLitterRequest(
    Guid MotherPatientId,
    Guid? FatherPatientId,
    string? ExternalFatherName,
    DateOnly BirthDate,
    int BornCount,
    int AliveCount,
    string? Notes);

public record AddOffspringToLitterRequest(
    string Name, Sex Sex, string Breed, string? MicrochipNumber);

public record LitterDto(
    Guid Id, Guid MotherPatientId, string MotherName,
    Guid? FatherPatientId, string? FatherName,
    string? ExternalFatherName, DateOnly BirthDate,
    int BornCount, int AliveCount, string? Notes,
    IReadOnlyList<LitterOffspringDto> Offspring);

public record LitterOffspringDto(
    Guid PatientId, string Name, Sex Sex, int? BirthOrder);
```

### 3. Commands + Handlers

#### CreateLitter

- Resolve mother via `IPatientReader.GetPatientByIdAsync()`
- Validate mother is Female or SpayedFemale
- If FatherPatientId set: resolve father, validate same species as mother
- Call `Litter.Create()`
- Save to BreedingDbContext

#### AddOffspringToLitter

- This command creates a new Patient in MedicalRecords AND links it to the litter
- Use a domain event `OffspringRegisteredEvent` (MediatR notification) to create the Patient
- OR use `IPatientRecordWriter` interface (if it exists or needs to be added to MedicalRecords.Contracts)
- **Architecture decision**: prefer a command-based approach -- the Breeding handler sends a `CreatePatientCommand` via ISender (since both modules are in the same process). The created PatientId is linked as LitterOffspring.
- **Alternative**: Add `IPatientCreator` to MedicalRecords.Contracts. This is cleaner for module isolation.

### 4. Queries

- `GetLitterByIdQuery` -> `Result<LitterDto>`
- `GetLittersByMotherQuery(Guid motherPatientId)` -> `Result<IReadOnlyList<LitterDto>>`

### 5. Endpoints

File: `src/backend/Modules/Breeding/Vetolib.Breeding/Api/LitterEndpoints.cs`

```
POST   /api/v1/litters                    — VetOrAdmin
GET    /api/v1/litters/{id}               — authenticated
GET    /api/v1/patients/{id}/litters      — authenticated
POST   /api/v1/litters/{id}/offspring     — VetOrAdmin
```

### 6. EF Configuration

- `Litter` table with FK to nothing (PatientId is cross-module, no FK constraint)
- `LitterOffspring` table with FK to Litter
- Multi-tenancy filter on both

### 7. Validation rules (FluentValidation)

- `CreateLitterValidator`: BornCount >= 1, AliveCount <= BornCount, BirthDate <= today
- Handler-level: mother sex check, species match for father

### 8. Unit tests

- Litter.Create() with valid data
- Litter.Create() with AliveCount > BornCount -> Invalid
- Litter.Create() with BornCount < 1 -> Invalid
- Litter.Create() with future BirthDate -> Invalid
- Litter.Create() with both FatherPatientId and ExternalFatherName -> Invalid
- Handler: mother is male -> rejected
- Handler: father different species -> rejected

## BDD

`tests/Vetolib.Tests.Acceptance/Features/Breeding/Litter.feature`
- 9 scenarios covering registration, offspring, validation, tenant isolation

## Completion criteria

- [ ] Litter + LitterOffspring entities with Result<T> factories
- [ ] CreateLitter + AddOffspring commands with handlers
- [ ] GetLitter + GetLittersByMother queries
- [ ] 4 endpoints registered
- [ ] Validation: mother sex, species match, counts, date
- [ ] Tenant isolation (ClinicId on all entities)
- [ ] Unit tests for domain + handler validation paths
- [ ] `dotnet build` + `dotnet test` GREEN
