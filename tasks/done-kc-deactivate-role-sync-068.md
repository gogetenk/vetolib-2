# Task: Sync DeactivateUser + ChangeUserRole with Keycloak

**Module:** Auth
**Priority:** MEDIUM (Keycloak Phase 2)
**Source:** docs/studies/keycloak-aspire-migration-20260401.md — 2.5 + 2.6
**Depends on:** done-kc-admin-api-client-064

## Scope
Two small handler modifications:

### DeactivateUserHandler
- After deactivating user in DB, call IKeycloakAdminService.DeactivateUserAsync
- Graceful degradation if Keycloak unavailable

### ChangeUserRoleHandler
- After changing role in DB, call IKeycloakAdminService.UpdateUserRolesAsync
- Sync the new role to the Keycloak organization membership

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] Both handlers call IKeycloakAdminService
- [ ] Graceful degradation
