# MedicalRecords Module Deep Audit - 2026-03-30

Scope: every `.cs` file in `Vetolib.MedicalRecords` and `Vetolib.MedicalRecords.Contracts`.

---

## CRITICAL - Prescription Safety

### CRIT-01: PrescriptionCreatedEvent is never published

**File:** `Contracts/PrescriptionCreatedEvent.cs`, `Application/Commands/AddPrescription/AddPrescriptionHandler.cs`

`PrescriptionCreatedEvent` is defined in Contracts but is never published anywhere in the MedicalRecords module. The `AddPrescriptionHandler` publishes `PrescriptionOverriddenEvent` when an override occurs, but never publishes `PrescriptionCreatedEvent`. The Stock module has a `PrescriptionCreatedConsumer` that listens for this event to decrement inventory. This means **prescriptions are created without ever decrementing stock**, leading to phantom inventory.

### CRIT-02: Prescription not linked to MedicalRecord via navigation

**File:** `Application/Commands/AddPrescription/AddPrescriptionHandler.cs` (line 49)

The handler adds the prescription directly to `_context.Prescriptions` rather than via `medicalRecord.AddPrescription(prescription)`. The `MedicalRecord` entity is loaded (line 29) but only used for existence checking. While the FK (`MedicalRecordId`) is set correctly in the domain factory, bypassing the aggregate root means the in-memory `MedicalRecord._prescriptions` list is not updated, breaking any downstream logic that relies on the loaded aggregate's state within the same request.

### CRIT-03: "Highest severity" logic is inverted

**File:** `Application/Commands/AddPrescription/AddPrescriptionHandler.cs` (lines 88-91)

```csharp
var highestSeverity = interactionResult.Value.Alerts
    .OrderBy(a => a.Severity)  // Critical=0, Moderate=1, Info=2
    .Select(a => a.Severity)
    .First();
```

The enum `InteractionSeverity` defines `Critical = 0, Moderate = 1, Info = 2`. `OrderBy` ascending puts `Critical` first, which is actually correct by accident. However, the comment says "highest severity" while the code picks the **lowest numeric value**. If the enum order ever changes, this breaks silently. Should use `OrderByDescending` or `Min()` with explicit intent.

### CRIT-04: No dosage range validation at prescription creation time

**File:** `Application/Commands/AddPrescription/AddPrescriptionHandler.cs`

The handler checks drug-drug interactions via `CheckInteractionsQuery` but does **not** validate the `DosageAmount` against the `DosageGuideline` min/max range for the patient's species and weight. A vet could prescribe 100x the recommended dose and the system would accept it silently as long as there are no drug-drug interactions. The preflight endpoint does this check, but the actual creation endpoint does not enforce it.

### CRIT-05: Override justification can bypass all safety checks

**File:** `Application/Commands/AddPrescription/AddPrescriptionHandler.cs` (lines 83-84)

Any string of 10+ characters is accepted as a justification to override critical alerts. There is no validation that the justification is meaningful (e.g., "aaaaaaaaaa" would pass). For a veterinary system handling controlled substances, this is a safety gap.

---

## HIGH - Data Integrity

### HIGH-01: Duplicate internal and Contracts paged result types

**Files:** `Application/Queries/ListPatients/ListPatientsQuery.cs` vs `Contracts/PatientPagedResultDto.cs`

`PatientPagedResult` is defined as `internal` in `ListPatientsQuery.cs` with fields `(Items, Total, Page, PageSize)`, while `PatientPagedResultDto` exists in Contracts with fields `(Items, TotalCount, Page, PageSize)`. Note the field name mismatch: `Total` vs `TotalCount`. The endpoint returns the internal type (not the Contracts DTO), meaning other modules cannot consume this query's result through the Contracts assembly.

### HIGH-02: Owner.UpdateName silently keeps old last name for single-word names

**File:** `Application/Domain/Owner.cs` (lines 60-68)

```csharp
var parts = fullName.Trim().Split(' ', 2);
FirstName = parts[0];
LastName = parts.Length > 1 ? parts[1] : LastName;  // keeps OLD LastName
```

If an owner's name is updated to a single word (e.g., "Madonna"), the `LastName` remains whatever it was before. This is silent data corruption. The same pattern exists in `ImportPatientsHandler` and `CreatePatientHandler` where `lastName = nameParts.Length > 1 ? nameParts[1] : "-"` uses a dash as fallback -- at least that is explicit, though inconsistent.

### HIGH-03: CreatePatientHandler generates fake emails for owners

**File:** `Application/Commands/CreatePatient/CreatePatientHandler.cs` (line 44)

```csharp
var ownerEmail = $"owner.{cmd.OwnerPhone.Replace(...)}@vetoclinic.ae";
```

Email is generated from phone number with a hardcoded domain `@vetoclinic.ae`. This creates fake email addresses that:
1. Could conflict with real emails if the domain is ever used
2. Are stored as if they were real, making it impossible to distinguish generated from genuine
3. Phone-based matching (line 48) means two owners with the same phone but different clinics could collide (multi-tenant filter helps but the email is still fake)

### HIGH-04: PatientAlertDataReader loads ALL patients into memory

**File:** `Infrastructure/PatientAlertDataReader.cs` (lines 21-25)

```csharp
var patients = await _context.Patients
    .Include(p => p.MedicalRecords.Where(r => r.ExaminedAt >= cutoffDate))
    .Include(p => p.WeightEntries)
    .AsNoTracking()
    .ToListAsync(ct);
```

This loads every patient in the clinic with their last 24 months of medical records and all weight entries into memory at once. For a clinic with thousands of patients, this will cause memory pressure and slow responses. No pagination, no streaming.

### HIGH-05: MedicalRecord ExaminedAt not validated against future dates

**File:** `Application/Domain/MedicalRecord.cs`

The `Create` factory validates empty GUIDs and empty strings, but does not check whether `examinedAt` is in the future. A record could be created with a date years ahead, which is medically meaningless and would corrupt chronological ordering in lists and patient detail views.

### HIGH-06: Prescription entity has no expiry date

**File:** `Application/Domain/Prescription.cs`

Prescriptions have no `ExpiresAt` or `DurationDays` field. The `GetActivePrescriptionsForPatientQuery` uses a configurable `ActiveWindowDays` (default 90) to determine "active" prescriptions, but this is a query-time heuristic, not a domain property. A prescription for a 5-day antibiotic course is treated the same as a lifelong maintenance medication.

---

## MEDIUM - Validation & Consistency

### MED-01: AddMedicalRecordCommand missing ExaminedAt parameter

**File:** `Application/Commands/AddMedicalRecord/AddMedicalRecordCommand.cs`

The command does not accept an `ExaminedAt` parameter. The handler hardcodes `DateTime.UtcNow` (line 34). This means:
1. Backdating records is impossible (e.g., importing paper records)
2. The API caller has no control over the examination date

### MED-02: RBAC inconsistency between endpoint and handler for AddPrescription

**Files:** `Api/MedicalRecordEndpoints.cs` (line 98), `Application/Commands/AddPrescription/AddPrescriptionHandler.cs` (line 26)

The endpoint checks `role is not "Vet"` (only Vet allowed), but the handler checks `role is not ("Vet" or "Admin")` (both Vet and Admin allowed). The endpoint is more restrictive, so Admin users are rejected at the endpoint level even though the handler would accept them. This is confusing and likely unintentional.

### MED-03: RBAC for AddMedicalRecord is in the endpoint, not the handler

**File:** `Api/MedicalRecordEndpoints.cs` (lines 53-55)

The `AddMedicalRecord` endpoint does RBAC checking inline, while the command handler has no role check. This means any code that sends `AddMedicalRecordCommand` directly (tests, other handlers, cross-module calls) bypasses authorization.

### MED-04: Owner email uniqueness check is case-sensitive in handler

**File:** `Application/Commands/CreateOwner/CreateOwnerHandler.cs` (line 23)

```csharp
.FirstOrDefaultAsync(o => o.Email == cmd.Email.ToLowerInvariant(), ct);
```

The query compares `o.Email` (already lowercased by `Owner.Create`) with `cmd.Email.ToLowerInvariant()`. This works, but depends on PostgreSQL's default case-sensitive collation matching the `ToLowerInvariant()` applied at creation. If any code path stores email without lowercasing, duplicates could slip through.

### MED-05: Patient.UpdateInfo early-returns on first validation error

**File:** `Application/Domain/Patient.cs` (lines 68-105)

Unlike `Patient.Create` which collects all validation errors, `UpdateInfo` returns `Result.Error()` on the first problem. A caller updating both name (empty) and breed (empty) would only see the name error. This is inconsistent with the Create pattern.

### MED-06: WeightEntry.Create silently truncates notes to 500 characters

**File:** `Application/Domain/WeightEntry.cs` (lines 43-45)

```csharp
if (trimmedNote is not null && trimmedNote.Length > 500)
    trimmedNote = trimmedNote[..500];
```

Notes longer than 500 chars are truncated without any indication to the caller. The validator also has `MaximumLength(500)` so this should be caught at the FluentValidation layer first. If it reaches the domain, silent truncation is surprising behavior.

### MED-07: CreateOwnerValidator missing ClinicId validation

**File:** `Application/Commands/CreateOwner/CreateOwnerValidator.cs`

The validator checks FirstName, LastName, and Email, but does not validate `ClinicId.NotEmpty()`. The domain `Owner.Create` does check it, but the FluentValidation pipeline should catch it earlier with a user-friendly message.

### MED-08: ImportPatientsHandler has no file size limit

**File:** `Application/Commands/ImportPatients/ImportPatientsHandler.cs`

There is no validation on the size of the CSV stream. A malicious user could upload a multi-GB file and exhaust server memory since `csv.GetRecords<CsvPatientRow>().ToList()` (line 192) loads all rows into memory.

### MED-09: ImportPatientsHandler validates Breed/DateOfBirth but not all fields

**File:** `Application/Commands/ImportPatients/ImportPatientsHandler.cs` (lines 196-208)

`ValidateRow` checks PatientName, Species, and OwnerName, but does NOT check Breed or DateOfBirth as required. These are caught later by `Patient.Create`, but the error messages are less specific (no row number context from the domain factory).

---

## LOW - Code Quality & Design

### LOW-01: Hardcoded domain strings

**Files:** Multiple

- `@vetoclinic.ae` in `CreatePatientHandler.cs` (line 44)
- `@import.vetoclinic.ae` in `ImportPatientsHandler.cs` (line 218)
- `"Staff (via Messaging)"` in `PatientRecordWriter.cs` (line 54)

These should be configurable or at least constants.

### LOW-02: Dead Contracts type -- PatientPagedResultDto

**File:** `Contracts/PatientPagedResultDto.cs`

This DTO exists in Contracts but `ListPatientsHandler` returns the internal `PatientPagedResult` type instead. Unless another module consumes `PatientPagedResultDto`, it is dead code.

### LOW-03: `using Vetolib.Stock.Contracts` in MedicalRecords handler

**File:** `Application/Queries/GetPrescriptionPreflight/GetPrescriptionPreflightHandler.cs` (line 4)

The preflight handler imports `Vetolib.Stock.Contracts` to use `CheckStockAvailabilityQuery` and `StockAvailabilityResult`. While Contracts-to-Contracts references are allowed by the architecture rules, this creates a hard dependency between MedicalRecords and Stock at the query level. If Stock is not registered, the handler will fail at runtime with no MediatR handler found.

### LOW-04: Inconsistent null-safety in PatientOwner.Create

**File:** `Application/Domain/PatientOwner.cs`

`PatientOwner.Create` does not return `Result<T>` -- it is the only domain factory that returns the entity directly without validation. No checks for `Guid.Empty` on clinicId, patientId, or ownerId.

### LOW-05: DrugInteraction and DosageGuideline factory methods lack validation

**Files:** `Application/Domain/DrugInteraction.cs`, `Application/Domain/DosageGuideline.cs`

Both `Create` methods return the entity directly without `Result<T>`. Validation is done in the parent `DrugCatalogEntry.AddInteraction()` and `AddDosageGuideline()`, but the child factory is unprotected if called directly.

### LOW-06: Species enum has no `Other` or `Unknown` value

**File:** `Contracts/Species.cs`

The `Species` enum lists specific species but has no fallback for unknown species. The CSV import will fail for any species not in the enum. The `Sex` enum has `Unknown` but `Species` does not.

### LOW-07: ListPatientsHandler uses `.ToLower()` for search

**File:** `Application/Queries/ListPatients/ListPatientsHandler.cs` (line 27)

```csharp
q = q.Where(p => p.Name.ToLower().Contains(query.Name.ToLower()));
```

This works but prevents PostgreSQL from using a standard index on Name. Should use `EF.Functions.ILike()` for PostgreSQL-optimized case-insensitive search, or create a `citext` column.

### LOW-08: Missing `PrescriptionCreatedEvent` publication means Stock module is dead code for prescriptions

**Files:** `Contracts/PrescriptionCreatedEvent.cs`, `AddPrescriptionHandler.cs`

Related to CRIT-01. The `PrescriptionCreatedEvent` has fields like `Quantity` and `StockDecrementConfirmed` that suggest it was designed for a stock-decrement workflow. Since it is never published, any consumer (like in Stock) never fires.

### LOW-09: MedicalRecordEndpoints.DeleteMedicalRecord returns Result.Error instead of Forbidden

**File:** `Api/MedicalRecordEndpoints.cs` (lines 80-87)

The delete endpoint returns `Result.Error` (500-level mapping) instead of `Result.Forbidden()` or a 405 Method Not Allowed. The intent is to communicate immutability, but the HTTP status code will be misleading.

### LOW-10: GetPatientDetailHandler hardcodes "last 5 records"

**File:** `Application/Queries/GetPatientDetail/GetPatientDetailHandler.cs` (line 32)

`.Take(5)` is hardcoded. Should be a configurable parameter or at least a named constant.

---

## Summary

| Severity | Count | Key themes |
|----------|-------|------------|
| CRITICAL | 5 | Stock event never published, dosage not enforced, override too permissive |
| HIGH     | 6 | Memory bomb on alert reader, fake emails, no future-date guard, no Rx expiry |
| MEDIUM   | 9 | RBAC inconsistencies, validation gaps, silent truncation |
| LOW      | 10 | Dead code, hardcoded strings, missing Result pattern on child entities |

### Top 3 actions recommended

1. **Publish `PrescriptionCreatedEvent`** in `AddPrescriptionHandler` after save -- Stock module is currently disconnected from prescriptions.
2. **Add dosage range enforcement** at prescription creation time (not just in preflight) -- this is a patient safety issue.
3. **Fix `PatientAlertDataReader`** to paginate or stream -- currently loads all patients + records into memory.
