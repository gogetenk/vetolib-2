# Task: Simplify SwitchClinicHandler for Keycloak token exchange

**Module:** Auth
**Priority:** MEDIUM (Keycloak Phase 2.7)
**Depends on:** done-kc-poc-jwt-validation-062

## Scope
SwitchClinicHandler currently does:
1. Validates user has access to target clinic (3 checks)
2. Generates a new JWT with different clinic_id

With Keycloak Organizations, the frontend can do token exchange directly via Keycloak's token endpoint with `organization` parameter. But for backward compatibility during migration:

1. Add a new endpoint `POST /api/v1/auth/switch-clinic/keycloak` that returns the Keycloak token endpoint URL + org ID (so frontend can do the exchange)
2. Keep the Legacy SwitchClinic endpoint working
3. Add `ListMyOrganizations` query that proxies Keycloak Admin API to list user's orgs (for ClinicSwitcher frontend component)

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] ListMyOrganizations endpoint works
- [ ] Legacy SwitchClinic still works
