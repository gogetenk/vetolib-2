# Task: Unit tests for Stock uncovered handlers

**Module**: Stock
**Type**: test
**Priority**: high (SonarCloud quality gate)

## Context
SonarCloud quality gate fails at 59.9% coverage on new code (needs 80%).
These Stock handlers changed in PR #14 but have no unit tests.

## Files to cover
1. `Application/Queries/GetStockAlerts/GetStockAlertsHandler.cs` — test returns alerts when below threshold
2. `Application/Queries/ListStockItems/ListStockItemsHandler.cs` — test returns stock items list

## Acceptance criteria
- [ ] Unit tests for GetStockAlertsHandler (items below threshold → returned, all OK → empty)
- [ ] Unit tests for ListStockItemsHandler (returns items, empty DB → empty list)
- [ ] All tests GREEN locally before PR

## Skills
`ardalis-result`, `cqrs-mediatr`
