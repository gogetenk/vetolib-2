# todo-front-fix-error-states-001 — Improve error states across dashboard pages

**Module** : Frontend
**Severity** : MEDIUM
**Skills** : `shadcn-nextjs`
**MSW** : non

---

## Problem

Error states across the dashboard are inconsistent and lack actionability:

1. `StatsCards` — error is a plain `<div className="text-destructive text-sm">` with no retry option
2. `AppointmentsTable` — errors are silently swallowed (`catch { // silent }`) — no feedback at all
3. `PatientsPage` — errors are silently swallowed (`catch { setPatients([]) }`) — user sees an
   empty state but doesn't know if it's a real empty or a failed fetch
4. `TeamPage` — errors are silently swallowed (`catch { // silent }`)
5. `InvoiceDetail` — shows an action error inline (`data-testid="action-error"`) as a small
   plain text string; no icon, no dismiss button

## Fix

### Pattern to follow (consistent across all pages):

```tsx
// Error state with retry
{error && (
  <div
    role="alert"
    data-testid="page-error"
    className="flex items-center gap-3 rounded-md border border-destructive/50 bg-destructive/10 px-4 py-3 text-sm text-destructive"
  >
    <AlertTriangle className="h-4 w-4 shrink-0" />
    <span>{error}</span>
    <button
      onClick={retry}
      className="ml-auto text-sm underline hover:no-underline"
      data-testid="retry-btn"
    >
      Retry
    </button>
  </div>
)}
```

### Changes required:

1. **`AppointmentsTable`** — add `const [error, setError] = useState<string | null>(null)`.
   Catch errors in `load()` with `setError('Failed to load appointments')`.
   Show error banner above the table with a retry button that calls `load()`.
   `data-testid="appointments-error"`.

2. **`PatientsPage`** — add error state. On catch: `setError('Failed to load patients')` instead
   of silently setting `setPatients([])`. Show error banner above the grid.
   `data-testid="patients-error"`.

3. **`TeamPage`** — add error state in `loadUsers`. Show error banner.
   `data-testid="team-error"`.

4. **`StatsCards`** — replace plain text error div with the styled banner pattern above.
   `data-testid="stats-error"` (keep existing testid).

5. **`InvoiceDetail` action errors** — show action errors using `toast.error()` (already
   used in the component for some actions) and remove the inline `<p data-testid="action-error">`.
   Keep `data-testid="action-error"` as a visually hidden alert for test accessibility if needed.

## Acceptance criteria

```
□ AppointmentsTable shows styled error banner + retry on fetch failure
□ PatientsPage shows styled error banner + retry on fetch failure
□ TeamPage shows styled error banner + retry on fetch failure
□ StatsCards error uses the styled banner (not plain text)
□ InvoiceDetail action errors use toast.error() (no inline plain text)
□ All error elements have role="alert"
□ npm run build → 0 errors
```
