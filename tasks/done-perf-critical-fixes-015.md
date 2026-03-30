# todo-perf-critical-fixes-015.md -- Fix HIGH priority performance issues

**Module** : Multiple (MedicalRecords, Billing, Stock)
**Priority** : Haute
**Dependencies** : aucune

## Context

Performance audit found 4 HIGH severity issues. See `docs/audits/performance-audit-20260329.md`.

## Scope

### P-04: Add missing index on medical_records.PatientId
- Add index on `PatientId` column in PatientConfiguration or via migration
- Impact: every patient detail view currently does a full table scan

### P-09: Add pagination to ListInvoicesHandler
- Add `PageNumber` and `PageSize` to `ListInvoicesQuery`
- Apply `.Skip().Take()` in handler
- Update endpoint to accept query params
- Default: page 1, size 20

### P-18: Fix Cartesian explosion in drug catalog search
- Use `.AsSplitQuery()` on the drug catalog search that has triple `.Include()`
- Or split into separate queries

### P-19: Add pagination to PatientReader.GetPatientContextAsync
- Limit medical records and prescriptions loaded (e.g., last 50)

## Completion criteria
- [ ] Index added + migration generated
- [ ] Invoices paginated
- [ ] Drug catalog split query
- [ ] PatientContext bounded
- [ ] `dotnet build` + `dotnet test` GREEN
