# Task: Modify RegisterClinicHandler to create Keycloak Organization + user

**Module:** Auth
**Priority:** HIGH (Keycloak Phase 2)
**Source:** docs/studies/keycloak-aspire-migration-20260401.md — 2.2
**Depends on:** done-kc-admin-api-client-064

## Scope
Modify RegisterClinicHandler so that when a new clinic registers:
1. Create a Keycloak Organization (name = clinic name, attributes.clinicId = new clinicId)
2. Create the admin user in Keycloak (email, password)
3. Add user to organization with Admin role
4. Keep existing DB operations (create Clinic entity, User entity)
5. Add KeycloakUserId to User entity (new nullable Guid property)
6. If Keycloak operations fail, the registration should still succeed (graceful degradation during migration)

## Skills
- `ardalis-result`, `cqrs-mediatr`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] RegisterClinicHandler calls IKeycloakAdminService.CreateOrganizationAsync + CreateUserAsync
- [ ] User entity has KeycloakUserId property
- [ ] Graceful degradation if Keycloak is down
