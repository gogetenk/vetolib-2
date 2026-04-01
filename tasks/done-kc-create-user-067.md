# Task: Modify CreateUserHandler to sync with Keycloak

**Module:** Auth
**Priority:** HIGH (Keycloak Phase 2)
**Source:** docs/studies/keycloak-aspire-migration-20260401.md — 2.3
**Depends on:** done-kc-admin-api-client-064

## Scope
Modify CreateUserHandler so when an admin creates a user:
1. Create user in Keycloak via IKeycloakAdminService.CreateUserAsync
2. Add user to current organization with the specified role
3. Store KeycloakUserId on the User entity
4. Graceful degradation if Keycloak is unavailable

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] CreateUserHandler calls IKeycloakAdminService
- [ ] KeycloakUserId stored on User
