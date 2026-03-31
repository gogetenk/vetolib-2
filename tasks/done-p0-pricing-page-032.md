# todo-p0-pricing-page-032.md — Pricing page with 3 plans

**Module** : Frontend
**Priority** : P0
**Dependencies** : aucune
**Skills** : `shadcn-nextjs`
**BDD** : `tests/Vetolib.Tests.Acceptance/Features/Pricing/PricingPage.feature`

## Context
Landing page has pricing section (PR #246) but no dedicated /pricing page. Need a full pricing page.

## Scope
1. Page at /[locale]/pricing
2. 3 plans: Starter (Free), Professional (149 AED/mo), Enterprise (Custom)
3. Feature comparison table
4. Annual billing toggle (15% discount)
5. CTA per plan
6. FAQ section
7. Public page (add to middleware isPublicPath)
8. i18n en/ar/fr
9. Mobile responsive

## Definition of Done
- [ ] /pricing page accessible
- [ ] 3 plans displayed correctly
- [ ] Annual toggle works
- [ ] FAQ section
- [ ] Mobile responsive
- [ ] BDD scenarios addressed
- [ ] lint + build GREEN
