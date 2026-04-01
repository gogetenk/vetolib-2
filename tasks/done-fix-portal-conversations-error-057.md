# Task: Fix portal landing conversations error

**Module:** Frontend — Portal
**Priority:** HIGH (P1 from screenshot audit — portal entry point broken)
**Source:** docs/audits/qa-screenshots-dashboard-portal-20260401.md
[MSW: oui]

## Problem
Portal landing page at /en/portal/demo-clinic shows "Something went wrong. Please try again later." — the conversations tab (entry point for pet owners) is broken. Likely a missing/broken MSW handler.

## Fix
1. Check the portal landing component and its API call
2. Verify MSW handler for portal conversations exists and returns correct data
3. Fix the error state

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] Portal landing page renders conversation list (not error state)
- [ ] MSW handler returns valid mock data
