# Task: Create e-reporting page for UAE government compliance

**Module:** Frontend — Dashboard/Billing
**Priority:** MEDIUM (regulatory)
**Source:** docs/audits/qa-night-frontend-pages-20260401.md — 3.3
[MSW: oui]

## Problem
Backend has e-reporting endpoints for UAE government reporting but no frontend UI.

## Fix
Create src/frontend/src/app/[locale]/(dashboard)/billing/reporting/page.tsx
- List of reporting periods
- Submit report action
- Status tracking (pending, submitted, accepted, rejected)
- Use MSW handlers for data

## Skills
- `shadcn-nextjs`, `msw-mock-api`

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] `npm run lint` 0 errors
- [ ] E-reporting page renders with period list + submit
- [ ] data-testid on all interactive elements
- [ ] i18n via useTranslations
