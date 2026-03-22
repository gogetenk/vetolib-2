# todo-refacto-20260311-bdd-steps-003 -- Fix MessageTriageSteps Given/When step keyword mismatch
**Priorite** : importante
**Fichiers concernes** :
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Messaging/MessageTriageSteps.cs` (lines 105-115)
- `tests/Vetolib.Tests.Acceptance/Features/Messaging/MessageTriage.feature` (lines 37, 43)

**Violation** : Step keyword mismatch. In the feature file:
```gherkin
# Line 36-37 (Low confidence scenario):
When an owner sends an ambiguous message "..."
And the AI confidence is below 0.7      <-- And after When = When step

# Line 42-43 (Bias toward emergency scenario):
When an owner sends a message "..."
And the AI is uncertain between "MedicalQuestion" and "MedicalUrgency"  <-- And after When = When step
```

The `And` keyword inherits the type of the previous step. Both steps follow a `When`, so they are `When` steps at runtime. But the bindings use `[Given]`:
```csharp
[Given(@"the AI confidence is below 0.7")]       // line 105
[Given(@"the AI is uncertain between ""(.*)"" and ""(.*)""")]  // line 111
```

In Reqnroll, `[Given]`/`[When]`/`[Then]` attributes are interchangeable for step matching (unlike older SpecFlow). However, to avoid confusion and follow best practices, these should use `[StepDefinition]` or add `[When]` aliases.

**Correction attendue** :
Replace `[Given]` with `[StepDefinition]` on both bindings:
```csharp
[StepDefinition(@"the AI confidence is below (.*)")]
public void GivenTheAiConfidenceIsBelow(double threshold)
{
    _ctx.Set(threshold, "AiConfidence");
}

[StepDefinition(@"the AI is uncertain between ""(.*)"" and ""(.*)""")]
public void GivenTheAiIsUncertain(string category1, string category2)
{
    _ctx.Set(true, "AiUncertain");
}
```

Note: Also parameterize the confidence threshold (currently hardcoded `0.7` in the regex but could change). The feature says `below 0.7` so a `(.*)` or `(\d+\.?\d*)` capture group is more robust.

**Critere** : The "Low confidence triggers uncertain triage" and "AI biases toward emergency for safety" scenarios in MessageTriage.feature pass step binding resolution.
