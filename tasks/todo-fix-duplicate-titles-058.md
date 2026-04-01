# Task: Fix duplicate "Vetara" in page titles

**Module:** Frontend
**Priority:** MEDIUM (SEO impact)
**Source:** docs/audits/qa-screenshots-public-pages-20260401.md
[MSW: oui]

## Problem
4 pages have duplicate "Vetara" in title tag (e.g., "Create Your Clinic -- Vetara -- Vetara"):
- Signup page
- Pricing page
- Pet-owners page
- Developers page

2 pages have generic titles instead of page-specific:
- Terms page
- Privacy page

## Fix
Check the metadata/title generation in these pages and remove the duplication. Each page should have: "Page Name -- Vetara" (not "Page Name -- Vetara -- Vetara").

## Definition of Done
- [ ] `npm run build` 0 errors
- [ ] No duplicate "Vetara" in any page title
- [ ] Terms and Privacy have specific titles
