# todo-back-patient-microchip-002.md -- Add MicrochipNumber field to Patient

**Module** : MedicalRecords
**Priority** : Critique
**Dependencies** : aucune (parallelisable avec todo-back-patient-sex-001)
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`, `aspnet-minimal-api`

## Context

Microchip identification is mandatory in France (since 2012) and increasingly required in UAE. The ISO 11784/11785 standard defines a 15-digit numeric format. Vetolib must store, validate, and search by this number.

## Scope

### 1. Add MicrochipNumber to Patient entity

File: `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Domain/Patient.cs`

- Add `public string? MicrochipNumber { get; private set; }`
- Update `Create()`: add optional `string? microchipNumber = null` parameter
- Add validation in `Create()`: if provided, must match `^\d{15}$` (exactly 15 digits)
- Update `UpdateInfo()`: add optional `string? microchipNumber` parameter with same validation

### 2. Update DTOs in Contracts

- `CreatePatientRequest` — add `string? MicrochipNumber`
- `UpdatePatientRequest` — add `string? MicrochipNumber`
- `PatientDto` — add `string? MicrochipNumber`

### 3. Update Command + Handler

- `CreatePatientCommand` — add `string? MicrochipNumber`
- `CreatePatientHandler` — pass to `Patient.Create()`, check uniqueness within clinic before saving
- `UpdatePatientCommand` — add `string? MicrochipNumber`
- `UpdatePatientHandler` — pass to `Patient.UpdateInfo()`, check uniqueness within clinic

### 4. Uniqueness check

The handler must query the DbContext to verify no other patient in the same clinic has this microchip number:

```csharp
if (microchipNumber is not null)
{
    var exists = await _context.Patients
        .AnyAsync(p => p.MicrochipNumber == microchipNumber && p.Id != patientId, ct);
    if (exists)
        return Result.Conflict("A patient with this microchip number already exists");
}
```

Note: the global query filter on ClinicId already scopes this to the current clinic.

### 5. Search by microchip

Update `ListPatientsQuery` to accept an optional `string? Microchip` parameter.
Update `ListPatientsHandler` to filter by exact microchip match when provided.
Update `PatientEndpoints.ListPatients` to accept `string? microchip` query parameter.

### 6. EF Migration

```sql
ALTER TABLE "Patients" ADD COLUMN "MicrochipNumber" varchar(15) NULL;
CREATE UNIQUE INDEX "IX_Patients_ClinicId_MicrochipNumber"
  ON "Patients" ("ClinicId", "MicrochipNumber")
  WHERE "MicrochipNumber" IS NOT NULL;
```

### 7. Update PatientEndpoints

- `CreatePatient`: pass `request.MicrochipNumber` to command
- `UpdatePatient`: pass `request.MicrochipNumber` to command
- `ListPatients`: add `string? microchip = null` parameter

### 8. Unit tests

- Test valid microchip (15 digits) accepted
- Test invalid microchip (less than 15, non-numeric) rejected with validation error
- Test null microchip accepted (optional)
- Test uniqueness check returns Conflict when duplicate

## BDD

`tests/Vetolib.Tests.Acceptance/Features/MedicalRecords/PatientExtendedFields.feature`
- Scenarios: "Register a patient with a microchip number", "Microchip number is optional", "Reject invalid microchip format", "Search patient by microchip number", "Microchip number must be unique within a clinic"

## Completion criteria

- [ ] `Patient.MicrochipNumber` property (nullable string)
- [ ] Validation: exactly 15 digits (ISO 11784/11785)
- [ ] Uniqueness within clinic (conflict if duplicate)
- [ ] Search by microchip on list endpoint
- [ ] DTOs updated (CreatePatientRequest, UpdatePatientRequest, PatientDto)
- [ ] EF migration with unique filtered index
- [ ] Unit tests for validation + uniqueness
- [ ] `dotnet build` + `dotnet test` GREEN
