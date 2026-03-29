# todo-back-patient-sex-001.md -- Add Sex enum and field to Patient entity

**Module** : MedicalRecords
**Priority** : Critique
**Dependencies** : aucune
**Skills** : `ardalis-result`, `cqrs-mediatr`, `ardalis-modular-monolith`, `multitenant-efcore`, `aspnet-minimal-api`

## Context

The Patient entity currently has no sex/gender field. This is fundamental for medical decisions (dosage, surgery type, reproductive assessments) and is a prerequisite for the Breeding module (Phase 2) which validates mother/father sex.

## Scope

### 1. Add `Sex` enum in Contracts (public)

File: `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts/Sex.cs`

```csharp
using System.Text.Json.Serialization;

namespace Vetolib.MedicalRecords.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Sex
{
    Male,
    Female,
    NeuteredMale,
    SpayedFemale,
    Unknown
}
```

### 2. Add Sex property to Patient entity

File: `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Domain/Patient.cs`

- Add `public Sex Sex { get; private set; } = Sex.Unknown;`
- Update `Create()` factory: add optional `Sex? sex = null` parameter, default to `Sex.Unknown`
- Update `UpdateInfo()`: add optional `Sex? sex = null` parameter

### 3. Update DTOs in Contracts

- `CreatePatientRequest` — add `Sex? Sex` (optional)
- `UpdatePatientRequest` — add `Sex? Sex` (optional)
- `PatientDto` — add `Sex Sex` field
- `PatientDetailDto` — no change needed (wraps PatientDto)

### 4. Update Command + Handler

- `CreatePatientCommand` — add `Sex?` parameter
- `CreatePatientHandler` — pass Sex to `Patient.Create()`
- `UpdatePatientCommand` — add `Sex?` parameter
- `UpdatePatientHandler` — pass Sex to `Patient.UpdateInfo()`

### 5. Update PatientEndpoints

- `CreatePatient` method: pass `request.Sex` to command
- `UpdatePatient` method: pass `request.Sex` to command

### 6. Update `ToDto()` on Patient

- Include `Sex` in the `PatientDto` constructor call

### 7. EF Migration

- Add migration: `ALTER TABLE "Patients" ADD COLUMN "Sex" integer NOT NULL DEFAULT 4;` (4 = Unknown)
- All existing patients get `Sex.Unknown` (backward-compatible)

### 8. Unit tests

- Test `Patient.Create()` with explicit Sex
- Test `Patient.Create()` without Sex defaults to Unknown
- Test `Patient.UpdateInfo()` with Sex change (Male -> NeuteredMale)
- Test Sex enum serialization (JsonStringEnumConverter)

## BDD

`tests/Vetolib.Tests.Acceptance/Features/MedicalRecords/PatientExtendedFields.feature`
- Scenarios: "Register a new patient with sex", "Register a neutered patient", "Sex defaults to Unknown when not provided", "Update sex after neutering"

## Completion criteria

- [ ] `Sex` enum exists in `Vetolib.MedicalRecords.Contracts`
- [ ] `Patient.Sex` property with default `Unknown`
- [ ] `Create()` and `UpdateInfo()` accept optional `Sex`
- [ ] DTOs updated (CreatePatientRequest, UpdatePatientRequest, PatientDto)
- [ ] EF migration created and applies cleanly
- [ ] Unit tests for Sex on Patient.Create/UpdateInfo
- [ ] `dotnet build` + `dotnet test` GREEN
