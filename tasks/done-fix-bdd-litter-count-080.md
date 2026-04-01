# Task: Fix BDD test failure — litter count assertion

**Module:** Breeding (Tests)
**Priority:** HIGH (CI RED)
**Source:** CI run 23844966493 — BDD tests

## Problem
BDD acceptance test fails: `Expected _litterList!.Count to be 2, but found 0 (difference of -2)`
This is in the Breeding module's litter listing scenario.

## Fix
1. Read the failing BDD step definition
2. Check if LitterEndpoints.cs changed (endpoint moved from group to app level in a prior PR)
3. Verify the test is calling the correct endpoint path
4. Fix the step definition or seed data

## Definition of Done
- [ ] BDD litter test passes
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` TU GREEN
