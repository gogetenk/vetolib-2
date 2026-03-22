# todo-back-predictive-health-003 -- API Endpoints + CQRS Handlers

**Module** : AI
**Dependencies** : todo-back-predictive-health-001 (domain + rules must exist)
**Priority** : HIGH
**Estimated** : 2-3 hours

## Context

Expose health alerts via Minimal API endpoints. Vet dashboard reads alerts, vet can dismiss,
acknowledge, or convert to appointment.

## Skills to read

- `skills/aspnet-minimal-api/SKILL.md`
- `skills/cqrs-mediatr/SKILL.md`
- `skills/ardalis-result/SKILL.md`

## Spec

`docs/specs/PREDICTIVE-HEALTH-ALERTS-SPEC.md` (section 6)

## BDD

`tests/Vetolib.Tests.Acceptance/Features/AI/PredictiveHealthAlerts.feature`

## Scope

### 1. Queries

**GetHealthAlertsQuery** -- GET /api/v1/ai/health-alerts
- Returns all non-dismissed alerts for the current clinic
- Filterable by: severity, status, patientId (query params)
- Sorted by severity (High first), then GeneratedAt (newest first)
- Returns `Result<IReadOnlyList<HealthAlertDto>>`

**GetPatientHealthAlertsQuery** -- GET /api/v1/ai/health-alerts/patient/{patientId}
- Returns all alerts for a specific patient (including dismissed for audit)
- Returns `Result<IReadOnlyList<HealthAlertDto>>`

### 2. Commands

**DismissHealthAlertCommand** -- PATCH /api/v1/ai/health-alerts/{id}/dismiss
- Request body: `{ "reason": "Owner declined screening" }`
- Calls `HealthAlert.Dismiss(reason, vetName)` (vetName from ClaimsPrincipal)
- Returns `Result`

**AcknowledgeHealthAlertCommand** -- PATCH /api/v1/ai/health-alerts/{id}/acknowledge
- Calls `HealthAlert.Acknowledge()`
- Returns `Result`

**ConvertAlertToAppointmentCommand** -- POST /api/v1/ai/health-alerts/{id}/convert-to-appointment
- Calls `HealthAlert.MarkScheduled(Guid.Empty)` -- actual appointment creation is frontend-driven
- Returns `Result<AppointmentPreFillDto>` with patient name, alert title, recommended action
- Frontend uses this data to navigate to appointment creation form

```csharp
public record AppointmentPreFillDto(
    Guid PatientId, string PatientName,
    string SuggestedNotes, string AlertTitle);
```

### 3. Validators (FluentValidation)

- DismissHealthAlertValidator: reason required, max 500 chars
- No validator needed for acknowledge (no body)

### 4. Endpoint registration

Add to existing `AIEndpoints.cs` -- new method `MapHealthAlertEndpoints()` called from
`MapAIApiEndpoints()`.

All endpoints under `/api/v1/ai/health-alerts`.
Auth: ClinicStaff for GET, VetOrAdmin for mutations.

### 5. Tests

- TU: Handler unit tests (dismiss already dismissed -> error, acknowledge already acknowledged, etc.)
- TI: 1 integration test per endpoint (6 total) -- contract testing with Testcontainers

## Completion criteria

- [ ] 2 query handlers + 3 command handlers
- [ ] FluentValidation on dismiss
- [ ] 6 Minimal API endpoints registered
- [ ] All endpoints use `ToMinimalApiResult()`
- [ ] Unit tests for handler edge cases
- [ ] Integration tests (1 per endpoint)
- [ ] `dotnet build` GREEN
- [ ] `dotnet test` GREEN
