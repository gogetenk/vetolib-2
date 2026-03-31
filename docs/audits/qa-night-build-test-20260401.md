# QA Night Build and Unit Test Audit 2026-04-01

**Triggered by**: Manual QA run
**Branch**: develop
**Timestamp**: 2026-04-01
**Status**: [QA_FAIL]

---

## 1. Build Results

**Outcome**: SUCCESS — 0 errors, 9 warnings

### Warnings inventory

#### Business code warning (BLOCKING per CLAUDE.md rule)

| Severity | File | Line | Code | Message |
|---|---|---|---|---|
| CS8604 | src/backend/Modules/Agenda/Vetolib.Agenda/Application/Commands/UpdateConsultationType/UpdateConsultationTypeHandler.cs | 35 | CS8604 | Possible null reference argument for parameter name in Result ConsultationType.Update(string name, ...) |

This warning is in production business code (Agenda module handler). Per CLAUDE.md rule 3b, dotnet build must produce 0 warnings on business code. This is **blocking**.

#### Test infrastructure warnings (non-blocking, but tracked)

| Severity | File | Code | Message |
|---|---|---|---|
| CS0108 | tests/Vetolib.Tests.Integration/Auth/AuthEndpointsTests.cs:18 | CS0108 | JsonOptions hides inherited IntegrationTestBase.JsonOptions — missing new keyword |
| CS0108 | tests/Vetolib.Tests.Integration/Agenda/AppointmentEndpointsTests.cs:16 | CS0108 | Same pattern |
| CS0108 | tests/Vetolib.Tests.Integration/Billing/InvoiceEndpointsTests.cs:15 | CS0108 | Same pattern |
| CS0108 | tests/Vetolib.Tests.Integration/Events/IntegrationEventTests.cs:18 | CS0108 | Same pattern |
| CS0108 | tests/Vetolib.Tests.Integration/MedicalRecords/PatientEndpointsTests.cs:16 | CS0108 | Same pattern |
| CS0108 | tests/Vetolib.Tests.Integration/MultiTenancy/TenantIsolationTests.cs:24 | CS0108 | Same pattern |
| CS0618 | tests/Vetolib.Tests.Integration/Infrastructure/VetolibWebApplicationFactory.cs:44 | CS0618 | PostgreSqlBuilder() parameterless constructor is obsolete — use constructor with image parameter |
| EF1003 | tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/DrugInteractionSteps.cs:133 | EF1003 | ExecuteSqlRawAsync — SQL injection risk, use ExecuteSqlAsync instead |
---

## 2. Unit Test Results

**Outcome**: FAIL
**Total**: 1804
**Passed**: 1781
**Failed**: 23
**Skipped**: 0
**Duration**: ~7s

---

## 3. Failing Tests Full List

All 23 failures are in the Auth module, across 6 handler/class groups.

### 3a. GetOnboardingStateHandler — 10 failures

All fail with: Expected result.IsSuccess to be True, but found False. The handler returns failure across all scenarios.

| Test | File | Line | Error |
|---|---|---|---|
| Handle_NewAdminUser_CreatesOnboardingStateAndReturnsInitialSteps | Auth/GetOnboardingStateHandlerTests.cs | 90 | result.IsSuccess expected True, got False |
| Handle_ExistingState_ReturnsCurrentProgress | Auth/GetOnboardingStateHandlerTests.cs | 130 | result.IsSuccess expected True, got False |
| Handle_PatientCountGreaterThanZero_AutoCompletesAddFirstPatientStep | Auth/GetOnboardingStateHandlerTests.cs | 155 | result.IsSuccess expected True, got False |
| Handle_AppointmentCountGreaterThanZero_AutoCompletesBookFirstAppointmentStep | Auth/GetOnboardingStateHandlerTests.cs | 179 | result.IsSuccess expected True, got False |
| Handle_InvoiceCountGreaterThanZero_AutoCompletesCreateFirstInvoiceStep | Auth/GetOnboardingStateHandlerTests.cs | 202 | result.IsSuccess expected True, got False |
| Handle_AllAutoCompletableStepsDone_DoesNotMarkFullyCompleted_WhenManualStepsRemain | Auth/GetOnboardingStateHandlerTests.cs | 226 | result.IsSuccess expected True, got False |
| Handle_AllStepsComplete_MarksIsCompletedTrue | Auth/GetOnboardingStateHandlerTests.cs | 257 | result.IsSuccess expected True, got False |
| Handle_VetUser_ReturnsVetSpecificSteps | Auth/GetOnboardingStateHandlerTests.cs | 304 | result.IsSuccess expected True, got False |
| Handle_ExistingStateDismissed_ReturnsCorrectVisibilityFlags | Auth/GetOnboardingStateHandlerTests.cs | 338 | result.IsSuccess expected True, got False |
| Handle_AutoCompletionAlreadyDone_DoesNotDuplicateStep | Auth/GetOnboardingStateHandlerTests.cs | 367 | result.IsSuccess expected True, got False |
### 3b. DismissChecklistHandler — 2 failures

| Test | File | Line | Error |
|---|---|---|---|
| Handle_WhenOnboardingStateExists_DismissesChecklistAndReturnsSuccess | Auth/DismissChecklistHandlerTests.cs | 57 | result.IsSuccess expected True, got False |
| Handle_WhenChecklistAlreadyDismissed_StillReturnsSuccess | Auth/DismissChecklistHandlerTests.cs | 98 | result.IsSuccess expected True, got False |

### 3c. DismissWelcomeBannerHandler — 2 failures

| Test | File | Line | Error |
|---|---|---|---|
| Handle_WhenOnboardingStateExists_DismissesBannerAndReturnsSuccess | Auth/DismissWelcomeBannerHandlerTests.cs | 57 | result.IsSuccess expected True, got False |
| Handle_WhenBannerAlreadyDismissed_StillReturnsSuccess | Auth/DismissWelcomeBannerHandlerTests.cs | 98 | result.IsSuccess expected True, got False |

### 3d. ChangeUserRoleHandler — 3 failures

| Test | File | Line | Error |
|---|---|---|---|
| Handle_PromoteReceptionistToVet_ReturnsSuccess | Auth/ChangeUserRoleHandlerTests.cs | 65 | result.IsSuccess expected True, got False |
| Handle_AdminTriesToChangeOwnRole_ReturnsError | Auth/ChangeUserRoleHandlerTests.cs | 109 | result.Status expected ResultStatus.Error, got ResultStatus.NotFound |
| Handle_DemoteVetToAssistant_ReturnsSuccess | Auth/ChangeUserRoleHandlerTests.cs | 130 | result.IsSuccess expected True, got False |

### 3e. DeactivateUserHandler — 2 failures

| Test | File | Line | Error |
|---|---|---|---|
| Handle_DeactivateActiveUser_SetsIsActiveFalse | Auth/DeactivateUserHandlerTests.cs | 65 | result.IsSuccess expected True, got False |
| Handle_DeactivateAlreadyInactiveUser_StillSucceeds | Auth/DeactivateUserHandlerTests.cs | 131 | result.IsSuccess expected True, got False |

### 3f. VerifyEmailHandler — 1 failure

| Test | File | Line | Error |
|---|---|---|---|
| Handle_ValidToken_SetsEmailVerified | Auth/VerifyEmailHandlerTests.cs | 58 | System.InvalidOperationException: Sequence contains no elements — .Single() on empty in-memory DB |

### 3g. SubscriptionChecker — 3 failures

| Test | File | Line | Error |
|---|---|---|---|
| CheckLimit_FreePlan_AtVetLimit_ReturnsError | Auth/SubscriptionCheckerTests.cs | 102 | result.IsSuccess expected False, got True — limit enforcement broken |
| CheckLimit_StarterPlan_AtVetLimit_ReturnsError | Auth/SubscriptionCheckerTests.cs | 129 | result.IsSuccess expected False, got True — limit enforcement broken |
| GetCurrentUsage_ReturnsCorrectPlanAndLimits | Auth/SubscriptionCheckerTests.cs | 263 | usage.CurrentVets expected 2, got 0 — vet count query returns 0 |
---

## 4. Analysis

### Root cause hypothesis

The 20 failures in GetOnboardingState, DismissChecklist, DismissWelcomeBanner, ChangeUserRole, and DeactivateUser all return IsSuccess = False where True is expected. This consistent pattern across unrelated handlers in the Auth module strongly suggests a **shared dependency or mock** in the test setup is broken, not each handler individually.

Likely candidates:

- A recent change to UserManager<AppUser> mock setup causing user lookups to return NotFound before handler logic runs
- A new constructor parameter added to one of these handlers that the test mocks do not supply
- A change to IClinicContext or ICurrentUserService that tests do not set up correctly

The VerifyEmailHandler failure (Sequence contains no elements) is a separate root cause: the in-memory DB test setup does not seed the expected entity before calling .Single().

The SubscriptionChecker failures (CurrentVets = 0, limit not enforced) indicate the vet-count query uses a filter (role, IsActive, ClinicId) that the in-memory DB setup does not satisfy — likely missing role assignment or wrong ClinicId on seeded users.

### Scope

All 23 failures are isolated to tests/Vetolib.Tests.Unit/Auth/. No other module is affected.

---

## 5. Blockers

| # | Issue | Severity | Location |
|---|---|---|---|
| 1 | 23 unit tests failing in Auth module | **BLOCKING** | tests/Vetolib.Tests.Unit/Auth/ |
| 2 | CS8604 nullable warning in production Agenda handler | **BLOCKING** (CLAUDE.md rule 3b) | src/backend/Modules/Agenda/Vetolib.Agenda/Application/Commands/UpdateConsultationType/UpdateConsultationTypeHandler.cs:35 |

## 6. Non-Blocking Suggestions

| # | Issue | Location |
|---|---|---|
| 1 | 6x CS0108 JsonOptions hiding — add new keyword | tests/Vetolib.Tests.Integration/ (6 files) |
| 2 | CS0618 obsolete PostgreSqlBuilder() — migrate to constructor with image parameter | tests/Vetolib.Tests.Integration/Infrastructure/VetolibWebApplicationFactory.cs:44 |
| 3 | EF1003 SQL injection warning — replace ExecuteSqlRawAsync with ExecuteSqlAsync | tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/DrugInteractionSteps.cs:133 |

---

**Conclusion**: develop branch is currently RED on unit tests. The Auth module has a regression affecting 23 tests across 6 test classes. A dev agent must investigate and fix the Auth module test setup before any PR targeting develop can be merged.