# Task: Add role-based filtering to desktop header navigation

**Module:** Frontend — Shell
**Priority:** HIGH (UX audit critical finding)
**Source:** docs/audits/navigation-ux-audit-20260401.md
[MSW: oui]

## Problem
Desktop header shows ALL nav items to all users regardless of role. A receptionist sees "Breeding", "AI Health Alerts", etc. even without access. The sidebar already has role filtering but the header does not.

## Fix
1. Read the Sidebar component to see how role filtering works (roles: ["VET", "ADMIN"] etc.)
2. Apply the same filtering logic to the Header/top-nav component
3. Ensure Header and Sidebar show the same items for the same role

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] Header nav items filtered by user role (same as sidebar)
- [ ] Receptionist does not see VET/ADMIN-only items in header
