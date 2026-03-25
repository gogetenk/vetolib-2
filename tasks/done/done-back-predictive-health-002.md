# todo-back-predictive-health-002 -- IPatientAlertDataReader Implementation

**Module** : MedicalRecords
**Dependencies** : todo-back-predictive-health-001 (contract must exist)
**Priority** : HIGH
**Estimated** : 1-2 hours

## Context

The AI module needs to read patient data (species, breed, age, weight, medical records) to evaluate
health alert rules. The contract `IPatientAlertDataReader` is defined in MedicalRecords.Contracts
(created by task 001). This task implements it in the MedicalRecords runtime.

## Skills to read

- `skills/ardalis-modular-monolith/SKILL.md`
- `skills/multitenant-efcore/SKILL.md`

## Scope

### 1. Implement IPatientAlertDataReader in MedicalRecords runtime

```csharp
internal class PatientAlertDataReader : IPatientAlertDataReader
{
    // Inject MedicalRecordsDbContext
    // Query all active patients with their recent medical records (last 24 months)
    // Build WeightHistory from records where weight was recorded
    // Map to PatientAlertDataDto
    // Multi-tenancy is automatic via global query filter
}
```

### 2. WeightHistory tracking

The current `Patient.WeightKg` only stores the latest weight. For weight trend detection,
we need historical weights. Two options:

**Option A (recommended for Phase 1)**: Extract weight from MedicalRecord data.
Add a `WeightKg` nullable field to `MedicalRecord` if not present, or parse from Treatment/notes.
For MVP: use the patient's current weight + any weight-related notes in records.

**Option B**: Create a `WeightHistory` table. Better but more work -- defer to Phase 2.

For Phase 1, the reader should return weight entries from medical records where weight
was explicitly recorded. If no history exists, return a single entry with current PatientWeight.

### 3. Register in DI

Register `IPatientAlertDataReader` as scoped in `ModuleServiceRegistrar`.

### 4. Integration test

One TI verifying the reader returns data correctly with Testcontainers PostgreSQL.

## Completion criteria

- [ ] IPatientAlertDataReader implemented in MedicalRecords runtime
- [ ] Registered in MedicalRecords DI
- [ ] Integration test passing (reader returns patient data with records)
- [ ] Multi-tenancy verified (only returns patients for current clinic)
- [ ] `dotnet build` GREEN
- [ ] `dotnet test` GREEN
