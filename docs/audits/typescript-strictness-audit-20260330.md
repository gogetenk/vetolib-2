# TypeScript Strictness Audit — 2026-03-30

Auditor: Claude Code (automated)
Scope: `src/frontend/` (Next.js 15 app)

---

## 1. tsconfig.json — Strict Mode

**Status: PASS**

`strict: true` is enabled in `src/frontend/tsconfig.json` (line 7). This activates all sub-flags:
- `strictNullChecks`
- `strictFunctionTypes`
- `strictBindCallApply`
- `strictPropertyInitialization`
- `noImplicitAny`
- `noImplicitThis`
- `alwaysStrict`

No strict sub-flags are overridden. `skipLibCheck: true` is set (standard for Next.js).

**Note:** `noUncheckedIndexedAccess` and `noPropertyAccessFromIndexSignature` are NOT enabled. These are not part of `strict` but provide additional safety. Consider enabling them.

---

## 2. `any` Type Usage

**Count: 4 occurrences in source code** (all are `as any` casts on zodResolver — see Section 5)

No explicit `: any` type annotations found in source code. The `@typescript-eslint/no-explicit-any` rule is active and enforced (each `as any` has an explicit eslint-disable comment).

**Verdict: LOW severity.** The 4 occurrences are a known react-hook-form/zod compatibility issue (see Section 5).

---

## 3. `@ts-ignore` / `@ts-expect-error`

**Count: 0**

No `@ts-ignore` or `@ts-expect-error` directives found anywhere in the codebase.

**Verdict: PASS.** Excellent discipline.

---

## 4. Non-Null Assertions (`!`)

**Count: 3 in source code** (excluding test files and string literals)

| # | File | Line | Code | Severity |
|---|------|------|------|----------|
| 1 | `src/components/features/appointments/AppointmentDetailLoader.tsx` | 98 | `appointment!` | MEDIUM |
| 2 | `src/components/features/portal/NewMessageForm.tsx` | 83 | `category!` | MEDIUM |
| 3 | `src/components/features/patients/PatientForm.tsx` | 171 | `patient!.name` | LOW |

### Analysis

1. **AppointmentDetailLoader.tsx:98** — `return <AppointmentDetail appointment={appointment!} />`. The component has early returns for loading/error states, so `appointment` should be defined at this point, but the type system does not prove it. A type guard or explicit null check would be safer.

2. **NewMessageForm.tsx:83** — `category: category!`. Used inside `handleSend()` after a `validate()` call that checks category is set. The validate function does not narrow the type. Should restructure validation to return typed data.

3. **PatientForm.tsx:171** — `patient!.name` in JSX when `isEdit` is true. The `patient` prop is optional but logically required when `isEdit` is true. A discriminated union type (`{isEdit: true, patient: PatientDto} | {isEdit: false}`) would eliminate this.

**Verdict: LOW-MEDIUM severity overall.** Only 3 occurrences, all have logical justification but could be eliminated with better type narrowing.

---

## 5. Dangerous Type Casts (`as any`, `as unknown as`)

### `as any` (4 occurrences — all identical pattern)

| # | File | Line | Code |
|---|------|------|------|
| 1 | `src/components/features/stock/StockMovementForm.tsx` | 68 | `zodResolver(schema) as any` |
| 2 | `src/components/features/stock/StockItemForm.tsx` | 57 | `zodResolver(stockItemSchema) as any` |
| 3 | `src/components/features/patients/MedicalRecordForm.tsx` | 105 | `zodResolver(medicalRecordSchema) as any` |
| 4 | `src/components/features/patients/PatientForm.tsx` | 89 | `zodResolver(patientSchema) as any` |

**Root cause:** Type mismatch between `@hookform/resolvers/zod` and `react-hook-form` generic types. This is a known upstream issue. Each has an `eslint-disable-next-line` comment.

**Severity: LOW.** Zod still validates at runtime; the cast only bypasses the TS resolver type signature. Fix: upgrade `@hookform/resolvers` or use a typed wrapper.

### `as unknown as` (19 occurrences)

| Location | Count | Pattern | Severity |
|----------|-------|---------|----------|
| `src/mocks/handlers/booking.ts` | 10 | `HttpResponse.json(...) as unknown as Response` / `ReturnType<...>` | LOW |
| `e2e/appointments/appointments.spec.ts` | 1 | `window as unknown as Record<string, unknown>` | LOW |
| `e2e/integration/onboarding-checklist.spec.ts` | 4 | `window as unknown as Record<string, unknown>` | LOW |
| `src/components/MSWProvider.tsx` | 1 | `window as unknown as Record<string, unknown>` | LOW |
| `src/hooks/use-onboarding.ts` | 2 | `window as unknown as Record<string, unknown>` | LOW |

**Analysis:**
- **MSW booking handler casts (10):** Workaround for MSW's `HttpResponse.json()` return type not matching the handler's expected return type. Mock-only code, zero production risk.
- **Window casts (8):** Used to attach/read global flags (`__DISABLE_MSW__`, `__onboardingRefetch__`) on `window`. The `Record<string, unknown>` intermediate type is the safest pattern for this. Test + MSW-only usage mostly.

**Verdict: LOW severity.** All are in mock/test infrastructure or use the safe `Record<string, unknown>` intermediary. No `as unknown as SomeConcreteType` in business logic.

### `undefined as T` (3 occurrences)

| # | File | Line | Context |
|---|------|------|---------|
| 1 | `src/lib/api/client.ts` | 103 | `return undefined as T` (204 No Content) |
| 2 | `src/lib/api/client.ts` | 105 | `return undefined as T` (non-JSON 201) |
| 3 | `src/lib/api/portal.ts` | 51 | `return undefined as T` (204 No Content) |

**Severity: MEDIUM.** This is a soundness hole. Callers of `apiPost<SomeDto>(...)` receive a `Promise<SomeDto>` but may get `undefined` at runtime. If the caller does not check for `undefined`, it will crash. Consider changing return types to `Promise<T | undefined>` or using a `Result<T>` wrapper to make this explicit.

---

## 6. Frontend/Backend Type Drift

### 6a. Species Enum — Duplicate & Divergent Definitions

The `Species` type is defined **3 times** in the frontend:

| File | Values |
|------|--------|
| `src/lib/api/types.ts:35` | Dog, Cat, Bird, Rabbit, Horse, **Camel**, Exotic, Falcon, Reptile |
| `src/lib/api/patients.ts:3` | Dog, Cat, Bird, Rabbit, Horse, **Camel**, Exotic, Falcon, Reptile |
| `src/lib/api/appointments.ts:12` | Dog, Cat, Bird, Rabbit, Horse, Exotic, Falcon, Reptile |

**Backend** (`Vetolib.MedicalRecords.Contracts/Species.cs`): Dog, Cat, Bird, Rabbit, Horse, Exotic, **Camel**, Falcon, Reptile

**Issue:** `appointments.ts` is **missing `Camel`**. If a camel appointment is returned by the API, TypeScript will not flag it since the response is not validated at runtime.

**Severity: MEDIUM.** Should be a single canonical definition imported everywhere.

### 6b. PagedResult — Defined 4 Times

`PagedResult<T>` is defined independently in:
- `src/lib/api/types.ts:28`
- `src/lib/api/patients.ts:40`
- `src/lib/api/appointments.ts:33`
- `src/lib/api/billing.ts:34`

All have identical shapes. This is a maintenance risk — if the backend adds a field (e.g., `hasMore`), only one definition might get updated.

**Severity: LOW.** No drift today but fragile. Should import from `types.ts` everywhere.

### 6c. AppointmentDto — Significant Drift from Backend

| Field | Frontend (`appointments.ts`) | Backend (`AppointmentDto.cs`) |
|-------|------------------------------|-------------------------------|
| Patient ID | `patientId?: string` | `AnimalId: Guid` (non-optional) |
| Patient name | `patientName: string` | `AnimalName: string` |
| Date/Time | `scheduledAt: string` | `Date: DateOnly` + `StartTime: TimeOnly` |
| Duration | `durationMinutes?: number` | `DurationMinutes: int` (non-optional) |
| Status values | `SCHEDULED`, `CHECKED_IN`, `IN_PROGRESS`, `COMPLETED`, `CANCELLED` | `AppointmentStatus` enum (separate file) |
| Reason | `reason: string` (required) | `Reason: string?` (nullable) |
| Source | not present | `Source: BookingSource` |
| RescheduleCount | not present | `RescheduleCount: int` |
| OriginalAppointmentId | not present | `OriginalAppointmentId: Guid?` |
| ownerPhone | `ownerPhone: string` | not present |
| notes | `notes?: string` | not present |
| cancellationReason | `cancellationReason?: string` | not present |
| consultationType | `consultationType?: string` | not present |
| species | `species: Species` | not present |
| clinicId | `clinicId: string` | `ClinicId: Guid` |

**Severity: HIGH.** The frontend and backend DTOs have major structural differences. Field names differ (`patientId` vs `AnimalId`), date representation differs (single ISO string vs separate `DateOnly`/`TimeOnly`), nullability mismatches, and several fields exist on one side but not the other. This will cause runtime failures when MSW mocks are replaced with real API calls during the wire phase.

### 6d. PatientDto — Moderate Drift

| Field | Frontend (`patients.ts`) | Backend (`PatientDto.cs`) |
|-------|--------------------------|---------------------------|
| dateOfBirth | `dateOfBirth: string` | `BirthDate: DateOnly` |
| gender | `gender: 'Male' \| 'Female' \| 'Unknown'` | not present |
| sex | `sex: Sex` (5 values incl. Intact variants) | `Sex: Sex` (backend enum, values unknown) |
| ageYears | `ageYears: number` | not present (computed?) |
| weightKg | `weightKg: number \| null` | not present |
| ownerEmail | `ownerEmail: string` | not present |
| lastVisitDate | `lastVisitDate: string \| null` | not present |
| nextAppointmentDate | `nextAppointmentDate: string \| null` | not present |

**Severity: MEDIUM.** Several convenience fields on the frontend don't exist on the backend DTO. The `gender` field appears redundant with `sex`. Name differences (`dateOfBirth` vs `BirthDate`) will cause silent nulls when wiring.

### 6e. LitterDto — Drift

| Field | Frontend (`breeding.ts`) | Backend (`LitterDto.cs`) |
|-------|--------------------------|--------------------------|
| motherId | `motherId: string` | `MotherPatientId: Guid` |
| motherName | `motherName: string` | not present |
| fatherId | `fatherId: string \| null` | `FatherPatientId: Guid?` |
| fatherName | `fatherName: string \| null` | not present |
| dateOfBirth | `dateOfBirth: string` | `BirthDate: DateOnly` |
| species | `species: string` | not present |
| breed | `breed: string \| null` | not present |
| offspringCount | `offspringCount: number` | not present (has `Offspring` list) |
| bornCount | not present | `BornCount: int` |
| aliveCount | not present | `AliveCount: int` |
| externalFatherName | not present | `ExternalFatherName: string?` |
| clinicId | `clinicId: string` | not present |

**Severity: MEDIUM.** Different field names and different shape. Frontend adds convenience fields; backend has raw data fields.

### 6f. InvoiceDto — Drift

Frontend is missing many backend e-invoicing fields: `CurrencyCode`, `SellerSiren`, `SellerVatNumber`, `BuyerSiren`, `BuyerVatNumber`, `BuyerName`, `BuyerAddress`, `OperationType`, `InvoiceTypeCode`, `PaymentTerms`, `CountryCode`, `PurchaseOrderReference`, `EInvoicingStatus`, `PlatformInvoiceId`.

Frontend `patientId` is required (`string`) but backend has `PatientId: Guid?` (nullable).

**Severity: LOW-MEDIUM.** The missing e-invoicing fields are likely not needed on the frontend yet. The nullability mismatch on `patientId` could cause issues.

### 6g. PregnancyDto / HeatCycleDto — Drift

**PregnancyDto:** Frontend has `patientName`, `checks`, `clinicId` that don't exist on backend. Backend has `FatherPatientId`, `MatingMethod`, `Outcome`, `OffspringCount`, `CreatedAt`, `UpdatedAt` that frontend lacks. Frontend calls the checks `checks` but backend calls them `ScheduledChecks`.

**HeatCycleDto:** Frontend has `patientName`, `phase`, `intensity`, `recordedBy`, `clinicId` that don't exist on backend. Backend only has `Id`, `PatientId`, `StartDate`, `EndDate`, `DurationDays`, `Notes`.

**Severity: HIGH for HeatCycleDto** (completely different shapes), **MEDIUM for PregnancyDto**.

### 6h. WeightCurvePointDto — Minor Drift

Frontend: `{ date: string, weightKg: number }`
Backend: `{ RecordedAt: DateTime, WeightKg: decimal }`

Field name differs: `date` vs `RecordedAt` (after camelCase conversion: `recordedAt`).

**Severity: LOW.** Will break silently during wire if not mapped.

---

## 7. Optional Chaining Usage

**84 occurrences across 34 component files.** This is within normal range for a React app of this size. No evidence of excessive chaining masking bugs.

Notable patterns:
- Most are safe access on optional props or nullable API fields
- `filters?.search`, `filters?.page` etc. in API functions are fine (optional parameters)
- Cookie access `request.cookies.get("access_token")?.value` in middleware is correct

**Verdict: PASS.** No red flags. Optional chaining is used appropriately.

---

## Summary

| Category | Count | Severity | Action Needed |
|----------|-------|----------|---------------|
| `strict: true` missing | 0 | PASS | Consider adding `noUncheckedIndexedAccess` |
| Explicit `any` annotations | 0 | PASS | None |
| `as any` casts | 4 | LOW | Upgrade `@hookform/resolvers` or add typed wrapper |
| `@ts-ignore` / `@ts-expect-error` | 0 | PASS | None |
| Non-null assertions `!` | 3 | LOW-MEDIUM | Refactor with type guards / discriminated unions |
| `as unknown as` casts | 19 | LOW | All in mock/test code; acceptable |
| `undefined as T` | 3 | MEDIUM | Change return type to `T \| undefined` |
| Species enum duplication | 3 defs | MEDIUM | Consolidate to single import from `types.ts` |
| PagedResult duplication | 4 defs | LOW | Consolidate to single import from `types.ts` |
| AppointmentDto drift | ~15 fields | HIGH | Align before wire phase |
| PatientDto drift | ~6 fields | MEDIUM | Align before wire phase |
| LitterDto drift | ~8 fields | MEDIUM | Align before wire phase |
| HeatCycleDto drift | ~6 fields | HIGH | Completely different shapes |
| PregnancyDto drift | ~7 fields | MEDIUM | Align before wire phase |
| InvoiceDto nullability | 1 field | LOW-MEDIUM | Fix `patientId` nullable |
| WeightCurvePointDto field name | 1 field | LOW | Map `date` to `recordedAt` |
| Optional chaining overuse | 84 uses | PASS | Normal usage |

### Top 3 Priorities

1. **[HIGH] DTO alignment** — `AppointmentDto` and `HeatCycleDto` frontend types are structurally incompatible with backend contracts. These WILL break during the wire phase. A wire task should include a contract alignment sub-task.

2. **[MEDIUM] `undefined as T` in API client** — The `client.ts` `apiFetch` function can return `undefined` while the type signature promises `T`. This is a soundness hole that can cause runtime crashes.

3. **[MEDIUM] Type duplication** — `Species` (3 copies, one missing `Camel`) and `PagedResult` (4 copies) should be consolidated into `src/lib/api/types.ts` with a single export.
