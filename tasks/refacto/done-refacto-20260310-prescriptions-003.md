# todo-refacto-20260310-prescriptions-003 -- Missing Gherkin: clinic admin adds custom drug to catalog

**Priorite** : importante
**Fichiers concernes** :
- `tests/Vetolib.Tests.Acceptance/Features/Prescriptions/DrugInteractionChecking.feature`
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Api/DrugCatalogEndpoints.cs`

**Violation** : US-5 from PRESCRIPTIONS-AI-SPEC.md ("As a clinic admin, I want to add custom drugs to my clinic's catalog") has no Gherkin scenario and no endpoint implementation.

**Details** :
The spec explicitly lists US-5 as a user story. The current DrugCatalogEndpoints only exposes GET (search + get-by-id). There is no POST endpoint for creating clinic-scoped catalog entries, and no Gherkin scenario covers this workflow.

**Correction attendue** :
1. Add Gherkin scenario for: Admin creates custom drug (ClinicId = current clinic), verify it appears in search, verify it is NOT visible to other clinics
2. Add POST endpoint on /api/v1/medical-records/drugs (ADMIN role required)
3. Add corresponding handler with Result<T> pattern

**Critere** : [] Feature file contains "Scenario: Clinic admin adds custom drug" and POST endpoint exists
