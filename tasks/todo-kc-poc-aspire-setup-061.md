# Task: Keycloak PoC — Aspire setup + Organizations

**Module:** AppHost + Auth
**Priority:** HIGH (epic Keycloak Phase 0)
**Source:** docs/studies/keycloak-aspire-migration-20260401.md
MODIF_SHARED: non (AppHost only)

## Context
Migration auth custom → Keycloak 26+ avec Organizations. Pattern C validé par le fondateur.

## Scope
1. Add `Aspire.Hosting.Keycloak` to AppHost
2. Create minimal realm JSON (`infra/keycloak/vetolib-realm.json`) with:
   - Client `vetolib-api` (confidential)
   - Realm roles: Admin, Vet, Receptionist, Assistant
   - Protocol Mapper to emit `clinic_id` claim from organization membership
   - Enable Organizations feature (KC_FEATURES=organization)
3. Wire in AppHost/Program.cs: `builder.AddKeycloak("keycloak").WithRealmImport(...)`
4. Wire Vetolib.Api to receive keycloak connection
5. Verify Keycloak starts with `dotnet run` in AppHost

## Skills
- `dotnet-aspire`

## Definition of Done
- [ ] `dotnet build` 0 errors (AppHost + Api)
- [ ] Keycloak container starts via Aspire
- [ ] Realm auto-imported on startup
- [ ] Admin console accessible at http://localhost:{port}
- [ ] Organizations feature enabled
