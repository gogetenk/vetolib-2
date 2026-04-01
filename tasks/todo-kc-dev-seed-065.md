# Task: Keycloak dev seed service

**Module:** Auth
**Priority:** MEDIUM (Keycloak Phase 1)
**Source:** docs/studies/keycloak-aspire-migration-20260401.md — 1.5
**Depends on:** todo-kc-admin-api-client-064

## Scope
Create `KeycloakSeedService` (IHostedService) that runs on startup in Development:

1. Create 2 demo organizations (Dubai Pet Clinic, Abu Dhabi Animal Hospital)
2. Create demo users:
   - admin@vetolib.ae (Admin in both orgs)
   - vet@vetolib.ae (Vet in Dubai clinic)
   - reception@vetolib.ae (Receptionist in Dubai clinic)
3. Assign users to organizations with roles
4. Skip if organizations already exist (idempotent)
5. Uses IKeycloakAdminService from task 064

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] Demo orgs and users created on first startup
- [ ] Idempotent (no errors on restart)
