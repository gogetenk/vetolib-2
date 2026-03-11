# todo-front-fix-accessibility-001 — WCAG AA accessibility gaps

**Module** : Frontend
**Severity** : MEDIUM
**Skills** : `shadcn-nextjs`
**MSW** : non

---

## Problem

Several accessibility issues found during audit that prevent WCAG AA compliance:

### 1. `UserMenu` — missing `aria-label` on trigger button

`UserMenu.tsx` line 87: The trigger `<button>` has `aria-expanded` and `aria-haspopup` but
no `aria-label`. Screen readers announce it as just "button". Should be `aria-label="User menu"`.

### 2. `AppointmentsTable` — check-in button in TodayAppointments has no loading label

`TodayAppointments.tsx` line 169:
```tsx
{checkingIn === appt.id ? '...' : 'Check In'}
```
Three dots `'...'` are meaningless to screen readers. Should be:
```tsx
{checkingIn === appt.id ? 'Checking in...' : 'Check In'}
```
And add `aria-busy={checkingIn === appt.id}` on the button.

### 3. `PatientDetailPage` — tabs missing `aria-controls`

The custom tab buttons at `/patients/[id]/page.tsx` lines 318–333 have `role="tab"` and
`aria-selected` but are missing `aria-controls` pointing to their panel IDs. Panels have
`role="tabpanel"` but no `id` attribute. Required for keyboard navigation spec compliance.

Fix:
```tsx
// Tab button
<button
  id={`tab-${tab.id}`}
  aria-controls={`panel-${tab.id}`}
  ...
>

// Tab panel
<div
  id={`panel-${tab.id}`}
  role="tabpanel"
  aria-labelledby={`tab-${tab.id}`}
  ...
>
```

### 4. `AnalyticsSection` — charts have no text alternative

`AnalyticsSection.tsx` — Recharts `<PieChart>` and `<BarChart>` have no accessible text
alternative. Screen readers see nothing. Each chart container should have:
```tsx
<div role="img" aria-label="Patients by species: 45% Dog, 30% Cat, ..." ...>
```
For the pie chart, generate the aria-label from `speciesData`.
For the bar chart, add `aria-label={t('revenue_by_month')}` on the container.

### 5. `PatientCard` — "View Record" button is the only interactive element but card is not keyboard-navigable as a unit

`PatientCard.tsx` — The card has a `hover:shadow-md` effect suggesting it's clickable, but
only the inner "View Record" `<Button>` is focusable. Either:
- Make the entire card a `<Link>` wrapper (simplest)
- Or keep as-is but ensure `View Record` button is the clear CTA (acceptable — just remove `hover:shadow-md` from the card to avoid implying the whole card is clickable)

Recommended: remove `hover:shadow-md` from the card, let the Button provide the hover affordance.

## Acceptance criteria

```
□ UserMenu trigger has aria-label="User menu"
□ Check-in loading state says "Checking in..." with aria-busy
□ Patient detail tabs have aria-controls + panel ids
□ Charts have role="img" with descriptive aria-label
□ PatientCard hover shadow removed (or card made fully clickable)
□ npm run build → 0 errors
```
