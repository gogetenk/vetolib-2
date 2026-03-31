# Task: Fix N+1 in GetPedigreeHandler (P-01)

**Module:** Breeding
**Priority:** HIGH
**Source:** docs/audits/qa-night-performance-20260401.md — P-01

## Problem
GetPedigreeHandler recursively calls `_patientReader.GetPatientByIdAsync()` and `_context.PatientLineages.FirstOrDefaultAsync()` per generation node. For a 5-generation pedigree: 31 individual DB queries.

## Fix
Preload all lineages + patient data for the subtree in a single batch query (or CTE), then walk the in-memory tree.

## Skills
- `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] GetPedigreeHandler uses at most 2-3 queries (batch load lineages + batch load patients)
- [ ] Existing TU/TI still pass
- [ ] No N+1 pattern (no DB call inside a loop/recursion)
