# todo-refacto-20260311-bdd-steps-001 -- Fix StockPrescriptionSteps missing "I am logged in as a VET" binding
**Priorite** : critique
**Fichiers concernes** :
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/StockPrescriptionSteps.cs`
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/DrugInteractionSteps.cs`

**Violation** : Step binding scope mismatch. The step `Given I am logged in as a VET` is used in both `DrugInteractionChecking.feature` and `StockPrescriptionIntegration.feature`, but the binding in `DrugInteractionSteps.cs` (line 71) has `[Scope(Feature = "Drug Interaction Checking")]`, making it invisible to the Stock-Prescription Integration feature.

**Correction attendue** :
Option A (recommended): Add login step bindings inside `StockPrescriptionSteps.cs`. Copy the pattern from `DrugInteractionSteps.LoginAs()` to add:
```csharp
[Given(@"I am logged in as a VET")]
public async Task GivenIAmLoggedInAsVet()
{
    // Same pattern as DrugInteractionSteps.LoginAs("VET")
    // Create user, login, set bearer token
}
```
This works because `StockPrescriptionSteps` already has `[Scope(Feature = "Stock-Prescription Integration")]`.

Option B (alternative): Extract login steps (`I am logged in as a VET/RECEPTIONIST/ASSISTANT/ADMIN`) from `DrugInteractionSteps.cs` into `SharedSteps.cs` without any `[Scope]` attribute, and remove them from `DrugInteractionSteps.cs`. This is cleaner long-term but risks breaking other features if step text patterns conflict.

Also needed: `StockPrescriptionSteps.cs` needs access to a `TestWebApplicationFactory`, `HttpClient`, `AuthDbContext`, and a stable `ClinicId`. Add `[BeforeScenario]` setup and a private `LoginAs(string role)` helper following the same pattern as `DrugInteractionSteps`.

**Critere** : The `Stock-Prescription Integration` feature's Background step `Given I am logged in as a VET` resolves to a valid binding at runtime.
