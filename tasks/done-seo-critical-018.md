# todo-seo-critical-018.md -- Fix CRITICAL SEO issues

**Module** : Frontend
**Priority** : Critique
**Dependencies** : aucune

## Scope

### SEO-01 CRITICAL: Ungate blog pages from authentication
- File: src/frontend/src/middleware.ts
- Add `/blog` to isPublicPath() list

### SEO-02 CRITICAL: Add hreflang tags
- File: src/frontend/src/app/[locale]/layout.tsx
- Add `<link rel="alternate" hreflang="en" href="...">` for all 3 locales

### SEO-03 CRITICAL: Add Breeding module to landing page
- Add a feature card for Breeding (pedigree, pregnancy tracking, heat cycles)
- Target copy for UAE breeders (falconry, Arabian horses)

### SEO-04 HIGH: Fix root URL redirect
- Unauthenticated users hitting `/` should see landing page, not login

### SEO-05 HIGH: Fix OG image
- Replace placeholder SVG with real OG image

## Completion criteria
- [ ] Blog accessible without auth
- [ ] Hreflang tags on all pages
- [ ] Breeding featured on landing
- [ ] Root URL shows landing for unauthenticated
- [ ] `npm run lint` + `npm run build` GREEN
