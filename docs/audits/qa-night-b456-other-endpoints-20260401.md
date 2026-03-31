# QA Audit: Billing, Breeding, Messaging, Notifications, Stock, AI Endpoints

**Date**: 2026-04-01
**Auditor**: QA Agent (automated)
**Scope**: All endpoints in Billing, Breeding, Messaging, Notifications, Stock, AI modules
**Status**: Read-only audit -- no PRs created

---

## Summary

| Module | Endpoints | WithSummary+Desc | Auth | Rate Limiting | Handler Exists | Validator Exists | Integration Tests |
|---|---|---|---|---|---|---|---|
| **Billing (Invoices)** | 8 | 8/8 | 8/8 | 8/8 | 8/8 | 3/4 commands | 4/8 endpoints |
| **Billing (EReporting)** | 2 | 2/2 | 2/2 | 2/2 | 2/2 | 1/1 command | 0/2 endpoints |
| **Breeding (HeatCycles)** | 3 | 3/3 | 3/3 | 3/3 | 3/3 | 1/1 command | 3/3 endpoints |
| **Breeding (Pregnancies)** | 8 | 8/8 | 8/8 | 8/8 | 8/8 | 6/6 commands | 8/8 endpoints |
| **Breeding (Lineage)** | 4 | 4/4 | 4/4 | 4/4 | 4/4 | 1/1 command | 4/4 endpoints |
| **Breeding (Litters)** | 4 | 4/4 | 4/4 | 3/4 | 4/4 | 2/2 commands | 4/4 endpoints |
| **Messaging (Staff)** | 22 | 22/22 | 22/22 | 22/22 | 22/22 | 18/18 commands | 7/22 endpoints |
| **Messaging (Portal)** | 10 | 10/10 | 10/10 | 9/10 | 9/9 | 3/3 commands | 10/10 endpoints |
| **Messaging (WhatsApp)** | 3 | 3/3 | 3/3 | 3/3 | 3/3 | 2/2 commands | 0/3 endpoints |
| **Messaging (SSE)** | 1 | 1/1 | 1/1 | 0/1 (intentional) | N/A (inline) | N/A | 0/1 endpoints |
| **Notifications (Reminders)** | 3 | 3/3 | 3/3 | 3/3 | 3/3 | 1/1 command | 2/3 endpoints |
| **Stock** | 5 | 5/5 | 5/5 | 5/5 | 5/5 | 3/3 commands | 5/5 endpoints |
| **AI (Core)** | 7 | 7/7 | 7/7 | 7/7 | 7/7 | 6/6 commands | 7/7 endpoints |
| **AI (Health Alerts)** | 6 | 6/6 | 6/6 | 6/6 | 6/6 | 3/3 commands | 6/6 endpoints |

**Overall: 86 endpoints audited.**

---

## Findings by Module

### 1. Billing -- InvoiceEndpoints (8 endpoints)

**Endpoints:**
| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | POST | /api/v1/invoices | OK | Admin/Vet/Receptionist | OK | CreateInvoiceHandler | CreateInvoiceValidator | YES |
| 2 | GET | /api/v1/invoices | OK | RequireAuth | OK | ListInvoicesHandler | N/A (query) | NO |
| 3 | GET | /api/v1/invoices/{id} | OK | RequireAuth | OK | GetInvoiceByIdHandler | N/A (query) | NO |
| 4 | PATCH | /api/v1/invoices/{id}/status | OK | RequireAuth | OK | UpdateInvoiceStatusHandler | UpdateInvoiceStatusValidator | YES |
| 5 | POST | /api/v1/invoices/{id}/items | OK | RequireAuth | OK | AddInvoiceItemHandler | AddInvoiceItemValidator | YES |
| 6 | GET | /api/v1/invoices/{id}/pdf | OK | RequireAuth | OK | GenerateInvoicePdfHandler | N/A (query) | YES |
| 7 | POST | /api/v1/invoices/{id}/submit-einvoicing | OK | Admin/Vet | OK | SubmitToEInvoicingHandler | SubmitToEInvoicingValidator | NO |
| 8 | GET | /api/v1/invoices/{id}/einvoicing-status | OK | RequireAuth | OK | GetEInvoicingStatusHandler | N/A (query) | NO |
| 9 | GET | /api/v1/invoices/export/csv | OK | Admin | OK | ExportInvoicesCsvHandler | N/A (query) | NO |

**Issues:**
- **CRITICAL -- Missing TI for 5 endpoints**: `GET /invoices`, `GET /invoices/{id}`, `POST /submit-einvoicing`, `GET /einvoicing-status`, `GET /export/csv` have no integration tests.
- `GET /invoices` and `GET /invoices/{id}` are basic CRUD -- should have at minimum a 200 + 401 test.

### 2. Billing -- EReportingEndpoints (2 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | POST | /api/v1/billing/ereporting/submit | OK | Admin | OK | SubmitEReportingHandler | SubmitEReportingValidator | NO |
| 2 | GET | /api/v1/billing/ereporting/periods | OK | Admin | OK | GetEReportingPeriodsHandler | N/A (query) | NO |

**Issues:**
- **CRITICAL -- Zero integration tests** for EReporting. No test file exists.

### 3. Breeding -- HeatCycleEndpoints (3 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | POST | /patients/{id}/heat-cycles | OK | VetOrAdmin | OK | RecordHeatCycleHandler | RecordHeatCycleValidator | YES |
| 2 | GET | /patients/{id}/heat-cycles | OK | VetOrAdmin | OK | GetHeatCyclesHandler | N/A | YES |
| 3 | GET | /patients/{id}/heat-cycles/prediction | OK | VetOrAdmin | OK | PredictNextHeatHandler | N/A | YES |

**Status: PASS -- all endpoints fully covered.**

### 4. Breeding -- PregnancyEndpoints (8 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | POST | /breeding/pregnancies | OK | RequireAuth | OK | CreatePregnancyHandler | CreatePregnancyValidator | YES |
| 2 | GET | /breeding/pregnancies/{id} | OK | RequireAuth | OK | GetPregnancyByIdHandler | N/A | YES |
| 3 | GET | /breeding/pregnancies/by-patient/{id} | OK | RequireAuth | OK | GetPregnanciesByPatientHandler | N/A | YES |
| 4 | GET | /breeding/pregnancies/active | OK | RequireAuth | OK | GetActivePregnanciesHandler | N/A | YES |
| 5 | PUT | /breeding/pregnancies/{id}/delivery | OK | RequireAuth | OK | RecordDeliveryHandler | RecordDeliveryValidator | YES |
| 6 | PUT | /breeding/pregnancies/{id}/loss | OK | RequireAuth | OK | RecordLossHandler | RecordLossValidator | YES |
| 7 | POST | /breeding/pregnancies/{id}/checks | OK | RequireAuth | OK | ScheduleCheckHandler | ScheduleCheckValidator | YES |
| 8 | PUT | /breeding/pregnancies/checks/{id}/complete | OK | RequireAuth | OK | CompleteCheckHandler | CompleteCheckValidator | YES |

**Status: PASS -- all endpoints fully covered.**

### 5. Breeding -- LineageEndpoints (4 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | PUT | /patients/{id}/lineage | OK | RequireAuth | OK | SetLineageHandler | SetLineageValidator | YES |
| 2 | GET | /patients/{id}/lineage | OK | RequireAuth | OK | GetLineageHandler | N/A | YES |
| 3 | GET | /patients/{id}/pedigree | OK | RequireAuth | OK | GetPedigreeHandler | N/A | YES |
| 4 | GET | /patients/{id}/descendants | OK | RequireAuth | OK | GetDescendantsHandler | N/A | YES |

**Status: PASS -- all endpoints fully covered.**

### 6. Breeding -- LitterEndpoints (4 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | POST | /litters | OK | RequireAuth | OK (group) | CreateLitterHandler | CreateLitterValidator | YES |
| 2 | GET | /litters/{id} | OK | RequireAuth | OK (group) | GetLitterByIdHandler | N/A | YES |
| 3 | POST | /litters/{id}/offspring | OK | RequireAuth | OK (group) | AddOffspringToLitterHandler | AddOffspringToLitterValidator | YES |
| 4 | GET | /patients/{id}/litters | OK | RequireAuth | **MISSING** | GetLittersByMotherHandler | N/A | YES |

**Issues:**
- **MEDIUM -- Missing rate limiting on `GET /patients/{id}/litters`**: This endpoint is registered outside the rate-limited group via `app.MapGet(...)` instead of `group.MapGet(...)`. It has auth but no `.RequireRateLimiting("api")`.

### 7. Messaging -- MessagingEndpoints (22 endpoints)

All 22 endpoints have WithSummary/WithDescription, proper auth, and rate limiting via the group. All handlers exist and return `Result<T>`.

**Issues:**
- **CRITICAL -- Missing TI for 15 endpoints**: The test file `MessagingEndpointsTests.cs` covers only 7 endpoints:
  - CreateOutboundConversation (3 tests)
  - ListConversations (3 tests)
  - GetConversationById (3 tests)
  - ChangeConversationStatus (3 tests)
  - SendReply (3 tests)
  - AddInternalNote (2 tests)

  **Missing TI for:**
  - PATCH /conversations/{id}/transfer
  - PATCH /conversations/{id}/category
  - POST /conversations/{id}/spam
  - POST /conversations/{id}/convert-to-appointment
  - POST /conversations/{id}/messages/{messageId}/add-to-record
  - GET /conversations/{id}/summary
  - PATCH /conversations/{id}/messages/{messageId}/classify
  - POST /conversations/{id}/messages/{messageId}/classify/feedback
  - GET /stats/classification-accuracy
  - POST /upload
  - GET /settings/hours
  - PUT /settings/hours
  - GET /stats
  - GET /templates
  - POST /templates
  - PUT /templates/{id}
  - DELETE /templates/{id}

### 8. Messaging -- PortalEndpoints (10 endpoints)

All portal endpoints have WithSummary/WithDescription and magic-link auth via `MagicLinkEndpointFilter`.

**Issues:**
- **MEDIUM -- `GET /categories` is inline** (no MediatR handler): The handler is a lambda that returns `Enum.GetValues<MessageCategory>()`. This is acceptable for a simple enum lookup but deviates from the CQRS pattern used everywhere else.
- **MEDIUM -- Rate limiting missing on test-token endpoint**: `POST /api/v1/portal/test-token` is registered outside the rate-limited group. It is test-only (Dev/Test env), so low risk.

**Status: All 10 endpoints have TI coverage via `OwnerPortalEndpointsTests.cs`. PASS.**

### 9. Messaging -- WhatsAppEndpoints (3 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | GET | /messaging/whatsapp/config | OK | Admin | OK | GetWhatsAppConfigHandler | N/A (query) | NO |
| 2 | PUT | /messaging/whatsapp/config | OK | Admin | OK | UpdateWhatsAppConfigHandler | UpdateWhatsAppConfigValidator | NO |
| 3 | POST | /messaging/whatsapp/test | OK | Admin | OK | SendWhatsAppTestHandler | SendWhatsAppTestValidator | NO |

**Issues:**
- **CRITICAL -- Zero integration tests** for WhatsApp endpoints. No test file exists.

### 10. Messaging -- SseEndpoints (1 endpoint)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | GET | /messaging/sse | OK | RequireAuth | DisableRateLimiting (intentional -- SSE) | Inline | N/A | NO |

**Issues:**
- **LOW -- No integration test**: SSE endpoints are inherently harder to test with standard HTTP test infrastructure. The `DisableRateLimiting()` is intentional for long-lived connections. SSE handler is inline (no MediatR) which is acceptable for streaming.

### 11. Notifications -- ReminderEndpoints (3 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | GET | /notifications/reminders/config | OK | VetOrAdmin | OK | GetReminderConfigHandler | N/A (query) | YES |
| 2 | PUT | /notifications/reminders/config | OK | VetOrAdmin | OK | UpdateReminderConfigHandler | UpdateReminderConfigValidator | YES |
| 3 | GET | /notifications/reminders/logs | OK | VetOrAdmin | OK | ListReminderLogsHandler | N/A (query) | NO |

**Issues:**
- **MEDIUM -- Missing TI for `GET /reminders/logs`**: The logs listing endpoint has no integration test.

### 12. Stock -- StockEndpoints (5 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | GET | /stock | OK | RequireAuth | OK | ListStockItemsHandler | N/A | YES |
| 2 | POST | /stock | OK | VetOrAdmin | OK | CreateStockItemHandler | CreateStockItemValidator | YES |
| 3 | PATCH | /stock/{id} | OK | VetOrAdmin | OK | UpdateStockItemHandler | UpdateStockItemValidator | YES |
| 4 | POST | /stock/{id}/movements | OK | VetOrAdmin | OK | RecordStockMovementHandler | RecordStockMovementValidator | YES |
| 5 | GET | /stock/alerts | OK | RequireAuth | OK | GetStockAlertsHandler | N/A | YES |

**Status: PASS -- all endpoints fully covered.**

### 13. AI -- AIEndpoints (7 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | POST | /ai/triage | OK | ClinicStaff | OK | TriageSymptomsHandler | TriageSymptomsValidator | YES |
| 2 | PUT | /ai/triage/{id}/accept | OK | VetOrAdmin | OK | AcceptTriageHandler | AcceptTriageValidator | YES |
| 3 | PUT | /ai/triage/{id}/override | OK | VetOrAdmin | OK | OverrideTriageHandler | OverrideTriageValidator | YES |
| 4 | GET | /ai/no-show-prediction/{id} | OK | ClinicStaff | OK | PredictNoShowHandler | PredictNoShowValidator | YES |
| 5 | POST | /ai/no-show-predictions/batch | OK | ClinicStaff | OK | PredictNoShowBatchHandler | PredictNoShowBatchValidator | YES |
| 6 | POST | /ai/soap-notes | OK | VetOrAdmin | OK | GenerateSoapNotesHandler | GenerateSoapNotesValidator | YES |
| 7 | POST | /ai/check-interactions | OK | VetOrAdmin | OK | CheckInteractionsHandler | N/A (query) | YES |

**Status: PASS -- all endpoints fully covered.**

### 14. AI -- HealthAlertEndpoints (6 endpoints)

| # | Method | Route | Summary/Desc | Auth | Rate Limit | Handler | Validator | TI |
|---|---|---|---|---|---|---|---|---|
| 1 | GET | /ai/health-alerts | OK | ClinicStaff | OK | GetHealthAlertsHandler | N/A | YES |
| 2 | GET | /ai/health-alerts/patient/{id} | OK | ClinicStaff | OK | GetPatientHealthAlertsHandler | N/A | YES |
| 3 | POST | /ai/health-alerts/generate | OK | VetOrAdmin | OK | GenerateHealthAlertsHandler | **MISSING** | YES |
| 4 | PATCH | /ai/health-alerts/{id}/dismiss | OK | VetOrAdmin | OK | DismissHealthAlertHandler | DismissHealthAlertValidator | YES |
| 5 | PATCH | /ai/health-alerts/{id}/acknowledge | OK | ClinicStaff | OK | AcknowledgeHealthAlertHandler | AcknowledgeHealthAlertValidator | YES |
| 6 | POST | /ai/health-alerts/{id}/convert-to-appointment | OK | VetOrAdmin | OK | ConvertAlertToAppointmentHandler | ConvertAlertToAppointmentValidator | YES |

**Issues:**
- **LOW -- Missing validator for `GenerateHealthAlertsCommand`**: This is a parameterless command (trigger), so a validator is arguably unnecessary but diverges from the pattern.

---

## Critical Issues (Action Required)

### P0 -- Missing Integration Tests (violates rule 3g)

1. **Billing EReporting**: 0/2 endpoints tested. No test file exists.
2. **Messaging WhatsApp**: 0/3 endpoints tested. No test file exists.
3. **Messaging Staff endpoints**: 15/22 endpoints have no TI.
4. **Billing Invoices**: 5/9 endpoints have no TI (ListInvoices, GetInvoiceById, SubmitToEInvoicing, GetEInvoicingStatus, ExportCsv).

### P1 -- Missing Rate Limiting

5. **`GET /api/v1/patients/{id}/litters`** (LitterEndpoints): Registered outside the rate-limited group. Missing `.RequireRateLimiting("api")`.

### P2 -- Minor Issues

6. **Notifications `GET /reminders/logs`**: Missing TI.
7. **Messaging SSE**: No TI (acceptable for SSE but should be documented as exception).
8. **AI `GenerateHealthAlertsCommand`**: No FluentValidation validator (parameterless command, low risk).

---

## Endpoints with Full Coverage (green)

- **Breeding HeatCycles**: 3/3 endpoints -- all pass
- **Breeding Pregnancies**: 8/8 endpoints -- all pass
- **Breeding Lineage**: 4/4 endpoints -- all pass
- **Breeding Litters**: 4/4 endpoints tested (1 rate-limit gap)
- **Messaging Portal**: 10/10 endpoints -- all pass
- **Stock**: 5/5 endpoints -- all pass
- **AI Core**: 7/7 endpoints -- all pass
- **AI Health Alerts**: 6/6 endpoints -- all pass

---

## Recommendations

1. **Create `EReportingEndpointsTests.cs`** with at least 1 test per endpoint (submit + list periods).
2. **Create `WhatsAppEndpointsTests.cs`** with at least 1 test per endpoint (get config, update config, send test).
3. **Expand `MessagingEndpointsTests.cs`** to cover the 15 missing staff endpoints (transfer, recategorize, spam, convert-to-appointment, add-to-record, summary, classify, feedback, accuracy, upload, settings/hours, stats, templates CRUD).
4. **Expand `InvoiceEndpointsTests.cs`** to cover ListInvoices, GetInvoiceById, SubmitToEInvoicing, GetEInvoicingStatus, ExportCsv.
5. **Add `ReminderLogsEndpointsTests`** or extend existing file for GET /logs.
6. **Fix rate limiting on `GET /patients/{id}/litters`**: Add `.RequireRateLimiting("api")` to the `app.MapGet(...)` call in `LitterEndpoints.cs`.

---

## Methodology

- Scanned all `*Endpoints.cs` files in `src/backend/Modules/{Module}/Vetolib.{Module}/Api/`
- Cross-referenced each `Map*` call against: `.WithSummary()`, `.WithDescription()`, `RequireAuthorization`, `RequireRateLimiting`, handler existence, validator existence
- Checked `tests/Vetolib.Tests.Integration/` for corresponding test classes with at least 1 test per endpoint route
