# Task: Create waitlist management page

**Module:** Frontend — Dashboard
**Priority:** MEDIUM
**Source:** docs/audits/qa-night-frontend-pages-20260401.md — 3.5
[MSW: oui]

## Problem
Backend has full waitlist CRUD (AddToWaitlist, RemoveFromWaitlist, ListWaitlistEntries) but no frontend UI exists.

## Fix
Create src/frontend/src/app/[locale]/(dashboard)/waitlist/page.tsx
- Table of waitlist entries (patient name, reason, preferred time, priority, added date)
- Add to waitlist button (form: patient select, preferred day/time, reason)
- Remove from waitlist action
- Link from sidebar navigation
- Use MSW handlers for data

## Skills
- `shadcn-nextjs`, `msw-mock-api`

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] `npm run lint` 0 errors
- [ ] Waitlist page renders with table + add/remove actions
- [ ] data-testid on all interactive elements
- [ ] i18n via useTranslations
- [ ] Sidebar nav updated with waitlist link
