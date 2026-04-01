# Task: Create IKeycloakAdminService wrapper

**Module:** Auth
**Priority:** HIGH (Keycloak Phase 1 — needed for user/org management)
**Source:** docs/studies/keycloak-aspire-migration-20260401.md — 1.3
**Depends on:** done-kc-poc-aspire-setup-061

## Scope
Create `IKeycloakAdminService` in Auth.Contracts and implementation in Auth module:

1. Interface with methods:
   - CreateUserAsync(email, password, firstName, lastName) → Result<Guid>
   - AddUserToOrganizationAsync(userId, orgId, roles) → Result
   - RemoveUserFromOrganizationAsync(userId, orgId) → Result
   - CreateOrganizationAsync(name, clinicId) → Result<Guid>
   - UpdateUserRolesAsync(userId, orgId, roles) → Result
   - DeactivateUserAsync(userId) → Result

2. Implementation using HttpClient to call Keycloak Admin REST API
3. Auth via service account client credentials (vetolib-api client)
4. Register in DI with typed HttpClient

## Skills
- `ardalis-result`, `cqrs-mediatr`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] IKeycloakAdminService registered in DI
- [ ] TU for the service (mocked HttpClient)
