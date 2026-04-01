# Task: Create clinic group management page

**Module:** Frontend — Dashboard/Settings
**Priority:** MEDIUM
**Source:** docs/audits/qa-night-frontend-pages-20260401.md — 3.2
[MSW: oui]

## Problem
Backend has full ClinicGroup CRUD + dashboard stats. Frontend only has ClinicSwitcher (header dropdown). No management UI for group admins.

## Fix
Create src/frontend/src/app/[locale]/(dashboard)/settings/clinic-group/page.tsx
- Show group info (name, clinics list)
- Add/remove clinics from group
- Group-level stats (total patients, revenue comparison)
- Only visible to Admin role
- Use MSW handlers for data

## Skills
- `shadcn-nextjs`, `msw-mock-api`

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] `npm run lint` 0 errors
- [ ] Clinic group management page renders
- [ ] data-testid on all interactive elements
- [ ] i18n via useTranslations
