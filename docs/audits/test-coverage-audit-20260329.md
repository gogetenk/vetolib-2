# Test Coverage Audit - 2026-03-29

## 1. MediatR Handlers: Unit Test Coverage

**Total handlers: 120 | Tested: 52 | Untested: 68 (43% coverage)**

### By Module

| Module | Handlers | Tested | Untested | Coverage |
|---|---|---|---|---|
| AI | 13 | 5 | 8 | 38% |
| Agenda | 15 | 5 | 10 | 33% |
| Auth | 19 | 9 | 10 | 47% |
| Billing | 14 | 9 | 5 | 64% |
| Breeding | 15 | 3 | 12 | 20% |
| MedicalRecords | 12 | 6 | 6 | 50% |
| Messaging | 34 | 7 | 27 | 21% |
| Notifications | 3 | 0 | 3 | 0% |
| Stock | 8 | 5 | 3 | 63% |

### Untested Handlers (by module)

**AI**
- AcceptTriageHandler
- AcknowledgeHealthAlertHandler
- ConvertAlertToAppointmentHandler
- DismissHealthAlertHandler
- GenerateSoapNotesHandler (only validator tested)
- OverrideTriageHandler
- PredictNoShowHandler
- PredictNoShowBatchHandler

**Agenda**
- CreateConsultationTypeHandler
- DeactivateConsultationTypeHandler
- GetAppointmentByIdHandler
- GetAppointmentCountHandler
- GetAppointmentsAnalyticsHandler
- GetAvailabilityHandler
- GetTodayAppointmentsHandler
- ListAppointmentsHandler
- ListConsultationTypesHandler
- UpdateConsultationTypeHandler

**Auth**
- AddClinicToGroupHandler
- CompleteOnboardingStepHandler
- CreateUserHandler
- GetCurrentUserHandler
- GetUsersHandler
- InviteUserHandler
- ListGroupClinicsHandler
- ListUsersHandler
- RefreshTokenHandler
- RemoveClinicFromGroupHandler

**Billing**
- GenerateInvoicePdfHandler
- GetInvoiceByIdHandler
- GetInvoiceCountHandler
- GetRevenueByMonthHandler
- ListInvoicesHandler

**Breeding**
- AddOffspringToLitterHandler
- CompleteCheckHandler
- GetActivePregnanciesHandler
- GetHeatCyclesHandler
- GetLitterByIdHandler
- GetLittersByMotherHandler
- GetPregnanciesByPatientHandler
- GetPregnancyByIdHandler
- PredictNextHeatHandler
- RecordDeliveryHandler
- RecordLossHandler
- ScheduleCheckHandler

**MedicalRecords**
- CreateOwnerHandler
- GetPatientByIdHandler
- GetPatientCountHandler
- GetPatientDetailHandler
- GetPatientsBySpeciesHandler
- ImportPatientsHandler

**Messaging** (27 untested -- worst module)
- AcceptConsentHandler, AddInternalNoteHandler, AddMessageToRecordHandler
- ChangeConversationStatusHandler, ConvertToAppointmentHandler
- CreateOutboundConversationHandler, CreateOwnerConversationHandler
- CreateTemplateHandler, DeleteTemplateHandler
- ExportOwnerConversationsHandler, GetConversationSummaryHandler
- GetMessagingHoursHandler, GetTriageStatsHandler, GetWhatsAppConfigHandler
- ListOwnerConversationsHandler, ListOwnerPetsHandler, ListTemplatesHandler
- MarkAsSpamHandler, RecategorizeConversationHandler
- SendOwnerMessageHandler, SendReplyHandler, SendWhatsAppTestHandler
- TransferConversationHandler, UpdateMessagingHoursHandler
- UpdateTemplateHandler, UpdateWhatsAppConfigHandler, UploadFilesHandler

**Notifications** (0% -- 3 handlers, none tested)
- UpdateReminderConfigHandler
- GetReminderConfigHandler
- ListReminderLogsHandler

**Stock**
- CreateStockItemHandler
- RecordStockMovementHandler
- UpdateStockItemHandler

---

## 2. Features vs Step Definitions (BDD Acceptance)

**Total .feature files: 40 | With step definitions: 27 | Missing steps: 13 (33% uncovered)**

### Features WITH step definitions
- AI/NoShowPrediction.feature -> NoShowPredictionSteps.cs
- AI/VeterinaryTriage.feature -> TriageSteps.cs
- Agenda/Appointments.feature -> AppointmentSteps.cs
- Agenda/SlotSuggestion.feature -> SlotSuggestionSteps.cs
- Audit/Audit.feature -> AuditSteps.cs
- Auth/ChangePassword.feature -> ChangePasswordSteps.cs
- Auth/ClinicSelfRegistration.feature -> ClinicSelfRegistrationSteps.cs
- Auth/Login.feature -> LoginSteps.cs
- Auth/OnboardingState.feature -> OnboardingSteps.cs
- Auth/RBAC.feature -> RbacSteps.cs
- Billing/Invoicing.feature -> InvoicingSteps.cs
- Dashboard/Dashboard.feature -> DashboardSteps.cs
- MedicalRecords/MedicalRecord.feature -> MedicalRecordSteps.cs
- Messaging/AdminMessaging.feature -> AdminMessagingSteps.cs
- Messaging/AssistantAccess.feature -> AssistantAccessSteps.cs
- Messaging/MessageTriage.feature -> MessageTriageSteps.cs
- Messaging/OwnerPortal.feature -> OwnerPortalSteps.cs
- Messaging/ReceptionistInbox.feature -> ReceptionistInboxSteps.cs
- Messaging/VetInbox.feature -> VetInboxSteps.cs
- Patients/CsvImport.feature -> CsvImportSteps.cs
- Patients/Patients.feature -> PatientSteps.cs
- Preferences/PreferencesIntegration.feature -> PreferencesIntegrationSteps.cs
- Prescriptions/DrugInteractionChecking.feature -> DrugInteractionSteps.cs
- Prescriptions/StockPrescriptionIntegration.feature -> StockPrescriptionSteps.cs
- Stock/StockManagement.feature -> StockManagementSteps.cs
- Users/TeamManagement.feature -> TeamManagementSteps.cs
- Dashboard/ClinicBenchmarking.feature -> DashboardSteps.cs (shared)

### Features MISSING step definitions
| Feature | Module | Priority |
|---|---|---|
| AI/PredictiveHealthAlerts.feature | AI | Medium |
| Auth/MultiClinic.feature | Auth | High |
| Auth/SubscriptionPlans.feature | Auth | High |
| Billing/LoyaltyProgram.feature | Billing | Medium |
| Messaging/MessageClassification.feature | Messaging | High |
| Messaging/StaffAttachments.feature | Messaging | Medium |
| Messaging/WhatsAppIntegration.feature | Messaging | High |
| Breeding/HeatCycle.feature | Breeding | High |
| Breeding/Lineage.feature | Breeding | Medium |
| Breeding/Litter.feature | Breeding | High |
| Breeding/Pregnancy.feature | Breeding | High |
| MedicalRecords/PatientExtendedFields.feature | MedicalRecords | Medium |
| MedicalRecords/WeightHistory.feature | MedicalRecords | Medium |

---

## 3. Endpoints vs Integration Tests

**Total endpoint files: 27 | Modules with TI: 4 | Modules without TI: 7 (74% modules uncovered)**

### Endpoint count by module (unique Map* calls, excluding legacy duplicates)

| Module | Endpoint File(s) | Endpoints | Integration Tests | Status |
|---|---|---|---|---|
| Agenda | AppointmentEndpoints, ConsultationTypeEndpoints | ~12 | AppointmentEndpointsTests.cs | PARTIAL |
| Auth | AuthEndpoints, UserEndpoints, ClinicEndpoints, ClinicGroupEndpoints, OnboardingEndpoints | ~18 | AuthEndpointsTests.cs | PARTIAL |
| Billing | InvoiceEndpoints, EReportingEndpoints | ~10 | InvoiceEndpointsTests.cs | PARTIAL |
| MedicalRecords | PatientEndpoints, MedicalRecordEndpoints, DrugCatalogEndpoints, OwnerEndpoints, WeightEndpoints | ~16 | PatientEndpointsTests.cs | PARTIAL |
| AI | AIEndpoints, HealthAlertEndpoints | ~13 | NONE | MISSING |
| Messaging | MessagingEndpoints, PortalEndpoints, SseEndpoints, WhatsAppEndpoints | ~33 | NONE | MISSING |
| Notifications | ReminderEndpoints | 3 | NONE | MISSING |
| Stock | StockEndpoints | 5 | NONE | MISSING |
| Breeding | BreedingEndpoints, PregnancyEndpoints, LitterEndpoints, HeatCycleEndpoints | ~15 | NONE | MISSING |

**Total endpoints: ~125 | Modules with any TI: 4/9 (44%) | 7 modules have ZERO integration tests**

---

## 4. Domain Entities: Factory Create() Tests

**Total entities with Create() factory: 39 | With domain tests: 15 | Untested: 24 (38% coverage)**

### Tested (have *DomainTests.cs files)
- Appointment (Agenda)
- ConsultationType (Agenda)
- User (Auth)
- HealthAlert (AI)
- Invoice (Billing) -- via InvoicePdfTests
- ClinicPreferenceDefault (Preferences)
- ConsentAuditEntry (Preferences)
- UserPreference (Preferences)
- ReminderLog (Notifications)
- ReminderConfig (Notifications)
- Patient (MedicalRecords)
- WeightEntry (MedicalRecords)
- HeatCycle (Breeding)
- Pregnancy (Breeding)
- PregnancyCheck (Breeding)
- Litter (Breeding)

### Untested Create() factories
| Entity | Module | Priority |
|---|---|---|
| TriageResult | AI | Medium |
| Clinic | Auth | High |
| ClinicGroup | Auth | Medium |
| OnboardingState | Auth | Low |
| RefreshToken | Auth | Medium |
| InvoiceNumber | Billing | Low |
| InvoiceItem | Billing | Medium |
| EReportingPeriod | Billing | Medium |
| Owner | MedicalRecords | Medium |
| MedicalRecord | MedicalRecords | High |
| Prescription | MedicalRecords | High |
| DrugCatalogEntry | MedicalRecords | Medium |
| Conversation | Messaging | High |
| Message | Messaging | High |
| MessageAttachment | Messaging | Low |
| MessagingHours | Messaging | Low |
| OwnerPortalToken | Messaging | Low |
| PendingUpload | Messaging | Low |
| ResponseTemplate | Messaging | Low |
| WhatsAppBusinessAccount | Messaging | Medium |
| WhatsAppPhoneMapping | Messaging | Low |
| ReplyAudit | Messaging | Low |
| StockItem | Stock | Medium |
| StockMovement | Stock | Medium |

---

## 5. Validators Without Tests

**Total FluentValidation validators: 67 | Tested: 11 | Untested: 56 (16% coverage)**

### Tested validators
- GenerateSoapNotesValidator (AI)
- RegisterClinicValidator (Auth)
- DismissChecklistValidator (Auth)
- DismissWelcomeBannerValidator (Auth)
- CreateInvoiceValidator (Billing)
- CreatePatientValidator (MedicalRecords)
- DeleteTemplateValidator (Messaging)
- MarkAsSpamValidator (Messaging)
- SendWhatsAppTestValidator (Messaging)
- FileTypeValidator (Messaging)
- DecrementStockByDrugCatalogEntryValidator (Stock)

### Untested validators (56 total) -- selected high-priority ones

| Validator | Module | Priority |
|---|---|---|
| CreateAppointmentValidator | Agenda | High |
| EditAppointmentValidator | Agenda | High |
| UpdateAppointmentStatusValidator | Agenda | High |
| LoginValidator | Auth | High |
| CreateUserValidator | Auth | High |
| InviteUserValidator | Auth | High |
| ChangePasswordValidator | Auth | High |
| AddMedicalRecordValidator | MedicalRecords | High |
| AddPrescriptionValidator | MedicalRecords | High |
| CreateOwnerValidator | MedicalRecords | Medium |
| UpdatePatientValidator | MedicalRecords | Medium |
| AddWeightEntryValidator | MedicalRecords | Medium |
| SendReplyValidator | Messaging | High |
| CreateOutboundConversationValidator | Messaging | High |
| UpdateWhatsAppConfigValidator | Messaging | Medium |
| CreateStockItemValidator | Stock | Medium |
| RecordStockMovementValidator | Stock | Medium |
| RecordHeatCycleValidator | Breeding | High |
| CreatePregnancyValidator | Breeding | High |
| CreateLitterValidator | Breeding | High |
| All 8 Breeding validators | Breeding | High |

---

## Summary

| Category | Total | Covered | Gap | Coverage % |
|---|---|---|---|---|
| Handlers (unit tests) | 120 | 52 | 68 | 43% |
| Features (step definitions) | 40 | 27 | 13 | 68% |
| Modules (integration tests) | 9 | 4 | 5 | 44% |
| Domain factories (Create tests) | 39 | 15 | 24 | 38% |
| Validators | 67 | 11 | 56 | 16% |

---

## Priority Action Plan

### P0 -- Critical (blocks quality confidence)

1. **Messaging module integration tests** -- 33 endpoints, 27 untested handlers, 0 integration tests. Largest module with worst coverage.
2. **Breeding module step definitions** -- 4 feature files (HeatCycle, Pregnancy, Litter, Lineage) with 0 step definitions. Module recently built, BDD gaps.
3. **Auth MultiClinic + SubscriptionPlans step definitions** -- core business features with no acceptance tests.

### P1 -- High (significant risk)

4. **Breeding handler unit tests** -- only 3/15 handlers tested (20%). RecordDelivery, RecordLoss, ScheduleCheck, CompleteCheck all untested.
5. **AI module integration tests** -- 13 endpoints, 0 integration tests.
6. **Stock module integration tests** -- 5 endpoints, 0 integration tests.
7. **Validator tests for Agenda, Auth, MedicalRecords** -- CreateAppointmentValidator, LoginValidator, CreateUserValidator all untested.
8. **MedicalRecords WeightHistory + PatientExtendedFields step definitions** -- recently added features, no BDD.

### P2 -- Medium (technical debt)

9. **Notifications handler tests** -- 0/3 handlers tested, though module is small.
10. **Query handler tests across all modules** -- most GET handlers lack unit tests (ListAppointments, GetAvailability, etc.).
11. **Domain factory tests** -- Conversation, Message, MedicalRecord, Prescription Create() untested.
12. **Remaining 45+ validator tests** -- especially all Breeding and remaining Messaging validators.

### P3 -- Low (nice to have)

13. **SSE endpoint testing** (Messaging)
14. **Legacy endpoint group testing** (Agenda)
15. **MessageAttachment, OwnerPortalToken, ReplyAudit domain tests**
