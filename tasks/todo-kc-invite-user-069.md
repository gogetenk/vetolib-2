# Task: Modify InviteUserHandler to sync with Keycloak

**Module:** Auth
**Priority:** HIGH (Keycloak Phase 2.4)
**Depends on:** done-kc-admin-api-client-064

## Scope
When an admin invites a user via email:
1. Create the user in Keycloak with requiredActions: ["UPDATE_PASSWORD"]
2. Add user to the clinic's organization with the specified role
3. Store KeycloakUserId on the User entity
4. Best-effort: if Keycloak fails, invitation still works (legacy flow)

Read: src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/InviteUser/InviteUserHandler.cs

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] InviteUserHandler calls IKeycloakAdminService
- [ ] Keycloak user created with UPDATE_PASSWORD action
