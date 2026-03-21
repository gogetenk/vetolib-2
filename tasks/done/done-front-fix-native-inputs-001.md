# todo-front-fix-native-inputs-001 — Replace native HTML inputs with shadcn components

**Module** : Frontend
**Severity** : HIGH
**Skills** : `shadcn-nextjs`
**MSW** : non

---

## Problem

Two components use raw HTML form controls instead of shadcn equivalents, causing visual
inconsistency (different focus rings, hover states, height, border radius) and missing RTL support.

## Affected files

### 1. `src/components/features/appointments/AppointmentsTable.tsx` — date filter (line 183)

```tsx
// Current — raw <input type="date"> with hand-rolled className
<input
  type="date"
  className="h-9 rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm"
  data-testid="date-filter"
  ...
/>
```

Should be replaced by shadcn `<Input>` from `@/components/ui/input`:
```tsx
<Input
  type="date"
  className="w-40"
  data-testid="date-filter"
  ...
/>
```

### 2. `src/components/features/billing/InvoiceTable.tsx` — status filter (line 91)

```tsx
// Current — native <select>
<select
  data-testid="status-filter"
  value={statusFilter}
  onChange={(e) => setStatusFilter(e.target.value as InvoiceStatus | '')}
  className="rounded-md border border-input bg-background px-3 py-1.5 text-sm"
>
  ...
</select>
```

Should be replaced by shadcn `<Select>` (already imported in other files):
```tsx
<Select value={statusFilter} onValueChange={(val) => setStatusFilter(val as InvoiceStatus | '')}>
  <SelectTrigger className="w-48" data-testid="status-filter">
    <SelectValue placeholder="All statuses" />
  </SelectTrigger>
  <SelectContent>
    {STATUS_OPTIONS.map((opt) => (
      <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
    ))}
  </SelectContent>
</Select>
```

Note: SelectItem `value` cannot be an empty string — use `"ALL"` as sentinel value (same
pattern as AppointmentsTable) and convert on query: `statusFilter !== 'ALL' ? statusFilter : undefined`.

## Acceptance criteria

```
□ AppointmentsTable date filter uses shadcn <Input type="date">
□ InvoiceTable status filter uses shadcn <Select>
□ Both have matching data-testid attributes
□ Visual appearance consistent with AppointmentsTable status filter
□ npm run build → 0 errors
```
