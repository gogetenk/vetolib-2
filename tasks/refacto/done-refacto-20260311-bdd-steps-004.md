# todo-refacto-20260311-bdd-steps-004 -- StockPrescriptionSteps: replace all PendingStepException with minimal viable implementations
**Priorite** : importante
**Fichiers concernes** :
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/StockPrescriptionSteps.cs` (ALL methods)

**Violation** : Every step definition in `StockPrescriptionSteps.cs` throws `PendingStepException()`. This means ALL 8 scenarios in `StockPrescriptionIntegration.feature` fail at runtime with "step pending" errors, contributing to the 88 failing tests.

**Correction attendue** :
1. Add `[BeforeScenario]` setup to initialize `_factory`, `_client`, `_clinicId` (same pattern as `StockManagementSteps`).
2. Implement each step with minimal viable logic:
   - **Given steps**: Seed data using the Stock and MedicalRecords APIs/DbContexts
   - **When steps**: Call the appropriate HTTP endpoints
   - **Then steps**: Assert on the HTTP response and/or query the database

This is a substantial implementation task. The step definitions should follow the same patterns established in:
- `StockManagementSteps.cs` (for stock API calls)
- `DrugInteractionSteps.cs` (for prescription/drug catalog operations)

Key dependencies:
- `StockDbContext` for stock items
- `MedicalRecordsDbContext` for drug catalog and prescriptions
- Stock-Prescription integration endpoint (e.g., `/api/v1/stock/check-availability`)

If the API endpoints for stock-prescription integration do not exist yet, the steps should use direct DB operations as placeholders and document which endpoints are needed.

**Critere** : `grep -c "PendingStepException" tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/StockPrescriptionSteps.cs` returns 0.
