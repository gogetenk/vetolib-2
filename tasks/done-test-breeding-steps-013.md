# todo-test-breeding-steps-013.md -- Add Reqnroll step definitions for Breeding features

**Module** : Breeding (Tests)
**Priority** : Haute
**Dependencies** : done-back-breeding-litter-008, done-back-breeding-pregnancy-010, done-back-breeding-heatcycle-011
**Skills** : `reqnroll-bindings`

## Context

4 Breeding .feature files exist but have zero step definitions. All 53 scenarios are untested.

## Scope

Create step definitions for:
1. `Features/Breeding/Litter.feature` (9 scenarios)
2. `Features/Breeding/Pregnancy.feature` (11 scenarios)
3. `Features/Breeding/HeatCycle.feature` (8 scenarios)
4. `Features/Breeding/Lineage.feature` (6 scenarios) — if task 009 is merged

Follow the pattern of existing step definitions (SharedSteps, API calls via HttpClient, Testcontainers).

## Completion criteria
- [ ] Step definitions compile
- [ ] `dotnet build` GREEN
- [ ] At least the Given/When/Then bindings exist (even if some fail due to API not wired yet)
