# Task: Unit tests for Auth module uncovered handlers/validators

**Module**: Auth
**Type**: test
**Priority**: high (SonarCloud quality gate)

## Context
SonarCloud quality gate fails at 59.9% coverage on new code (needs 80%).
These Auth files changed in PR #14 but have no unit tests.

## Files to cover
1. `Modules/Auth/Vetolib.Auth/Application/Commands/DismissChecklist/DismissChecklistValidator.cs` — simple validator, test empty/valid Guid
2. `Modules/Auth/Vetolib.Auth/Application/Commands/DismissWelcomeBanner/DismissWelcomeBannerValidator.cs` — simple validator, test empty/valid Guid
3. `Modules/Auth/Vetolib.Auth/Api/OnboardingEndpoints.cs` — covered by TI/TF indirectly
4. `Modules/Auth/Vetolib.Auth/Api/UserEndpoints.cs` — covered by TI/TF indirectly

## Acceptance criteria
- [ ] Unit tests for DismissChecklistValidator (empty guid → error, valid guid → pass)
- [ ] Unit tests for DismissWelcomeBannerValidator (same pattern)
- [ ] All tests GREEN locally before PR

## Skills
`ardalis-result`, `cqrs-mediatr`
