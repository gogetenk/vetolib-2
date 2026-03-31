# todo-p0-pet-owner-landing-030.md — Pet owner B2C landing page

**Module** : Frontend
**Priority** : P0
**Dependencies** : aucune
**Skills** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**
**BDD** : `tests/Vetolib.Tests.Acceptance/Features/Landing/PetOwnerLanding.feature`

## Context
No B2C landing page exists. Owners land on the B2B SaaS pitch. Need a separate landing targeting pet owners: "Access your pet's medical records online."

## Scope
1. New landing page at /[locale]/pet-owners (or /[locale]/owners)
2. Hero: "Your pet's health, always in your pocket"
3. Value props: see medical records, book appointments, get vaccination reminders, share records with any vet
4. Clinic search bar: find your clinic
5. CTA: "Sign up" or "Ask your vet to join"
6. Mobile-first design (owners use phones)
7. SEO optimized (meta, JSON-LD, hreflang)
8. i18n en/ar/fr
9. data-testid on all interactive elements

## Definition of Done
- [ ] B2C landing page exists and is accessible
- [ ] Clinic search works (MSW)
- [ ] "Ask your vet" flow works
- [ ] Mobile responsive
- [ ] SEO meta tags
- [ ] i18n complete
- [ ] BDD scenarios addressed
- [ ] `npm run lint` + `npm run build` GREEN
