# todo-front-predictive-health-001 -- Health Alerts Dashboard + Patient Alerts

**Module** : Frontend (AI section)
**Dependencies** : none (MSW-first)
**Priority** : HIGH
**Estimated** : 3-4 hours
**[MSW: oui]**
**[Branchement ulterieur]** : wire-ai-health-alerts

## Context

Display predictive health alerts on the vet dashboard and on individual patient pages.
Vet can dismiss, acknowledge, or convert alerts to appointments.
Develop entirely with MSW -- no backend dependency.

## Skills to read

- `skills/shadcn-nextjs/SKILL.md`
- `skills/msw-mock-api/SKILL.md`

## Spec

`docs/specs/PREDICTIVE-HEALTH-ALERTS-SPEC.md` (section 5 of the study for UI mockups)

## BDD

`tests/Vetolib.Tests.Acceptance/Features/AI/PredictiveHealthAlerts.feature`

## Scope

### 1. MSW Handlers

```typescript
// GET /api/v1/ai/health-alerts -> list of HealthAlertDto
// GET /api/v1/ai/health-alerts/patient/:patientId -> patient-specific alerts
// PATCH /api/v1/ai/health-alerts/:id/dismiss -> 200
// PATCH /api/v1/ai/health-alerts/:id/acknowledge -> 200
// POST /api/v1/ai/health-alerts/:id/convert-to-appointment -> AppointmentPreFillDto
```

Mock data: realistic UAE clinic names, Arabic/English pet owner names, realistic breeds.

### 2. API client functions

In `lib/api/health-alerts.ts`:
- `getHealthAlerts(filters?): Promise<HealthAlertDto[]>`
- `getPatientHealthAlerts(patientId): Promise<HealthAlertDto[]>`
- `dismissAlert(id, reason): Promise<void>`
- `acknowledgeAlert(id): Promise<void>`
- `convertToAppointment(id): Promise<AppointmentPreFillDto>`

### 3. Dashboard Alert Panel

Component: `HealthAlertPanel` on the main dashboard.
- Grouped by severity (High, Medium, Low)
- Each alert shows: patient name, breed, age, alert title, description
- Action buttons: [Schedule Appointment] [Acknowledge] [Dismiss]
- Badge with total alert count
- "View All" link to full alerts page
- `data-testid` on all interactive elements

### 4. Patient Health Alerts Tab

On the patient detail page, add a "Health Alerts" tab:
- Shows all alerts for this patient (including dismissed, greyed out)
- Same action buttons as dashboard
- Alert history (when dismissed, by whom, reason)

### 5. Dismiss Dialog

Modal with:
- Reason text field (required)
- Confirm / Cancel buttons

### 6. Convert to Appointment Flow

When clicking "Schedule Appointment":
- Call convert endpoint
- Navigate to appointment creation page with pre-filled data (patient, notes)

### 7. Playwright Tests

- Vet sees health alerts on dashboard
- Vet dismisses an alert (modal, reason, alert disappears)
- Vet acknowledges an alert (status changes, stays visible but deprioritized)
- No alerts shown for patient with up-to-date care
- Alert panel grouped by severity

## Completion criteria

- [ ] MSW handlers for all 5 endpoints
- [ ] API client functions in lib/api/
- [ ] HealthAlertPanel component on dashboard
- [ ] Patient health alerts tab
- [ ] Dismiss dialog with reason
- [ ] Convert to appointment navigation
- [ ] `data-testid` on all interactive elements
- [ ] Playwright tests (5+ scenarios)
- [ ] `npm run lint` GREEN
- [ ] `npm run build` GREEN
