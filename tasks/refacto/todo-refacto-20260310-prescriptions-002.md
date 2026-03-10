# todo-refacto-20260310-prescriptions-002 -- Missing Gherkin: active prescription window boundary

**Priorite** : importante
**Fichiers concernes** :
- `tests/Vetolib.Tests.Acceptance/Features/Prescriptions/DrugInteractionChecking.feature`

**Violation** : Spec section 4.4 defines a configurable "active prescription window" (default 90 days). No Gherkin scenario tests this boundary.

**Details** :
The spec states: "A prescription is considered 'active' if it was created within the last 90 days. This window is configurable per clinic." The current drug-drug interaction scenario uses "from 5 days ago" but never tests that a prescription from 91 days ago is NOT considered active (and thus no interaction warning is raised).

**Correction attendue** : Add at least two scenarios:
1. Prescription from 89 days ago -> interaction warning shown
2. Prescription from 91 days ago -> no interaction warning (expired window)

**Critere** : [] Feature file contains scenario for expired prescription window
