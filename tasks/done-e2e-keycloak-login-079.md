# Task: Add E2E test for Keycloak login flow

**Module:** Frontend (E2E)
**Priority:** MEDIUM
**Depends on:** done-kc-frontend-auth-flow-073

## Scope
Add Playwright E2E test for the Keycloak OIDC login flow:
1. Navigate to /en/login
2. Click "Sign in with Keycloak" button
3. Verify redirect to Keycloak login page (or mock with MSW)
4. Complete login
5. Verify redirect back to dashboard with valid session
6. Verify ClinicSwitcher shows organizations

Since Keycloak isn't running in E2E (MSW mode), this may need to:
- Test that the button exists and triggers the OIDC flow
- Test the callback handling with a mock token
- Test ClinicSwitcher with my-organizations MSW handler

## Definition of Done
- [ ] E2E test for Keycloak login button + flow
- [ ] E2E test for ClinicSwitcher with organizations
- [ ] `npm run e2e` passes
