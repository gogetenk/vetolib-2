# Task: Add missing integration tests for Auth endpoints

**Module:** Auth
**Priority:** HIGH (rule 3g violation)
**Source:** docs/audits/qa-night-b1-auth-endpoints-20260401.md

## Problem
19 Auth endpoints have 0 integration tests:
- ClinicGroup: 8 endpoints (CRUD + dashboard stats)
- Portal: 4 endpoints (register, login, invite-vet, link-microchip)
- Webhook: 4 endpoints (CRUD + receive)
- Onboarding: 4 endpoints (state, complete step, dismiss banner/checklist)
- Auth: logout, change-password, switch-clinic

## Fix
1. Add integration tests in tests/Vetolib.Tests.Integration/Auth/
2. Create ClinicGroupEndpointsTests.cs, PortalEndpointsTests.cs, WebhookEndpointsTests.cs, OnboardingEndpointsTests.cs
3. Each endpoint needs: authenticated success, unauthenticated 401, Admin-only 403

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] At least 1 TI per untested Auth endpoint (19 minimum)
