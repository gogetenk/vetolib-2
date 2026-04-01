# Task: Create portal profile page (currently 404)

**Module:** Frontend — Portal
**Priority:** HIGH (users see Profile tab → 404)
**Source:** docs/audits/qa-night-frontend-pages-20260401.md — 2.3
[MSW: oui]

## Problem
PortalLayout.tsx navigation includes a "profile" tab pointing to `${basePath}/profile`, but NO page.tsx exists. Portal users get a 404.

## Fix
Create src/frontend/src/app/[locale]/portal/[clinicSlug]/profile/page.tsx
- Show owner account info (name, email, phone)
- List linked pets
- Allow basic profile editing
- Use MSW handlers for data

## Skills
- `shadcn-nextjs`, `msw-mock-api`

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] `npm run lint` 0 errors
- [ ] page.tsx exists and renders
- [ ] data-testid on all interactive elements
- [ ] i18n via useTranslations
