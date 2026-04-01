# Task: Add integration tests for untested Agenda endpoints

**Module:** Agenda
**Priority:** HIGH (rule 3g violation)
**Source:** docs/audits/qa-night-b2-agenda-endpoints-20260401.md

## Problem
13 Agenda endpoints have 0 integration tests:
- ConsultationTypeEndpoints: 4 endpoints (CRUD)
- FollowUpRuleEndpoints: 4 endpoints (CRUD)
- StaffScheduleEndpoints: 5 endpoints (CRUD + list)

## Fix
Create test files:
- tests/Vetolib.Tests.Integration/Agenda/ConsultationTypeEndpointsTests.cs
- tests/Vetolib.Tests.Integration/Agenda/FollowUpRuleEndpointsTests.cs
- tests/Vetolib.Tests.Integration/Agenda/StaffScheduleEndpointsTests.cs

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN (TU)
- [ ] At least 1 TI per endpoint (13 minimum)
