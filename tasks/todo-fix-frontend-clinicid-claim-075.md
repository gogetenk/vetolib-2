# Task: Fix frontend clinicId claim mismatch (snake_case vs camelCase)

**Module:** Frontend
**Priority:** HIGH (bug — Keycloak JWT archi study finding)
**Source:** docs/studies/keycloak-jwt-rbac-design-20260401.md
[MSW: oui]

## Problem
Backend emits `clinic_id` (snake_case) in JWT claims. Frontend ClinicSwitcher.tsx reads `parseJwtClaim("clinicId")` (camelCase). This is a latent bug that may cause clinic switching to fail with real JWTs.

## Fix
1. Find all places in frontend that parse JWT claims (grep for parseJwtClaim, clinicId, clinic_id)
2. Standardize to `clinic_id` (snake_case) to match backend
3. Verify ClinicSwitcher, auth context, and any other JWT parsing

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] All JWT claim parsing uses `clinic_id` (snake_case)
- [ ] ClinicSwitcher reads correct claim
