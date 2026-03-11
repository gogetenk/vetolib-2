# Task: Unit tests for Preferences uncovered domain/services

**Module**: Preferences
**Type**: test
**Priority**: medium (SonarCloud quality gate)

## Context
SonarCloud quality gate fails at 59.9% coverage on new code (needs 80%).
These Preferences files changed in PR #14 but have no unit tests.

## Files to cover
1. `Application/Domain/ClinicPreferenceDefault.cs` — test default values
2. `Application/Domain/SystemDefaults.cs` — test system defaults
3. `Application/Domain/UserPreference.cs` — test Create/Update methods
4. `Application/Services/PreferenceChecker.cs` — test preference resolution logic

## Acceptance criteria
- [ ] Unit tests for domain models (Create, defaults, validation)
- [ ] Unit tests for PreferenceChecker (user pref > clinic pref > system default)
- [ ] All tests GREEN locally before PR

## Skills
`ardalis-result`, `cqrs-mediatr`
