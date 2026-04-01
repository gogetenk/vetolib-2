# Task: Lazy password migration to Keycloak

**Module:** Auth
**Priority:** HIGH (Keycloak Phase 3.1)
**Depends on:** todo-kc-user-entity-cleanup-071

## Scope
During the dual-stack period, when a Legacy user logs in successfully:
1. Verify BCrypt hash (existing flow)
2. If OK and user has no KeycloakUserId: create the user in Keycloak with the plaintext password
3. Set AuthProvider = Both
4. Set KeycloakUserId
5. Next login can use either Legacy or Keycloak

This transparently migrates users to Keycloak on their next login — zero downtime, zero mass email.

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] LoginHandler migrates Legacy users to Keycloak on successful login
- [ ] AuthProvider updated from Legacy to Both
- [ ] TU covering migration flow
