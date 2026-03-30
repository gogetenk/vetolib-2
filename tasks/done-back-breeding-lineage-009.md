# todo-back-breeding-lineage-009.md -- PatientLineage entity + pedigree tree

**Module** : Breeding
**Priority** : Haute
**Dependencies** : todo-back-breeding-scaffold-007, todo-back-breeding-litter-008 (offspring auto-linked)
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`, `aspnet-minimal-api`

## Context

Lineage links patients to parents (MotherPatientId, FatherPatientId) and enables pedigree tree navigation. Since Breeding cannot modify MedicalRecords entities, lineage data is stored in the Breeding module's own `PatientLineage` entity. When offspring are added to a litter, lineage is auto-set.

## Scope

### 1. Domain entity: PatientLineage

File: `src/backend/Modules/Breeding/Vetolib.Breeding/Application/Domain/PatientLineage.cs`

```csharp
internal class PatientLineage : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }  // unique per clinic
    public Guid? MotherPatientId { get; private set; }
    public Guid? FatherPatientId { get; private set; }
    public string? RegistryNumber { get; private set; }  // LOF, LOOF, SIRE, EAHS
    public RegistryType? RegistryType { get; private set; }
}
```

Factory `Create()` / `SetParents()` returning `Result`:
- Parent and offspring must be same species (checked via IPatientReader)
- Mother must be Female/SpayedFemale
- Father must be Male/NeuteredMale
- Cannot set self as parent
- No circular lineage (A parent of B, B parent of A)

### 2. Enum: RegistryType (Contracts)

File: `src/backend/Modules/Breeding/Vetolib.Breeding.Contracts/RegistryType.cs`

```csharp
public enum RegistryType { LOF, LOOF, SIRE, EAHS, FEI, Other }
```

### 3. DTOs in Contracts

```csharp
public record SetLineageRequest(
    Guid? MotherPatientId, Guid? FatherPatientId,
    string? RegistryNumber, RegistryType? RegistryType);

public record PatientLineageDto(
    Guid PatientId, string PatientName,
    Guid? MotherPatientId, string? MotherName,
    Guid? FatherPatientId, string? FatherName,
    string? RegistryNumber, RegistryType? RegistryType);

public record PedigreeNodeDto(
    Guid PatientId, string Name, Species Species, string Breed, Sex Sex,
    string? RegistryNumber,
    PedigreeNodeDto? Mother, PedigreeNodeDto? Father);
```

### 4. Commands + Handlers

#### SetLineage

- Upsert PatientLineage for the given PatientId
- Validate sex/species rules via IPatientReader
- Circular lineage check: query existing lineage records

#### Auto-set lineage from litter

When offspring is added to a litter (task 008), the handler should also create/update a PatientLineage record linking the offspring to mother and father. This can be done:
- Via a domain event `OffspringAddedToLitterEvent` handled by a Breeding handler
- Or directly in the AddOffspring handler (same module)

### 5. Queries

#### GetLineage
- Returns direct parents of a patient

#### GetPedigree(patientId, generations=3)
- Recursive query: for each ancestor, fetch their PatientLineage
- Build a tree of `PedigreeNodeDto` up to N generations
- Max 3 generations by default (configurable query param)

#### GetDescendants(patientId)
- Find all PatientLineage records where MotherPatientId or FatherPatientId = given patientId
- Recursive for grandchildren

### 6. Endpoints

```
PUT  /api/v1/patients/{id}/lineage              — VetOrAdmin
GET  /api/v1/patients/{id}/lineage              — authenticated
GET  /api/v1/patients/{id}/pedigree?generations=3 — authenticated
GET  /api/v1/patients/{id}/descendants           — authenticated
```

### 7. EF Configuration

- `PatientLineage` table
- Unique index on (ClinicId, PatientId)
- Multi-tenancy filter

### 8. Unit tests

- SetParents with valid data
- SetParents: mother is Male -> rejected
- SetParents: father is Female -> rejected
- SetParents: different species -> rejected
- SetParents: self as parent -> rejected
- Circular lineage detection
- Pedigree tree building (3 generations)

## BDD

`tests/Vetolib.Tests.Acceptance/Features/Breeding/Lineage.feature`
- 6 scenarios: set parents, navigate up/down, species validation, LOF, falcon pedigree

## Completion criteria

- [ ] PatientLineage entity with Result<T> factory
- [ ] RegistryType enum in Contracts
- [ ] SetLineage command with sex/species/circular validation
- [ ] Pedigree tree query (recursive, configurable depth)
- [ ] Descendants query
- [ ] Auto-link from litter offspring
- [ ] 4 endpoints registered
- [ ] Unit tests for all validation rules
- [ ] `dotnet build` + `dotnet test` GREEN
