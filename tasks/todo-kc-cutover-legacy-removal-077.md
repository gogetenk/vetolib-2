# Task: Keycloak Phase 4 — Remove Legacy auth scheme

**Module:** Auth
**Priority:** MEDIUM (Phase 4 — only after all users migrated)
**Source:** docs/studies/keycloak-aspire-migration-20260401.md — 4.1 + 4.2

## Pre-condition
All users must have AuthProvider = Both or Keycloak. Run a check query before starting:
`SELECT COUNT(*) FROM auth.users WHERE auth_provider = 'Legacy'`
Must be 0.

## Scope
1. Remove Legacy JWT scheme from AuthModuleServiceRegistrar (keep only Keycloak)
2. Remove MultiScheme policy selector
3. Delete handlers: LoginHandler, RefreshTokenHandler, LogoutHandler, ChangePasswordHandler, VerifyEmailHandler
4. Delete handlers: OwnerPortalLoginHandler, RegisterOwnerAccountHandler
5. Delete services: JwtTokenService, OwnerPortalJwtService
6. Delete entities: RefreshToken
7. Delete related validators, commands, DTOs
8. Remove lazy migration logic from LoginHandler (no longer needed)
9. Update TU: remove tests for deleted handlers, add tests for Keycloak-only flow

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] No Legacy JWT code remains
- [ ] Only Keycloak authentication works
