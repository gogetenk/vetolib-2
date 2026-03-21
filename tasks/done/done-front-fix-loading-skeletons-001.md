# todo-front-fix-loading-skeletons-001 — Replace text-only loading states with Skeletons

**Module** : Frontend
**Severity** : HIGH
**Skills** : `shadcn-nextjs`
**MSW** : non

---

## Problem

Several components show plain text (`"Loading..."`, `"Loading invoices..."`) during async data fetching
instead of Skeleton components. This causes layout shift and a poor perceived performance.
The shadcn `<Skeleton>` component is already installed and used correctly in `StatsCards` and
`AnalyticsSection` — the pattern just needs to be applied consistently.

## Affected files

### `src/components/features/billing/InvoiceDetail.tsx` (line 174)
```tsx
// Current — text-only
return <p className="text-sm text-muted-foreground" data-testid="invoice-detail-loading">Loading...</p>

// Expected — layout-matching skeleton
return (
  <div className="space-y-6" data-testid="invoice-detail-loading">
    <Skeleton className="h-32 w-full rounded-lg" />
    <Skeleton className="h-24 w-full rounded-lg" />
    <Skeleton className="h-48 w-full rounded-lg" />
  </div>
)
```

### `src/components/features/billing/InvoiceTable.tsx` (line 107)
```tsx
// Current — text only
<p className="text-sm text-muted-foreground" data-testid="invoices-loading">Loading invoices...</p>

// Expected — table skeleton rows (3 rows)
```

### `src/components/features/appointments/AppointmentsTable.tsx` (line 221–225)
```tsx
// Current — single centred "Loading..." cell
<TableRow>
  <TableCell colSpan={columns.length} className="text-center py-8">
    <span data-testid="loading-indicator">Loading...</span>
  </TableCell>
</TableRow>

// Expected — 3 skeleton TableRows matching column widths
```

### `src/app/[locale]/(dashboard)/settings/team/page.tsx` (line 99)
```tsx
// Current — text only
<div data-testid="team-loading" className="text-sm text-muted-foreground">{t('loading')}</div>

// Expected — skeleton rows representing the team table
```

## Fix

1. Import `Skeleton` from `@/components/ui/skeleton` in all four files
2. Replace text-only loading states with Skeleton layouts that approximate the eventual content shape
3. Keep `data-testid` attributes on the loading containers

## Acceptance criteria

```
□ InvoiceDetail loading → 3 card-shaped Skeletons
□ InvoiceTable loading → 3 skeleton rows inside the table
□ AppointmentsTable loading → 3 skeleton TableRows
□ TeamPage loading → skeleton rows matching the table
□ No "Loading..." plain text visible during data fetch
□ npm run build → 0 errors
```
