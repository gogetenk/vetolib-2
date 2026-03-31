# E2E QA Master Plan -- Vetara

**Date**: 2026-04-01
**Author**: PO + Architecte (Claude Opus 4.6)
**Scope**: Exhaustive inventory of every screen, endpoint, workflow, gap, performance risk, and security surface.

---

## Table of Contents

1. [Section 1: Frontend Screen Inventory](#section-1-frontend-screen-inventory)
2. [Section 2: Backend Endpoint Inventory](#section-2-backend-endpoint-inventory)
3. [Section 3: Critical E2E Workflows](#section-3-critical-e2e-workflows)
4. [Section 4: What is NOT Testable Today](#section-4-what-is-not-testable-today)
5. [Section 5: Performance Queries to Audit](#section-5-performance-queries-to-audit)
6. [Section 6: Security Pen Test Checklist](#section-6-security-pen-test-checklist)

---

## Section 1: Frontend Screen Inventory

### 1.1 Public Pages (No Auth Required)

| # | Route Pattern | Role | Key Components | data-testid Status | E2E Spec |
|---|---|---|---|---|---|
| 1 | `/{locale}` | public | Landing hero, CTA signup, feature cards | HAS testids | `landing.spec.ts` |
| 2 | `/{locale}/pricing` | public | Pricing tiers, comparison table, CTA | HAS testids | -- |
| 3 | `/{locale}/blog` | public | Blog list, post cards | HAS testids | -- |
| 4 | `/{locale}/blog/{slug}` | public | Blog detail, markdown render | HAS testids | -- |
| 5 | `/{locale}/developers` | public | API docs / developer landing | HAS testids | -- |
| 6 | `/{locale}/help` | public | Help center, article search | HAS testids | -- |
| 7 | `/{locale}/pet-owners` | public | Pet owner landing page | HAS testids | -- |
| 8 | `/{locale}/privacy` | public | Privacy policy | HAS testids | -- |
| 9 | `/{locale}/terms` | public | Terms of service | HAS testids | -- |
| 10 | `/{locale}/shared/{token}` | public | Shared medical record viewer | MISSING testids | -- |

### 1.2 Auth Pages

| # | Route Pattern | Role | Key Components | data-testid Status | E2E Spec |
|---|---|---|---|---|---|
| 11 | `/{locale}/login` | public | Login form, email/password, forgot link | MISSING testids | `login.spec.ts`, `auth.spec.ts` |
| 12 | `/{locale}/signup` | public | Signup form, clinic registration | MISSING testids | `signup.spec.ts` |

### 1.3 Dashboard Pages (Authenticated -- Clinic Staff)

| # | Route Pattern | Role | Key Components | data-testid Status | E2E Spec |
|---|---|---|---|---|---|
| 13 | `/{locale}/dashboard` | vet/admin/receptionist | Stats cards, today appts, recent activity, analytics charts | HAS testids | `dashboard.spec.ts` |
| 14 | `/{locale}/appointments` | vet/admin/receptionist | Appointment list, filters, calendar view | HAS testids | `appointments.spec.ts`, `agenda.spec.ts` |
| 15 | `/{locale}/appointments/new` | vet/admin/receptionist | New appointment form, slot picker, patient search | HAS testids | `appointments.spec.ts` |
| 16 | `/{locale}/appointments/{id}` | vet/admin/receptionist | Appointment detail, status transitions, QR check-in | HAS testids | `appointments.spec.ts` |
| 17 | `/{locale}/patients` | vet/admin/receptionist | Patient list, search, filters, CSV import | MISSING testids (page.tsx) | `patients-standalone.spec.ts`, `patients.spec.ts` |
| 18 | `/{locale}/patients/new` | vet/admin | New patient form, owner linking | HAS testids | `patients-standalone.spec.ts` |
| 19 | `/{locale}/patients/{id}` | vet/admin/receptionist | Patient detail, tabs (records, weights, breeding) | HAS testids | `patients-standalone.spec.ts` |
| 20 | `/{locale}/patients/{id}/records/new` | vet/admin | New medical record form, SOAP notes, prescriptions | HAS testids | `medical-records.spec.ts` |
| 21 | `/{locale}/billing` | vet/admin/receptionist | Invoice list, status filters | HAS testids | `billing.spec.ts` |
| 22 | `/{locale}/billing/new` | vet/admin/receptionist | New invoice form, item builder | HAS testids | `billing.spec.ts` |
| 23 | `/{locale}/billing/{id}` | vet/admin/receptionist | Invoice detail, status change, PDF download, e-invoicing | HAS testids | `billing.spec.ts` |
| 24 | `/{locale}/breeding` | vet/admin | Breeding dashboard (heat cycles, pregnancies, litters, lineage) | HAS testids | -- |
| 25 | `/{locale}/messages` | vet/admin/receptionist | Conversation inbox, filters | MISSING testids | `inbox.spec.ts` |
| 26 | `/{locale}/messages/{id}` | vet/admin/receptionist | Conversation detail, reply, notes, classification | MISSING testids | `conversation.spec.ts` |
| 27 | `/{locale}/stock` | vet/admin | Stock dashboard, alerts | MISSING testids (page.tsx) | `stock.spec.ts` |
| 28 | `/{locale}/stock/drugs` | vet/admin | Drug catalog list | HAS testids | -- |
| 29 | `/{locale}/stock/drugs/{id}` | vet/admin | Drug detail, interactions, dosage | HAS testids | -- |
| 30 | `/{locale}/stock/history` | vet/admin | Stock movement history | HAS testids | -- |
| 31 | `/{locale}/profile` | any authenticated | User profile, avatar | MISSING testids | -- |
| 32 | `/{locale}/settings` | admin | Settings hub | MISSING testids | -- |
| 33 | `/{locale}/settings/team` | admin | Team management, invite, role change | HAS testids | `team.spec.ts` |
| 34 | `/{locale}/settings/working-hours` | admin | Working hours config per day | HAS testids | `preferences.spec.ts` |
| 35 | `/{locale}/settings/notifications` | admin | Notification/reminder config | HAS testids | -- |
| 36 | `/{locale}/settings/preferences` | admin | General preferences | HAS testids | `preferences.spec.ts` |
| 37 | `/{locale}/settings/messaging/hours` | admin | Messaging hours config | MISSING testids | `admin-settings.spec.ts` |
| 38 | `/{locale}/settings/messaging/stats` | admin | Messaging stats dashboard | MISSING testids | -- |
| 39 | `/{locale}/settings/messaging/templates` | admin | Message template CRUD | MISSING testids | `admin-settings.spec.ts` |
| 40 | `/{locale}/settings/messaging/whatsapp` | admin | WhatsApp config | MISSING testids | -- |

### 1.4 Owner Portal Pages (Pet Owner Authenticated)

| # | Route Pattern | Role | Key Components | data-testid Status | E2E Spec |
|---|---|---|---|---|---|
| 41 | `/{locale}/portal/{clinicSlug}` | owner | Portal home, clinic info | MISSING testids | `portal.spec.ts` |
| 42 | `/{locale}/portal/{clinicSlug}/new` | owner | Register as new owner | MISSING testids | -- |
| 43 | `/{locale}/portal/{clinicSlug}/pets` | owner | My pets list | MISSING testids | -- |
| 44 | `/{locale}/portal/{clinicSlug}/pets/{animalId}` | owner | Pet detail (records, vaccinations, weight) | MISSING testids | -- |
| 45 | `/{locale}/portal/{clinicSlug}/book` | owner | Booking landing | MISSING testids | -- |
| 46 | `/{locale}/portal/{clinicSlug}/book/new` | owner | New booking form | MISSING testids | -- |
| 47 | `/{locale}/portal/{clinicSlug}/book/appointments` | owner | My appointments list | MISSING testids | -- |
| 48 | `/{locale}/portal/{clinicSlug}/book/appointments/{id}` | owner | Appointment detail | MISSING testids | -- |
| 49 | `/{locale}/portal/{clinicSlug}/conversations/{id}` | owner | Conversation with clinic | MISSING testids | `portal.spec.ts` |
| 50 | `/{locale}/portal/{clinicSlug}/consent` | owner | Consent form | MISSING testids | `consent.spec.ts` |
| 51 | `/{locale}/portal/{clinicSlug}/export` | owner | Export my data | MISSING testids | -- |
| 52 | `/{locale}/portal/{clinicSlug}/animals/{id}` | owner | Animal detail (alt route) | HAS testids | -- |

**Summary**: 52 pages total. ~30 pages MISSING data-testids on the page.tsx file itself (components used within them may have testids).

---

## Section 2: Backend Endpoint Inventory

### 2.1 Auth Module (7 endpoints + 5 user mgmt + 4 onboarding + 4 clinic-group + 1 referral)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 1 | POST | `/api/v1/auth/login` | Anonymous | auth (10/min) | YES | YES (Login.feature) |
| 2 | POST | `/api/v1/auth/refresh` | Anonymous | auth (10/min) | YES | -- |
| 3 | POST | `/api/v1/auth/verify-email` | Anonymous | auth (10/min) | YES | YES (EmailVerification.feature) |
| 4 | POST | `/api/v1/auth/logout` | Authenticated | -- | YES | -- |
| 5 | GET | `/api/v1/auth/me` | Authenticated | -- | YES | -- |
| 6 | POST | `/api/v1/auth/change-password` | Authenticated | auth (10/min) | YES | YES (ChangePassword.feature) |
| 7 | POST | `/api/v1/clinics/register` | Anonymous | signup (3/h) | -- | YES (ClinicSelfRegistration.feature) |
| 8 | GET | `/api/v1/clinics/search` | Anonymous | -- | -- | YES (ClinicSearch.feature) |
| 9 | POST | `/api/v1/users/` | Authenticated | -- | -- | YES (TeamManagement.feature) |
| 10 | GET | `/api/v1/users/` | Authenticated | -- | -- | YES (TeamManagement.feature) |
| 11 | POST | `/api/v1/users/invite` | Authenticated | -- | -- | YES (TeamManagement.feature) |
| 12 | PATCH | `/api/v1/users/{id}/role` | Authenticated | -- | -- | YES (RBAC.feature) |
| 13 | DELETE | `/api/v1/users/{id}` | Authenticated | -- | -- | YES (TeamManagement.feature) |
| 14 | GET | `/api/v1/onboarding/` | Authenticated | -- | -- | YES (OnboardingState.feature) |
| 15 | POST | `/api/v1/onboarding/steps/{stepId}/complete` | Authenticated | -- | -- | YES (OnboardingState.feature) |
| 16 | POST | `/api/v1/onboarding/banner/dismiss` | Authenticated | -- | -- | YES (OnboardingState.feature) |
| 17 | POST | `/api/v1/onboarding/checklist/dismiss` | Authenticated | -- | -- | YES (OnboardingState.feature) |
| 18 | POST | `/api/v1/clinic-groups/` | Authenticated | -- | -- | YES (MultiClinic.feature) |
| 19 | POST | `/api/v1/clinic-groups/{id}/clinics` | Authenticated | -- | -- | YES (MultiClinic.feature) |
| 20 | GET | `/api/v1/clinic-groups/{id}/clinics` | Authenticated | -- | -- | YES (MultiClinic.feature) |
| 21 | DELETE | `/api/v1/clinic-groups/{id}/clinics/{clinicId}` | Authenticated | -- | -- | YES (MultiClinic.feature) |
| 22 | POST | `/api/v1/auth/switch-clinic` | Authenticated | -- | -- | YES (MultiClinic.feature) |
| 23 | GET | `/api/v1/portal/referral-code/` | Authenticated | -- | -- | -- |

### 2.2 Portal/Owner Auth (4 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 24 | POST | `/api/v1/portal/register` | Anonymous | auth (10/min) | -- | YES (OwnerRegistration.feature) |
| 25 | POST | `/api/v1/portal/invite-vet` | Anonymous | auth (10/min) | -- | -- |
| 26 | POST | `/api/v1/portal/login` | Anonymous | auth (10/min) | -- | -- |
| 27 | POST | `/api/v1/portal/link-microchip` | Authenticated | -- | -- | -- |

### 2.3 Agenda Module (12 main + 4 consultation types + 3 feedback + 3 waitlist = 22 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 28 | POST | `/api/v1/appointments/` | Admin/Vet/Receptionist | -- | YES | YES (Appointments.feature) |
| 29 | GET | `/api/v1/appointments/` | Authenticated | -- | YES | YES (Appointments.feature) |
| 30 | PATCH | `/api/v1/appointments/{id}/status` | Authenticated | -- | YES | YES (Appointments.feature) |
| 31 | PATCH | `/api/v1/appointments/{id}/transition` | Authenticated | -- | YES | YES (Appointments.feature) |
| 32 | GET | `/api/v1/appointments/{id}` | Authenticated | -- | YES | YES |
| 33 | PUT | `/api/v1/appointments/{id}` | Authenticated | -- | YES | YES |
| 34 | GET | `/api/v1/appointments/availability` | Authenticated | -- | YES | YES (SlotSuggestion.feature) |
| 35 | POST | `/api/v1/appointments/suggest-slot` | Authenticated | -- | YES | YES (SlotSuggestion.feature) |
| 36 | POST | `/api/v1/appointments/series` | Admin/Vet/Receptionist | -- | YES | YES (RecurringAppointments.feature) |
| 37 | DELETE | `/api/v1/appointments/series/{seriesId}` | Admin/Vet/Receptionist | -- | YES | YES (RecurringAppointments.feature) |
| 38 | GET | `/api/v1/appointments/{id}/checkin-qr` | Authenticated | -- | YES | YES (QRCheckIn.feature) |
| 39 | POST | `/api/v1/appointments/checkin` | Authenticated | -- | YES | YES (QRCheckIn.feature) |
| 40 | POST | `/api/v1/consultation-types/` | Admin | -- | -- | -- |
| 41 | PUT | `/api/v1/consultation-types/{id}` | Admin | -- | -- | -- |
| 42 | DELETE | `/api/v1/consultation-types/{id}` | Admin | -- | -- | -- |
| 43 | GET | `/api/v1/consultation-types/` | Admin/Vet/Receptionist | -- | -- | -- |
| 44 | POST | `/api/v1/appointments/{id}/feedback` | Authenticated | -- | YES | YES (VisitFeedback.feature) |
| 45 | GET | `/api/v1/feedback/` | Admin | -- | YES | YES (VisitFeedback.feature) |
| 46 | GET | `/api/v1/feedback/stats` | Admin | -- | YES | YES (VisitFeedback.feature) |
| 47 | POST | `/api/v1/waitlist/` | Authenticated | -- | YES | YES (WaitingList.feature) |
| 48 | GET | `/api/v1/waitlist/` | Authenticated | -- | YES | YES (WaitingList.feature) |
| 49 | DELETE | `/api/v1/waitlist/{id}` | Admin/Vet/Receptionist | -- | YES | YES (WaitingList.feature) |

### 2.4 MedicalRecords Module (13 patient + 4 records + 4 templates + 1 owner + 5 portal + 4 shared + 3 weight + 4 drugs = 38 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 50 | POST | `/api/v1/patients/` | VetOrAdmin | api (100/min) | YES | YES (Patients.feature) |
| 51 | GET | `/api/v1/patients/` | Authenticated | api (100/min) | YES | YES (Patients.feature) |
| 52 | GET | `/api/v1/patients/{id}` | Authenticated | -- | YES | YES |
| 53 | GET | `/api/v1/patients/{id}/detail` | Authenticated | cache 2min | YES | YES |
| 54 | GET | `/api/v1/patients/{id}/export/summary` | Authenticated | cache 2min | YES | YES (PatientSummaryExport.feature) |
| 55 | GET | `/api/v1/patients/{id}/export/fhir` | Authenticated | -- | -- | -- |
| 56 | PATCH | `/api/v1/patients/{id}` | VetOrAdmin | -- | YES | YES (PatientExtendedFields.feature) |
| 57 | POST | `/api/v1/patients/import` | VetOrAdmin | -- | YES | YES (CsvImport.feature) |
| 58 | POST | `/api/v1/patients/import/fhir` | VetOrAdmin | -- | -- | -- |
| 59 | GET | `/api/v1/patients/import/template` | Authenticated | -- | -- | -- |
| 60 | POST | `/api/v1/patients/{id}/photo` | VetOrAdmin | -- | YES | -- |
| 61 | GET | `/api/v1/patients/{id}/photo` | Authenticated | -- | YES | -- |
| 62 | DELETE | `/api/v1/patients/{id}/photo` | VetOrAdmin | -- | -- | -- |
| 63 | POST | `/api/v1/patients/{patientId}/records/` | Authenticated | -- | -- | YES (MedicalRecord.feature) |
| 64 | GET | `/api/v1/patients/{patientId}/records/` | Authenticated | -- | -- | YES (MedicalRecord.feature) |
| 65 | DELETE | `/api/v1/patients/{patientId}/records/{recordId}` | Authenticated | -- | -- | -- |
| 66 | POST | `/api/v1/patients/{patientId}/records/{recordId}/prescriptions` | Authenticated | -- | -- | YES (DrugInteractionChecking.feature) |
| 67 | GET | `/api/v1/medical-records/templates/` | Authenticated | -- | YES | YES (MedicalRecordTemplates.feature) |
| 68 | POST | `/api/v1/medical-records/templates/` | Authenticated | -- | YES | YES (MedicalRecordTemplates.feature) |
| 69 | PUT | `/api/v1/medical-records/templates/{id}` | Authenticated | -- | YES | YES |
| 70 | DELETE | `/api/v1/medical-records/templates/{id}` | Authenticated | -- | YES | YES |
| 71 | POST | `/api/v1/owners/` | Authenticated | -- | -- | -- |
| 72 | GET | `/api/v1/portal/my-animals` | Authenticated | -- | -- | -- |
| 73 | GET | `/api/v1/portal/animals/{id}/records` | Authenticated | -- | -- | -- |
| 74 | GET | `/api/v1/portal/animals/{id}/vaccinations` | Authenticated | -- | YES (VaccinationReminders.feature) |
| 75 | GET | `/api/v1/portal/animals/{id}/prescriptions` | Authenticated | -- | -- | -- |
| 76 | GET | `/api/v1/portal/animals/{id}/weight` | Authenticated | -- | -- | -- |
| 77 | GET | `/api/v1/shared/{token}` | Anonymous | api (100/min) | -- | YES (RecordSharing.feature) |
| 78 | POST | `/api/v1/portal/animals/{id}/share` | Authenticated | api (100/min) | -- | YES (RecordSharing.feature) |
| 79 | GET | `/api/v1/portal/shares` | Authenticated | api (100/min) | -- | YES (RecordSharing.feature) |
| 80 | DELETE | `/api/v1/portal/shares/{id}` | Authenticated | api (100/min) | -- | YES (RecordSharing.feature) |
| 81 | POST | `/api/v1/patients/{patientId}/weights/` | VetOrAdmin | -- | -- | YES (WeightHistory.feature) |
| 82 | GET | `/api/v1/patients/{patientId}/weights/` | Authenticated | -- | -- | YES (WeightHistory.feature) |
| 83 | GET | `/api/v1/patients/{patientId}/weights/curve` | Authenticated | -- | -- | YES (WeightHistory.feature) |
| 84 | GET | `/api/v1/medical-records/drugs/` | Authenticated | api (100/min) | -- | YES (DrugInteractionChecking.feature) |
| 85 | GET | `/api/v1/medical-records/drugs/{id}` | Authenticated | api (100/min) | -- | -- |
| 86 | POST | `/api/v1/medical-records/drugs/` | VetOrAdmin | api (100/min) | -- | -- |
| 87 | POST | `/api/v1/medical-records/prescriptions/preflight` | VetOrAdmin | -- | -- | YES (DrugInteractionChecking.feature) |

### 2.5 Billing Module (8 invoice + 2 e-reporting = 10 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 88 | POST | `/api/v1/invoices/` | Admin/Vet/Receptionist | -- | YES | YES (Invoicing.feature) |
| 89 | GET | `/api/v1/invoices/` | Authenticated | -- | YES | YES (Invoicing.feature) |
| 90 | GET | `/api/v1/invoices/{id}` | Authenticated | -- | YES | YES |
| 91 | PATCH | `/api/v1/invoices/{id}/status` | Authenticated | -- | YES | YES (Invoicing.feature) |
| 92 | POST | `/api/v1/invoices/{id}/items` | Authenticated | -- | YES | YES |
| 93 | GET | `/api/v1/invoices/{id}/pdf` | Authenticated | -- | YES | -- |
| 94 | POST | `/api/v1/invoices/{id}/submit-einvoicing` | Admin/Vet | -- | YES | -- |
| 95 | GET | `/api/v1/invoices/{id}/einvoicing-status` | Authenticated | -- | YES | -- |
| 96 | POST | `/api/v1/billing/ereporting/submit` | Admin | -- | -- | -- |
| 97 | GET | `/api/v1/billing/ereporting/periods` | Admin | -- | -- | -- |

### 2.6 Breeding Module (3 heat + 4 lineage + 4 litter + 8 pregnancy = 19 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 98 | POST | `/api/v1/patients/{patientId}/heat-cycles/` | VetOrAdmin | -- | YES | YES (HeatCycle.feature) |
| 99 | GET | `/api/v1/patients/{patientId}/heat-cycles/` | VetOrAdmin | -- | YES | YES (HeatCycle.feature) |
| 100 | GET | `/api/v1/patients/{patientId}/heat-cycles/prediction` | VetOrAdmin | -- | YES | YES (HeatCycle.feature) |
| 101 | PUT | `/api/v1/patients/{id}/lineage` | Authenticated | -- | YES | YES (Lineage.feature) |
| 102 | GET | `/api/v1/patients/{id}/lineage` | Authenticated | -- | YES | YES (Lineage.feature) |
| 103 | GET | `/api/v1/patients/{id}/pedigree` | Authenticated | -- | YES | YES (Lineage.feature) |
| 104 | GET | `/api/v1/patients/{id}/descendants` | Authenticated | -- | YES | YES (Lineage.feature) |
| 105 | POST | `/api/v1/litters/` | Authenticated | -- | YES | YES (Litter.feature) |
| 106 | GET | `/api/v1/litters/{id}` | Authenticated | -- | YES | YES (Litter.feature) |
| 107 | POST | `/api/v1/litters/{id}/offspring` | Authenticated | -- | YES | YES (Litter.feature) |
| 108 | GET | `/api/v1/patients/{id}/litters` | Authenticated | -- | YES | YES (Litter.feature) |
| 109 | POST | `/api/v1/breeding/pregnancies/` | Authenticated | -- | YES | YES (Pregnancy.feature) |
| 110 | GET | `/api/v1/breeding/pregnancies/{id}` | Authenticated | -- | YES | YES (Pregnancy.feature) |
| 111 | GET | `/api/v1/breeding/pregnancies/by-patient/{patientId}` | Authenticated | -- | YES | YES (Pregnancy.feature) |
| 112 | GET | `/api/v1/breeding/pregnancies/active` | Authenticated | -- | YES | YES (Pregnancy.feature) |
| 113 | PUT | `/api/v1/breeding/pregnancies/{id}/delivery` | Authenticated | -- | YES | YES (Pregnancy.feature) |
| 114 | PUT | `/api/v1/breeding/pregnancies/{id}/loss` | Authenticated | -- | YES | YES (Pregnancy.feature) |
| 115 | POST | `/api/v1/breeding/pregnancies/{id}/checks` | Authenticated | -- | YES | YES (Pregnancy.feature) |
| 116 | PUT | `/api/v1/breeding/pregnancies/checks/{checkId}/complete` | Authenticated | -- | YES | YES (Pregnancy.feature) |

### 2.7 AI Module (7 main + 6 health alerts = 13 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 117 | POST | `/api/v1/ai/triage` | ClinicStaff | api (100/min) | YES | YES (VeterinaryTriage.feature) |
| 118 | PUT | `/api/v1/ai/triage/{id}/accept` | VetOrAdmin | api (100/min) | YES | YES |
| 119 | PUT | `/api/v1/ai/triage/{id}/override` | VetOrAdmin | api (100/min) | YES | YES |
| 120 | GET | `/api/v1/ai/no-show-prediction/{appointmentId}` | ClinicStaff | api (100/min) | YES | YES (NoShowPrediction.feature) |
| 121 | POST | `/api/v1/ai/no-show-predictions/batch` | ClinicStaff | api (100/min) | YES | YES |
| 122 | POST | `/api/v1/ai/soap-notes` | VetOrAdmin | api (100/min) | YES | -- |
| 123 | POST | `/api/v1/ai/check-interactions` | VetOrAdmin | api (100/min) | YES | YES (DrugInteractionChecking.feature) |
| 124 | GET | `/api/v1/ai/health-alerts/` | ClinicStaff | api (100/min) | YES | YES (PredictiveHealthAlerts.feature) |
| 125 | GET | `/api/v1/ai/health-alerts/patient/{patientId}` | ClinicStaff | api (100/min) | YES | YES |
| 126 | POST | `/api/v1/ai/health-alerts/generate` | VetOrAdmin | api (100/min) | YES | YES |
| 127 | PATCH | `/api/v1/ai/health-alerts/{id}/dismiss` | VetOrAdmin | api (100/min) | YES | YES |
| 128 | PATCH | `/api/v1/ai/health-alerts/{id}/acknowledge` | ClinicStaff | api (100/min) | YES | YES |
| 129 | POST | `/api/v1/ai/health-alerts/{id}/convert-to-appointment` | VetOrAdmin | api (100/min) | YES | YES |

### 2.8 Messaging Module (20+ staff endpoints + 10 portal + 1 SSE + 3 WhatsApp = ~34 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 130 | GET | `/api/v1/messaging/conversations` | Authenticated | -- | YES | YES (VetInbox.feature, ReceptionistInbox.feature) |
| 131 | GET | `/api/v1/messaging/conversations/{id}` | Authenticated | -- | YES | YES |
| 132 | POST | `/api/v1/messaging/conversations/{id}/reply` | Authenticated | -- | YES | YES |
| 133 | POST | `/api/v1/messaging/conversations/{id}/notes` | Authenticated | -- | YES | YES (StaffAttachments.feature) |
| 134 | PATCH | `/api/v1/messaging/conversations/{id}/status` | Authenticated | -- | YES | YES |
| 135 | PATCH | `/api/v1/messaging/conversations/{id}/transfer` | Authenticated | -- | YES | -- |
| 136 | PATCH | `/api/v1/messaging/conversations/{id}/category` | Authenticated | -- | YES | YES (MessageClassification.feature) |
| 137 | POST | `/api/v1/messaging/conversations/{id}/spam` | Authenticated | -- | YES | -- |
| 138 | POST | `/api/v1/messaging/conversations/{id}/convert-to-appointment` | ClinicStaff | -- | YES | -- |
| 139 | POST | `/api/v1/messaging/conversations/{cid}/messages/{mid}/add-to-record` | Vet/Admin | -- | YES | -- |
| 140 | POST | `/api/v1/messaging/conversations/outbound` | Admin | -- | YES | YES (AdminMessaging.feature) |
| 141 | GET | `/api/v1/messaging/conversations/{id}/summary` | Authenticated | -- | YES | -- |
| 142 | PATCH | `/api/v1/messaging/conversations/{id}/messages/{mid}/classify` | ClinicStaff | -- | -- | YES (MessageClassification.feature) |
| 143 | POST | `/api/v1/messaging/conversations/{id}/messages/{mid}/classify/feedback` | Authenticated | -- | -- | -- |
| 144 | GET | `/api/v1/messaging/stats/classification-accuracy` | Admin | -- | -- | -- |
| 145 | POST | `/api/v1/messaging/upload` | Authenticated | -- | YES | YES (StaffAttachments.feature) |
| 146 | GET | `/api/v1/messaging/settings/hours` | Admin | -- | YES | -- |
| 147 | PUT | `/api/v1/messaging/settings/hours` | Admin | -- | YES | -- |
| 148 | GET | `/api/v1/messaging/stats` | Admin | -- | YES | -- |
| 149 | GET | `/api/v1/messaging/templates` | ClinicStaff | -- | YES | -- |
| 150 | POST | `/api/v1/messaging/templates` | Admin | -- | YES | -- |
| 151 | PUT | `/api/v1/messaging/templates/{id}` | Admin | -- | YES | -- |
| 152 | DELETE | `/api/v1/messaging/templates/{id}` | Admin | -- | YES | -- |
| 153 | GET | `/api/v1/messaging/sse` | Authenticated | disabled | -- | -- |
| 154 | GET | `/api/v1/messaging/whatsapp/config` | Admin | -- | -- | YES (WhatsAppIntegration.feature) |
| 155 | PUT | `/api/v1/messaging/whatsapp/config` | Admin | -- | -- | YES |
| 156 | POST | `/api/v1/messaging/whatsapp/test` | Admin | -- | -- | YES |
| 157 | GET | `/api/v1/portal/categories` | Portal | -- | YES | -- |
| 158 | GET | `/api/v1/portal/conversations` | Portal | -- | YES | YES (OwnerPortal.feature) |
| 159 | GET | `/api/v1/portal/conversations/{id}` | Portal | -- | YES | YES |
| 160 | POST | `/api/v1/portal/conversations` | Portal | -- | YES | YES |
| 161 | POST | `/api/v1/portal/conversations/{id}/messages` | Portal | -- | YES | YES |
| 162 | POST | `/api/v1/portal/consent` | Portal | -- | YES | -- |
| 163 | GET | `/api/v1/portal/export` | Portal | -- | YES | -- |
| 164 | GET | `/api/v1/portal/pets` | Portal | -- | YES | -- |
| 165 | GET | `/api/v1/portal/booking/veterinarians` | Portal | -- | YES | -- |

### 2.9 Stock Module (5 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 166 | GET | `/api/v1/stock/` | Authenticated | -- | YES | YES (StockManagement.feature) |
| 167 | POST | `/api/v1/stock/` | VetOrAdmin | -- | YES | YES |
| 168 | PATCH | `/api/v1/stock/{id}` | VetOrAdmin | -- | YES | YES |
| 169 | POST | `/api/v1/stock/{id}/movements` | VetOrAdmin | -- | YES | YES |
| 170 | GET | `/api/v1/stock/alerts` | Authenticated | -- | YES | YES |

### 2.10 Notifications Module (3 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 171 | GET | `/api/v1/notifications/reminders/config` | VetOrAdmin | -- | YES | YES (ReminderChannels.feature) |
| 172 | PUT | `/api/v1/notifications/reminders/config` | VetOrAdmin | -- | YES | YES |
| 173 | GET | `/api/v1/notifications/reminders/logs` | VetOrAdmin | -- | YES | YES |

### 2.11 Preferences Module (2 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 174 | GET | `/api/v1/preferences/working-hours/` | Authenticated | -- | YES | YES (PreferencesIntegration.feature) |
| 175 | PUT | `/api/v1/preferences/working-hours/` | Admin | -- | YES | YES |

### 2.12 Dashboard (8 endpoints)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 176 | GET | `/api/dashboard/stats` | Authenticated | api (100/min) | YES | YES (Dashboard.feature) |
| 177 | GET | `/api/dashboard/today-appointments` | Authenticated | api (100/min) | YES | YES |
| 178 | GET | `/api/dashboard/recent-activity` | Authenticated | api (100/min) | YES | YES |
| 179 | GET | `/api/dashboard/analytics` | VetOrAdmin | api (100/min) | YES | YES |
| 180 | GET | `/api/dashboard/revenue-trend` | VetOrAdmin | api (100/min) | YES | YES (ClinicBenchmarking.feature) |
| 181 | GET | `/api/dashboard/consultation-breakdown` | Authenticated | api (100/min) | YES | -- |
| 182 | GET | `/api/dashboard/species-distribution` | Authenticated | api (100/min) | YES | -- |
| 183 | GET | `/api/dashboard/vet-workload` | Authenticated | api (100/min) | YES | -- |

### 2.13 Audit (1 endpoint)

| # | Method | URL | Auth | Rate Limit | Has TI | Has TF/Gherkin |
|---|---|---|---|---|---|---|
| 184 | GET | `/api/audit/` | Admin | api (100/min) | -- | YES (Audit.feature) |

**Total: 184 endpoints across 12 modules.**

---

## Section 3: Critical E2E Workflows

### Workflow 1: Clinic Signup to First Completed Consultation

**Priority**: P0 -- Core revenue path

| Step | Frontend Page | Backend Endpoint(s) | Gherkin Coverage | E2E Spec |
|---|---|---|---|---|
| 1. Clinic owner visits signup | `/signup` | -- | ClinicSelfRegistration.feature | signup.spec.ts |
| 2. Fill registration form | `/signup` | `POST /api/v1/clinics/register` | YES | signup.spec.ts |
| 3. Verify email | (email link) | `POST /api/v1/auth/verify-email` | EmailVerification.feature | -- |
| 4. Login | `/login` | `POST /api/v1/auth/login` | Login.feature | login.spec.ts |
| 5. See onboarding checklist | `/dashboard` | `GET /api/v1/onboarding/` | OnboardingState.feature | onboarding-checklist.spec.ts |
| 6. Create first patient | `/patients/new` | `POST /api/v1/patients/` | Patients.feature | patients-standalone.spec.ts |
| 7. Complete onboarding step | `/dashboard` | `POST /api/v1/onboarding/steps/{id}/complete` | OnboardingState.feature | onboarding-checklist.spec.ts |
| 8. Create first appointment | `/appointments/new` | `POST /api/v1/appointments/` | Appointments.feature | appointments.spec.ts |
| 9. Patient check-in via QR | `/appointments/{id}` | `POST /api/v1/appointments/checkin` | QRCheckIn.feature | -- |
| 10. Transition to In Progress | `/appointments/{id}` | `PATCH /api/v1/appointments/{id}/transition` | Appointments.feature | appointments.spec.ts |
| 11. Add medical record + SOAP | `/patients/{id}/records/new` | `POST /api/v1/patients/{id}/records/` | MedicalRecord.feature | medical-records.spec.ts |
| 12. Complete appointment | `/appointments/{id}` | `PATCH /api/v1/appointments/{id}/transition` | Appointments.feature | appointments.spec.ts |
| 13. Owner submits feedback | (email link) | `POST /api/v1/appointments/{id}/feedback` | VisitFeedback.feature | -- |

**Gaps**: Steps 9 and 13 have no E2E spec. Email verification (step 3) has no E2E spec (needs email interception or MSW bypass).

### Workflow 2: Pet Owner Registration to Record Sharing

**Priority**: P0 -- Owner engagement path

| Step | Frontend Page | Backend Endpoint(s) | Gherkin Coverage | E2E Spec |
|---|---|---|---|---|
| 1. Owner visits clinic portal | `/portal/{slug}` | -- | -- | portal.spec.ts |
| 2. Register as owner | `/portal/{slug}/new` | `POST /api/v1/portal/register` | OwnerRegistration.feature | -- |
| 3. Login | `/portal/{slug}` | `POST /api/v1/portal/login` | -- | portal.spec.ts |
| 4. Link animal by microchip | `/portal/{slug}/pets` | `POST /api/v1/portal/link-microchip` | -- | -- |
| 5. View my pets | `/portal/{slug}/pets` | `GET /api/v1/portal/my-animals` | -- | -- |
| 6. View medical records | `/portal/{slug}/pets/{id}` | `GET /api/v1/portal/animals/{id}/records` | MedicalRecordViewer.feature | -- |
| 7. View vaccinations | `/portal/{slug}/pets/{id}` | `GET /api/v1/portal/animals/{id}/vaccinations` | VaccinationReminders.feature | -- |
| 8. Share record via link | `/portal/{slug}/pets/{id}` | `POST /api/v1/portal/animals/{id}/share` | RecordSharing.feature | -- |
| 9. External viewer opens link | `/shared/{token}` | `GET /api/v1/shared/{token}` | RecordSharing.feature | -- |
| 10. Export my data | `/portal/{slug}/export` | `GET /api/v1/portal/export` | -- | -- |
| 11. Book an appointment | `/portal/{slug}/book/new` | Booking endpoints | -- | -- |

**Gaps**: Steps 2-11 have no dedicated E2E spec. Portal pages all MISSING data-testids. Microchip linking (step 4) has no Gherkin. Export (step 10) has no Gherkin.

### Workflow 3: Vet Morning -- Waiting Room to Prescription

**Priority**: P0 -- Daily clinical workflow

| Step | Frontend Page | Backend Endpoint(s) | Gherkin Coverage | E2E Spec |
|---|---|---|---|---|
| 1. Login as vet | `/login` | `POST /api/v1/auth/login` | Login.feature | login.spec.ts |
| 2. View dashboard | `/dashboard` | `GET /api/dashboard/stats`, `today-appointments` | Dashboard.feature | dashboard.spec.ts |
| 3. See waiting room (appts in progress) | `/appointments` | `GET /api/v1/appointments/` | Appointments.feature | appointments.spec.ts |
| 4. Open patient's appointment | `/appointments/{id}` | `GET /api/v1/appointments/{id}` | YES | appointments.spec.ts |
| 5. View patient detail | `/patients/{id}` | `GET /api/v1/patients/{id}/detail` | Patients.feature | patients-standalone.spec.ts |
| 6. AI triage (symptoms) | `/appointments/{id}` | `POST /api/v1/ai/triage` | VeterinaryTriage.feature | ai-triage.spec.ts |
| 7. Check drug interactions | (during record creation) | `POST /api/v1/ai/check-interactions` | DrugInteractionChecking.feature | prescriptions.spec.ts |
| 8. Add prescription | `/patients/{id}/records/new` | `POST /api/v1/patients/{id}/records/{rid}/prescriptions` | DrugInteractionChecking.feature | prescriptions.spec.ts |
| 9. Generate SOAP notes | `/patients/{id}/records/new` | `POST /api/v1/ai/soap-notes` | -- | -- |
| 10. Complete medical record | `/patients/{id}/records/new` | `POST /api/v1/patients/{id}/records/` | MedicalRecord.feature | medical-records.spec.ts |
| 11. Complete appointment | `/appointments/{id}` | `PATCH /api/v1/appointments/{id}/transition` | Appointments.feature | appointments.spec.ts |
| 12. Check health alerts | `/dashboard` | `GET /api/v1/ai/health-alerts/` | PredictiveHealthAlerts.feature | health-alerts.spec.ts |

**Gaps**: SOAP note generation (step 9) has no Gherkin feature.

### Workflow 4: Admin -- Team Management and Analytics

**Priority**: P1 -- Admin operations

| Step | Frontend Page | Backend Endpoint(s) | Gherkin Coverage | E2E Spec |
|---|---|---|---|---|
| 1. Login as admin | `/login` | `POST /api/v1/auth/login` | Login.feature | login.spec.ts |
| 2. Manage team | `/settings/team` | `GET /api/v1/users/` | TeamManagement.feature | team.spec.ts |
| 3. Invite new vet | `/settings/team` | `POST /api/v1/users/invite` | TeamManagement.feature | team.spec.ts |
| 4. Change user role | `/settings/team` | `PATCH /api/v1/users/{id}/role` | RBAC.feature | team.spec.ts |
| 5. Configure working hours | `/settings/working-hours` | `PUT /api/v1/preferences/working-hours/` | PreferencesIntegration.feature | preferences.spec.ts |
| 6. View analytics | `/dashboard` | `GET /api/dashboard/analytics` | Dashboard.feature | dashboard.spec.ts |
| 7. View revenue trend | `/dashboard` | `GET /api/dashboard/revenue-trend` | ClinicBenchmarking.feature | dashboard.spec.ts |
| 8. View audit log | (no dedicated page) | `GET /api/audit/` | Audit.feature | -- |
| 9. Export invoices CSV | `/billing` | (frontend-only from list) | -- | -- |
| 10. Multi-clinic management | (no dedicated page) | Clinic group endpoints | MultiClinic.feature | -- |

**Gaps**: No audit log page in frontend. No invoice CSV export endpoint. Multi-clinic UI not implemented.

### Workflow 5: Breeding -- Full Reproductive Cycle

**Priority**: P1 -- Breeder specialty feature

| Step | Frontend Page | Backend Endpoint(s) | Gherkin Coverage | E2E Spec |
|---|---|---|---|---|
| 1. Record heat cycle | `/breeding` | `POST /api/v1/patients/{id}/heat-cycles/` | HeatCycle.feature | -- |
| 2. View heat history | `/breeding` | `GET /api/v1/patients/{id}/heat-cycles/` | HeatCycle.feature | -- |
| 3. Predict next heat | `/breeding` | `GET /api/v1/patients/{id}/heat-cycles/prediction` | HeatCycle.feature | -- |
| 4. Register pregnancy | `/breeding` | `POST /api/v1/breeding/pregnancies/` | Pregnancy.feature | -- |
| 5. Schedule pregnancy check | `/breeding` | `POST /api/v1/breeding/pregnancies/{id}/checks` | Pregnancy.feature | -- |
| 6. Complete check | `/breeding` | `PUT /api/v1/breeding/pregnancies/checks/{id}/complete` | Pregnancy.feature | -- |
| 7. Record delivery | `/breeding` | `PUT /api/v1/breeding/pregnancies/{id}/delivery` | Pregnancy.feature | -- |
| 8. Register litter | `/breeding` | `POST /api/v1/litters/` | Litter.feature | -- |
| 9. Add offspring | `/breeding` | `POST /api/v1/litters/{id}/offspring` | Litter.feature | -- |
| 10. Set lineage | `/patients/{id}` | `PUT /api/v1/patients/{id}/lineage` | Lineage.feature | -- |
| 11. View pedigree | `/patients/{id}` | `GET /api/v1/patients/{id}/pedigree` | Lineage.feature | -- |
| 12. View descendants | `/patients/{id}` | `GET /api/v1/patients/{id}/descendants` | Lineage.feature | -- |

**Gaps**: ZERO E2E specs for the entire breeding module. The breeding page exists but has no Playwright test coverage at all.

### Workflow 6: Billing -- Invoice Lifecycle

**Priority**: P0 -- Revenue collection

| Step | Frontend Page | Backend Endpoint(s) | Gherkin Coverage | E2E Spec |
|---|---|---|---|---|
| 1. Create invoice | `/billing/new` | `POST /api/v1/invoices/` | Invoicing.feature | billing.spec.ts |
| 2. Add line items | `/billing/{id}` | `POST /api/v1/invoices/{id}/items` | Invoicing.feature | billing.spec.ts |
| 3. Send invoice | `/billing/{id}` | `PATCH /api/v1/invoices/{id}/status` (sent) | Invoicing.feature | billing.spec.ts |
| 4. Mark as paid | `/billing/{id}` | `PATCH /api/v1/invoices/{id}/status` (paid) | Invoicing.feature | billing.spec.ts |
| 5. Download PDF | `/billing/{id}` | `GET /api/v1/invoices/{id}/pdf` | -- | -- |
| 6. Submit e-invoicing | `/billing/{id}` | `POST /api/v1/invoices/{id}/submit-einvoicing` | -- | -- |
| 7. Check e-invoicing status | `/billing/{id}` | `GET /api/v1/invoices/{id}/einvoicing-status` | -- | -- |
| 8. E-Reporting submit | (admin settings) | `POST /api/v1/billing/ereporting/submit` | -- | -- |
| 9. E-Reporting periods | (admin settings) | `GET /api/v1/billing/ereporting/periods` | -- | -- |
| 10. Loyalty program | `/billing` | (within invoicing) | LoyaltyProgram.feature | -- |

**Gaps**: PDF download, e-invoicing, and e-reporting have no Gherkin and no E2E. Loyalty program has Gherkin but no E2E.

### Workflow 7: Messaging -- Full Conversation Lifecycle

**Priority**: P1 -- Client communication

| Step | Frontend Page | Backend Endpoint(s) | Gherkin Coverage | E2E Spec |
|---|---|---|---|---|
| 1. Owner starts conversation | `/portal/{slug}/conversations` | `POST /api/v1/portal/conversations` | OwnerPortal.feature | portal.spec.ts |
| 2. Receptionist sees inbox | `/messages` | `GET /api/v1/messaging/conversations` | ReceptionistInbox.feature | inbox.spec.ts |
| 3. Receptionist replies | `/messages/{id}` | `POST /api/v1/messaging/conversations/{id}/reply` | VetInbox.feature | conversation.spec.ts |
| 4. Transfer to vet | `/messages/{id}` | `PATCH /api/v1/messaging/conversations/{id}/transfer` | -- | -- |
| 5. Vet classifies message | `/messages/{id}` | `PATCH /api/v1/messaging/conversations/{id}/messages/{mid}/classify` | MessageClassification.feature | -- |
| 6. Add to medical record | `/messages/{id}` | `POST /api/v1/messaging/conversations/{cid}/messages/{mid}/add-to-record` | -- | -- |
| 7. Convert to appointment | `/messages/{id}` | `POST /api/v1/messaging/conversations/{id}/convert-to-appointment` | -- | -- |
| 8. Close conversation | `/messages/{id}` | `PATCH /api/v1/messaging/conversations/{id}/status` | VetInbox.feature | conversation.spec.ts |
| 9. Admin outbound message | `/messages` | `POST /api/v1/messaging/conversations/outbound` | AdminMessaging.feature | -- |
| 10. Admin configures WhatsApp | `/settings/messaging/whatsapp` | `PUT /api/v1/messaging/whatsapp/config` | WhatsAppIntegration.feature | -- |

**Gaps**: Transfer, add-to-record, and convert-to-appointment have no Gherkin. WhatsApp config has no E2E.

---

## Section 4: What is NOT Testable Today

### 4.1 Endpoints Without Integration Tests (TI)

| Endpoint File | Module | Endpoints Count | Risk |
|---|---|---|---|
| ConsultationTypeEndpoints.cs | Agenda | 4 | MEDIUM -- admin config |
| ClinicGroupEndpoints.cs | Auth | 5 | HIGH -- multi-clinic is monetization feature |
| ReferralEndpoints.cs | Auth | 1 | LOW |
| EReportingEndpoints.cs | Billing | 2 | HIGH -- UAE regulatory compliance |
| SharedRecordEndpoints.cs | MedicalRecords | 4 | MEDIUM -- owner-facing feature |
| SseEndpoints.cs | Messaging | 1 | MEDIUM -- real-time notifications |
| WhatsAppEndpoints.cs | Messaging | 3 | MEDIUM -- key communication channel |

**Total: 20 endpoints without TI.**

### 4.2 Features Without Gherkin Coverage

| Feature Area | Missing Gherkin | Risk |
|---|---|---|
| Consultation type CRUD | No .feature file at all | MEDIUM |
| SOAP note generation (AI) | No .feature | MEDIUM |
| PDF invoice download | No .feature | HIGH -- users need printable invoices |
| E-invoicing submit/status | No .feature | HIGH -- UAE regulatory |
| E-Reporting submit/periods | No .feature | HIGH -- UAE regulatory |
| FHIR import/export | No .feature | LOW -- future interop |
| Patient photo CRUD | No .feature | LOW |
| Owner creation (standalone) | No .feature | LOW |
| Portal data export | No .feature | MEDIUM -- GDPR/data portability |
| Portal microchip linking | No .feature | MEDIUM -- key owner feature |
| Portal login (standalone) | No .feature | MEDIUM |
| Referral code | No .feature | LOW |
| Messaging transfer | No .feature | MEDIUM |
| Message add-to-record | No .feature | MEDIUM |
| Conversation convert-to-appointment | No .feature | MEDIUM |
| AI classification feedback | No .feature | LOW |
| Classification accuracy stats | No .feature | LOW |
| Dashboard consultation breakdown | No .feature | LOW |
| Dashboard species distribution | No .feature | LOW |
| Dashboard vet workload | No .feature | LOW |
| SSE real-time events | No .feature (hard to BDD) | MEDIUM |

### 4.3 Frontend Pages Without data-testid

The following page.tsx files have ZERO data-testid attributes:

| Page | Impact |
|---|---|
| `/login` (page.tsx) | HIGH -- critical auth flow |
| `/signup` (page.tsx) | HIGH -- critical onboarding flow |
| `/patients` (page.tsx) | HIGH -- core daily use |
| `/messages` (page.tsx) | MEDIUM |
| `/messages/{id}` (page.tsx) | MEDIUM |
| `/profile` (page.tsx) | LOW |
| `/settings` (page.tsx) | LOW |
| `/settings/messaging/hours` | LOW |
| `/settings/messaging/stats` | LOW |
| `/settings/messaging/templates` | LOW |
| `/settings/messaging/whatsapp` | LOW |
| `/stock` (page.tsx) | MEDIUM |
| ALL `/portal/*` pages (12 pages) | HIGH -- entire owner portal untestable |
| `/shared/{token}` | MEDIUM |

**Note**: Components used within these pages may have testids (1689 total testids found across the codebase), but the page-level elements themselves are missing them.

### 4.4 E2E Specs Missing Entirely

| Area | Has Gherkin? | Has E2E Spec? | Priority |
|---|---|---|---|
| Breeding (all workflows) | YES (4 features) | NO | P1 |
| Owner Portal registration | YES | NO | P0 |
| Owner Portal pets/records | YES | NO | P0 |
| Owner Portal booking | NO | NO | P1 |
| Record sharing (public link) | YES | NO | P1 |
| Email verification flow | YES | NO | P1 |
| QR check-in scan | YES | NO | P2 |
| Audit log viewer | YES | NO | P2 |
| Multi-clinic switching | YES | NO | P1 |
| E-invoicing/E-reporting | NO | NO | P0 (regulatory) |
| SOAP note generation | NO | NO | P2 |
| WhatsApp config | YES | NO | P2 |
| PDF invoice download | NO | NO | P1 |
| FHIR import/export | NO | NO | P3 |

### 4.5 Dead Code / Orphaned Endpoints

| Item | Details |
|---|---|
| Legacy appointment group | `/api/appointments/*` -- duplicate of `/api/v1/appointments/*`. Should be removed or marked deprecated. |
| `POST /api/v1/portal/test-token` | Debug/test endpoint in Messaging PortalEndpoints. **Must NOT be deployed to production.** |
| `POST /api/v1/portal/invite-vet` | Anonymous endpoint to invite a vet -- no Gherkin, no TI, unclear business purpose. |

---

## Section 5: Performance Queries to Audit

### 5.1 N+1 Query Pattern (Confirmed)

| Location | Issue | Severity |
|---|---|---|
| `GetDescendantsHandler.cs` (Breeding) | Recursive `_patientReader.GetPatientByIdAsync()` inside `foreach` loop with up to 5 levels of depth. Each descendant triggers a separate DB call via inter-module reader. | **CRITICAL** -- O(n^depth) queries |

### 5.2 Unbounded ToListAsync (Missing Pagination)

| Handler | Query | Risk |
|---|---|---|
| `CancelAppointmentSeriesHandler` | Loads all future appointments in a series | LOW (bounded by series) |
| `CreateAppointmentHandler` | Loads all vet appointments for a day | LOW (bounded by day) |
| `CreateAppointmentSeriesHandler` | Loads all appointments in date range | MEDIUM |
| `GetAvailabilityHandler` | All appointments for date range | MEDIUM -- could be large for popular vets |
| `ListAppointmentsHandler` | Paginated but loads all for date filter | CHECK pagination impl |
| `ListConsultationTypesHandler` | All types for clinic | LOW |
| `ListVisitFeedbackHandler` | All feedback | CHECK pagination |
| `ListWaitlistEntriesHandler` | All waitlist entries | LOW |
| `GetAppointmentsAnalyticsHandler` | GroupBy on all recent appointments | MEDIUM |
| `SubmitEReportingHandler` | All invoices in period | MEDIUM |
| `ListInvoicesHandler` | Paginated but `Include(Items)` on all | CHECK -- N+1 risk with Items |
| `GetHeatCyclesHandler` | All cycles for patient | LOW |
| `GetActivePregnanciesHandler` | All active pregnancies clinic-wide | MEDIUM |
| `GetLittersByMotherHandler` | All litters with offspring | LOW |

### 5.3 Include Chains to Review

| Handler | Include Chain | Risk |
|---|---|---|
| `ListInvoicesHandler` | `.Include(i => i.Items)` on list query | MEDIUM -- loads all items for every invoice in page |
| `GetDrugCatalogEntryByIdHandler` | `.Include(SpeciesContraindications).Include(Interactions).Include(DosageGuidelines)` | LOW -- single entity |
| `ExportPatientFhirHandler` | `.Include(PatientOwners).ThenInclude(Owner)` + separate query `.Include(Prescriptions)` | MEDIUM |
| `GetPatientSummaryHandler` | `.Include(PatientOwners).ThenInclude(Owner)` | LOW |

### 5.4 CacheOutput Endpoints to Verify

| Endpoint | Cache Policy | Verify |
|---|---|---|
| `GET /api/v1/patients/` (list) | 30s, VaryByQuery + VaryByHeader(Auth) | Verify cache invalidation on patient create/update |
| `GET /api/v1/patients/{id}` (detail) | Moderate2min | Verify eviction on patient update |
| `GET /api/v1/patients/{id}/export/summary` | Moderate2min | Verify eviction on record changes |
| `GET /api/dashboard/stats` | Dashboard1min | Acceptable staleness |
| `GET /api/dashboard/today-appointments` | Dashboard1min | Verify new check-ins appear within 1min |
| `GET /api/dashboard/analytics` | Dashboard1min | Acceptable |
| `GET /api/dashboard/revenue-trend` | Dashboard1min | Acceptable |
| `GET /api/dashboard/consultation-breakdown` | Dashboard1min | Acceptable |
| `GET /api/dashboard/species-distribution` | Dashboard1min | Acceptable |
| `GET /api/dashboard/vet-workload` | Dashboard1min | Acceptable |

### 5.5 Missing Indexes to Investigate

Endpoints that likely need composite indexes not yet visible in configurations:

| Query Pattern | Current Index? | Recommendation |
|---|---|---|
| Invoices filtered by ClinicId + Status + CreatedAt | YES (migration 20260330) | Recently added -- verify it works |
| MedicalRecords by PatientId + ExaminedAt | YES (migration 20260330) | Recently added -- verify |
| Patient search by Name, Microchip, OwnerPhone | YES (migration 20260330) | Recently added -- verify |
| Conversations by ClinicId + Status + LastMessageAt | UNKNOWN | CHECK MessagingDbContext configuration |
| HealthAlerts by ClinicId + Status + CreatedAt | UNKNOWN | CHECK AIDbContext configuration |
| WaitlistEntries by ClinicId + PreferredTimeSlot | UNKNOWN | CHECK -- used in slot notification handler |

---

## Section 6: Security Pen Test Checklist

### 6.1 SQL Injection / NoSQL Injection

| # | Test | Target | Method |
|---|---|---|---|
| S-1 | SQL injection in patient search | `GET /api/v1/patients/?name='OR 1=1--` | Parameterized query via EF Core should prevent |
| S-2 | SQL injection in clinic search | `GET /api/v1/clinics/search?query='DROP TABLE` | Same as above |
| S-3 | SQL injection in appointment filters | `GET /api/v1/appointments/?date='; DROP TABLE` | Same |
| S-4 | SQL injection in invoice list filters | `GET /api/v1/invoices/?status=paid' OR '1'='1` | Same |
| S-5 | Import CSV with malicious SQL in cells | `POST /api/v1/patients/import` | Verify CSV parser sanitizes |

### 6.2 XSS (Cross-Site Scripting)

| # | Test | Target | Method |
|---|---|---|---|
| S-6 | XSS in patient name | `POST /api/v1/patients/` with `name: <script>alert(1)</script>` | Verify output encoding |
| S-7 | XSS in medical record notes | `POST /api/v1/patients/{id}/records/` with SOAP containing JS | Verify HTML sanitization |
| S-8 | XSS in conversation message | `POST /api/v1/messaging/conversations/{id}/reply` | Verify sanitization |
| S-9 | XSS in invoice description | `POST /api/v1/invoices/{id}/items` | Verify |
| S-10 | XSS in owner name via portal | `POST /api/v1/portal/register` | Verify |
| S-11 | XSS in drug name (custom drug) | `POST /api/v1/medical-records/drugs/` | Verify |
| S-12 | Stored XSS in message template | `POST /api/v1/messaging/templates` | Verify -- admin-created, shown to owners |
| S-13 | XSS in blog slug/URL path | `/{locale}/blog/<script>` | Verify Next.js routing handles |

### 6.3 CSRF (Cross-Site Request Forgery)

| # | Test | Target | Method |
|---|---|---|---|
| S-14 | CSRF on state-changing endpoints | All POST/PUT/PATCH/DELETE | Verify SameSite cookie or anti-CSRF token |
| S-15 | CSRF on clinic registration | `POST /api/v1/clinics/register` | Verify |
| S-16 | CSRF on role change | `PATCH /api/v1/users/{id}/role` | HIGH risk -- privilege escalation |

### 6.4 Authentication Bypass

| # | Test | Target | Method |
|---|---|---|---|
| S-17 | Access authenticated endpoint without token | All `RequireAuthorization()` endpoints | Send request with no Authorization header |
| S-18 | Access with expired JWT | All endpoints | Send expired token, verify 401 |
| S-19 | Access with malformed JWT | All endpoints | Send garbage token |
| S-20 | Access with token from wrong signing key | All endpoints | Forge a token with different key |
| S-21 | Token reuse after logout | `GET /api/v1/auth/me` | Login, logout, reuse token |
| S-22 | Refresh token reuse after rotation | `POST /api/v1/auth/refresh` | Use old refresh token after new one issued |
| S-23 | Anonymous access to shared records | `GET /api/v1/shared/{token}` | Verify only valid tokens work |
| S-24 | Portal test-token endpoint in production | `POST /api/v1/portal/test-token` | **CRITICAL** -- must be disabled in prod |

### 6.5 Authorization / Role Bypass

| # | Test | Target | Method |
|---|---|---|---|
| S-25 | Receptionist tries VetOrAdmin endpoints | `POST /api/v1/patients/`, stock CRUD, etc. | Login as receptionist, call vet endpoints |
| S-26 | Vet tries Admin-only endpoints | `PATCH /api/v1/users/{id}/role`, audit, e-reporting | Login as vet, call admin endpoints |
| S-27 | Owner tries clinic staff endpoints | All `/api/v1/` (non-portal) endpoints | Login with portal token, call staff API |
| S-28 | Anonymous tries to submit feedback | `POST /api/v1/appointments/{id}/feedback` | Requires auth -- verify |
| S-29 | Admin of Clinic A tries Clinic B endpoints | Any endpoint | Verify multi-tenancy filter |

### 6.6 IDOR (Insecure Direct Object Reference) / Tenant Isolation

| # | Test | Target | Method |
|---|---|---|---|
| S-30 | Access patient from different clinic | `GET /api/v1/patients/{id}` | Login as Clinic A, request Clinic B patient ID |
| S-31 | Access appointment from different clinic | `GET /api/v1/appointments/{id}` | Same pattern |
| S-32 | Access invoice from different clinic | `GET /api/v1/invoices/{id}` | Same pattern |
| S-33 | Access medical record from different clinic | `GET /api/v1/patients/{pid}/records/` | Same pattern |
| S-34 | Access conversation from different clinic | `GET /api/v1/messaging/conversations/{id}` | Same pattern |
| S-35 | Modify patient from different clinic | `PATCH /api/v1/patients/{id}` | Same pattern |
| S-36 | Access shared record with brute-forced token | `GET /api/v1/shared/{guessed-token}` | Verify tokens are cryptographically random |
| S-37 | Portal owner accesses another owner's animals | `GET /api/v1/portal/animals/{id}/records` | Login as Owner A, use Owner B animal ID |
| S-38 | Portal owner accesses another clinic's portal | Cross-clinic portal access | Verify clinic slug validation |
| S-39 | EF Core IgnoreQueryFilters bypass | Audit all usages of IgnoreQueryFilters | **Must not exist outside seeds/migrations** |

**Note**: The `MultiTenantDbContext` global query filter (`WHERE ClinicId = @current`) is the primary defense. Verify it cannot be bypassed via:
- Direct SQL queries (if any exist)
- Endpoints that don't set the clinic context correctly
- The portal endpoints (different auth context)

### 6.7 Rate Limiting Verification

| # | Test | Target | Expected |
|---|---|---|---|
| S-40 | Brute force login | `POST /api/v1/auth/login` | 429 after 10 requests/min |
| S-41 | Brute force portal login | `POST /api/v1/portal/login` | 429 after 10 requests/min |
| S-42 | Spam clinic registrations | `POST /api/v1/clinics/register` | 429 after 3 requests/hour |
| S-43 | API abuse on patient list | `GET /api/v1/patients/` | 429 after 100 requests/min |
| S-44 | **Endpoints WITHOUT rate limiting** | See list below | **VULNERABILITY** |

**25 endpoint files have NO rate limiting at all:**
- AppointmentEndpoints (12 routes)
- ConsultationTypeEndpoints (4 routes)
- FeedbackEndpoints (3 routes)
- WaitlistEndpoints (3 routes)
- ClinicGroupEndpoints (5 routes)
- OnboardingEndpoints (4 routes)
- ReferralEndpoints (1 route)
- UserEndpoints (5 routes)
- EReportingEndpoints (2 routes)
- InvoiceEndpoints (8 routes)
- HeatCycleEndpoints (3 routes)
- LineageEndpoints (4 routes)
- LitterEndpoints (4 routes)
- PregnancyEndpoints (8 routes)
- MedicalRecordEndpoints (4 routes)
- MedicalRecordTemplateEndpoints (4 routes)
- OwnerEndpoints (1 route)
- OwnerPortalEndpoints (5 routes)
- WeightEndpoints (3 routes)
- MessagingEndpoints (~20 routes)
- PortalEndpoints (Messaging, ~10 routes)
- WhatsAppEndpoints (3 routes)
- ReminderEndpoints (3 routes)
- WorkingHoursEndpoints (2 routes)
- StockEndpoints (5 routes)

All are behind `RequireAuthorization()` so require a valid JWT, but a compromised token could abuse these without throttling.

### 6.8 File Upload Security

| # | Test | Target | Method |
|---|---|---|---|
| S-45 | Upload malicious file as patient photo | `POST /api/v1/patients/{id}/photo` | Upload .exe disguised as .jpg |
| S-46 | Upload oversized file | `POST /api/v1/patients/{id}/photo` | 100MB file |
| S-47 | Upload malicious file in messaging | `POST /api/v1/messaging/upload` | Same as above |
| S-48 | Path traversal in file upload | Photo/messaging upload | `filename: ../../../etc/passwd` |
| S-49 | SVG with embedded JS | Patient photo | Upload SVG containing `<script>` |

### 6.9 Other Security Checks

| # | Test | Target | Method |
|---|---|---|---|
| S-50 | CORS misconfiguration | All endpoints | Verify `Access-Control-Allow-Origin` is restrictive |
| S-51 | HTTP security headers | All responses | Check `X-Content-Type-Options`, `X-Frame-Options`, `Strict-Transport-Security` |
| S-52 | Sensitive data in error responses | All endpoints | Verify stack traces not leaked in production |
| S-53 | JWT in URL query parameter | Shared record token vs JWT confusion | Verify tokens are different systems |
| S-54 | SSE connection hijacking | `GET /api/v1/messaging/sse` | Verify auth check on SSE stream |
| S-55 | Denial of service via SSE | `GET /api/v1/messaging/sse` | Open many connections (no rate limit, disabled) |
| S-56 | Password complexity bypass | `POST /api/v1/auth/change-password` | Try weak passwords |
| S-57 | Email enumeration via login | `POST /api/v1/auth/login` | Check if error reveals registered emails |
| S-58 | Email enumeration via registration | `POST /api/v1/clinics/register` | Same |
| S-59 | Verify QR check-in HMAC cannot be forged | `POST /api/v1/appointments/checkin` | Modify QR payload, recalculate HMAC attempt |

---

## Summary Statistics

| Metric | Count |
|---|---|
| **Total frontend pages** | 52 |
| **Pages missing data-testids** | ~30 (page.tsx level) |
| **Total backend endpoints** | 184 |
| **Endpoints missing TI** | 20 (across 7 files) |
| **Endpoints missing rate limiting** | ~120+ (across 25 files) |
| **Gherkin feature files** | 54 |
| **Features without any Gherkin** | 21 areas identified |
| **E2E Playwright specs** | 37 files |
| **Workflows with zero E2E coverage** | Breeding (all), Owner Portal (most), E-invoicing, Multi-clinic |
| **N+1 query patterns** | 1 confirmed critical (GetDescendants), others to verify |
| **CacheOutput endpoints** | 10 (need invalidation testing) |
| **Security tests defined** | 59 checks |
| **Critical security findings** | test-token endpoint, SSE DoS, 25 endpoint files without rate limiting |

---

## Priority Action Items

### P0 -- Block launch
1. Remove or gate `POST /api/v1/portal/test-token` from production builds
2. Add rate limiting to all 25 endpoint files without it
3. Add data-testids to login and signup pages
4. Write E2E specs for Owner Portal registration and pet viewing
5. Write Gherkin + TI for E-invoicing and E-reporting (UAE regulatory)
6. Write TI for ClinicGroupEndpoints (multi-clinic monetization)

### P1 -- Before GA
7. Add data-testids to all 12 portal pages
8. Write E2E specs for breeding workflows
9. Write E2E for PDF invoice download
10. Fix N+1 in GetDescendantsHandler (batch load patients)
11. Write TI for SharedRecordEndpoints, WhatsAppEndpoints, SseEndpoints
12. Write Gherkin for consultation type CRUD
13. Write E2E for multi-clinic switching
14. Verify cache invalidation on all CacheOutput endpoints
15. Add missing indexes for Messaging and AI modules

### P2 -- Post-GA
16. Write E2E for QR check-in flow
17. Write E2E for SOAP note generation
18. Write Gherkin for FHIR import/export
19. Complete security pen test (all 59 checks)
20. Remove legacy `/api/appointments/*` group
21. Add pagination verification for unbounded ToListAsync queries
