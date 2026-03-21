# Task: Unit tests for MedicalRecords uncovered handlers

**Module**: MedicalRecords
**Type**: test
**Priority**: high (SonarCloud quality gate)

## Context
SonarCloud quality gate fails at 59.9% coverage on new code (needs 80%).
These MedicalRecords handlers changed in PR #14 but have no unit tests.

## Files to cover
1. `Application/Queries/GetDrugCatalogEntryById/GetDrugCatalogEntryByIdHandler.cs` — query handler, test found/not-found
2. `Application/Queries/ListMedicalRecords/ListMedicalRecordsHandler.cs` — query handler, test list returns results
3. `Application/Queries/SearchDrugCatalog/SearchDrugCatalogHandler.cs` — query handler, test search with/without results

## Acceptance criteria
- [x] Unit tests for GetDrugCatalogEntryByIdHandler (found → Success, not found → NotFound)
- [x] Unit tests for ListMedicalRecordsHandler (returns paged results)
- [x] Unit tests for SearchDrugCatalogHandler (matching/no matching results)
- [x] All tests GREEN locally before PR (454/454 passing)

## Skills
`ardalis-result`, `cqrs-mediatr`
