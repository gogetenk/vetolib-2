# Task: Clean up User entity for Keycloak (remove credential fields)

**Module:** Auth
**Priority:** MEDIUM (Keycloak Phase 2.8)
**Depends on:** todo-kc-invite-user-069, todo-kc-switch-clinic-simplify-070

## Scope
With all handlers now syncing to Keycloak, prepare the User entity for eventual credential removal:
1. Mark PasswordHash as nullable (will be empty for Keycloak-only users)
2. Add `AuthProvider` enum field (Legacy, Keycloak, Both)
3. Add migration for nullable PasswordHash + AuthProvider column
4. Update User.Create() to support Keycloak-only mode (no password required if AuthProvider=Keycloak)

DO NOT remove existing fields yet — this is prep for Phase 3 cutover.

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] PasswordHash is nullable
- [ ] AuthProvider enum added
- [ ] Migration audited
