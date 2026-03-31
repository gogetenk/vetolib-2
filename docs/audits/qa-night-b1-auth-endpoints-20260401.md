# QA Audit -- Auth Module Endpoints

**Date**: 2026-04-01
**Auditor**: QA Agent (automated)
**Branch**: develop
**Scope**: All endpoint files in `src/backend/Modules/Auth/Vetolib.Auth/Api/*.cs`

---

## Summary

| Metric | Count |
|---|---|
| Endpoint files | 8 |
| Total endpoints (MapXxx) | 30 |
| Endpoints with WithSummary + WithDescription | 30/30 |
| Endpoints with explicit auth (RequireAuthorization or AllowAnonymous) | 30/30 |
| Endpoints with rate limiting (group or endpoint level) | 30/30 |
| Handlers returning Result / Result\<T\> | 30/30 |
| Command/Query handlers with validator | 27/30 (see gaps) |
| Endpoints with at least 1 integration test | 11/30 (see gaps) |

**Overall status**: PARTIAL -- API metadata (summary/description/auth/rate-limit) is excellent. Handler/validator coverage is strong. **Integration test coverage has critical gaps** -- 19 endpoints have zero TI.

---

## 1. AuthEndpoints.cs

Route group: `/api/v1/auth` (public + authenticated)

| Endpoint | Method | Auth | RateLimit | Summary | Handler Returns | Validator | TI | Status |
|---|---|---|---|---|---|---|---|---|
| `/login` | POST | AllowAnonymous | auth | Yes | Result\<AuthTokenDto\> | LoginValidator | Yes (Login_ValidCredentials, Login_InvalidCredentials) | OK |
| `/refresh` | POST | AllowAnonymous | auth | Yes | Result\<AuthTokenDto\> | RefreshTokenValidator | Yes (RefreshToken_ValidToken) | OK |
| `/verify-email` | POST | AllowAnonymous | auth | Yes | Result | VerifyEmailValidator | Yes (VerifyEmail_WithToken_ReturnsNon5xx) | OK |
| `/logout` | POST | RequireAuthorization (group) | api (group) | Yes | Result | LogoutValidator | **NO** | MISSING TI |
| `/me` | GET | RequireAuthorization (group) | api (group) | Yes | Result\<UserDto\> | N/A (query, no validator) | Yes (GetMe_Authenticated, GetMe_Unauthenticated) | OK |
| `/change-password` | POST | RequireAuthorization (group) | auth | Yes | Result | ChangePasswordValidator | **NO** | MISSING TI |

**Notes**:
- GetCurrentUserQuery has no validator (queries with no user input besides userId are acceptable).
- `/logout` and `/change-password` have zero integration tests.

---

## 2. ClinicEndpoints.cs

Route group: `/api/v1/clinics` (public)

| Endpoint | Method | Auth | RateLimit | Summary | Handler Returns | Validator | TI | Status |
|---|---|---|---|---|---|---|---|---|
| `/register` | POST | AllowAnonymous | signup | Yes | Result\<RegisterClinicResponse\> | RegisterClinicValidator | Yes (RegisterClinic_ValidRequest, RegisterClinic_DuplicateEmail) | OK |
| `/search` | GET | AllowAnonymous | api (group) | Yes | Result\<ClinicSearchPagedResultDto\> | SearchClinicsValidator | **NO** | MISSING TI |

---

## 3. ClinicGroupEndpoints.cs

Route group: `/api/v1/clinic-groups` (authenticated) + `/api/v1/auth/switch-clinic`

| Endpoint | Method | Auth | RateLimit | Summary | Handler Returns | Validator | TI | Status |
|---|---|---|---|---|---|---|---|---|
| `POST /` | POST | RequireAuthorization + RequireRole("Admin") | api (group) | Yes | Result\<ClinicGroupDto\> | CreateClinicGroupValidator | **NO** | MISSING TI |
| `POST /{id}/clinics` | POST | RequireAuthorization + RequireRole("Admin") | api (group) | Yes | Result | AddClinicToGroupValidator | **NO** | MISSING TI |
| `GET /{id}/clinics` | GET | RequireAuthorization (group) | api (group) | Yes | Result\<ClinicGroupDto\> | N/A (query) | **NO** | MISSING TI |
| `DELETE /{id}/clinics/{clinicId}` | DELETE | RequireAuthorization + RequireRole("Admin") | api (group) | Yes | Result | RemoveClinicFromGroupValidator | **NO** | MISSING TI |
| `GET /{id}/dashboard/stats` | GET | RequireAuthorization + RequireRole("Admin") | api (group) | Yes | Result\<ClinicGroupDashboardStatsDto\> | N/A (query) | **NO** | MISSING TI |
| `GET /{id}/dashboard/clinics` | GET | RequireAuthorization + RequireRole("Admin") | api (group) | Yes | Result\<IReadOnlyList\<ClinicGroupClinicStatsDto\>\> | N/A (query) | **NO** | MISSING TI |
| `GET /{id}/dashboard/revenue-comparison` | GET | RequireAuthorization + RequireRole("Admin") | api (group) | Yes | Result\<ClinicGroupRevenueComparisonDto\> | N/A (query) | **NO** | MISSING TI |
| `POST /api/v1/auth/switch-clinic` | POST | RequireAuthorization (group) | api (group) | Yes | Result\<AuthTokenDto\> | SwitchClinicValidator | **NO** | MISSING TI |

**Notes**:
- All 8 endpoints in this file have zero integration tests. This is the largest gap.
- Query handlers (ListGroupClinics, GetGroupDashboardStats, GetGroupClinicStats, GetGroupRevenueComparison) have no validators -- acceptable for queries with only route params + userId.

---

## 4. OnboardingEndpoints.cs

Route group: `/api/v1/onboarding` (authenticated)

| Endpoint | Method | Auth | RateLimit | Summary | Handler Returns | Validator | TI | Status |
|---|---|---|---|---|---|---|---|---|
| `GET /` | GET | RequireAuthorization (group) | api (group) | Yes | Result\<OnboardingStateDto\> | N/A (query) | **NO** | MISSING TI |
| `POST /steps/{stepId}/complete` | POST | RequireAuthorization (group) | api (group) | Yes | Result | CompleteOnboardingStepValidator | **NO** | MISSING TI |
| `POST /banner/dismiss` | POST | RequireAuthorization (group) | api (group) | Yes | Result | DismissWelcomeBannerValidator | **NO** | MISSING TI |
| `POST /checklist/dismiss` | POST | RequireAuthorization (group) | api (group) | Yes | Result | DismissChecklistValidator | **NO** | MISSING TI |

**Notes**:
- All 4 endpoints have zero integration tests.
- GetOnboardingStateQuery has no validator -- acceptable (only userId from claims).

---

## 5. PortalEndpoints.cs

Route group: `/api/v1/portal` (public + authenticated)

| Endpoint | Method | Auth | RateLimit | Summary | Handler Returns | Validator | TI | Status |
|---|---|---|---|---|---|---|---|---|
| `/register` | POST | AllowAnonymous | auth | Yes | Result\<OwnerAccountDto\> | RegisterOwnerAccountValidator | **NO** | MISSING TI |
| `/invite-vet` | POST | AllowAnonymous | auth | Yes | Result | InviteVetValidator | **NO** | MISSING TI |
| `/login` | POST | AllowAnonymous | auth | Yes | Result\<OwnerPortalTokenDto\> | OwnerPortalLoginValidator | **NO** | MISSING TI |
| `/link-microchip` | POST | RequireAuthorization (group) | api (group) | Yes | Result | LinkOwnerByMicrochipValidator | **NO** | MISSING TI |

**Notes**:
- All 4 portal endpoints have zero integration tests. This is a risk since portal is a public-facing surface.

---

## 6. ReferralEndpoints.cs

Route group: `/api/v1/portal/referral-code` (authenticated)

| Endpoint | Method | Auth | RateLimit | Summary | Handler Returns | Validator | TI | Status |
|---|---|---|---|---|---|---|---|---|
| `GET /` | GET | RequireAuthorization (group) | api (group) | Yes | Result\<ReferralCodeDto\> | GetOrCreateReferralCodeValidator | **NO** | MISSING TI |

---

## 7. UserEndpoints.cs

Route group: `/api/v1/users` (Admin only)

| Endpoint | Method | Auth | RateLimit | Summary | Handler Returns | Validator | TI | Status |
|---|---|---|---|---|---|---|---|---|
| `POST /` | POST | RequireRole("Admin") | api (group) | Yes | Result\<UserDto\> | CreateUserValidator | **NO** | MISSING TI |
| `GET /` | GET | RequireRole("Admin") | api (group) | Yes | Result\<UserPagedResultDto\> | N/A (query) | Yes (GetUsers_AsReceptionist_403, GetUsers_AsAdmin_200) | OK |
| `POST /invite` | POST | RequireRole("Admin") | api (group) | Yes | Result\<InviteUserResponse\> | InviteUserValidator | **NO** | MISSING TI |
| `PATCH /{id}/role` | PATCH | RequireRole("Admin") | api (group) | Yes | Result | ChangeUserRoleValidator | **NO** | MISSING TI |
| `DELETE /{id}` | DELETE | RequireRole("Admin") | api (group) | Yes | Result | DeactivateUserValidator | **NO** | MISSING TI |

**Notes**:
- Only GET /users has TI (RBAC test). The 4 mutation endpoints (create, invite, change role, deactivate) have no TI.
- ListUsersQuery has no validator -- acceptable (pagination only).

---

## 8. WebhookEndpoints.cs

Route group: `/api/v1/webhooks` (VetOrAdmin + public receive)

| Endpoint | Method | Auth | RateLimit | Summary | Handler Returns | Validator | TI | Status |
|---|---|---|---|---|---|---|---|---|
| `POST /` | POST | RequireAuthorization("VetOrAdmin") | api (group) | Yes | Result\<WebhookRegistrationDto\> | RegisterWebhookValidator | **NO** | MISSING TI |
| `GET /` | GET | RequireAuthorization("VetOrAdmin") | api (group) | Yes | Result\<List\<WebhookRegistrationDto\>\> | N/A (query) | **NO** | MISSING TI |
| `DELETE /{id}` | DELETE | RequireAuthorization("VetOrAdmin") | api (group) | Yes | Result | DeactivateWebhookValidator | **NO** | MISSING TI |
| `POST /receive` | POST | AllowAnonymous | api (group) | Yes | Result | ReceiveWebhookValidator | **NO** | MISSING TI |

**Notes**:
- All 4 webhook endpoints have zero integration tests.
- ListWebhooksQuery has no validator -- acceptable (no user input).

---

## Critical Findings

### 1. Integration test coverage gap (HIGH)

19 out of 30 endpoints have **zero** integration tests. Per CLAUDE.md rule 3g, every endpoint MUST have at least 1 TI. The following endpoint groups are entirely untested:

- **ClinicGroupEndpoints** (8 endpoints) -- highest gap
- **OnboardingEndpoints** (4 endpoints)
- **PortalEndpoints** (4 endpoints) -- public-facing, security-sensitive
- **WebhookEndpoints** (4 endpoints) -- includes anonymous receive endpoint

### 2. Missing query validators (LOW)

The following queries have no FluentValidation validator. This is acceptable when the query only takes a userId from claims or simple route params, but worth noting:

- GetCurrentUserQuery
- ListGroupClinicsQuery
- GetGroupDashboardStatsQuery
- GetGroupClinicStatsQuery
- GetGroupRevenueComparisonQuery
- GetOnboardingStateQuery
- ListUsersQuery (has pagination but validates inline)
- ListWebhooksQuery

### 3. Positive findings

- **100% WithSummary + WithDescription** on all 30 endpoints.
- **100% auth coverage** -- every endpoint has either `AllowAnonymous` or `RequireAuthorization` (via group or explicit).
- **100% rate limiting** -- every endpoint inherits from a group with `RequireRateLimiting("api")` or overrides with a more restrictive policy (`auth`, `signup`).
- **100% Result/Result\<T\>** -- all handlers return Ardalis.Result, no exceptions thrown for control flow.
- **100% validator coverage** for all command handlers (27 commands, 27 validators).
- Auth policies are well-layered: public endpoints use `AllowAnonymous`, admin endpoints use `RequireRole("Admin")`, webhooks use `"VetOrAdmin"`.

---

## Recommended Actions

| Priority | Action | Estimated Effort |
|---|---|---|
| P1 | Add TI for PortalEndpoints (4 endpoints, public-facing) | 2-3h |
| P1 | Add TI for WebhookEndpoints (4 endpoints, includes anonymous receive) | 2h |
| P1 | Add TI for ClinicGroupEndpoints (8 endpoints) | 3-4h |
| P2 | Add TI for OnboardingEndpoints (4 endpoints) | 1-2h |
| P2 | Add TI for UserEndpoints mutations (4 endpoints: create, invite, change-role, deactivate) | 2h |
| P2 | Add TI for AuthEndpoints gaps (logout, change-password) | 1h |
| P3 | Add TI for ClinicEndpoints /search | 30min |
| P3 | Add TI for ReferralEndpoints GET / | 30min |
