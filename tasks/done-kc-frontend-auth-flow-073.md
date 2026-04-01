# Task: Frontend Keycloak auth flow (OIDC PKCE)

**Module:** Frontend
**Priority:** HIGH (Keycloak Phase 3)
**Depends on:** done-kc-poc-aspire-setup-061
[MSW: non — real Keycloak needed]

## Scope
Update the frontend to support Keycloak OIDC login alongside legacy:
1. Add next-auth or a lightweight OIDC client library
2. Configure Keycloak provider (vetolib-frontend client, PKCE)
3. On login page: add "Sign in with Keycloak" button (or make it the default)
4. Token stored in httpOnly cookie or session
5. ClinicSwitcher: call GET /api/v1/auth/my-organizations to list available orgs
6. Switch clinic: call Keycloak token endpoint with organization parameter

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] Login via Keycloak OIDC works
- [ ] Token contains clinic_id claim
- [ ] ClinicSwitcher uses my-organizations endpoint
