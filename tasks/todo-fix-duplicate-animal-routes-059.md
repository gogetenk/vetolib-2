# Task: Remove duplicate animal detail route in portal

**Module:** Frontend — Portal
**Priority:** MEDIUM
**Source:** docs/audits/qa-night-frontend-pages-20260401.md — 2.2
[MSW: oui]

## Problem
Two separate routes serve animal detail in the portal:
- /portal/[clinicSlug]/animals/[id] — 34 data-testid, useTranslations directly in page
- /portal/[clinicSlug]/pets/[animalId] — delegates to PortalAnimalDetail component

This is confusing and likely legacy duplication.

## Fix
1. Determine which route is canonical (probably pets/[animalId] since it matches the nav)
2. Remove the other route or redirect it
3. Update any links pointing to the removed route

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] Only ONE animal detail route in portal
- [ ] All links point to the canonical route
