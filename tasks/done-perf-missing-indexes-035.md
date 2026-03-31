# Task: Add missing database indexes (P-11, P-12, P-13)

**Module:** MedicalRecords, Stock, Messaging
**Priority:** HIGH
**Source:** docs/audits/qa-night-performance-20260401.md — P-11, P-12, P-13

## Problem
- Prescription: no index on (ClinicId, MedicalRecordId) or (ClinicId, CreatedAt)
- StockItem: no index on (ClinicId, Quantity) or (ClinicId, ExpiryDate)
- Conversation: no index on (ClinicId, Status, Category)

## Fix
Add EF Core migrations with composite indexes for each table. Follow rule 3f (audit generated migration).

## Skills
- `ardalis-result`, `multitenant-efcore`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] Migration generated and audited (no phantom ops)
- [ ] Prescription: composite index on (ClinicId, MedicalRecordId) and (ClinicId, CreatedAt)
- [ ] StockItem: composite index on (ClinicId, Quantity) and (ClinicId, ExpiryDate)
- [ ] Conversation: composite index on (ClinicId, Status, Category)
- [ ] `dotnet ef migrations has-pending-model-changes` returns "No changes" for each context
