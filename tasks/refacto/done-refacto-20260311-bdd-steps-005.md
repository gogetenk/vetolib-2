# todo-refacto-20260311-bdd-steps-005 -- Remove duplicate step bindings that shadow SharedSteps

**Priorite** : critique
**Fichiers concernes** :

### Category 1: `a clinic "(.*)"` duplicated in scoped files (SharedSteps line 39 is the canonical)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Billing/FacturationSteps.cs` (line 51)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Dashboard/DashboardSteps.cs` (line 59)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Auth/ChangePasswordSteps.cs` (line 47)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Audit/AuditSteps.cs` (line 50)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Agenda/AppointmentSteps.cs` (line 688)

### Category 2: `I am authenticated as <ROLE>` literal duplicates (SharedSteps line 58 regex `(.*)` is the canonical)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Agenda/AppointmentSteps.cs` (line 696: `I am authenticated as ADMIN`)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Billing/FacturationSteps.cs` (line 65: `I am authenticated as VET`)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Dashboard/DashboardSteps.cs` (line 67: `I am authenticated as ADMIN`, line 73: `I am authenticated as RECEPTIONIST`)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Audit/AuditSteps.cs` (line 58: `I am authenticated as ADMIN`, line 64: `I am authenticated as VET`)

### Category 3: `I am logged in as a VET/RECEPTIONIST/ASSISTANT/ADMIN` duplicated (SharedSteps lines 64-86 are the canonical)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/DrugInteractionSteps.cs` (lines 71, 78, 85, 92)

### Category 4: `the system rejects with code "(.*)"` duplicated (SharedSteps line 90 is the canonical)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Billing/FacturationSteps.cs` (line 356)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Auth/LoginSteps.cs` (line 456)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Stock/StockSteps.cs` (line 185)

### Category 5: `the error message is "(.*)"` duplicated (SharedSteps line 113 is the canonical)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Billing/FacturationSteps.cs` (line 364)
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/Auth/LoginSteps.cs` (line 485)

**Violation** : Step binding duplication across SharedSteps (unscoped/global) and feature-specific steps (scoped). Although the scoped files have `[Scope(Feature = "...")]` which prevents compile-time ambiguity, the duplicated steps have divergent implementations:

1. **SharedSteps** stores error data in `ScenarioContext` keys (`"LastResponse"`, `"ErrorResponseBody"`).
2. **Feature-specific steps** store error data in private fields (`_errorResponseBody`, `_lastResponse`).

If a feature title changes or a `[Scope]` attribute is misspelled, the scoped bindings silently stop matching and SharedSteps kicks in -- but SharedSteps reads from ScenarioContext where no data was stored by the feature's When steps (which use private fields). This causes silent test failures or false passes.

Additionally, LoginSteps line 456 has a custom switch/case implementation of `the system rejects with code` that handles `INSUFFICIENT_PERMISSIONS`, `ACCOUNT_LOCKED`, etc. differently from SharedSteps. If the scope ever falls through, behavior changes silently.

**Correction attendue** :

**Phase 1 -- Remove duplicates from scoped files that have identical behavior to SharedSteps:**
For each file listed above, remove the duplicate step method entirely. The feature scenarios will then fall through to the unscoped SharedSteps binding.

**Phase 2 -- Align error storage pattern:**
Feature-specific When steps that store errors in private fields (`_errorResponseBody`) must ALSO store them in ScenarioContext under the keys SharedSteps expects:
- `_ctx.Set(lastResponse, "LastResponse");`
- `_ctx.Set(errorBody, "ErrorResponseBody");`

This ensures SharedSteps' Then assertions (`the system rejects with code`, `the error message is`) can read the data regardless of which When step produced it.

**Phase 3 -- Handle LoginSteps special case:**
LoginSteps has a richer `the system rejects with code` implementation (line 456) with switch/case for `INVALID_CREDENTIALS`, `ACCOUNT_LOCKED`, `FORBIDDEN`, etc. Merge this logic into SharedSteps or keep it scoped. If kept scoped, ensure the scope attribute stays accurate.

**Phase 4 -- Handle clinic context divergence:**
SharedSteps.GivenAClinic stores clinic IDs in `ScenarioContext["ClinicIds"]` (a dictionary). Feature-specific versions (FacturationSteps, DashboardSteps, AuditSteps) store in a private `_clinicId` field. After removing the duplicates, the feature's other steps that reference `_clinicId` must be updated to read from ScenarioContext instead.

**Critere** :
- [ ] `grep -rn 'Given.*a clinic.*\.\*' tests/Vetolib.Tests.Acceptance/StepDefinitions/ --include="*.cs" | grep -v SharedSteps | grep -v "with identifier\|already registered\|with hours\|exists"` returns 0 results
- [ ] `grep -rn 'I am authenticated as ADMIN\|I am authenticated as VET\|I am authenticated as RECEPTIONIST' tests/Vetolib.Tests.Acceptance/StepDefinitions/ --include="*.cs" | grep -v SharedSteps` returns 0 results
- [ ] `grep -rn 'I am logged in as a VET\|I am logged in as a RECEPTIONIST\|I am logged in as an ASSISTANT\|I am logged in as an ADMIN' tests/Vetolib.Tests.Acceptance/StepDefinitions/ --include="*.cs" | grep -v SharedSteps` returns 0 results
- [ ] `grep -rn 'the system rejects with code' tests/Vetolib.Tests.Acceptance/StepDefinitions/ --include="*.cs" | grep -v SharedSteps` returns 0 results (or only LoginSteps if kept scoped intentionally)
- [ ] `grep -rn 'the error message is' tests/Vetolib.Tests.Acceptance/StepDefinitions/ --include="*.cs" | grep -v SharedSteps` returns 0 results
- [ ] `dotnet build tests/Vetolib.Tests.Acceptance -c Release` succeeds with 0 errors
