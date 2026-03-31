# Security Pen Test Audit — 2026-04-01

Automated security audit of all API endpoints across all modules.

---

## Summary

| Severity | Count |
|----------|-------|
| CRITICAL | 2     |
| HIGH     | 5     |
| MEDIUM   | 7     |
| LOW      | 4     |

---

## 1. Auth Bypass Findings

### 1.1 AllowAnonymous Endpoints — Intentional (OK)

All `AllowAnonymous` endpoints are correctly public:

| Endpoint | File | Justified |
|----------|------|-----------|
| `POST /api/v1/auth/login` | AuthEndpoints.cs | Yes — login |
| `POST /api/v1/auth/refresh` | AuthEndpoints.cs | Yes — token refresh |
| `POST /api/v1/auth/verify-email` | AuthEndpoints.cs | Yes — email verification |
| `POST /api/v1/clinics/register` | ClinicEndpoints.cs | Yes — self-registration |
| `GET /api/v1/clinics/search` | ClinicEndpoints.cs | Yes — public directory |
| `POST /api/v1/portal/register` | PortalEndpoints.cs | Yes — owner registration |
| `POST /api/v1/portal/invite-vet` | PortalEndpoints.cs | Yes — viral invite |
| `POST /api/v1/portal/login` | PortalEndpoints.cs | Yes — owner login |
| `GET /api/v1/shared/{token}` | SharedRecordEndpoints.cs | Yes — public share link |

### 1.2 [MEDIUM] SSE Endpoint Disables Rate Limiting

**File:** `Modules/Messaging/Vetolib.Messaging/Api/SseEndpoints.cs:28`
**Endpoint:** `GET /api/v1/messaging/sse`

The SSE endpoint calls `.DisableRateLimiting()`. While the broadcaster has its own subscriber limit (returns 429 if too many connections), disabling framework rate limiting means an attacker could open many SSE connections per IP before the broadcaster limit kicks in. The broadcaster limit is per-clinic, not per-IP.

**Recommendation:** Apply a dedicated SSE rate limiter (e.g., 5 connections per IP) instead of fully disabling it.

### 1.3 [MEDIUM] Clinic Search Endpoint Missing Rate Limiting

**File:** `Modules/Auth/Vetolib.Auth/Api/ClinicEndpoints.cs:30-36`
**Endpoint:** `GET /api/v1/clinics/search`

This public endpoint has `AllowAnonymous()` but no `.RequireRateLimiting(...)`. An attacker could scrape the entire clinic directory without throttling.

**Recommendation:** Add `.RequireRateLimiting("api")` to the search endpoint.

### 1.4 [LOW] Shared Record Endpoint Rate Limiting Policy

**File:** `Modules/MedicalRecords/Vetolib.MedicalRecords/Api/SharedRecordEndpoints.cs:21-29`
**Endpoint:** `GET /api/v1/shared/{token}`

Uses `RequireRateLimiting("api")` which is 100 req/min. For a public anonymous endpoint serving medical data, this is generous. An attacker could brute-force share tokens at 100/min per IP.

**Recommendation:** Apply a tighter rate limit (e.g., 10 req/min per IP) for this anonymous medical data endpoint.

---

## 2. IDOR (Insecure Direct Object Reference) Findings

### 2.1 Multi-Tenant Filter — Generally Effective

The `MultiTenantDbContext` applies global query filters (`WHERE ClinicId = @current`) automatically. Most endpoints benefit from this protection. IDOR via simple ID guessing is mitigated for standard tenant-scoped entities.

### 2.2 [CRITICAL] ClinicGroup Endpoints — Cross-Tenant IDOR

**Files:**
- `Modules/Auth/Vetolib.Auth/Api/ClinicGroupEndpoints.cs`
- `Modules/Auth/Vetolib.Auth/Application/Queries/ListGroupClinics/ListGroupClinicsHandler.cs`
- `Modules/Auth/Vetolib.Auth/Application/Commands/AddClinicToGroup/AddClinicToGroupHandler.cs`

**Endpoints affected:**
- `GET /api/v1/clinic-groups/{id}/clinics` — **No ownership check.** Any authenticated user can view any group's clinics by guessing the group ID. The handler queries `ClinicGroups` without filtering by the current user or clinic.
- `POST /api/v1/clinic-groups/{id}/clinics` — While it requires Admin role, it does not verify the admin owns/belongs to the target group. An Admin from Clinic A could add their clinic to Clinic B's group.
- `DELETE /api/v1/clinic-groups/{id}/clinics/{clinicId}` — Same issue: no ownership verification of the group.

**Impact:** Full cross-tenant data leakage of group membership. Potential unauthorized group manipulation.

**Recommendation:** Add ownership validation in each handler: verify that the requesting user's clinicId is a member of the group, or that the group's `OwnerUserId` matches the current user.

### 2.3 [CRITICAL] SwitchClinic — Insufficient Authorization Check

**File:** `Modules/Auth/Vetolib.Auth/Api/ClinicGroupEndpoints.cs:108-119`
**Endpoint:** `POST /api/v1/auth/switch-clinic`

Accepts a `ClinicId` from the request body and issues new tokens scoped to that clinic. The endpoint only verifies the user is authenticated. If the `SwitchClinicHandler` does not validate that the user has membership in the target clinic, any user could switch to any clinic and receive valid tokens.

**Recommendation:** Verify in the handler that the user belongs to the target clinic (via clinic group membership or direct assignment). This is likely already handled in the handler, but the endpoint-level check is missing. Needs handler-level audit to confirm.

### 2.4 [HIGH] OwnerPortal Endpoints — Animal ID Not Validated Against Owner

**File:** `Modules/MedicalRecords/Vetolib.MedicalRecords/Api/OwnerPortalEndpoints.cs`
**Endpoints:**
- `GET /api/v1/portal/animals/{id}/records`
- `GET /api/v1/portal/animals/{id}/vaccinations`
- `GET /api/v1/portal/animals/{id}/prescriptions`
- `GET /api/v1/portal/animals/{id}/weight`

The endpoints extract `owner_account_id` from the JWT and pass it to the query along with the animal ID. The handler must verify the animal belongs to the owner. This is done via `OwnerAuthorizationService` which uses `IgnoreQueryFilters()` — the authorization check is handler-level. If the authorization check has any bug, it would allow any owner to view any animal's medical data.

**Recommendation:** Add integration tests specifically for cross-owner access attempts. The pattern is correct but fragile since it relies on `IgnoreQueryFilters()`.

### 2.5 [HIGH] Messaging Portal Test-Token Endpoint

**File:** `Modules/Messaging/Vetolib.Messaging/Api/PortalEndpoints.cs:32-61`
**Endpoint:** `POST /api/v1/portal/test-token`

This endpoint creates valid portal tokens with hardcoded `ClinicId = 11111111-1111-1111-1111-111111111111`. While gated by `IsDevelopment() || IsEnvironment("Test")`, a misconfigured deployment (e.g., `ASPNETCORE_ENVIRONMENT=Test` in production) would expose unauthenticated token generation.

**Recommendation:** Add an additional safeguard: check for a specific configuration flag (e.g., `EnableTestEndpoints=true`) or use a compile-time `#if DEBUG` directive.

---

## 3. Input Validation Findings

### 3.1 FluentValidation Coverage

94 validator files found across modules. Strong coverage overall.

### 3.2 [HIGH] Missing Validators for Commands

The following commands/queries lack FluentValidation validators:

| Command | Module | Endpoint |
|---------|--------|----------|
| `CancelAppointmentSeriesCommand` | Agenda | `DELETE /api/v1/appointments/series/{seriesId}` |
| `DeactivateConsultationTypeCommand` | Agenda | `DELETE /api/v1/consultation-types/{id}` |
| `UploadFilesCommand` | Messaging | `POST /api/v1/messaging/upload` |
| `GenerateHealthAlertsCommand` | AI | `POST /api/v1/ai/health-alerts/generate` |
| `AcknowledgeHealthAlertCommand` | AI | `PATCH /api/v1/ai/health-alerts/{id}/acknowledge` |
| `ConvertAlertToAppointmentCommand` | AI | `POST /api/v1/ai/health-alerts/{id}/convert-to-appointment` |

While some of these are simple ID-based commands where validation is minimal, the `UploadFilesCommand` handles file uploads and should have formal validation. The handler (`UploadFilesHandler`) does have inline validation using `FileTypeValidator`, but this bypasses the FluentValidation pipeline behavior (MediatR pipeline).

**Recommendation:** Add validators for all commands, even if minimal (e.g., validate Guid is non-empty). The upload handler's inline validation is acceptable but inconsistent with the codebase pattern.

### 3.3 [MEDIUM] Query Parameter pageSize Not Bounded on All Endpoints

Several endpoints accept `pageSize` as a query parameter without upper bounds at the endpoint level:

- `GET /api/v1/users` — `pageSize = 20` default, no max
- `GET /api/v1/invoices` — `pageSize = 20` default, no max
- `GET /api/v1/messaging/conversations` — `pageSize = 20` default, no max
- `GET /api/v1/appointments` — `pageSize = 50` default, no max

A malicious user could send `pageSize=999999` to cause memory/performance issues.

**Recommendation:** Cap `pageSize` at a maximum (e.g., 200) at the endpoint level, similar to what `AuditEndpoints.cs` does (`if (pageSize is < 1 or > 200) pageSize = 50`).

### 3.4 [LOW] Drug Catalog Search Limit Parameter Unbounded

**File:** `Modules/MedicalRecords/Vetolib.MedicalRecords/Api/DrugCatalogEndpoints.cs:51`
**Endpoint:** `GET /api/v1/medical-records/drugs?limit=20`

The `limit` parameter has no maximum. A request with `limit=1000000` could return the entire catalog in one response.

**Recommendation:** Cap `limit` at a sensible maximum (e.g., 100).

---

## 4. Injection Risk Findings

### 4.1 [LOW] Raw SQL in DbInitializer — Acceptable

**File:** `Vetolib.Api/DbInitializer.cs:106`

Uses `ExecuteSqlRawAsync` with parameterized values (`{0}`, `{1}`, etc.) which is safe against SQL injection. The parameters are hardcoded seed data, not user input. No risk.

### 4.2 No String Concatenation in Queries

No instances of string concatenation in LINQ queries or raw SQL with user input were found across all handlers. All queries use EF Core LINQ or parameterized raw SQL. **PASS.**

### 4.3 IgnoreQueryFilters Usage Review

56 uses of `IgnoreQueryFilters()` found. All reviewed instances fall into justified categories:

- **Seed/migration scenarios** (DbInitializer, DrugCatalogSeedData, MedicalRecordTemplateSeedData) — OK
- **Cross-tenant auth operations** (Login, RefreshToken, RegisterClinic, VerifyEmail, ChangePassword, SwitchClinic, InviteUser, CreateUser) — OK, these operations inherently need cross-tenant access
- **Portal cross-clinic access** (GetMyAnimals, GetAnimalRecords, etc.) — OK with owner_account_id validation
- **Background services** (ReminderScheduler, EmergencyEscalation, PendingUploadCleanup) — OK, system-level operations
- **Public endpoints** (SearchClinics, GetSharedRecord) — OK, intentionally public

### 4.4 [MEDIUM] GetSharedRecord Handler Extensive IgnoreQueryFilters

**File:** `Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Queries/GetSharedRecord/GetSharedRecordHandler.cs`

7 separate `IgnoreQueryFilters()` calls in one handler to load patient, owner, medical records, prescriptions, and weight data. While each is necessary for the cross-tenant share link feature, the density of filter bypasses in a public anonymous handler increases the attack surface if the token validation has any weakness (e.g., timing attack on token comparison).

**Recommendation:** Ensure share link tokens are compared using constant-time comparison (e.g., `CryptographicOperations.FixedTimeEquals`). Add logging for failed token lookups to detect brute-force attempts.

---

## 5. Rate Limiting Findings

### 5.1 Auth Endpoints — Properly Rate Limited (PASS)

| Endpoint | Rate Limit |
|----------|-----------|
| `POST /api/v1/auth/login` | `auth` (10/min/IP) |
| `POST /api/v1/auth/refresh` | `auth` (10/min/IP) |
| `POST /api/v1/auth/verify-email` | `auth` (10/min/IP) |
| `POST /api/v1/auth/change-password` | `auth` (10/min/IP) |
| `POST /api/v1/clinics/register` | `signup` (3/hour/IP) |
| `POST /api/v1/portal/register` | `auth` (10/min/IP) |
| `POST /api/v1/portal/login` | `auth` (10/min/IP) |
| `POST /api/v1/portal/invite-vet` | `auth` (10/min/IP) |

### 5.2 [HIGH] Most Authenticated Endpoints Missing Rate Limiting

The following endpoint groups do NOT have `.RequireRateLimiting(...)`:

| Group | File |
|-------|------|
| `/api/v1/appointments` (all) | AppointmentEndpoints.cs |
| `/api/v1/consultation-types` | ConsultationTypeEndpoints.cs |
| `/api/v1/waitlist` | WaitlistEndpoints.cs |
| `/api/v1/appointments/{id}/feedback` | FeedbackEndpoints.cs |
| `/api/v1/feedback` | FeedbackEndpoints.cs |
| `/api/v1/users` | UserEndpoints.cs |
| `/api/v1/clinic-groups` | ClinicGroupEndpoints.cs |
| `/api/v1/onboarding` | OnboardingEndpoints.cs |
| `/api/v1/portal/referral-code` | ReferralEndpoints.cs |
| `/api/v1/portal/link-microchip` | PortalEndpoints.cs (Auth) |
| `/api/v1/invoices` | InvoiceEndpoints.cs |
| `/api/v1/billing/ereporting` | EReportingEndpoints.cs |
| `/api/v1/patients/{id}/records` | MedicalRecordEndpoints.cs |
| `/api/v1/owners` | OwnerEndpoints.cs |
| `/api/v1/patients/{id}/weights` | WeightEndpoints.cs |
| `/api/v1/medical-records/templates` | MedicalRecordTemplateEndpoints.cs |
| `/api/v1/portal` (MedicalRecords) | SharedRecordEndpoints.cs (portal group) |
| `/api/v1/portal` (OwnerPortal) | OwnerPortalEndpoints.cs |
| `/api/v1/messaging` | MessagingEndpoints.cs |
| `/api/v1/messaging/whatsapp` | WhatsAppEndpoints.cs |
| `/api/v1/breeding/pregnancies` | PregnancyEndpoints.cs |
| `/api/v1/patients/{id}/heat-cycles` | HeatCycleEndpoints.cs |
| `/api/v1/patients/{id}/lineage` etc. | LineageEndpoints.cs |
| `/api/v1/litters` | LitterEndpoints.cs |
| `/api/v1/stock` | StockEndpoints.cs |
| `/api/v1/notifications/reminders` | ReminderEndpoints.cs |
| `/api/v1/preferences/working-hours` | WorkingHoursEndpoints.cs |

**Endpoints WITH rate limiting (correct):**
- Patients group (`/api/v1/patients`) — `RequireRateLimiting("api")`
- Drug catalog (`/api/v1/medical-records/drugs`) — `RequireRateLimiting("api")`
- AI endpoints (`/api/v1/ai`) — `RequireRateLimiting("api")`
- Health alerts (`/api/v1/ai/health-alerts`) — `RequireRateLimiting("api")`
- Dashboard (`/api/dashboard`) — `RequireRateLimiting("api")`
- Audit (`/api/audit`) — `RequireRateLimiting("api")`

**Recommendation:** Add `.RequireRateLimiting("api")` to ALL endpoint groups at minimum. While authenticated endpoints have some natural protection (token required), a compromised token could be used for DoS.

### 5.3 [MEDIUM] No Global Rate Limiter Fallback

Rate limiting is opt-in per endpoint group. There is no global fallback rate limiter. Endpoints without explicit `.RequireRateLimiting(...)` are completely unthrottled.

**Recommendation:** Configure a global rate limiter as the default policy in `Program.cs` so all endpoints are rate-limited by default, with opt-out for specific endpoints (e.g., SSE).

---

## 6. Additional Security Findings

### 6.1 [HIGH] User Endpoints — Manual Role Check Instead of Policy

**File:** `Modules/Auth/Vetolib.Auth/Api/UserEndpoints.cs`

All endpoints manually check `user.FindFirst(ClaimTypes.Role)?.Value` against `"Admin"` inside the handler method. This is fragile:
- Case sensitivity issues (what if the claim is "admin"?)
- Missing role claim fails silently (returns Forbidden, but no logging)
- Not using ASP.NET Core's built-in `RequireAuthorization(policy => policy.RequireRole("Admin"))` which is tested and standardized

This pattern is repeated in: ClinicGroupEndpoints, MedicalRecordEndpoints, MedicalRecordTemplateEndpoints, OwnerEndpoints.

**Recommendation:** Replace manual role checks with `.RequireAuthorization(policy => policy.RequireRole("Admin"))` on the endpoint definitions, consistent with how ConsultationTypeEndpoints, EReportingEndpoints, and MessagingEndpoints handle it.

### 6.2 [MEDIUM] Patient Photo Upload — No Content-Type Validation

**File:** `Modules/MedicalRecords/Vetolib.MedicalRecords/Api/PatientEndpoints.cs:216-234`
**Endpoint:** `POST /api/v1/patients/{id}/photo`

The upload handler validates file size (5 MB max) but passes `file.ContentType` directly to the storage command without validating it against an allowed list (JPEG, PNG, WebP as stated in the description). An attacker could upload a file with `Content-Type: application/javascript` if the content-type header is spoofed.

The `UploadPatientPhotoCommand` stores `file.ContentType` as-is. If the photo is later served back via `GetPatientPhoto` with the stored content-type, this could lead to XSS if the browser interprets it as executable content.

**Recommendation:** Validate content-type against an allowlist AND perform magic-byte validation (like the Messaging `UploadFilesHandler` already does with `FileTypeValidator.DetectContentType`).

### 6.3 [LOW] CSV Import — DisableAntiforgery Without Additional Protection

**File:** `Modules/MedicalRecords/Vetolib.MedicalRecords/Api/PatientEndpoints.cs:80`
**Endpoint:** `POST /api/v1/patients/import`

Uses `.DisableAntiforgery()` for multipart form upload. This is standard for API-only backends (no cookies = no CSRF), but should be documented as intentional.

---

## Remediation Priority

### Immediate (CRITICAL)
1. **ClinicGroup IDOR** — Add ownership validation to all ClinicGroup endpoints
2. **SwitchClinic authorization** — Verify handler validates user membership in target clinic

### High Priority (within 1 sprint)
3. **Add global rate limiter** — Apply `"api"` rate limit to all endpoint groups
4. **Patient photo content-type validation** — Add magic-byte validation
5. **Replace manual role checks** — Use `RequireAuthorization` policies consistently
6. **Missing validators** — Add FluentValidation for all commands
7. **OwnerPortal IDOR tests** — Add cross-owner access integration tests

### Medium Priority (within 2 sprints)
8. **SSE rate limiting** — Add per-IP connection limit
9. **ClinicSearch rate limiting** — Add `RequireRateLimiting("api")`
10. **pageSize bounds** — Cap at 200 on all paginated endpoints
11. **Share token security** — Verify constant-time comparison
12. **Global rate limiter fallback** — Default policy in Program.cs
13. **Photo upload content-type** — Validate against allowlist

### Low Priority
14. **Drug catalog limit cap** — Max 100
15. **CSV import antiforgery** — Document as intentional
16. **Share link rate limit** — Tighten to 10/min
17. **Test-token endpoint** — Add compile-time guard
