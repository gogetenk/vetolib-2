# QA Audit: Agenda Module Endpoints

**Date**: 2026-04-01
**Scope**: All endpoint definitions in `src/backend/Modules/Agenda/Vetolib.Agenda/Api/*.cs`
**Auditor**: Claude QA Agent (batch 2)

---

## Summary

| Metric | Count |
|---|---|
| Endpoint files | 6 |
| Total versioned (v1) endpoints | 30 |
| Legacy (unversioned) endpoints | 6 |
| Endpoints with `.WithSummary()` + `.WithDescription()` | 24/30 |
| Endpoints missing metadata | 6 (all legacy aliases) |
| Endpoints with authorization | 30/30 |
| Endpoints with rate limiting | 30/30 |
| Handlers returning `Result<T>` or `Result` | 30/30 |
| Commands with FluentValidation validator | 17/17 (commands) + 1 query validator |
| Endpoints with integration test coverage | 20/30 |
| Endpoints MISSING integration test | 10 |

**Verdict: 3 CRITICAL gaps, 2 MODERATE gaps, 1 LOW issue.**

---

## Endpoint Files Audited

1. `AppointmentEndpoints.cs` — 14 v1 endpoints + 6 legacy aliases
2. `ConsultationTypeEndpoints.cs` — 4 endpoints
3. `WaitlistEndpoints.cs` — 3 endpoints
4. `FeedbackEndpoints.cs` — 3 endpoints
5. `FollowUpRuleEndpoints.cs` — 4 endpoints
6. `StaffScheduleEndpoints.cs` — 5 endpoints

---

## Detailed Audit per Endpoint

### 1. AppointmentEndpoints.cs

| # | Method | Route | Summary | Description | Auth | RateLimit | Handler | Validator | Integration Test |
|---|---|---|---|---|---|---|---|---|---|
| 1 | POST | `/api/v1/appointments` | YES | YES | Role(Admin,Vet,Receptionist) | YES (group) | CreateAppointmentHandler -> Result\<AppointmentDto\> | CreateAppointmentValidator | YES (2 tests) |
| 2 | GET | `/api/v1/appointments` | YES | YES | RequireAuthorization (group) | YES (group) | ListAppointmentsHandler -> Result\<AppointmentPagedResultDto\> | N/A (query) | YES (2 tests) |
| 3 | PATCH | `/api/v1/appointments/{id}/status` | YES | YES | RequireAuthorization (group) | YES (group) | UpdateAppointmentStatusHandler -> Result\<AppointmentDto\> | UpdateAppointmentStatusValidator | **NO** |
| 4 | PATCH | `/api/v1/appointments/{id}/transition` | YES | YES | RequireAuthorization (group) | YES (group) | (reuses UpdateAppointmentStatusHandler) | UpdateAppointmentStatusValidator | YES (2 tests) |
| 5 | GET | `/api/v1/appointments/{id}` | YES | YES | RequireAuthorization (group) | YES (group) | GetAppointmentByIdHandler -> Result\<AppointmentDto\> | N/A (query) | **NO** |
| 6 | PUT | `/api/v1/appointments/{id}` | YES | YES | RequireAuthorization (group) | YES (group) | EditAppointmentHandler -> Result\<AppointmentDto\> | EditAppointmentValidator | **NO** |
| 7 | GET | `/api/v1/appointments/availability` | YES | YES | RequireAuthorization (group) | YES (group) | GetAvailabilityHandler -> Result\<IReadOnlyList\<AvailabilitySlotDto\>\> | N/A (query) | **NO** |
| 8 | POST | `/api/v1/appointments/suggest-slot` | YES | YES | RequireAuthorization (group) | YES (group) | SuggestSlotHandler -> Result\<SlotSuggestionsResponse\> | SuggestSlotValidator | **NO** |
| 9 | POST | `/api/v1/appointments/series` | YES | YES | Role(Admin,Vet,Receptionist) | YES (group) | CreateAppointmentSeriesHandler -> Result\<List\<AppointmentDto\>\> | CreateAppointmentSeriesValidator | YES (2 tests) |
| 10 | DELETE | `/api/v1/appointments/series/{seriesId}` | YES | YES | Role(Admin,Vet,Receptionist) | YES (group) | CancelAppointmentSeriesHandler -> Result\<int\> | CancelAppointmentSeriesValidator | YES (2 tests) |
| 11 | PUT | `/api/v1/appointments/{id}/waiting-room` | YES | YES | Role(Admin,Receptionist) | YES (group) | MarkWaitingRoomHandler -> Result\<AppointmentDto\> | MarkWaitingRoomValidator | **NO** |
| 12 | GET | `/api/v1/appointments/waiting-room` | YES | YES | RequireAuthorization (group) | YES (group) | GetWaitingRoomHandler -> Result\<List\<WaitingRoomAppointmentDto\>\> | N/A (query) | **NO** |
| 13 | GET | `/api/v1/appointments/{id}/checkin-qr` | YES | YES | RequireAuthorization (group) | YES (group) | GenerateCheckInQrHandler -> Result\<CheckInQrPayloadDto\> | GenerateCheckInQrValidator | YES (2 tests) |
| 14 | POST | `/api/v1/appointments/checkin` | YES | YES | RequireAuthorization (group) | YES (group) | CheckInFromQrHandler -> Result\<AppointmentDto\> | CheckInFromQrValidator | YES (2 tests) |

**Legacy aliases (lines 111-123)** — 6 endpoints under `/api/appointments/`:

| # | Method | Route | Summary | Description | Auth | RateLimit | Integration Test |
|---|---|---|---|---|---|---|---|
| L1 | POST | `/api/appointments/` | NO | NO | Role(Admin,Vet,Receptionist) | YES (group) | NO |
| L2 | GET | `/api/appointments/` | NO | NO | RequireAuthorization (group) | YES (group) | NO |
| L3 | PATCH | `/api/appointments/{id}/transition` | NO | NO | RequireAuthorization (group) | YES (group) | NO |
| L4 | GET | `/api/appointments/availability` | NO | NO | RequireAuthorization (group) | YES (group) | NO |
| L5 | GET | `/api/appointments/{id}` | NO | NO | RequireAuthorization (group) | YES (group) | NO |
| L6 | PUT | `/api/appointments/{id}` | NO | NO | RequireAuthorization (group) | YES (group) | NO |

### 2. ConsultationTypeEndpoints.cs

| # | Method | Route | Summary | Description | Auth | RateLimit | Handler | Validator | Integration Test |
|---|---|---|---|---|---|---|---|---|---|
| 1 | POST | `/api/v1/consultation-types` | YES | YES | Role(Admin) | YES (group) | CreateConsultationTypeHandler -> Result\<ConsultationTypeDto\> | CreateConsultationTypeValidator | **NO** |
| 2 | PUT | `/api/v1/consultation-types/{id}` | YES | YES | Role(Admin) | YES (group) | UpdateConsultationTypeHandler -> Result\<ConsultationTypeDto\> | UpdateConsultationTypeValidator | **NO** |
| 3 | DELETE | `/api/v1/consultation-types/{id}` | YES | YES | Role(Admin) | YES (group) | DeactivateConsultationTypeHandler -> Result | DeactivateConsultationTypeValidator | **NO** |
| 4 | GET | `/api/v1/consultation-types` | YES | YES | Role(Admin,Vet,Receptionist) | YES (group) | ListConsultationTypesHandler -> Result\<IReadOnlyList\<ConsultationTypeDto\>\> | N/A (query) | **NO** |

### 3. WaitlistEndpoints.cs

| # | Method | Route | Summary | Description | Auth | RateLimit | Handler | Validator | Integration Test |
|---|---|---|---|---|---|---|---|---|---|
| 1 | POST | `/api/v1/waitlist` | YES | YES | RequireAuthorization (group) | YES (group) | AddToWaitlistHandler -> Result\<WaitlistEntryDto\> | AddToWaitlistValidator | YES (2 tests) |
| 2 | GET | `/api/v1/waitlist` | YES | YES | Role(Admin,Vet,Receptionist) | YES (group) | ListWaitlistEntriesHandler -> Result\<WaitlistPagedResultDto\> | N/A (query) | YES (2 tests) |
| 3 | DELETE | `/api/v1/waitlist/{id}` | YES | YES | RequireAuthorization (group) | YES (group) | RemoveFromWaitlistHandler -> Result | RemoveFromWaitlistValidator | YES (2 tests) |

### 4. FeedbackEndpoints.cs

| # | Method | Route | Summary | Description | Auth | RateLimit | Handler | Validator | Integration Test |
|---|---|---|---|---|---|---|---|---|---|
| 1 | POST | `/api/v1/appointments/{id}/feedback` | YES | YES | RequireAuthorization (group) | YES (group) | SubmitVisitFeedbackHandler -> Result\<VisitFeedbackDto\> | SubmitVisitFeedbackValidator | YES (2 tests) |
| 2 | GET | `/api/v1/feedback` | YES | YES | Role(Admin) | YES (group) | ListVisitFeedbackHandler -> Result\<List\<VisitFeedbackDto\>\> | N/A (query) | YES (3 tests) |
| 3 | GET | `/api/v1/feedback/stats` | YES | YES | Role(Admin) | YES (group) | GetVisitFeedbackStatsHandler -> Result\<VisitFeedbackStatsDto\> | N/A (query) | YES (2 tests) |

### 5. FollowUpRuleEndpoints.cs

| # | Method | Route | Summary | Description | Auth | RateLimit | Handler | Validator | Integration Test |
|---|---|---|---|---|---|---|---|---|---|
| 1 | GET | `/api/v1/follow-up-rules` | YES | YES | Role(Admin) | YES (group) | ListFollowUpRulesHandler -> Result\<List\<FollowUpRuleDto\>\> | N/A (query) | **NO** |
| 2 | POST | `/api/v1/follow-up-rules` | YES | YES | Role(Admin) | YES (group) | CreateFollowUpRuleHandler -> Result\<FollowUpRuleDto\> | CreateFollowUpRuleValidator | **NO** |
| 3 | PUT | `/api/v1/follow-up-rules/{id}` | YES | YES | Role(Admin) | YES (group) | UpdateFollowUpRuleHandler -> Result\<FollowUpRuleDto\> | UpdateFollowUpRuleValidator | **NO** |
| 4 | DELETE | `/api/v1/follow-up-rules/{id}` | YES | YES | Role(Admin) | YES (group) | DeactivateFollowUpRuleHandler -> Result | DeactivateFollowUpRuleValidator | **NO** |

### 6. StaffScheduleEndpoints.cs

| # | Method | Route | Summary | Description | Auth | RateLimit | Handler | Validator | Integration Test |
|---|---|---|---|---|---|---|---|---|---|
| 1 | GET | `/api/v1/schedule` | YES | YES | RequireAuthorization (group) | YES (group) | ListStaffSchedulesHandler -> Result\<List\<StaffScheduleDto\>\> | N/A (query) | **NO** |
| 2 | GET | `/api/v1/schedule/me` | YES | YES | RequireAuthorization (group) | YES (group) | GetMyScheduleHandler -> Result\<List\<StaffScheduleDto\>\> | N/A (query) | **NO** |
| 3 | POST | `/api/v1/schedule` | YES | YES | Role(Admin) | YES (group) | CreateStaffScheduleHandler -> Result\<StaffScheduleDto\> | CreateStaffScheduleValidator | **NO** |
| 4 | DELETE | `/api/v1/schedule/{id}` | YES | YES | Role(Admin) | YES (group) | DeleteStaffScheduleHandler -> Result | DeleteStaffScheduleValidator | **NO** |
| 5 | GET | `/api/v1/schedule/availability` | YES | YES | RequireAuthorization (group) | YES (group) | GetStaffAvailabilityHandler -> Result\<List\<StaffScheduleDto\>\> | N/A (query) | **NO** |

---

## Findings

### CRITICAL (violates CLAUDE.md rule 3g)

**C1. ConsultationTypeEndpoints: 0/4 endpoints have integration tests.**
All 4 endpoints (CRUD) have zero TI coverage. Per rule 3g, each endpoint MUST have at least 1 integration test.

**C2. FollowUpRuleEndpoints: 0/4 endpoints have integration tests.**
All 4 endpoints (CRUD) have zero TI coverage.

**C3. StaffScheduleEndpoints: 0/5 endpoints have integration tests.**
All 5 endpoints have zero TI coverage.

### MODERATE

**M1. AppointmentEndpoints: 7/14 versioned endpoints missing integration tests.**
The following v1 endpoints have no TI:
- `PATCH /api/v1/appointments/{id}/status` (UpdateStatus)
- `GET /api/v1/appointments/{id}` (GetAppointmentById)
- `PUT /api/v1/appointments/{id}` (EditAppointment)
- `GET /api/v1/appointments/availability` (GetAvailability)
- `POST /api/v1/appointments/suggest-slot` (SuggestSlot)
- `PUT /api/v1/appointments/{id}/waiting-room` (MarkWaitingRoom)
- `GET /api/v1/appointments/waiting-room` (GetWaitingRoomList)

**M2. Legacy endpoints missing `.WithSummary()` / `.WithDescription()` / `.WithName()`.**
The 6 legacy alias endpoints under `/api/appointments/` (lines 116-122) have no OpenAPI metadata. While they are aliases of v1 endpoints, they will appear as undocumented in Swagger.

### LOW

**L1. Legacy endpoints should be deprecated or removed.**
The unversioned `/api/appointments/` group (lines 111-123) exists "for BDD step definitions and older clients". If BDD tests have been migrated to v1 routes, these should be removed to reduce surface area. If still needed, they should at minimum have `.WithTags("Appointments (deprecated)")`.

---

## Positive Findings

- All 24 versioned endpoints have `.WithSummary()` and `.WithDescription()` -- well done.
- All endpoint groups use `.RequireAuthorization()` at the group level, with role-specific policies on write operations.
- All endpoint groups use `.RequireRateLimiting("api")`.
- Every handler returns `Result<T>` or `Result` (Ardalis.Result) -- no exceptions used for control flow.
- Every command has a FluentValidation validator (17 commands, 17 validators + 1 query validator for SuggestSlot).
- All endpoint methods delegate to MediatR via `sender.Send()` and map results via `.ToMinimalApiResult()`.
- No controller-based endpoints found (Minimal APIs only, per CLAUDE.md rule 5).
- The `UpdateAppointmentStatusRawRequest` pattern (line 178) gracefully handles invalid enum strings instead of relying on JSON deserialization (which would produce 500).

---

## Recommended Actions (priority order)

1. **Create integration test files** for the 3 endpoint groups with zero coverage:
   - `tests/Vetolib.Tests.Integration/Agenda/ConsultationTypeEndpointsTests.cs`
   - `tests/Vetolib.Tests.Integration/Agenda/FollowUpRuleEndpointsTests.cs`
   - `tests/Vetolib.Tests.Integration/Agenda/StaffScheduleEndpointsTests.cs`

2. **Add missing TI tests** to `AppointmentEndpointsTests.cs` for the 7 uncovered v1 endpoints.

3. **Add OpenAPI metadata** to legacy endpoints or deprecate/remove them.

---

## Test Coverage Matrix

| Endpoint Group | Endpoints | With TI | Coverage |
|---|---|---|---|
| AppointmentEndpoints (v1) | 14 | 7 | 50% |
| AppointmentEndpoints (legacy) | 6 | 0 | 0% |
| ConsultationTypeEndpoints | 4 | 0 | **0%** |
| WaitlistEndpoints | 3 | 3 | 100% |
| FeedbackEndpoints | 3 | 3 | 100% |
| FollowUpRuleEndpoints | 4 | 0 | **0%** |
| StaffScheduleEndpoints | 5 | 0 | **0%** |
| **TOTAL (v1 only)** | **33** | **13** | **39%** |
