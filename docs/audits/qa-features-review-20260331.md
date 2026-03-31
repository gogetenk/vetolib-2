# QA Features Review - Nightly Session PRs 211-245 - 2026-03-31

**Reviewer:** QA Agent  
**Date:** 2026-03-31  
**Scope:** 14 new features delivered during the nightly session

---

## Methodology

Each feature was verified against:
1. Endpoint registered via Minimal API MapXxx
2. Handler uses Ardalis.Result, no throw for business flow
3. Validator present for all mutating commands
4. Migration clean (no phantom AlterColumn/DropTable), Designer.cs present
5. Unit tests for handler and domain edge cases
6. At least 1 integration test per endpoint
7. Gherkin scenarios exist and have step bindings
8. Frontend: MSW handler covers the contract, data-testid on interactive elements
9. Frontend API type matches backend DTO

---

## Feature Results

---

### 1. Email Verification Flow (Auth)

**Backend: PASS**

- Endpoint: POST /api/auth/verify-email registered in AuthEndpoints.cs - PASS
- Handler: VerifyEmailHandler.cs uses Result - PASS
- Validator: VerifyEmailValidator.cs present - PASS
- Migration: 20260330222115_AddEmailVerification.cs clean AddColumn, Designer.cs present - PASS
- Unit tests: VerifyEmailHandlerTests.cs and VerifyEmailValidatorTests.cs present - PASS
- Integration test: No VerifyEmail TI in AuthEndpointsTests.cs - MISSING
- Gherkin: No .feature covering email verification in Features/Auth/ - MISSING
- Note: IgnoreQueryFilters used legitimately (cross-tenant token lookup), documented in handler - PASS

**Frontend: FAIL**

- No MSW handler for POST /api/auth/verify-email in handlers/auth.ts
- No frontend page or component found for the email verification flow

**Verdict: FAIL** - missing TI, missing TF, missing MSW handler

---

### 2. Vaccination Due Reminder (AI)

**Backend: PARTIAL**

- Endpoints: POST /api/v1/ai/health-alerts/generate, GET /api/v1/ai/health-alerts/ - PASS
- Handler uses Result - PASS
- Unit tests: GenerateHealthAlertsHandlerTests.cs, HealthAlertRulesTests.cs - PASS
- Integration tests: GenerateHealthAlerts_AsVet_Returns200 present - PASS
- Gherkin: PredictiveHealthAlerts.feature has "Overdue vaccination generates a health alert" (@wip) - scenario present
- Step bindings: No [Binding] class scoped to PredictiveHealthAlerts - MISSING

**Frontend: PASS**

- health-alerts.ts MSW handler present
- PatientHealthAlerts.tsx has data-testid

**Verdict: FAIL** - 11 scenarios in PredictiveHealthAlerts.feature with zero step bindings

---

### 3. Patient Photo Upload (MedicalRecords)

**Backend: FAIL**

- Endpoints: POST/GET/DELETE /{id}/photo registered - PASS
- Handler: UploadPatientPhotoHandler.cs uses Result - PASS
- Validator: MISSING for UploadPatientPhotoCommand (no file size, MIME type, null guard)
- Migration: 20260330223257_AddPatientPhoto.cs clean AddColumn, Designer.cs present - PASS
- Unit tests: PatientPhotoDomainTests.cs present; no UploadPatientPhotoHandlerTests.cs - MISSING
- Integration test: No photo endpoint test in PatientEndpointsTests.cs - MISSING
- Gherkin: No photo scenario in MedicalRecord.feature - MISSING

**Frontend: FAIL**

- No MSW handler for POST/GET/DELETE /api/v1/patients/:id/photo

**Verdict: FAIL** - missing validator, handler unit test, TI, TF, and MSW handler

---

### 4. Reminder Channels Email/WhatsApp/Both (Notifications)

**Backend: PASS**

- Endpoint: PUT /api/v1/notifications/reminders/config registered - PASS
- Handler: UpdateReminderConfigHandler.cs uses Result - PASS
- Validator: UpdateReminderConfigValidator.cs validates lead times and channel enum - PASS
- ReminderChannel enum: Email=0, WhatsApp=1, Both=2 in Vetolib.Notifications.Contracts - PASS
- Unit tests: ReminderConfigDomainTests.cs present - PASS
- Integration test: No TI for Notifications/Reminders endpoints - MISSING
- Gherkin: No scenario for reminder channel selection - MISSING

**Frontend: FAIL - CONTRACT MISMATCH**

Backend ReminderConfigDto (flat): Appointment24hEnabled, VaccinationDueEnabled, FollowUpEnabled, Appointment24hLeadTimeHours, VaccinationDueLeadTimeDays, PreferredReminderChannel (enum Email/WhatsApp/Both).

Frontend ReminderConfigDto in lib/api/reminders.ts (nested): { appointmentReminders: { enabled, timingHours }, vaccinationReminders: { enabled }, followUpReminders: { enabled, daysAfter } }. PreferredReminderChannel is entirely absent from the frontend type. The MSW handler in reminders.ts mirrors the wrong DTO. The MSW PUT returns 204 but backend returns Result<ReminderConfigDto>.

**Verdict: FAIL** - frontend DTO incompatible with backend, PreferredReminderChannel absent, missing TI, missing TF

---

### 5. Recurring Appointment Series (Agenda)

**Backend: PASS**

- Endpoints: POST and DELETE /api/v1/appointments/series - PASS
- Handler: CreateAppointmentSeriesHandler.cs uses Result, conflict detection and business hours validation present - PASS
- Validator: CreateAppointmentSeriesValidator.cs present - PASS
- Migration: 20260330225448_AddSeriesIdToAppointment.cs clean AddColumn for SeriesId (nullable uuid), Designer.cs present - PASS
- Unit tests: CreateAppointmentSeriesHandlerTests.cs, CancelAppointmentSeriesHandlerTests.cs, RecurrenceRuleTests.cs - PASS
- Integration test: No series TI in AppointmentEndpointsTests.cs - MISSING
- Gherkin: No series scenario in Appointments.feature - MISSING

**Frontend: not evaluated** (no component found)

**Verdict: FAIL** - missing TI, missing TF

---

### 6. Enhanced Dashboard Stats (Dashboard)

**Backend: PASS**

- All 8 endpoints registered in DashboardEndpoints.cs (/stats, /today-appointments, /recent-activity, /analytics, /revenue-trend, /consultation-breakdown, /species-distribution, /vet-workload) - PASS
- All handlers use Result<T> with explicit error codes - PASS
- Unit tests: GetConsultationBreakdownHandlerTests.cs, GetVetWorkloadHandlerTests.cs - PASS
- Integration test: DashboardEndpointsTests.cs present - PASS
- Gherkin: Dashboard.feature with 6 scenarios, DashboardSteps.cs bindings present - PASS

**Frontend: PASS**

- handlers/dashboard.ts MSW handler present
- StatsCards.tsx, TodayAppointments.tsx, AnalyticsSection.tsx, RecentActivity.tsx all have data-testid
- e2e/dashboard/dashboard.spec.ts present

**Verdict: PASS**

---

### 7. Clinic Working Hours (Preferences)

**Backend: PASS**

- Endpoints: GET/PUT /api/v1/preferences/working-hours - registered in WorkingHoursEndpoints.cs - PASS
- Handlers and validator (UpsertWorkingHoursValidator.cs) present - PASS
- Migration: 20260330230418_AddClinicWorkingHours.cs clean, Designer.cs present - PASS
- Unit tests: ClinicWorkingHoursDomainTests.cs, UpsertWorkingHoursValidatorTests.cs - PASS
- Integration test: No TI for working hours - MISSING
- Gherkin: PreferencesIntegration.feature does not cover working hours CRUD - MISSING

**Frontend: FAIL**

- No MSW handler for GET/PUT /api/v1/preferences/working-hours anywhere in handlers/
- No frontend component implementing working hours configuration UI
- No lib/api/ client method for working hours

**Verdict: FAIL** - no frontend implementation at all, missing TI, missing TF

---

### 8. Patient Waiting List (Agenda)

**Backend: PASS**

- Endpoints: POST/GET/DELETE /api/v1/waitlist/ - registered in WaitlistEndpoints.cs - PASS
- Handlers and validators present for all three operations - PASS
- Unit tests: AddToWaitlistHandlerTests.cs, RemoveFromWaitlistHandlerTests.cs, WaitlistEntryDomainTests.cs - PASS
- Integration test: No waitlist TI in AppointmentEndpointsTests.cs - MISSING
- Gherkin: No waitlist scenario in any feature file - MISSING

**Frontend: not evaluated** (no component found)

**Verdict: FAIL** - missing TI, missing TF

---

### 9. Medical Record Templates (MedicalRecords)

**Backend: PASS with observation**

- All 4 CRUD endpoints registered in MedicalRecordTemplateEndpoints.cs - PASS
- Handlers and validator (CreateMedicalRecordTemplateValidator.cs) present - PASS
- Migration: 20260330232513_AddMedicalRecordTemplates.cs clean CreateTable, Designer.cs present - PASS
- Unit tests: 5 test files (creation, update, delete, domain, validator) - PASS
- Integration test: No template TI in PatientEndpointsTests.cs or separate file - MISSING
- Gherkin: No template scenario in MedicalRecord.feature - MISSING
- Observation: RBAC done inline via ClaimsPrincipal.FindFirst(ClaimTypes.Role) instead of .RequireAuthorization("VetOrAdmin") - inconsistent with project standard

**Frontend: not evaluated** (no component found)

**Verdict: FAIL** - missing TI, missing TF

---

### 10. Visit Feedback / NPS (Agenda)

**Backend: PASS**

- Endpoints: POST /appointments/{id}/feedback, GET /feedback/, GET /feedback/stats - registered in FeedbackEndpoints.cs - PASS
- Handlers and validator (SubmitVisitFeedbackValidator.cs) present - PASS
- Migration: 20260330233618_AddVisitFeedback.cs clean CreateTable for visit_feedbacks, Designer.cs present - PASS
- Unit tests: SubmitVisitFeedbackHandlerTests.cs, SubmitVisitFeedbackValidatorTests.cs, GetVisitFeedbackStatsHandlerTests.cs, VisitFeedbackDomainTests.cs - PASS
- Integration test: No feedback TI in AppointmentEndpointsTests.cs - MISSING
- Gherkin: No feedback scenario - MISSING

**Frontend: not evaluated** (no component found)

**Verdict: FAIL** - missing TI, missing TF

---

### 11. Patient Summary Export (MedicalRecords)

**Backend: PASS**

- Endpoint: GET /api/v1/patients/{id}/export/summary - registered in PatientEndpoints.cs - PASS
- Handler: GetPatientSummaryHandler.cs present - PASS
- Unit tests: GetPatientSummaryHandlerTests.cs present - PASS
- Integration test: Not covered in PatientEndpointsTests.cs - MISSING
- Gherkin: No patient summary/export scenario - MISSING

**Frontend: not evaluated** (no component found)

**Verdict: FAIL** - missing TI, missing TF

---

### 12. Stock Low Alerts (Stock)

**Backend: PASS**

- Endpoint: GET /api/v1/stock/alerts - registered in StockEndpoints.cs - PASS
- Handler: GetStockAlertsHandler.cs uses Result - PASS
- Unit tests: GetStockAlertsHandlerTests.cs present - PASS
- Integration test: GetAlerts_Authenticated_Returns200 and GetAlerts_Unauthenticated_Returns401 present in StockEndpointsTests.cs - PASS
- Gherkin: "Low stock alert" and "Expiring soon alert" scenarios in StockManagement.feature, StockManagementSteps.cs bindings present - PASS
- Note: StockManagement.feature tagged @wip - not running in CI by default

**Frontend: PASS**

- StockAlerts.tsx has data-testid on all alert containers
- stock.ts MSW handler present
- e2e/stock/stock.spec.ts present

**Verdict: PASS** - non-blocking: @wip tag prevents BDD scenarios from running in CI

---

### 13. Patient Search Improvements (MedicalRecords)

**Backend: PASS**

- Endpoint: GET /api/v1/patients accepts name, species, microchip, ownerPhone filters - PASS
- Migration: 20260330234943_AddPatientSearchIndexes.cs clean CreateIndex on ClinicId+Name, ClinicId+Species, ClinicId+Phone (with NULL filter), Designer.cs present - PASS
- Integration test: ListPatients_SearchByName_ReturnsFilteredResults present in PatientEndpointsTests.cs - PASS
- Gherkin: No dedicated patient search feature; microchip and ownerPhone filter scenarios missing - PARTIAL

**Frontend: PASS**

- patients.ts MSW handler covers GET /api/patients with search params
- PatientCard.tsx has data-testid

**Verdict: PASS** - non-blocking: microchip and ownerPhone filter Gherkin coverage missing

---

### 14. QR Code Check-In (Agenda)

**Backend: FAIL**

- Endpoints: GET /appointments/{id}/checkin-qr, POST /appointments/checkin - registered in AppointmentEndpoints.cs - PASS
- Handlers: GenerateCheckInQrHandler.cs, CheckInFromQrHandler.cs present - PASS
- Validators: MISSING for GenerateCheckInQrCommand and CheckInFromQrCommand
- Unit tests: GenerateCheckInQrHandlerTests.cs, CheckInFromQrHandlerTests.cs present - PASS
- Integration test: No QR TI in AppointmentEndpointsTests.cs - MISSING
- Gherkin: No QR check-in scenario in Appointments.feature - MISSING

**Frontend: not evaluated** (no component found)

**Verdict: FAIL** - missing validators for both commands, missing TI, missing TF

---

## Summary Table

| Feature | Backend | Frontend | TI | TF | Overall |
|---------|---------|----------|----|----|---------||
| 1. Email Verification | PASS | FAIL | FAIL | FAIL | **FAIL** |
| 2. Vaccination Due Reminder | PASS | PASS | PASS | FAIL | **FAIL** |
| 3. Patient Photo Upload | FAIL | FAIL | FAIL | FAIL | **FAIL** |
| 4. Reminder Channels | PASS | FAIL | FAIL | FAIL | **FAIL** |
| 5. Recurring Series | PASS | N/A | FAIL | FAIL | **FAIL** |
| 6. Enhanced Dashboard Stats | PASS | PASS | PASS | PASS | **PASS** |
| 7. Clinic Working Hours | PASS | FAIL | FAIL | FAIL | **FAIL** |
| 8. Patient Waiting List | PASS | N/A | FAIL | FAIL | **FAIL** |
| 9. Medical Record Templates | PASS | N/A | FAIL | FAIL | **FAIL** |
| 10. Visit Feedback / NPS | PASS | N/A | FAIL | FAIL | **FAIL** |
| 11. Patient Summary Export | PASS | N/A | FAIL | FAIL | **FAIL** |
| 12. Stock Low Alerts | PASS | PASS | PASS | PASS | **PASS** |
| 13. Patient Search | PASS | PASS | PASS | PARTIAL | **PASS** |
| 14. QR Code Check-In | FAIL | N/A | FAIL | FAIL | **FAIL** |

**PASS: 3 / 14 - FAIL: 11 / 14**

---

## Consolidated Blocking Issues

### Missing Validators (CLAUDE.md: FluentValidation required on all mutating commands)

- UploadPatientPhotoCommand - no validation on file size, MIME type, or null data
- GenerateCheckInQrCommand - no validator
- CheckInFromQrCommand - no validator

### Missing Integration Tests (1 TI minimum per endpoint, CLAUDE.md rule)

- Auth: POST /api/auth/verify-email
- Notifications: GET/PUT /api/v1/notifications/reminders/config
- Preferences: GET/PUT /api/v1/preferences/working-hours
- Agenda series: POST and DELETE /api/v1/appointments/series
- Agenda waitlist: POST, GET, DELETE /api/v1/waitlist/
- Agenda feedback: POST /appointments/{id}/feedback, GET /feedback/, GET /feedback/stats
- Agenda QR: GET /appointments/{id}/checkin-qr, POST /appointments/checkin
- MedicalRecords photo: POST, GET, DELETE /api/v1/patients/{id}/photo
- MedicalRecords templates: all 4 CRUD endpoints on /api/v1/medical-records/templates
- MedicalRecords summary: GET /api/v1/patients/{id}/export/summary

### Missing Gherkin Scenarios (BDD-first rule, CLAUDE.md section 3)

- Email verification - no .feature file exists
- Reminder channel selection (Email/WhatsApp/Both) - no scenario
- Recurring appointment series - no scenario in Appointments.feature
- Patient waiting list - no scenario
- Medical record templates - no scenario in MedicalRecord.feature
- Visit feedback / NPS - no scenario
- Patient summary export - no scenario
- QR code check-in - no scenario in Appointments.feature

### Missing Gherkin Step Bindings

- PredictiveHealthAlerts.feature has 11 scenarios, all tagged @wip, with zero step definitions bound to the feature

### Frontend Contract Mismatch (BLOCKING - silent runtime breakage)

lib/api/reminders.ts ReminderConfigDto is structurally incompatible with the backend.

Backend flat DTO: Appointment24hEnabled (bool), VaccinationDueEnabled (bool), FollowUpEnabled (bool), Appointment24hLeadTimeHours (int), VaccinationDueLeadTimeDays (int), PreferredReminderChannel (enum Email|WhatsApp|Both).

Frontend nested DTO: { appointmentReminders: { enabled, timingHours }, vaccinationReminders: { enabled }, followUpReminders: { enabled, daysAfter } }. The field PreferredReminderChannel does not exist in the frontend type. The MSW handler mirrors the wrong structure. The MSW PUT returns 204 but the backend returns Result<ReminderConfigDto>.

### Missing Frontend Implementation

- Clinic Working Hours - no MSW handler, no lib/api/ client, no page component (feature absent from frontend entirely)
- Email Verification - no MSW handler for POST /api/auth/verify-email
- Patient Photo Upload - no MSW handler for POST/GET/DELETE /api/v1/patients/:id/photo

---

## Non-Blocking Observations

- StockManagement.feature and PreferencesIntegration.feature tagged @wip and do not run in CI. Should be promoted to remove the tag.
- MedicalRecordTemplateEndpoints.cs performs RBAC inline via ClaimsPrincipal.FindFirst(ClaimTypes.Role) instead of .RequireAuthorization("VetOrAdmin") - inconsistent.
- AddPatientPhoto migration stores base64 inline in a text column. Performance risk at scale; object store reference preferred.
- WeightChart.tsx and SpeciesIcon.tsx missing data-testid - acceptable as display-only.

---

## QA Report Status

**Status: [QA_FAIL]**

11 of 14 features fail QA. The PR batch cannot merge in its current state.

Recommended remediation priority for dev agent:
1. Fix ReminderConfigDto frontend contract mismatch (feature 4) - silent runtime breakage, highest risk
2. Add validators: UploadPatientPhotoCommand, GenerateCheckInQrCommand, CheckInFromQrCommand
3. Add Gherkin scenarios and step bindings for 8 missing features + PredictiveHealthAlerts bindings
4. Add integration tests for all missing endpoint TIs (10+ endpoints)
5. Add frontend MSW handlers for working hours, email verification, patient photo
6. Implement working hours frontend (no component exists at all)