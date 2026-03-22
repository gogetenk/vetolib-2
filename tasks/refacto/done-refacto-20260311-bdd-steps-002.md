# todo-refacto-20260311-bdd-steps-002 -- Fix StockPrescriptionSteps "skip stock decrement" step text mismatch
**Priorite** : critique
**Fichiers concernes** :
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/StockPrescriptionSteps.cs` (line 166)
- `tests/Vetolib.Tests.Acceptance/Features/Prescriptions/StockPrescriptionIntegration.feature` (line 59)

**Violation** : Step text mismatch between .feature and step definition.

The feature file says:
```gherkin
And I should be able to skip stock decrement entirely
```

But the step definition binding says:
```csharp
[Then(@"Or skip stock decrement entirely")]
```

The generated `.feature.cs` (line 459) calls `AndAsync("I should be able to skip stock decrement entirely", ...)` which will NOT match the regex `"Or skip stock decrement entirely"`.

**Correction attendue** :
Change the step binding attribute in `StockPrescriptionSteps.cs` line 166 from:
```csharp
[Then(@"Or skip stock decrement entirely")]
```
to:
```csharp
[Then(@"I should be able to skip stock decrement entirely")]
```

The method body can remain `throw new PendingStepException()` for now (the entire StockPrescriptionSteps file uses PendingStepException -- this is tracked separately).

**Critere** : `grep -n "Or skip stock decrement" tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/StockPrescriptionSteps.cs` returns no results.
