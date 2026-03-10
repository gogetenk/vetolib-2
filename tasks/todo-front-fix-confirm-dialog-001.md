# todo-front-fix-confirm-dialog-001 — Replace window.confirm() with shadcn Dialog

**Module** : Frontend
**Severity** : HIGH
**Skills** : `shadcn-nextjs`
**MSW** : non

---

## Problem

`InvoiceDetail.handleDelete` uses the native browser `window.confirm()` dialog for a
destructive action (invoice deletion). This is:
- Visually inconsistent with the rest of the app (shadcn Dialog is used everywhere else)
- Cannot be styled or translated
- Blocks the browser thread
- Does not have a `data-testid` (untestable by Playwright)

The correct pattern is already implemented in `AppointmentDetail` which uses a shadcn
`<Dialog>` with a confirm button for all transitions.

## Affected file

`src/components/features/billing/InvoiceDetail.tsx`

Line 143:
```tsx
if (!confirm('Delete this invoice? This action cannot be undone.')) return
```

## Fix

Replace `window.confirm()` with a shadcn Dialog confirmation:

1. Add state: `const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)`
2. Replace the `confirm()` guard with `setDeleteDialogOpen(true)` and return early
3. Add a `<Dialog>` at the bottom of the component with:
   - `data-testid="delete-confirm-dialog"`
   - Title: "Delete Invoice"
   - Description: "This action cannot be undone. The invoice will be permanently deleted."
   - Cancel button: `data-testid="delete-confirm-cancel"`
   - Confirm button (destructive): `data-testid="delete-confirm-ok"`, triggers the actual delete logic
4. Move the delete API call + router.push into the dialog confirm handler

## Acceptance criteria

```
□ window.confirm() removed from codebase
□ Delete button opens shadcn Dialog (not native alert)
□ Dialog has data-testid="delete-confirm-dialog"
□ Confirm button has data-testid="delete-confirm-ok"
□ Cancel closes dialog without deleting
□ Successful delete still navigates to /billing
□ npm run build → 0 errors
```
