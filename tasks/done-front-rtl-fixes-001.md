# todo-front-rtl-fixes-001.md — Fix RTL/Bidi rendering bugs across all zones

**Dépendances** : aucune
**Skills** : shadcn-nextjs
**[MSW: oui]**

## Objectif
Fix all bidirectional text rendering bugs in RTL (Arabic) mode. These are critical blockers for UAE market launch.

## Bugs to fix

### 1. Date formatting garbled in RTL
- **Where**: Appointments list, Billing list, Stock list
- **Bug**: "Mar 2026, 01:00 PM 01" instead of "01 Mar 2026, 01:00 PM" — day number displaced to end
- **Fix**: Wrap all date strings in `<span dir="ltr">` or use `Intl.DateTimeFormat('ar-AE')` with explicit LTR direction marks
- **Files**: All table components that display dates

### 2. Phone numbers reversed in RTL
- **Where**: Patients list (AR view)
- **Bug**: "6789 345 52 971+" instead of "+971 52 345 6789"
- **Fix**: Apply `dir="ltr"` and `unicode-bidi: embed` on all phone number elements
- **Files**: Patient card component, patient detail component

### 3. Quantity+unit strings reversed in RTL
- **Where**: Stock list (AR view)
- **Bug**: "bottles 5" instead of "5 bottles", "tablets 120" instead of "120 tablets"
- **Fix**: Wrap quantity+unit in `<span dir="ltr">`
- **Files**: Stock list table component

### 4. Summary footer text broken in RTL
- **Where**: Billing list (AR view)
- **Bug**: ":invoices — Total filtered 3" with colon at beginning, number at end
- **Fix**: Use ICU MessageFormat or wrap in `dir="ltr"` span
- **Files**: Billing list footer component

### 5. "View all" arrow direction wrong in RTL
- **Where**: Dashboard
- **Bug**: Right arrow "→" used in RTL mode, should be left arrow "←"
- **Fix**: Conditionally flip arrow based on locale direction
- **Files**: Dashboard component

### 6. Header layout broken in RTL
- **Where**: Dashboard, Appointments (AR views)
- **Bug**: Clinic name misplaced next to user avatar instead of near Vetolib logo
- **Fix**: Check RTL mirroring of header component
- **Files**: Header/nav component

## Global approach
Consider creating a utility component or CSS class for LTR-embedded content:
```tsx
// components/ui/ltr-text.tsx
export function LtrText({ children, className }: { children: React.ReactNode; className?: string }) {
  return <span dir="ltr" className={className}>{children}</span>
}
```
Use it for: dates, phone numbers, email addresses, quantities with units, invoice numbers, financial amounts.

## Critère de complétion
- [ ] All dates render correctly in AR locale
- [ ] Phone numbers display correctly in AR locale
- [ ] Quantities display correctly in AR locale
- [ ] Summary text displays correctly in AR locale
- [ ] Arrow directions flip correctly in RTL
- [ ] Header layout is correct in RTL
- [ ] `npm run build` passes
