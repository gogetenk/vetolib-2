# QA Report -- Patients and Medical Records

**Branch**: develop
**Date**: 2026-03-12
**Analyst**: QA Agent
**Zone**: /patients, /patients/[id], /patients/new, /patients/[id]/records/new, /medical-records

---

## Test Results

- Passed: 102 (overall suite -- zone not isolated)
- Skipped: 34
- Failed: 0

Test files analyzed:
- e2e/patients/patients-standalone.spec.ts -- 27 tests (MSW-based, standalone)
- e2e/patients/medical-records.spec.ts -- 21 tests (MSW-based, standalone)
- e2e/recette/patients.spec.ts -- 6 tests (real backend)
- e2e/recette/medical-records.spec.ts -- 4 tests (real backend, mixed UI + API)
- e2e/integration/prescriptions.spec.ts -- 30+ tests (drug selector, stock, interactions, RBAC)

---

## Coverage Analysis

### Pages covered

- /patients: list, search by name, search by owner, RBAC (Add Patient hidden for RECEPTIONIST and ASSISTANT)
- /patients/new: form render, Camel species option, validation (required fields, UAE phone, future date), creation + redirect, Cancel
- /patients/[id]: header fields, three tabs, RBAC, edit drawer, patient update flow
- /patients/[id]/records/new: form, validation, drug selector, interaction alerts, stock, dispense toggle, RBAC redirect
- Patient weight display in header (normal and null state)

### Pages/features NOT covered

- /medical-records standalone route: only a skeleton placeholder, zero tests
- CSV Import dialog: no dedicated Playwright test despite fully implemented MSW handlers
- Species filter on patient list: testid species-filter does not exist; P3-PATIENTS-03 always falls to fallback branch
- Patient detail owner email (patient-detail-email): rendered but never asserted
- patient-not-found error state on /patients/[id]: never triggered in tests
- new-record-patient-not-found error state: never triggered
- Patient list error state (patients-error): no test triggers API failure
- Patient list loading skeleton (patients-loading): not asserted
- Vaccination tab empty state (vaccinations-empty): no test with zero-vaccination patient
- Prescription tab empty state (prescriptions-empty): no test with zero-prescription patient
- ADMIN role: canWrite=true for edit, but no test covers ADMIN token in this zone

---

## Bugs Found

### [BUG-001] save-record-btn testid mismatch in recette test

- **Severity**: High
- **File**: e2e/recette/medical-records.spec.ts line 93
- **Description**: P4-MEDICAL-01 uses getByTestId(btn-save-record). Actual testid in MedicalRecordForm.tsx line 487 is save-record-btn. Always fails if UI path exercised.
- **Expected**: Change test testid to save-record-btn.

### [BUG-002] medical-record-item- prefix in recette test does not exist in the component

- **Severity**: High
- **File**: e2e/recette/medical-records.spec.ts line 97
- **Description**: Locator [data-testid^=medical-record-item-] never matches. MedicalRecordsList renders data-testid=medical-record-{id} with no -item- infix.
- **Expected**: Change locator prefix to medical-record-.

### [BUG-003] ADMIN role inconsistency between patient detail and records/new guard

- **Severity**: Medium
- **File**: patients/[id]/records/new/page.tsx lines 21-23 vs patients/[id]/page.tsx line 297
- **Description**: records/new redirects if role \!== VET (ADMIN also redirected). Patient detail already shows New Medical Record only for VET. Outcomes are consistent but two different checks are used (canWrite = VET|ADMIN vs role === VET). Undocumented and untested.
- **Expected**: Use a single shared predicate canCreateRecord = role === VET applied consistently in both files.

### [BUG-004] Wrong icon for Horse species (Beef icon used)

- **Severity**: Low
- **File**: src/components/features/patients/SpeciesIcon.tsx line 25
- **Description**: Imports Beef from lucide-react for the Horse case. Beef renders a steak icon, not a horse. Visible UI defect on horse patient cards.
- **Expected**: Replace with an appropriate horse icon.

### [BUG-005] Medical record form: only error-reason asserted on empty submit, 6 other required fields missing

- **Severity**: Medium
- **File**: e2e/patients/medical-records.spec.ts line 243
- **Description**: Schema marks reason, anamnesis, weight, temperature, heartRate, diagnosis, treatment as required. Test only checks error-reason. Six required-field errors are never asserted.
- **Expected**: Also assert error-anamnesis, error-weight, error-temperature, error-heart-rate, error-diagnosis, error-treatment.

### [BUG-006] Species filter test P3-PATIENTS-03 always skips its core assertion

- **Severity**: Medium
- **File**: e2e/recette/patients.spec.ts lines 124-150
- **Description**: species-filter testid absent from PatientsPage. Test always falls to else branch and only asserts the page loads. Gives false coverage.
- **Expected**: Implement the filter with the expected testid or mark test.skip with a documented reason.

### [BUG-007] /patients/new subtitle is hardcoded in English

- **Severity**: Low
- **File**: src/app/[locale]/(dashboard)/patients/new/page.tsx line 28
- **Description**: Subtitle Register a new patient and their owner. is a raw string literal, not translated.
- **Expected**: Add key to patients.form namespace and render via t().

### [BUG-008] PatientForm has numerous hardcoded English strings bypassing i18n

- **Severity**: Low
- **File**: src/components/features/patients/PatientForm.tsx
- **Description**: Hardcoded in English despite matching keys in messages/en.json under patients.form:
  Card title (New Patient / Edit Patient), field labels (Animal Name, Breed, Date of Birth, Sex, Weight, Owner Information, Full Name, Phone, Email),
  button labels (Cancel, Save Patient, Saving...), toasts (Patient created/updated), error (Failed to save patient).
- **Expected**: Wire all through useTranslations using the existing form.* keys.

---

## Missing data-testid

| Element | File | Issue |
|---|---|---|
| Weight input on patient form | PatientForm.tsx:259 | Testid patient-weight-input exists but no test fills or asserts it on the creation form |
| Owner email on patient detail | patients/[id]/page.tsx:258 | Testid patient-detail-email exists but never asserted |
| Species filter | patients/page.tsx | Testid species-filter referenced in P3-PATIENTS-03 does not exist in the component |
| Cancel button on medical record form | MedicalRecordForm.tsx:505 | Testid cancel-record-btn exists but no test clicks it |
| Back button on records/new page | records/new/page.tsx:59 | Testid back-to-patient-btn exists but no test clicks it |

---

## i18n Issues

Strings rendered directly in English without going through t(). Will appear in English on Arabic locale.

patients/new/page.tsx line 28: Register a new patient and their owner.

patients/[id]/page.tsx:
  New Prescription (line 105), No prescriptions found. (line 111), No vaccination records found. (line 58),
  column headers Vaccine / Date Given / Next Due / Vet (lines 65-68), Patient not found. (line 197),
  Edit button (line 276), Edit Patient sheet title (line 281), New Medical Record button (line 302),
  tab labels Medical Records / Prescriptions / Vaccinations (lines 180-183)

patients/[id]/records/new/page.tsx: Patient not found. (line 45), Back to Patients (line 47)

PatientForm.tsx: see BUG-008 -- 10+ strings including all form labels and buttons

MedicalRecordForm.tsx: Reason for Consultation, Anamnesis, Clinical Examination, Weight (kg), Temperature, Heart Rate,
  Diagnosis, Treatment, Prescription (optional), Recommended Next Visit, New Medical Record -- {name},
  Save Record, Cancel, all error and override messages

MedicalRecordsList.tsx: Weight, Temperature, Heart Rate, Diagnosis:, Treatment:, Prescription:, No medical records found.

Note: medical_records namespace in en.json only has 3 keys (title, records, coming_soon). Entire form and list layer has no i18n keys.

---

## a11y Issues

| Issue | Severity | File | Details |
|---|---|---|---|
| CSV dropzone not keyboard accessible | Medium | CsvImportDialog.tsx:156-200 | div has onClick but no tabIndex, role=button, or onKeyDown |
| clear-file-btn has no accessible label | Low | CsvImportDialog.tsx:183 | button contains only X icon with no aria-label |
| patients-loading skeleton has no ARIA feedback | Low | patients/page.tsx:104-112 | animated divs with no aria-busy or live region |
| Tab keyboard navigation not tested | Low | patients/[id]/page.tsx:318-334 | role=tab and aria-selected set correctly but arrow-key navigation not tested |
| Duplicate SpeciesIcon in PatientCard | Low | PatientCard.tsx:10-23 | private local copy duplicates shared component and will diverge |

---

## Recommendations

1. Fix BUG-001 and BUG-002 (blocking): fix testids in e2e/recette/medical-records.spec.ts -- btn-save-record to save-record-btn, and medical-record-item- prefix to medical-record-.
2. Add Playwright tests for CSV Import dialog: fully implemented with MSW handlers, zero dedicated test coverage. Minimum: file select, preview, import submit, report display, non-CSV error.
3. Fix Horse species icon (BUG-004): replace Beef import with an appropriate icon.
4. Address i18n technical debt: create medical_records.form.* keys and wire all form labels in PatientForm, MedicalRecordForm, MedicalRecordsList, and patient detail.
5. Mark or fix species filter test (BUG-006): P3-PATIENTS-03 passes silently without testing the filter feature.
6. Expand medical record validation test (BUG-005): assert all 7 required-field errors on empty submit.
7. Fix CSV dropzone keyboard accessibility: add tabIndex=0, role=button, onKeyDown, aria-label on the X clear button.
8. Add tests for error and empty states: patient list API failure, patient-not-found, empty vaccinations, empty prescriptions, empty medical records list.
