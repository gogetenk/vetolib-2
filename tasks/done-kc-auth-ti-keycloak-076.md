# Task: Add integration tests for Keycloak Auth endpoints

**Module:** Auth (Tests)
**Priority:** HIGH (Phase 2.10 — verify Keycloak wiring works)
**Depends on:** done-kc-poc-jwt-validation-062

## Scope
Add TI for the new Keycloak-related endpoints and verify existing endpoints still work with dual-stack:

1. GET /api/v1/auth/my-organizations — authenticated success, 401 unauthenticated
2. Verify LoginHandler returns JWT that ClinicContext can read (existing test, may need update)
3. Verify RegisterClinic creates Keycloak org (mock or skip if no Keycloak container)
4. Verify dual-stack: Legacy JWT still accepted, Keycloak JWT format accepted

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] At least 1 TI for my-organizations endpoint
- [ ] Existing Auth TI still pass
