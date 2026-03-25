# todo-front-color-tokens-001 -- Fix hardcoded hex colors to CSS tokens

**Module** : Frontend
**Priority** : Critique (UX audit P0)
**Dependencies** : none

## Context

Several pages use hardcoded hex colors instead of Tailwind/shadcn CSS tokens.
This breaks dark mode and design consistency.

## Files to fix

- `src/frontend/src/app/[locale]/(dashboard)/patients/page.tsx` (PatientsPageClient)
- `src/frontend/src/app/[locale]/(dashboard)/stock/page.tsx` (StockPageClient)
- `src/frontend/src/app/[locale]/(dashboard)/settings/page.tsx` (SettingsPageClient)
- `src/frontend/src/components/features/billing/InvoiceTable.tsx`

## Rules

- Replace all `#xxxxxx` hex colors with Tailwind classes (text-muted-foreground, bg-card, border, etc.)
- Replace hardcoded English strings in InvoiceTable status options with i18n keys
- Keep all `data-testid` attributes
- `npm run lint` + `npm run build` GREEN

## Completion criteria

- [ ] Zero hardcoded hex colors in the 4 files
- [ ] InvoiceTable status strings use next-intl
- [ ] `npm run lint` GREEN
- [ ] `npm run build` GREEN
