# Task: Keycloak Phase 4 — EF cleanup migration

**Module:** Auth
**Priority:** MEDIUM (Phase 4 — after legacy removal)
**Depends on:** todo-kc-cutover-legacy-removal-077

## Scope
EF Core migration to clean up legacy auth columns:
1. Drop table `refresh_tokens`
2. Drop columns from `users`: password_hash, failed_login_attempts, is_locked, locked_until, email_verification_token, email_verification_sent_at, email_verified_at, must_change_password
3. Drop `auth_provider` column (all users are Keycloak now)
4. Make KeycloakUserId NOT NULL (was nullable during migration)

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] Migration audited (no phantom ops)
- [ ] `dotnet test` GREEN
