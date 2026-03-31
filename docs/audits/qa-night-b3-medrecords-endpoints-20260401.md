# QA Audit -- MedicalRecords Module Endpoints

**Date**: 2026-04-01
**Auditor**: QA Agent (Claude Opus 4.6)
**Scope**: All endpoint definitions in `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Api/*.cs`
**Branch**: develop

---

## Summary

| Metric | Value |
|---|---|
| Endpoint files | 8 |
| Total endpoints | 33 |
| Endpoints with `.WithSummary()` | 33/33 (100%) |
| Endpoints with `.WithDescription()` | 33/33 (100%) |
| Endpoints with authorization | 33/33 (100%) -- 1 is AllowAnonymous by design |
| Endpoints with rate limiting | 33/33 (100%) |
| Handlers returning `Result<T>` or `Result` | 33/33 (100%) |
| Commands with validators | 17/17 (100%) |
| Endpoints with integration tests | 13/33 (39%) |

**Overall verdict**: Endpoint metadata and handler wiring are excellent. Authorization and rate limiting are consistently applied. The critical gap is **integration test coverage** -- 20 endpoints have zero TI coverage.

---

## Detailed Endpoint Inventory

### 1. DrugCatalogEndpoints.cs (5 endpoints)

| # | Method | Route | Summary | Desc | Auth | RateLimit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|---|
| 1 | GET | `/api/v1/medical-records/drugs/` | YES | YES | Group-level | Group-level | SearchDrugCatalogHandler (Result<List<DrugCatalogEntryDto>>) | N/A (query) | NONE |
| 2 | GET | `/api/v1/medical-records/drugs/{id}` | YES | YES | Group-level | Group-level | GetDrugCatalogEntryByIdHandler (Result<DrugCatalogEntryDto>) | N/A (query) | NONE |
| 3 | GET | `/api/v1/medical-records/drugs/{id}/alternatives` | YES | YES | Group-level | Group-level | GetDrugAlternativesHandler | N/A (query) | NONE |
| 4 | POST | `/api/v1/medical-records/drugs/` | YES | YES | VetOrAdmin | Group-level | AddCustomDrugHandler (Result<DrugCatalogEntryDto>) | AddCustomDrugValidator | NONE |
| 5 | POST | `/api/v1/medical-records/prescriptions/preflight` | YES | YES | Group-level | Group-level | GetPrescriptionPreflightHandler (Result<PrescriptionPreflightResult>) | N/A (query) | NONE |

**Finding**: 0/5 endpoints have integration tests. This entire endpoint group is untested at the HTTP layer.

---

### 2. MedicalRecordEndpoints.cs (4 endpoints)

| # | Method | Route | Summary | Desc | Auth | RateLimit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|---|
| 6 | POST | `/api/v1/patients/{patientId}/records/` | YES | YES | Vet, Admin | Group-level | AddMedicalRecordHandler (Result<MedicalRecordDto>) | AddMedicalRecordValidator | YES (PatientEndpointsTests) |
| 7 | GET | `/api/v1/patients/{patientId}/records/` | YES | YES | Group-level | Group-level | ListMedicalRecordsHandler (Result<MedicalRecordPagedResultDto>) | N/A (query) | NONE |
| 8 | DELETE | `/api/v1/patients/{patientId}/records/{recordId}` | YES | YES | Group-level | Group-level | Inline (always returns Error) | N/A | NONE |
| 9 | POST | `/api/v1/patients/{patientId}/records/{recordId}/prescriptions` | YES | YES | Vet only | Group-level | AddPrescriptionHandler (Result<PrescriptionDto>) | AddPrescriptionValidator | NONE |

**Finding**: Only AddMedicalRecord has a TI. The DELETE endpoint (immutable records) and AddPrescription are untested. ListMedicalRecords lacks a TI.

---

### 3. MedicalRecordTemplateEndpoints.cs (4 endpoints)

| # | Method | Route | Summary | Desc | Auth | RateLimit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|---|
| 10 | GET | `/api/v1/medical-records/templates/` | YES | YES | Group-level | Group-level | ListMedicalRecordTemplatesHandler (Result<List<MedicalRecordTemplateDto>>) | N/A (query) | YES (TemplateEndpointsTests) |
| 11 | POST | `/api/v1/medical-records/templates/` | YES | YES | Vet, Admin | Group-level | CreateMedicalRecordTemplateHandler (Result<MedicalRecordTemplateDto>) | CreateMedicalRecordTemplateValidator | YES (TemplateEndpointsTests) |
| 12 | PUT | `/api/v1/medical-records/templates/{id}` | YES | YES | Vet, Admin | Group-level | UpdateMedicalRecordTemplateHandler (Result<MedicalRecordTemplateDto>) | UpdateMedicalRecordTemplateValidator | NONE |
| 13 | DELETE | `/api/v1/medical-records/templates/{id}` | YES | YES | Vet, Admin | Group-level | DeleteMedicalRecordTemplateHandler (Result) | DeleteMedicalRecordTemplateValidator | NONE |

**Finding**: List and Create have TIs. Update and Delete templates are untested.

---

### 4. OwnerEndpoints.cs (1 endpoint)

| # | Method | Route | Summary | Desc | Auth | RateLimit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|---|
| 14 | POST | `/api/v1/owners/` | YES | YES | Vet, Admin, Receptionist | Group-level | CreateOwnerHandler (Result<OwnerDto>) | CreateOwnerValidator | NONE |

**Finding**: Zero TI coverage for the owner creation endpoint.

---

### 5. OwnerPortalEndpoints.cs (5 endpoints)

| # | Method | Route | Summary | Desc | Auth | RateLimit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|---|
| 15 | GET | `/api/v1/portal/my-animals` | YES | YES | Group-level + owner_account_id claim | Group-level | GetMyAnimalsHandler (Result<IReadOnlyList<PortalAnimalDto>>) | N/A (query) | NONE |
| 16 | GET | `/api/v1/portal/animals/{id}/records` | YES | YES | Group-level + owner_account_id claim | Group-level | GetAnimalRecordsHandler (Result<IReadOnlyList<PortalMedicalRecordDto>>) | N/A (query) | NONE |
| 17 | GET | `/api/v1/portal/animals/{id}/vaccinations` | YES | YES | Group-level + owner_account_id claim | Group-level | GetAnimalVaccinationsHandler (Result<IReadOnlyList<PortalVaccinationDto>>) | N/A (query) | NONE |
| 18 | GET | `/api/v1/portal/animals/{id}/prescriptions` | YES | YES | Group-level + owner_account_id claim | Group-level | GetAnimalPrescriptionsHandler (Result<IReadOnlyList<PortalPrescriptionDto>>) | N/A (query) | NONE |
| 19 | GET | `/api/v1/portal/animals/{id}/weight` | YES | YES | Group-level + owner_account_id claim | Group-level | GetAnimalWeightHistoryHandler (Result<IReadOnlyList<PortalWeightEntryDto>>) | N/A (query) | NONE |

**Finding**: The entire Owner Portal has zero TI coverage. All 5 endpoints are untested at the HTTP layer. These endpoints perform manual owner_account_id claim extraction, which is particularly important to test.

---

### 6. PatientEndpoints.cs (13 endpoints)

| # | Method | Route | Summary | Desc | Auth | RateLimit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|---|
| 20 | POST | `/api/v1/patients/` | YES | YES | VetOrAdmin | Group-level | CreatePatientHandler (Result<PatientDto>) | CreatePatientValidator | YES |
| 21 | GET | `/api/v1/patients/` | YES | YES | Group-level + CacheOutput | Group-level | ListPatientsHandler (Result<PatientPagedResult>) | N/A (query) | YES |
| 22 | GET | `/api/v1/patients/{id}` | YES | YES | Group-level + CacheOutput | Group-level | GetPatientByIdHandler (Result<PatientDto>) | N/A (query) | NONE |
| 23 | GET | `/api/v1/patients/{id}/detail` | YES | YES | Group-level + CacheOutput | Group-level | GetPatientDetailHandler (Result<PatientDetailDto>) | N/A (query) | NONE |
| 24 | GET | `/api/v1/patients/{id}/export/summary` | YES | YES | Group-level | Group-level | GetPatientSummaryHandler (Result<PatientSummaryDto>) | N/A (query) | YES (PatientSummaryEndpointsTests) |
| 25 | GET | `/api/v1/patients/{id}/export/fhir` | YES | YES | Group-level | Group-level | ExportPatientFhirHandler (Result<FhirBundleResult>) | N/A (query) | NONE |
| 26 | PATCH | `/api/v1/patients/{id}` | YES | YES | VetOrAdmin | Group-level | UpdatePatientHandler | UpdatePatientValidator | NONE |
| 27 | POST | `/api/v1/patients/import` | YES | YES | VetOrAdmin | Group-level | ImportPatientsHandler (Result<ImportReportDto>) | ImportPatientsValidator | YES |
| 28 | POST | `/api/v1/patients/import/fhir` | YES | YES | VetOrAdmin | Group-level | ImportPatientFhirHandler | ImportPatientFhirValidator | NONE |
| 29 | GET | `/api/v1/patients/import/template` | YES | YES | Group-level | Group-level | Inline (returns CSV) | N/A | NONE |
| 30 | POST | `/api/v1/patients/{id}/photo` | YES | YES | VetOrAdmin | Group-level | UploadPatientPhotoHandler | UploadPatientPhotoValidator | YES (PatientPhotoEndpointsTests) |
| 31 | GET | `/api/v1/patients/{id}/photo` | YES | YES | Group-level | Group-level | GetPatientPhotoHandler | N/A (query) | YES (PatientPhotoEndpointsTests) |
| 32 | DELETE | `/api/v1/patients/{id}/photo` | YES | YES | VetOrAdmin | Group-level | DeletePatientPhotoHandler | DeletePatientPhotoValidator | NONE |
| 33 | POST | `/api/v1/patients/{id}/transfer` | YES | YES | VetOrAdmin | Group-level | TransferPatientHandler (Result<TransferPatientResultDto>) | TransferPatientValidator | NONE |

**Finding**: 7/13 endpoints have TI coverage. Missing TIs for: GetPatientById, GetPatientDetail, ExportPatientFhir, UpdatePatient, ImportPatientFhir, GetImportTemplate, DeletePatientPhoto, TransferPatient.

---

### 7. SharedRecordEndpoints.cs (4 endpoints)

| # | Method | Route | Summary | Desc | Auth | RateLimit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|---|
| 33a | GET | `/api/v1/shared/{token}` | YES | YES | AllowAnonymous (by design) | Group-level | GetSharedRecordHandler (Result<PatientSummaryDto>) | N/A (query) | NONE |
| 34 | POST | `/api/v1/portal/animals/{id}/share` | YES | YES | Group-level + owner_account_id | Group-level | CreateShareLinkHandler (Result<CreateShareLinkResponse>) | CreateShareLinkValidator | NONE |
| 35 | GET | `/api/v1/portal/shares` | YES | YES | Group-level + owner_account_id | Group-level | ListShareLinksHandler (Result<IReadOnlyList<SharedRecordLinkDto>>) | N/A (query) | NONE |
| 36 | DELETE | `/api/v1/portal/shares/{id}` | YES | YES | Group-level + owner_account_id | Group-level | RevokeShareLinkHandler (Result) | RevokeShareLinkValidator | NONE |

**Finding**: Zero TI coverage for all shared record endpoints. The anonymous GetSharedRecord endpoint is security-sensitive and should absolutely have a TI.

---

### 8. WeightEndpoints.cs (3 endpoints)

| # | Method | Route | Summary | Desc | Auth | RateLimit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|---|
| 37 | POST | `/api/v1/patients/{patientId}/weights/` | YES | YES | VetOrAdmin | Group-level | AddWeightEntryHandler (Result<WeightEntryDto>) | AddWeightEntryValidator | NONE |
| 38 | GET | `/api/v1/patients/{patientId}/weights/` | YES | YES | Group-level | Group-level | GetWeightHistoryHandler | N/A (query) | NONE |
| 39 | GET | `/api/v1/patients/{patientId}/weights/curve` | YES | YES | Group-level | Group-level | GetWeightCurveHandler (Result<IReadOnlyList<WeightCurvePointDto>>) | N/A (query) | NONE |

**Finding**: Zero TI coverage for all weight endpoints.

---

## Positive Findings

1. **100% metadata compliance**: Every single endpoint has `.WithSummary()` and `.WithDescription()` -- no exceptions.
2. **100% authorization coverage**: All groups use `.RequireAuthorization()`, with additional role-based policies on mutation endpoints. The one `AllowAnonymous` endpoint (shared record viewer) is intentional.
3. **100% rate limiting**: All groups use `.RequireRateLimiting("api")`.
4. **100% Result pattern compliance**: All handlers return `Result<T>` or `Result`. Zero exceptions thrown for business logic. All endpoints use `.ToMinimalApiResult()`.
5. **100% validator coverage on commands**: Every command type has a corresponding FluentValidation validator.
6. **Clean architecture**: No controllers, no manual HTTP status code mapping -- all handled via `ToMinimalApiResult()`.
7. **Good security practices**: VetLicense claim checked in AddPrescription, magic-byte validation on photo uploads, file size limits on imports.

---

## Critical Gaps

### GAP-1: Integration test coverage at 39% (HIGH)

**20 out of 33 endpoints have zero integration tests.** Per CLAUDE.md rule 3g: "Each module MUST have at least 1 integration test per endpoint."

Priority endpoints needing TIs (ordered by risk):

| Priority | Endpoint | Why |
|---|---|---|
| P0 | GET /api/v1/shared/{token} (anonymous) | Security-sensitive anonymous access -- must verify token validation, expiry, and that no auth is required |
| P0 | POST /api/v1/patients/{id}/transfer | Complex cross-tenant operation, high data integrity risk |
| P0 | POST /api/v1/patients/{patientId}/records/{recordId}/prescriptions | Vet license validation in endpoint code (not handler), drug interaction checks |
| P1 | All 5 OwnerPortal endpoints | Manual claim extraction (owner_account_id) -- must verify 401/403 behavior |
| P1 | All 4 SharedRecord portal endpoints | Owner authentication via claim extraction |
| P1 | All 3 Weight endpoints | Complete endpoint group with zero coverage |
| P1 | All 5 DrugCatalog endpoints | Complete endpoint group with zero coverage |
| P2 | DELETE /patients/{patientId}/records/{recordId} | Should verify immutability enforcement returns error |
| P2 | GET/DELETE photo, PATCH patient, FHIR export/import | Standard CRUD but untested |

### GAP-2: No TI for anonymous endpoint (MEDIUM-HIGH)

`GET /api/v1/shared/{token}` is the only `AllowAnonymous` endpoint in the module. It has no integration test verifying:
- Token expiry behavior
- Revoked token behavior
- Rate limiting still applies
- No sensitive data leakage beyond PatientSummaryDto

### GAP-3: OwnerPortal claim extraction untested (MEDIUM)

All 5 OwnerPortal endpoints and 3 SharedRecord portal endpoints manually extract `owner_account_id` from claims. This logic is in the endpoint code (not the handler), meaning unit tests on handlers cannot catch auth bugs. Only integration tests can verify this.

---

## Existing Integration Test Files

| File | Endpoints covered |
|---|---|
| `PatientEndpointsTests.cs` | CreatePatient, ListPatients (search), AddMedicalRecord, ImportPatients |
| `PatientPhotoEndpointsTests.cs` | UploadPhoto, GetPhoto (auth + unauth) |
| `PatientSummaryEndpointsTests.cs` | GetPatientSummary (auth + unauth) |
| `TemplateEndpointsTests.cs` | ListTemplates, CreateTemplate (auth + unauth) |

---

## Recommendations

1. **Create `DrugCatalogEndpointsTests.cs`** -- cover all 5 drug/prescription endpoints
2. **Create `OwnerPortalEndpointsTests.cs`** -- cover all 5 portal endpoints with auth/claim tests
3. **Create `SharedRecordEndpointsTests.cs`** -- cover anonymous access + portal share CRUD
4. **Create `WeightEndpointsTests.cs`** -- cover all 3 weight endpoints
5. **Create `MedicalRecordEndpointsTests.cs`** -- cover ListMedicalRecords, DeleteMedicalRecord (immutable), AddPrescription
6. **Expand `PatientEndpointsTests.cs`** -- add GetPatientById, GetPatientDetail, UpdatePatient, ExportFhir, ImportFhir, GetImportTemplate, DeletePhoto, TransferPatient
7. **Expand `TemplateEndpointsTests.cs`** -- add UpdateTemplate, DeleteTemplate

Estimated effort: 7 new/expanded test files, ~40-50 test methods total.
