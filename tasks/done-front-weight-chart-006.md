# todo-front-weight-chart-006.md -- Frontend: weight history tab with chart

**Module** : Frontend (MedicalRecords)
**Priority** : Haute
**Dependencies** : todo-back-weight-history-004
**Skills** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**
**[Branchement ulterier: wire-weight-history]**

## Context

The backend weight history feature (task 004) exposes 3 endpoints. The frontend needs a Weight tab on the patient detail page with a line chart, history table, and add weight dialog.

## Scope

### 1. Install recharts

```bash
cd src/frontend && npm install recharts
```

### 2. MSW handlers for weight endpoints

```
POST /api/v1/patients/:id/weights
GET  /api/v1/patients/:id/weights
GET  /api/v1/patients/:id/weights/curve
```

Mock realistic data: Arabian Horse weights (420-460 kg range), multiple entries over months.

### 3. Weight tab on patient detail page

Add "Weight" tab to patient detail page tabs (alongside Overview, Medical Records).

#### Weight tab content:

- **Current weight** (large, prominent display)
- **Weight curve chart** (Recharts LineChart, x=date, y=kg)
  - Responsive container
  - Tooltip on hover
  - Grid lines
- **Weight history table** (date, weight, note, recorded by)
  - Sorted most recent first
  - Paginated if > 10 entries
- **"+ Add Weight" button** (visible only to VET/ADMIN)

### 4. Add Weight dialog

- Weight input (decimal, kg)
- Date picker (defaults to today)
- Optional note textarea (max 500 chars)
- Submit button
- Validation: weight > 0, weight <= 10000

### 5. data-testid

- `data-testid="weight-tab"`
- `data-testid="weight-current-value"`
- `data-testid="weight-chart"`
- `data-testid="weight-history-table"`
- `data-testid="weight-add-button"`
- `data-testid="weight-input"`
- `data-testid="weight-date-input"`
- `data-testid="weight-note-input"`
- `data-testid="weight-submit-button"`

## BDD

No .feature for frontend-only. Playwright tests during wire task.

## Completion criteria

- [ ] Weight tab on patient detail page
- [ ] Line chart with recharts (responsive, tooltip)
- [ ] Weight history table (sorted, paginated)
- [ ] Add weight dialog with validation
- [ ] MSW handlers for all 3 weight endpoints
- [ ] All interactive elements have `data-testid`
- [ ] `npm run lint` + `npm run build` GREEN
