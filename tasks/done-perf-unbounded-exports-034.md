# Task: Fix unbounded queries in FHIR export + conversation export (P-16, P-17, P-18)

**Module:** MedicalRecords, Messaging
**Priority:** HIGH
**Source:** docs/audits/qa-night-performance-20260401.md — P-16, P-17, P-18

## Problem
- ExportPatientFhirHandler loads ALL medical records + weight entries unbounded
- ExportOwnerConversationsHandler loads ALL conversations + messages unbounded
- GetPatientSummaryHandler loads ALL vaccination/allergy records unbounded
Risk of OOM on large datasets (10+ years of records).

## Fix
- Add `.Take(1000)` safety limits on export queries
- Add pagination parameters (page/pageSize) to list endpoints
- For exports: consider streaming or date range filters
- GetPatientSummary: add `.Take(100)` on vaccination/allergy subqueries

## Skills
- `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] No ToListAsync() without .Take() or pagination on these handlers
- [ ] ExportPatientFhir: uses reasonable limit or streaming
- [ ] ExportOwnerConversations: uses reasonable limit
- [ ] GetPatientSummary: vaccination/allergy queries bounded
