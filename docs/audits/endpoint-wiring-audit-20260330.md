# Endpoint Wiring Audit — 2026-03-30

Audit of every API endpoint registered in Vetolib.
Checked: handler existence, MediatR command/query existence, integration test coverage, policy registration.

---

## Summary

- **Total endpoint groups**: 29 (including legacy aliases)
- **Total individual routes**: ~107 (excluding legacy duplicates)
- **DEAD groups (zero routes)**: 2 (BreedingEndpoints, Preferences)
- **UNREACHABLE endpoints (Notifications)**: 3 (registered but never mapped in Program.cs)
- **Endpoints with integration tests**: ~25
- **Endpoints WITHOUT integration tests**: ~82

---

## Critical Findings

### 1. DEAD CODE: BreedingEndpoints.cs — empty group
**File**: `src/backend/Modules/Breeding/Vetolib.Breeding/Api/BreedingEndpoints.cs`
Registers `/api/v1/breeding` group with `RequireAuthorization()` but contains ZERO routes.
Comment says "Endpoints will be added by subsequent tasks". Still mapped in `BreedingModuleServiceRegistrar.MapBreedingEndpoints()`.

### 2. DEAD CODE: Preferences module — no endpoints at all
**File**: `src/backend/Modules/Preferences/Vetolib.Preferences/ModuleServiceRegistrar.cs`
`MapPreferencesEndpoints()` is called in Program.cs (line 339) but the method body is empty.
Comment says "No endpoints in this scaffold task". No endpoint files exist in the Preferences module.

### 3. UNREACHABLE: Notifications/Reminders — never mapped in Program.cs
**File**: `src/backend/Modules/Notifications/Vetolib.Notifications/NotificationsModuleServiceRegistrar.cs`
`MapNotificationsEndpoints()` exists and maps `ReminderEndpoints` (3 routes), but **Program.cs never calls it**.
Program.cs line 340 has `app.MapBreedingEndpoints()` — there is no `app.MapNotificationsEndpoints()` call anywhere.
The 3 reminder endpoints are defined, have handlers, have MediatR commands — but are **unreachable at runtime**.

---

## Full Endpoint Inventory

### Module: Agenda

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/appointments` | POST | CreateAppointment | CreateAppointmentCommand | Yes | LIVE |
| `/api/v1/appointments` | GET | ListAppointments | ListAppointmentsQuery | Yes | LIVE |
| `/api/v1/appointments/{id}/status` | PATCH | UpdateStatus | UpdateAppointmentStatusCommand | No | LIVE |
| `/api/v1/appointments/{id}/transition` | PATCH | TransitionAppointment | UpdateAppointmentStatusCommand | Yes | LIVE |
| `/api/v1/appointments/{id}` | GET | GetAppointmentById | GetAppointmentByIdQuery | No | LIVE |
| `/api/v1/appointments/{id}` | PUT | EditAppointment | EditAppointmentCommand | No | LIVE |
| `/api/v1/appointments/availability` | GET | GetAvailability | GetAvailabilityQuery | No | LIVE |
| `/api/v1/appointments/suggest-slot` | POST | SuggestSlot | SuggestSlotQuery | No | LIVE |
| `/api/appointments` (legacy) | POST | CreateAppointment | (same as above) | No | LIVE (alias) |
| `/api/appointments` (legacy) | GET | ListAppointments | (same as above) | No | LIVE (alias) |
| `/api/appointments/{id}/transition` (legacy) | PATCH | TransitionAppointment | (same as above) | No | LIVE (alias) |
| `/api/appointments/availability` (legacy) | GET | GetAvailability | (same as above) | No | LIVE (alias) |
| `/api/appointments/{id}` (legacy) | GET | GetAppointmentById | (same as above) | No | LIVE (alias) |
| `/api/appointments/{id}` (legacy) | PUT | EditAppointment | (same as above) | No | LIVE (alias) |
| `/api/v1/consultation-types` | POST | Create | CreateConsultationTypeCommand | No | LIVE |
| `/api/v1/consultation-types/{id}` | PUT | Update | UpdateConsultationTypeCommand | No | LIVE |
| `/api/v1/consultation-types/{id}` | DELETE | Deactivate | DeactivateConsultationTypeCommand | No | LIVE |
| `/api/v1/consultation-types` | GET | List | ListConsultationTypesQuery | No | LIVE |

### Module: Auth

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/auth/login` | POST | Login | LoginCommand | Yes | LIVE |
| `/api/v1/auth/refresh` | POST | Refresh | RefreshTokenCommand | Yes | LIVE |
| `/api/v1/auth/logout` | POST | Logout | LogoutCommand | No | LIVE |
| `/api/v1/auth/me` | GET | GetMe | GetCurrentUserQuery | Yes | LIVE |
| `/api/v1/auth/change-password` | POST | ChangePassword | ChangePasswordCommand | No | LIVE |
| `/api/v1/auth/switch-clinic` | POST | SwitchClinic | SwitchClinicCommand | No | LIVE |
| `/api/v1/clinics/register` | POST | RegisterClinic | RegisterClinicCommand | Yes | LIVE |
| `/api/v1/clinic-groups` | POST | CreateClinicGroup | CreateClinicGroupCommand | No | LIVE |
| `/api/v1/clinic-groups/{id}/clinics` | POST | AddClinicToGroup | AddClinicToGroupCommand | No | LIVE |
| `/api/v1/clinic-groups/{id}/clinics` | GET | ListGroupClinics | ListGroupClinicsQuery | No | LIVE |
| `/api/v1/clinic-groups/{id}/clinics/{clinicId}` | DELETE | RemoveClinicFromGroup | RemoveClinicFromGroupCommand | No | LIVE |
| `/api/v1/users` | POST | CreateUser | CreateUserCommand | No | LIVE |
| `/api/v1/users` | GET | GetUsers | ListUsersQuery | Yes | LIVE |
| `/api/v1/users/invite` | POST | InviteUser | InviteUserCommand | No | LIVE |
| `/api/v1/users/{id}/role` | PATCH | ChangeRole | ChangeUserRoleCommand | No | LIVE |
| `/api/v1/users/{id}` | DELETE | DeactivateUser | DeactivateUserCommand | No | LIVE |
| `/api/v1/onboarding` | GET | GetState | GetOnboardingStateQuery | No | LIVE |
| `/api/v1/onboarding/steps/{stepId}/complete` | POST | CompleteStep | CompleteOnboardingStepCommand | No | LIVE |
| `/api/v1/onboarding/banner/dismiss` | POST | DismissBanner | DismissWelcomeBannerCommand | No | LIVE |
| `/api/v1/onboarding/checklist/dismiss` | POST | DismissChecklist | DismissChecklistCommand | No | LIVE |

### Module: AI

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/ai/triage` | POST | TriageSymptoms | TriageSymptomsCommand | No | LIVE |
| `/api/v1/ai/triage/{id}/accept` | PUT | AcceptTriage | AcceptTriageCommand | No | LIVE |
| `/api/v1/ai/triage/{id}/override` | PUT | OverrideTriage | OverrideTriageCommand | No | LIVE |
| `/api/v1/ai/no-show-prediction/{appointmentId}` | GET | PredictNoShow | PredictNoShowCommand | No | LIVE |
| `/api/v1/ai/no-show-predictions/batch` | POST | PredictNoShowBatch | PredictNoShowBatchCommand | No | LIVE |
| `/api/v1/ai/soap-notes` | POST | GenerateSoapNotes | GenerateSoapNotesCommand | No | LIVE |
| `/api/v1/ai/check-interactions` | POST | CheckInteractions | CheckInteractionsQuery | No | LIVE |
| `/api/v1/ai/health-alerts` | GET | GetHealthAlerts | GetHealthAlertsQuery | No | LIVE |
| `/api/v1/ai/health-alerts/patient/{patientId}` | GET | GetPatientHealthAlerts | GetPatientHealthAlertsQuery | No | LIVE |
| `/api/v1/ai/health-alerts/generate` | POST | GenerateAlerts | GenerateHealthAlertsCommand | No | LIVE |
| `/api/v1/ai/health-alerts/{id}/dismiss` | PATCH | DismissHealthAlert | DismissHealthAlertCommand | No | LIVE |
| `/api/v1/ai/health-alerts/{id}/acknowledge` | PATCH | AcknowledgeHealthAlert | AcknowledgeHealthAlertCommand | No | LIVE |
| `/api/v1/ai/health-alerts/{id}/convert-to-appointment` | POST | ConvertAlertToAppointment | ConvertAlertToAppointmentCommand | No | LIVE |

### Module: Billing

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/invoices` | POST | CreateInvoice | CreateInvoiceCommand | Yes | LIVE |
| `/api/v1/invoices` | GET | ListInvoices | ListInvoicesQuery | No | LIVE |
| `/api/v1/invoices/{id}` | GET | GetInvoiceById | GetInvoiceByIdQuery | No | LIVE |
| `/api/v1/invoices/{id}/status` | PATCH | UpdateInvoiceStatus | UpdateInvoiceStatusCommand | Yes | LIVE |
| `/api/v1/invoices/{id}/items` | POST | AddInvoiceItem | AddInvoiceItemCommand | Yes | LIVE |
| `/api/v1/invoices/{id}/pdf` | GET | DownloadInvoicePdf | GenerateInvoicePdfQuery | Yes | LIVE |
| `/api/v1/invoices/{id}/submit-einvoicing` | POST | SubmitToEInvoicing | SubmitToEInvoicingCommand | No | LIVE |
| `/api/v1/invoices/{id}/einvoicing-status` | GET | GetEInvoicingStatus | GetEInvoicingStatusQuery | No | LIVE |
| `/api/v1/billing/ereporting/submit` | POST | SubmitEReporting | SubmitEReportingCommand | No | LIVE |
| `/api/v1/billing/ereporting/periods` | GET | GetEReportingPeriods | GetEReportingPeriodsQuery | No | LIVE |

### Module: MedicalRecords

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/patients` | POST | CreatePatient | CreatePatientCommand | Yes | LIVE |
| `/api/v1/patients` | GET | ListPatients | ListPatientsQuery | Yes | LIVE |
| `/api/v1/patients/{id}` | GET | GetPatientById | GetPatientByIdQuery | No | LIVE |
| `/api/v1/patients/{id}/detail` | GET | GetPatientDetail | GetPatientDetailQuery | No | LIVE |
| `/api/v1/patients/{id}` | PATCH | UpdatePatient | UpdatePatientCommand | No | LIVE |
| `/api/v1/patients/import` | POST | ImportPatients | ImportPatientsCommand | Yes | LIVE |
| `/api/v1/patients/import/template` | GET | GetImportTemplate | (inline, no MediatR) | No | LIVE |
| `/api/v1/patients/{patientId}/records` | POST | AddMedicalRecord | AddMedicalRecordCommand | Yes | LIVE |
| `/api/v1/patients/{patientId}/records` | GET | ListMedicalRecords | ListMedicalRecordsQuery | No | LIVE |
| `/api/v1/patients/{patientId}/records/{recordId}` | DELETE | DeleteMedicalRecord | (inline, always returns error) | No | LIVE |
| `/api/v1/patients/{patientId}/records/{recordId}/prescriptions` | POST | AddPrescription | AddPrescriptionCommand | No | LIVE |
| `/api/v1/owners` | POST | CreateOwner | CreateOwnerCommand | No | LIVE |
| `/api/v1/medical-records/drugs` | GET | SearchDrugs | SearchDrugCatalogQuery | No | LIVE |
| `/api/v1/medical-records/drugs/{id}` | GET | GetDrugById | GetDrugCatalogEntryByIdQuery | No | LIVE |
| `/api/v1/medical-records/drugs` | POST | AddCustomDrug | AddCustomDrugCommand | No | LIVE |
| `/api/v1/medical-records/prescriptions/preflight` | POST | PrescriptionPreflight | GetPrescriptionPreflightQuery | No | LIVE |
| `/api/v1/patients/{patientId}/weights` | POST | AddWeightEntry | AddWeightEntryCommand | No | LIVE |
| `/api/v1/patients/{patientId}/weights` | GET | GetWeightHistory | GetWeightHistoryQuery | No | LIVE |
| `/api/v1/patients/{patientId}/weights/curve` | GET | GetWeightCurve | GetWeightCurveQuery | No | LIVE |

### Module: Breeding

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/breeding` | GROUP | (empty) | N/A | No | **DEAD** |
| `/api/v1/patients/{patientId}/heat-cycles` | POST | RecordHeatCycle | RecordHeatCycleCommand | No | LIVE |
| `/api/v1/patients/{patientId}/heat-cycles` | GET | GetHeatCycles | GetHeatCyclesQuery | No | LIVE |
| `/api/v1/patients/{patientId}/heat-cycles/prediction` | GET | PredictNextHeat | PredictNextHeatQuery | No | LIVE |
| `/api/v1/patients/{id}/lineage` | PUT | SetLineage | SetLineageCommand | No | LIVE |
| `/api/v1/patients/{id}/lineage` | GET | GetLineage | GetLineageQuery | No | LIVE |
| `/api/v1/patients/{id}/pedigree` | GET | GetPedigree | GetPedigreeQuery | No | LIVE |
| `/api/v1/patients/{id}/descendants` | GET | GetDescendants | GetDescendantsQuery | No | LIVE |
| `/api/v1/litters` | POST | Create | CreateLitterCommand | No | LIVE |
| `/api/v1/litters/{id}` | GET | GetById | GetLitterByIdQuery | No | LIVE |
| `/api/v1/litters/{id}/offspring` | POST | AddOffspring | AddOffspringToLitterCommand | No | LIVE |
| `/api/v1/patients/{id}/litters` | GET | GetByMother | GetLittersByMotherQuery | No | LIVE |
| `/api/v1/breeding/pregnancies` | POST | Create | CreatePregnancyCommand | No | LIVE |
| `/api/v1/breeding/pregnancies/{id}` | GET | GetById | GetPregnancyByIdQuery | No | LIVE |
| `/api/v1/breeding/pregnancies/by-patient/{patientId}` | GET | GetByPatient | GetPregnanciesByPatientQuery | No | LIVE |
| `/api/v1/breeding/pregnancies/active` | GET | GetActive | GetActivePregnanciesQuery | No | LIVE |
| `/api/v1/breeding/pregnancies/{id}/delivery` | PUT | RecordDelivery | RecordDeliveryCommand | No | LIVE |
| `/api/v1/breeding/pregnancies/{id}/loss` | PUT | RecordLoss | RecordLossCommand | No | LIVE |
| `/api/v1/breeding/pregnancies/{id}/checks` | POST | ScheduleCheck | ScheduleCheckCommand | No | LIVE |
| `/api/v1/breeding/pregnancies/checks/{checkId}/complete` | PUT | CompleteCheck | CompleteCheckCommand | No | LIVE |

### Module: Messaging

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/messaging/conversations` | GET | (inline) | ListConversationsQuery | Yes | LIVE |
| `/api/v1/messaging/conversations/{id}` | GET | (inline) | GetConversationByIdQuery | Yes | LIVE |
| `/api/v1/messaging/conversations/{id}/reply` | POST | (inline) | SendReplyCommand | Yes | LIVE |
| `/api/v1/messaging/conversations/{id}/notes` | POST | (inline) | AddInternalNoteCommand | Yes | LIVE |
| `/api/v1/messaging/conversations/{id}/status` | PATCH | (inline) | ChangeConversationStatusCommand | Yes | LIVE |
| `/api/v1/messaging/conversations/{id}/transfer` | PATCH | (inline) | TransferConversationCommand | No | LIVE |
| `/api/v1/messaging/conversations/{id}/category` | PATCH | (inline) | RecategorizeConversationCommand | No | LIVE |
| `/api/v1/messaging/conversations/{id}/spam` | POST | (inline) | MarkAsSpamCommand | No | LIVE |
| `/api/v1/messaging/conversations/{id}/convert-to-appointment` | POST | (inline) | ConvertToAppointmentCommand | No | LIVE |
| `/api/v1/messaging/conversations/{cId}/messages/{mId}/add-to-record` | POST | (inline) | AddMessageToRecordCommand | No | LIVE |
| `/api/v1/messaging/conversations/outbound` | POST | (inline) | CreateOutboundConversationCommand | Yes | LIVE |
| `/api/v1/messaging/conversations/{id}/summary` | GET | (inline) | GetConversationSummaryQuery | No | LIVE |
| `/api/v1/messaging/conversations/{id}/messages/{mId}/classify` | PATCH | (inline) | OverrideClassificationCommand | No | LIVE |
| `/api/v1/messaging/conversations/{id}/messages/{mId}/classify/feedback` | POST | (inline) | ClassificationFeedbackCommand | No | LIVE |
| `/api/v1/messaging/stats/classification-accuracy` | GET | (inline) | GetClassificationAccuracyQuery | No | LIVE |
| `/api/v1/messaging/upload` | POST | (inline) | UploadFilesCommand | No | LIVE |
| `/api/v1/messaging/settings/hours` | GET | (inline) | GetMessagingHoursQuery | No | LIVE |
| `/api/v1/messaging/settings/hours` | PUT | (inline) | UpdateMessagingHoursCommand | No | LIVE |
| `/api/v1/messaging/stats` | GET | (inline) | GetTriageStatsQuery | No | LIVE |
| `/api/v1/messaging/templates` | GET | (inline) | ListTemplatesQuery | No | LIVE |
| `/api/v1/messaging/templates` | POST | (inline) | CreateTemplateCommand | No | LIVE |
| `/api/v1/messaging/templates/{id}` | PUT | (inline) | UpdateTemplateCommand | No | LIVE |
| `/api/v1/messaging/templates/{id}` | DELETE | (inline) | DeleteTemplateCommand | No | LIVE |
| `/api/v1/messaging/sse` | GET | HandleSseAsync | (direct DB, no MediatR) | No | LIVE |
| `/api/v1/messaging/whatsapp/config` | GET | (inline) | GetWhatsAppConfigQuery | No | LIVE |
| `/api/v1/messaging/whatsapp/config` | PUT | (inline) | UpdateWhatsAppConfigCommand | No | LIVE |
| `/api/v1/messaging/whatsapp/test` | POST | (inline) | SendWhatsAppTestCommand | No | LIVE |

### Module: Messaging — Owner Portal (MagicLink auth)

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/portal/test-token` | POST | (inline, dev only) | (direct DB) | No | LIVE (dev/test only) |
| `/api/v1/portal/categories` | GET | (inline) | N/A (returns enum values) | No | LIVE |
| `/api/v1/portal/conversations` | GET | (inline) | ListOwnerConversationsQuery | No | LIVE |
| `/api/v1/portal/conversations/{id}` | GET | (inline) | GetOwnerConversationByIdQuery | No | LIVE |
| `/api/v1/portal/conversations` | POST | (inline) | CreateOwnerConversationCommand | No | LIVE |
| `/api/v1/portal/conversations/{id}/messages` | POST | (inline) | SendOwnerMessageCommand | No | LIVE |
| `/api/v1/portal/consent` | POST | (inline) | AcceptConsentCommand | No | LIVE |
| `/api/v1/portal/export` | GET | (inline) | ExportOwnerConversationsQuery | No | LIVE |
| `/api/v1/portal/pets` | GET | (inline) | ListOwnerPetsQuery | No | LIVE |
| `/api/v1/portal/booking/veterinarians` | GET | (inline) | ListClinicVeterinariansQuery | No | LIVE |

### Module: Notifications

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/notifications/reminders/config` | GET | GetConfig | GetReminderConfigQuery | No | **UNREACHABLE** |
| `/api/v1/notifications/reminders/config` | PUT | UpdateConfig | UpdateReminderConfigCommand | No | **UNREACHABLE** |
| `/api/v1/notifications/reminders/logs` | GET | GetLogs | ListReminderLogsQuery | No | **UNREACHABLE** |

### Module: Stock

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/v1/stock` | GET | List | ListStockItemsQuery | No | LIVE |
| `/api/v1/stock` | POST | Create | CreateStockItemCommand | No | LIVE |
| `/api/v1/stock/{id}` | PATCH | Update | UpdateStockItemCommand | No | LIVE |
| `/api/v1/stock/{id}/movements` | POST | RecordMovement | RecordStockMovementCommand | No | LIVE |
| `/api/v1/stock/alerts` | GET | GetAlerts | GetStockAlertsQuery | No | LIVE |

### Module: Preferences

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| (none) | — | — | — | No | **DEAD (empty registrar)** |

### Cross-module: Dashboard (Vetolib.Api)

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/dashboard/stats` | GET | GetStats | GetTodayAppointmentsQuery + GetUnpaidInvoicesTotalQuery + GetPatientCountQuery | No | LIVE |
| `/api/dashboard/today-appointments` | GET | GetTodayAppointments | GetTodayAppointmentsQuery | No | LIVE |
| `/api/dashboard/recent-activity` | GET | GetRecentActivity | GetTodayAppointmentsQuery | No | LIVE |
| `/api/dashboard/analytics` | GET | GetAnalytics | GetRevenueByMonthQuery + GetAppointmentsAnalyticsQuery + GetPatientsBySpeciesQuery | No | LIVE |

### Cross-module: Audit (Vetolib.Api)

| Route | Method | Handler | MediatR Command/Query | Has TI? | Status |
|---|---|---|---|---|---|
| `/api/audit` | GET | GetAuditLog | (direct DB query, no MediatR) | No | LIVE |

---

## Policy Audit

### Authorization Policies

| Policy Name | Used By | Registered? |
|---|---|---|
| `ClinicStaff` | AI, Messaging (multiple endpoints) | Yes (AuthModuleServiceRegistrar line 78) |
| `VetOrAdmin` | AI, Dashboard, Breeding, MedicalRecords, Stock, Notifications | Yes (AuthModuleServiceRegistrar line 85) |
| Role-based (`RequireRole(...)`) | Auth, Agenda, Billing, Messaging, Audit | Built-in (no custom policy needed) |

All named authorization policies are registered. No orphan policies found.

### Rate Limiting Policies

| Policy Name | Used By | Registered? |
|---|---|---|
| `auth` | Auth login, refresh, change-password | Yes (Program.cs line 260) |
| `signup` | Clinic registration | Yes (Program.cs line 267) |
| `api` | AI, MedicalRecords (drugs, patients), Dashboard, Audit | Yes (Program.cs line 274) |

All named rate limiting policies are registered. No orphan policies found.

### Output Cache Policies

| Policy Name | Used By | Registered? |
|---|---|---|
| `Dashboard1min` | Dashboard stats, today-appointments, analytics | Yes (Program.cs line 289) |
| `Moderate2min` | Patient by ID, patient detail | Yes (Program.cs line 293) |
| Inline `.CacheOutput(p => ...)` | ListPatients | N/A (inline config) |

All named output cache policies are registered. No orphan policies found.

---

## Integration Test Coverage by Module

| Module | Endpoints with TI | Endpoints without TI | Coverage |
|---|---|---|---|
| Agenda | 3 | 15 (incl. legacy) | 17% |
| Auth | 5 | 15 | 25% |
| AI | 0 | 13 | 0% |
| Billing | 4 | 6 | 40% |
| MedicalRecords | 4 | 15 | 21% |
| Breeding | 0 | 20 | 0% |
| Messaging (staff) | 6 | 17 | 26% |
| Messaging (portal) | 0 | 10 | 0% |
| Notifications | 0 | 3 | 0% (also unreachable) |
| Stock | 0 | 5 | 0% |
| Preferences | 0 | 0 | N/A (dead) |
| Dashboard | 0 | 4 | 0% |
| Audit | 0 | 1 | 0% |

---

## Actionable Items

1. **FIX**: Add `app.MapNotificationsEndpoints();` to Program.cs — 3 endpoints are fully implemented but unreachable.
2. **CLEANUP**: Remove or mark `BreedingEndpoints.cs` — the empty group at `/api/v1/breeding` serves no purpose (pregnancy endpoints use `/api/v1/breeding/pregnancies` directly).
3. **CLEANUP**: Remove or mark `MapPreferencesEndpoints()` call in Program.cs — the method is a no-op.
4. **TESTING**: AI module has 13 endpoints and 0 integration tests.
5. **TESTING**: Breeding module has 20 endpoints and 0 integration tests.
6. **TESTING**: Stock module has 5 endpoints and 0 integration tests.
7. **TESTING**: Dashboard has 4 endpoints and 0 integration tests.
8. **TESTING**: Owner Portal has 10 endpoints and 0 integration tests.
9. **TESTING**: Notifications has 3 endpoints and 0 integration tests.
10. **NOTE**: Legacy `/api/appointments/*` aliases duplicate the `/api/v1/appointments/*` routes — consider deprecation timeline.
