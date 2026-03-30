# Security Audit — Vetolib Backend

**Date**: 2026-03-29
**Auditor**: Claude Opus 4.6 (automated)
**Scope**: All backend code in `src/backend/`
**Branch**: `develop` (commit `025a1d8`)

---

## Summary

| Severity | Count |
|----------|-------|
| CRITICAL | 1     |
| HIGH     | 5     |
| MEDIUM   | 7     |
| LOW      | 5     |
| INFO     | 4     |

---

## Findings

### CRITICAL

#### C-01: Temporary password generated with `System.Random` (not cryptographically secure)

**File**: `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/InviteUser/InviteUserHandler.cs` (lines 55-61)
**Description**: The `GenerateTemporaryPassword()` method uses `new Random()`, which is not cryptographically secure. `System.Random` is seeded from the system clock and its output is predictable. An attacker who knows the approximate time an invitation was sent could brute-force the temporary password.
**Contrast**: The `DbInitializer.GenerateSeedPassword()` correctly uses `RandomNumberGenerator` from `System.Security.Cryptography`.
**Impact**: Account takeover of any invited user if the attacker can predict or enumerate the temporary password.
**Recommendation**: Replace `new Random()` with `RandomNumberGenerator.GetBytes()` or `RandomNumberGenerator.GetString()` (.NET 8+).

---

### HIGH

#### H-01: No CORS policy configured

**Files**: `src/backend/Vetolib.Api/Program.cs`, `src/backend/Modules/Auth/Vetolib.Auth/AuthModuleServiceRegistrar.cs`
**Description**: There is no call to `AddCors()` or `UseCors()` anywhere in the backend. While `appsettings.Development.json` defines `Cors:AllowedOrigins`, no code reads this configuration or registers CORS middleware. In production, browsers will block legitimate cross-origin requests from the Next.js frontend. More importantly, if a reverse proxy (Caddy) adds permissive CORS headers, the backend has no defense-in-depth.
**Impact**: Either the frontend cannot call the API (functional bug) or the API relies entirely on the reverse proxy for CORS enforcement (single point of failure).
**Recommendation**: Add `builder.Services.AddCors()` with a named policy reading from `Cors:AllowedOrigins` configuration, and `app.UseCors()` before `UseAuthentication()`.

#### H-02: No global exception handler — stack traces may leak to clients

**Files**: `src/backend/Vetolib.Api/Program.cs`
**Description**: There is no `app.UseExceptionHandler()` or `app.UseProblemDetails()` middleware registered. If an unhandled exception occurs (e.g., database timeout, null reference), ASP.NET Core's default behavior in Development mode is to return the full stack trace. In Production mode it returns a generic 500, but the response body format is not controlled.
**Impact**: In development/staging, stack traces, internal paths, and connection info could leak. In production, inconsistent error responses.
**Recommendation**: Add `app.UseExceptionHandler()` with a `/error` handler or register `ProblemDetails` services for RFC 7807-compliant error responses.

#### H-03: OpenAPI endpoint exposed in all environments

**File**: `src/backend/Vetolib.Api/Program.cs` (line 295)
**Description**: `app.MapOpenApi()` is called unconditionally, meaning the full API schema (all endpoints, request/response DTOs, routes) is accessible in production at `/openapi/v1.json`.
**Impact**: Information disclosure — attackers get a complete map of the API surface, parameter names, and types.
**Recommendation**: Gate behind `app.Environment.IsDevelopment()` or require authentication for the OpenAPI endpoint in non-dev environments.

#### H-04: No HTTPS redirection middleware

**File**: `src/backend/Vetolib.Api/Program.cs`
**Description**: There is no call to `app.UseHttpsRedirection()` or `app.UseHsts()`. While TLS may be terminated at the reverse proxy (Caddy), the backend itself does not enforce HTTPS. If the backend is ever exposed directly (misconfiguration, debugging), all traffic including JWT tokens would be sent in cleartext.
**Impact**: Token theft via network sniffing if backend is exposed without TLS termination.
**Recommendation**: Add `app.UseHttpsRedirection()` and `app.UseHsts()` for defense-in-depth, or at minimum document that TLS is always terminated at the proxy layer.

#### H-05: Temporary password returned in API response and published to integration event

**Files**:
- `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/InviteUser/InviteUserHandler.cs` (line 47-52) — returns `TemporaryPassword` in HTTP response
- `src/backend/Modules/Auth/Vetolib.Auth.Contracts/InviteUserResponse.cs` — DTO includes `TemporaryPassword`
- `src/backend/Modules/Auth/Vetolib.Auth.Contracts/UserInvitedIntegrationEvent.cs` — includes `TemporaryPassword`
**Description**: The plaintext temporary password is (a) returned in the HTTP response to the admin who invites the user, and (b) published as a MassTransit integration event (persisted to the outbox table). This means the password exists in HTTP response logs, message bus logs, and the EF Core outbox table.
**Impact**: Password exposure through multiple channels — any log aggregator, message broker consumer, or outbox table query reveals the credential.
**Recommendation**: Send the temporary password only via email (already handled by the domain event flow). Return only a confirmation that the invitation was sent, not the password itself. Remove `TemporaryPassword` from the integration event; if the notification consumer needs it, pass a reset token instead.

---

### MEDIUM

#### M-01: Several commands lack FluentValidation validators

**Description**: The following commands have no corresponding `*Validator.cs` file, meaning no input validation occurs in the MediatR pipeline:
- `SubmitToEInvoicingCommand`
- `ImportPatientsCommand`
- `AddClinicToGroupCommand`
- `RemoveClinicFromGroupCommand`
- `SwitchClinicCommand`
- `AcknowledgeHealthAlertCommand`
- `GenerateHealthAlertsCommand`
- `ConvertAlertToAppointmentCommand`
- `UploadFilesCommand`
- `DeactivateConsultationTypeCommand`
**Impact**: Malformed or malicious input reaches handlers without validation. While some handlers may validate internally, the defense-in-depth pattern is broken.
**Recommendation**: Add validators for all commands, even trivial ones (e.g., `RuleFor(x => x.Id).NotEmpty()`).

#### M-02: Most endpoint groups lack rate limiting

**Description**: Rate limiting is applied to auth endpoints (`auth`), signup (`signup`), and a few specific modules (Dashboard, Audit, AI, DrugCatalog, Patients). However, the following modules have NO rate limiting:
- Billing (all endpoints)
- Stock (all endpoints)
- Breeding (all endpoints)
- Messaging (most endpoints except inheriting from group)
- Notifications (all endpoints)
- Onboarding (all endpoints)
- ClinicGroups (all endpoints)
- Users (all endpoints)
- Owner Portal (all endpoints)
- WhatsApp config (all endpoints)
**Impact**: These endpoints are vulnerable to abuse, data scraping, and denial-of-service attacks.
**Recommendation**: Apply the `api` rate limiter to all authenticated endpoint groups. Consider a separate limiter for the portal (owner-facing, no JWT auth).

#### M-03: Portal endpoints lack rate limiting

**File**: `src/backend/Modules/Messaging/Vetolib.Messaging/Api/PortalEndpoints.cs`
**Description**: The owner portal endpoints are authenticated via magic link token, not JWT. They have no rate limiting at all. An attacker with a valid (or stolen) magic link token could abuse these endpoints without restriction.
**Impact**: Data exfiltration, message spam, or DoS against the portal.
**Recommendation**: Add a dedicated rate limiter for portal endpoints (e.g., 30 req/min per token).

#### M-04: SSE endpoint disables rate limiting

**File**: `src/backend/Modules/Messaging/Vetolib.Messaging/Api/SseEndpoints.cs` (line 26)
**Description**: The SSE endpoint explicitly calls `.DisableRateLimiting()`. While this is necessary for long-lived connections, there is no connection limit. An attacker could open hundreds of SSE connections to exhaust server resources.
**Impact**: Resource exhaustion / denial of service.
**Recommendation**: Implement a per-user or per-clinic connection limit (e.g., max 5 concurrent SSE connections per clinic).

#### M-05: Hardcoded fallback connection strings in DesignTimeDbContextFactory classes

**Files**: All `*DbContextFactory.cs` files (9 total across modules + Shared)
**Description**: Each design-time factory contains `"Host=localhost;Database=vetolibdb;Username=postgres;Password=postgres"` as a fallback. While these are only used for `dotnet ef migrations` CLI tooling and not at runtime, the credentials are committed to source control.
**Impact**: Low direct risk (dev-only, localhost), but establishes a pattern of hardcoded credentials. If a developer copies this pattern for runtime code, it becomes a real vulnerability.
**Recommendation**: Use `DATABASE_URL` environment variable exclusively (already the primary path). Remove the hardcoded fallback or replace with a clear error message.

#### M-06: `ClinicContext.ClinicId` returns `Guid.Empty` when claim is missing

**File**: `src/backend/Shared/Vetolib.Shared.Infrastructure/ClinicContext.cs`
**Description**: When the `clinic_id` claim is missing from the JWT (e.g., forged token with claim removed), `ClinicContext.ClinicId` silently returns `Guid.Empty`. The global query filter `WHERE ClinicId = '00000000-...'` would then match any entity where `ClinicId` happens to be `Guid.Empty`.
**Impact**: Unlikely in practice (no entities should have `ClinicId = Guid.Empty`), but a defense gap. If a seed or migration bug creates an entity with `Guid.Empty`, it would be accessible to any unauthenticated or malformed request.
**Recommendation**: Validate that `ClinicId != Guid.Empty` early in the request pipeline (middleware or filter). Return 401/403 if the claim is missing or invalid.

#### M-07: `DisableAntiforgery()` on file upload endpoints

**Files**:
- `src/backend/Modules/Messaging/Vetolib.Messaging/Api/MessagingEndpoints.cs` (line 224) — `/upload`
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Api/PatientEndpoints.cs` (line 51) — `/import`
**Description**: Both file upload endpoints call `.DisableAntiforgery()`. This is necessary for multipart form uploads from SPA clients, but it removes CSRF protection for these state-changing POST endpoints.
**Impact**: CSRF attacks could upload files or import patients on behalf of authenticated users if they visit a malicious page. Mitigated by JWT auth (bearer tokens are not sent automatically by browsers), but still a defense-in-depth concern.
**Recommendation**: Acceptable as-is since JWT bearer tokens are not auto-attached by browsers. Document this design decision.

---

### LOW

#### L-01: JWT `ClockSkew = TimeSpan.Zero` — tight but correct

**File**: `src/backend/Modules/Auth/Vetolib.Auth/AuthModuleServiceRegistrar.cs` (line 68)
**Description**: Zero clock skew means tokens are invalid the instant they expire. This is the most secure setting but can cause issues with server clock drift.
**Impact**: Legitimate tokens may be rejected if there is clock drift between the API server and the token issuer (same server in this case, so minimal risk).
**Recommendation**: Acceptable. Monitor for auth failures due to clock drift in production.

#### L-02: Refresh token expiry is 7 days (hardcoded default)

**File**: `src/backend/Modules/Auth/Vetolib.Auth/Application/Domain/RefreshToken.cs` (line 15)
**Description**: The refresh token expiration of 7 days is hardcoded as a default parameter. It is not configurable via `appsettings.json`.
**Impact**: Cannot adjust refresh token lifetime without code change and redeployment.
**Recommendation**: Make configurable via `AuthSecurityOptions` (like `TokenExpirationMinutes`).

#### L-03: Refresh tokens are not invalidated on password change

**File**: `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/ChangePassword/ChangePasswordHandler.cs`
**Description**: When a user changes their password, existing refresh tokens are not revoked. The old refresh token remains valid for up to 7 days.
**Impact**: If an attacker has stolen a refresh token, changing the password does not revoke their access.
**Recommendation**: Revoke all existing refresh tokens for the user when their password is changed.

#### L-04: `appsettings.json` contains placeholder JWT key `REPLACE_IN_PRODUCTION`

**File**: `src/backend/Vetolib.Api/appsettings.json` (line 10)
**Description**: The base appsettings contains `"Key": "REPLACE_IN_PRODUCTION"`. The fail-fast validation in `Program.cs` requires the key to be at least 32 characters, so this 22-character placeholder would cause a startup failure if no override is provided. This is actually good — but the presence of a "secret-looking" value in a committed config file is a code smell.
**Impact**: None (fail-fast prevents runtime use). But could confuse developers.
**Recommendation**: Replace with an empty string or remove the key entirely from the base config, relying on environment variables.

#### L-05: No old refresh token cleanup / expiration purge

**Description**: Revoked and expired refresh tokens remain in the database indefinitely. There is no background job or scheduled cleanup.
**Impact**: Database table grows unbounded over time.
**Recommendation**: Add a background service or scheduled job to purge expired/revoked refresh tokens older than N days.

---

### INFO

#### I-01: IgnoreQueryFilters usage audit — all justified

**Description**: All 20+ `IgnoreQueryFilters()` calls were reviewed. Each has a documented justification:
- **Auth handlers** (Login, RefreshToken, ChangePassword, RegisterClinic, SwitchClinic): Pre-authentication or cross-tenant by design
- **Seed data** (DbInitializer, DrugCatalogSeedData): ClinicId is null for global catalog
- **Background services** (ReminderScheduler, AppointmentReminder, EmergencyEscalation, PendingUploadCleanup): Cross-clinic batch operations
- **Messaging portal** (MagicLinkEndpointFilter, BusinessHoursChecker): Cross-tenant lookup by design
- **Subscription checker** (SubscriptionChecker): Needs cross-tenant clinic lookup
- **Clinic groups** (AddClinicToGroup, CreateClinicGroup): Cross-tenant group management

**Recommendation**: No action needed. All uses are documented and appropriate.

#### I-02: Password policy is reasonable

**Description**: Password requirements (8+ chars, 1 uppercase, 1 digit) are enforced in the `User` domain entity. BCrypt is used for hashing (via `BCrypt.Net.BCrypt`). Account lockout is configured (5 attempts, 15-minute lockout).
**Recommendation**: Consider adding a special character requirement and a max password length limit (to prevent BCrypt DoS with very long passwords — BCrypt truncates at 72 bytes).

#### I-03: Sentry PII protection is enabled

**File**: `src/backend/Vetolib.Api/Program.cs` (line 74)
**Description**: `SendDefaultPii = false` is correctly set, preventing automatic PII capture in error reports.
**Recommendation**: No action needed.

#### I-04: Audit trail correctly redacts sensitive properties

**File**: `src/backend/Shared/Vetolib.Shared.Infrastructure/AuditSaveChangesInterceptor.cs` (lines 33-38)
**Description**: The audit interceptor excludes `PasswordHash`, `SecurityStamp`, and `RefreshToken` from audit log entries.
**Recommendation**: No action needed. Consider adding `TemporaryPassword` to the exclusion list if it ever becomes a persisted entity property.

---

## Dependency CVE Check (manual review)

| Package | Version | Notes |
|---------|---------|-------|
| Aspire.Npgsql.EntityFrameworkCore.PostgreSQL | 13.1.2 | Current |
| MassTransit | 8.* | Floating — pin to avoid surprise breaking changes |
| Sentry.AspNetCore | 6.1.0 | Current |
| Serilog.AspNetCore | 8.* | Floating |
| BCrypt.Net-Next | (indirect) | No known CVEs |

**Note**: Several packages use floating versions (`8.*`, `10.*`). This can introduce untested versions. Consider pinning to exact versions in production.

**Recommendation**: Run `dotnet list package --vulnerable` to check for known CVEs in all transitive dependencies.

---

## Top Priority Remediation

1. **C-01**: Replace `System.Random` with `RandomNumberGenerator` in `InviteUserHandler` — immediate fix, 5 minutes
2. **H-01**: Add CORS policy — prevents both functional bugs and security gaps
3. **H-02**: Add global exception handler — prevents stack trace leakage
4. **H-03**: Gate OpenAPI behind environment check — 1 line change
5. **H-05**: Stop returning temporary password in API response
6. **M-06**: Validate `ClinicId != Guid.Empty` in middleware
7. **L-03**: Revoke refresh tokens on password change
