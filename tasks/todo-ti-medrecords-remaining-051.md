# Task: Add integration tests for untested MedicalRecords endpoints

**Module:** MedicalRecords
**Priority:** HIGH (rule 3g violation)
**Source:** docs/audits/qa-night-b3-medrecords-endpoints-20260401.md

## Problem
~20 MedicalRecords endpoints have 0 integration tests:
- DrugCatalogEndpoints: 5 endpoints (search, alternatives, details, interactions, categories)
- OwnerPortalEndpoints: 5 endpoints (animals, records, vaccinations, prescriptions, weight)
- SharedRecordEndpoints: GET /api/v1/shared/{token} (anonymous, security-sensitive)
- WeightEndpoints: 3 endpoints (list, add, delete)
- MedicalRecordTemplateEndpoints: CRUD

## Fix
Create/extend test files in tests/Vetolib.Tests.Integration/MedicalRecords/

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN (TU)
- [ ] At least 1 TI per untested endpoint
