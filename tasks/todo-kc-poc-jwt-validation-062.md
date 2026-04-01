# Task: Keycloak PoC — JWT validation with Organizations

**Module:** Auth
**Priority:** HIGH (epic Keycloak Phase 0)
**Source:** docs/studies/keycloak-jwt-rbac-design-20260401.md
**Depends on:** todo-kc-poc-aspire-setup-061

## Context
Verify that Keycloak Organizations emit the right claims for ClinicContext.

## Scope
1. Create 2 test organizations via Keycloak Admin API (or realm JSON)
2. Create test users assigned to organizations with roles
3. Get a JWT token and verify it contains:
   - `sub` (user UUID)
   - `clinic_id` or `organization.id` (mapped to clinic_id via Protocol Mapper)
   - `role` (organization-level role, not realm-level)
4. Configure `AddJwtBearer` in AuthModuleServiceRegistrar to validate Keycloak JWKS
5. Test that ClinicContext reads clinic_id correctly from the Keycloak JWT
6. Document Go/No-Go decision

## Skills
- `ardalis-result`, `aspnet-minimal-api`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] JWT from Keycloak contains `clinic_id` claim readable by ClinicContext
- [ ] `role` claim matches organization membership
- [ ] Go/No-Go documented in docs/studies/keycloak-poc-results-20260401.md
