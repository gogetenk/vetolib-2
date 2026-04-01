# Task: Add missing integration tests for Billing endpoints

**Module:** Billing
**Priority:** HIGH (rule 3g violation)
**Source:** docs/audits/qa-night-b456-other-endpoints-20260401.md

## Problem
5 Billing endpoints have 0 integration tests:
- GET /api/v1/invoices (ListInvoices)
- GET /api/v1/invoices/{id} (GetInvoiceById)
- POST /api/v1/billing/ereporting/submit (SubmitToEInvoicing)
- GET /api/v1/billing/ereporting/status (GetEInvoicingStatus)
- GET /api/v1/invoices/export/csv (ExportCsv)

EReportingEndpoints has no test file at all.

## Fix
1. Add integration tests in tests/Vetolib.Tests.Integration/Billing/
2. Each endpoint needs at least: authenticated 200/success, unauthenticated 401, basic validation 400/422
3. Follow the existing pattern from AppointmentEndpointsTests.cs

## Skills
- `ardalis-result`, `reqnroll-bindings`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] At least 1 TI per Billing endpoint (5 endpoints minimum)
- [ ] EReporting test file created
