# Landing Page, SEO & Onboarding Audit

**Date**: 2026-03-29
**Auditor**: Claude Opus 4.6
**Scope**: Landing page copy/CRO, SEO infrastructure, signup onboarding flow
**Markets**: UAE (Dubai, Abu Dhabi) and France

---

## Executive Summary

The landing page is well-structured with strong CRO elements (sticky CTA, exit intent popup, social proof, demo form). SEO foundations exist (robots.txt, sitemap, JSON-LD, blog) but have critical gaps -- most notably, blog pages are gated behind authentication middleware, hreflang tags are missing, and the Breeding module (a key differentiator) is entirely absent from the landing page. France market support is incomplete. The signup flow is lean (5 fields, single step) but lacks progress indicators and social login.

---

## 1. Landing Page Audit

### 1.1 Current State

**Page structure** (top to bottom):
1. Sticky CTA bar (appears on scroll)
2. Exit-intent popup (mouse-leave detection)
3. Sticky nav with language switcher (EN/AR/FR) + Sign In / Start Trial
4. Hero section with headline, subtitle, CTA, trust badge, dashboard screenshot
5. Social proof bar (50+ clinics, 5000+ patients, AED 2M+ invoiced, 99.9% uptime)
6. Features section (9 cards: Health Passport, AI, Messaging, Stock, Multilingual, AI SOAP, WhatsApp, File Attachments, Multi-Clinic)
7. How It Works (3 steps)
8. Trust Signals (Arabic+English, WhatsApp, UAE Hosting, MOCCAE Ready)
9. Testimonials (3 quotes)
10. Pricing (3 tiers: Starter AED 299, Pro AED 449, Enterprise AED 649 -- per vet/month)
11. Demo form (Book a Demo)
12. FAQ (8 questions)
13. Latest blog posts (3 cards)
14. Final CTA
15. Footer (4 columns + contact emails + language switcher)

**Locales supported**: EN, AR, FR (routing configured; AR has RTL support)

### 1.2 Copy Assessment

**Strengths:**
- Headline ("Run Your Veterinary Clinic in Half the Time") is clear and benefit-driven
- Subtitle emphasizes AI + WhatsApp + focus on animals -- concrete benefits
- Social proof numbers are visible (though "50+ clinics" is modest)
- Trust badge ("30-day free Pro trial -- no credit card required") removes friction
- CTA text ("Start Free Trial") is clear and repeated 7+ times across the page
- FAQ answers are practical and address real objections (VAT compliance, Arabic support, data security)
- USD hints on pricing help international comparison

**Issues (prioritized):**

| # | Severity | Issue | Detail |
|---|----------|-------|--------|
| 1 | **CRITICAL** | Breeding module completely absent | The Breeding module (heat cycles, litters, pregnancy tracking) is described as the "new killer feature" but has ZERO presence on the landing page. No feature card, no mention in hero copy, no blog post about it. |
| 2 | HIGH | No France-specific value props | The landing copy is 100% UAE-focused. France is a target market but there is no mention of French regulations, Ordre des Veterinaires, RGPD, or EUR pricing. The `fr.json` has translations but copy still references UAE, AED, TRN. |
| 3 | HIGH | Testimonials are fake (acknowledged) | The `en.json` contains `"disclaimer": "These testimonials are illustrative examples."` but this disclaimer is NOT shown on the page. Users see fake names and clinics presented as real. |
| 4 | MEDIUM | Dashboard screenshot is a placeholder SVG | The hero image (`/dashboard-placeholder.svg`) is a gray SVG placeholder, not a real screenshot. This is the first visual element visitors see. |
| 5 | MEDIUM | "50+ clinics" may hurt credibility | For a SaaS, 50 clinics is very small social proof. Consider phrasing it differently or hiding it until the number is more impressive. |
| 6 | MEDIUM | No video demo or interactive walkthrough | Competitors often include a video tour. The "Book a Demo" form is good but a self-serve video would reduce friction. |
| 7 | LOW | Footer links are mostly dead | Integrations, Changelog, About, Careers, API Docs, Status Page, Privacy Policy, Terms of Service, DPA all point to `#` (disabled with "Coming Soon" tooltip). |
| 8 | LOW | CompetitiveTableSection exists in code but is NOT rendered | There is a `CompetitiveTableSection.tsx` component and corresponding i18n keys comparing Vetara vs ezyVet vs Digitail, but it is not included in the page. |

### 1.3 Mobile Responsiveness

**Good**: The page uses Tailwind responsive classes throughout (`sm:`, `md:`, `lg:`). There is a `MobileLandingNav` component (hamburger menu). Grid layouts collapse properly (2-col on mobile, 4-col on desktop for social proof). The testimonials section has a scroll hint on mobile.

**Concern**: No explicit mobile-first testing evidence. The sticky CTA bar and exit-intent popup may conflict with mobile viewport (exit-intent uses `mouseout` which does not work on touch devices -- this is handled correctly by simply not triggering).

### 1.4 CRO Elements

- Sticky CTA bar: Good, appears after 600px scroll
- Exit-intent popup: Good, uses cookie to avoid repeat display (7-day cooldown)
- Analytics tracking via PostHog (`trackEvent` calls on CTA clicks)
- Multiple CTAs throughout the page (hero, how-it-works, pricing, demo form, final CTA, footer CTA, sticky bar)
- Early access pricing badge ("locked for founding clinics") creates urgency

---

## 2. SEO Audit

### 2.1 Meta Tags & OG Tags

**Landing page** (`page.tsx` `generateMetadata`):
- Title: "Vetara -- The Smart Veterinary Practice Management Platform" (from i18n)
- Description: "UAE veterinary clinic management platform -- AI triage, AI SOAP notes, WhatsApp integration, Arabic + English support, and VAT-compliant invoicing. 30-day free trial."
- OG title/description: Set (same as meta)
- OG image: `/dashboard-placeholder.svg` (720x460) -- **PROBLEM**: SVGs render poorly as social shares and this is a placeholder

**Layout** (`[locale]/layout.tsx`):
- Title template: `%s -- Vetara`
- Default description set

**Blog article pages**: Good -- canonical URL, OG tags, Twitter card, BlogPosting JSON-LD

### 2.2 Critical SEO Issues

| # | Severity | Issue | Detail |
|---|----------|-------|--------|
| 1 | **CRITICAL** | Blog pages are behind authentication | The middleware (`middleware.ts`) only marks `/login`, `/signup`, `/portal/`, and locale roots as public. Blog paths (`/{locale}/blog`, `/{locale}/blog/{slug}`) are NOT in the public list, meaning unauthenticated users are redirected to login. **Google cannot crawl blog content.** |
| 2 | **CRITICAL** | No hreflang tags | There are no `<link rel="alternate" hreflang="en">` or `hreflang="ar"` tags anywhere. With 3 locales (EN/AR/FR) and duplicate content across them, Google may index the wrong locale version or flag duplicate content. |
| 3 | **CRITICAL** | Help pages are behind authentication | `/help` is not in the public paths list. Same crawl problem as blog. |
| 4 | HIGH | No `logo.png` exists | The blog JSON-LD references `https://vetara.com/logo.png` as the publisher logo, but no `logo.png` file exists in `/public/`. This produces a 404 for schema validators. |
| 5 | HIGH | OG image is a placeholder SVG | Social sharing previews (Facebook, LinkedIn, Twitter) will show a broken or meaningless gray rectangle. Need a proper 1200x630 PNG/JPG. |
| 6 | HIGH | `countriesSupported` in JSON-LD only lists "AE" | If targeting France too, this should include "FR". |
| 7 | HIGH | Sitemap is incomplete | The sitemap only includes EN locale pages. Missing: `/ar` blog pages, `/fr` pages, `/ar/blog/{slug}`, help pages. Also includes `/en/portal/booking` which may not be a real public page. |
| 8 | MEDIUM | Root `/` redirects to `/en` (not to landing page) | The `page.tsx` at root does `redirect("/en")`. However, the middleware redirects `/` to `/login` if no token is present, which contradicts the component. Authenticated users go to `/appointments`. This means the root URL does NOT land on the marketing page for logged-out users. |
| 9 | MEDIUM | No `robots` meta tag on landing page | The login page correctly has `robots: { index: false }`, but the landing page and blog should explicitly set `robots: { index: true, follow: true }`. |
| 10 | MEDIUM | Blog content is hardcoded in TypeScript | Articles are `.ts` files with inline HTML content. No CMS, no MDX, no markdown. Adding new articles requires a developer and a deploy. |
| 11 | LOW | No breadcrumb structured data on blog | Blog articles have BlogPosting JSON-LD but no BreadcrumbList schema. |
| 12 | LOW | `sitemap.xml` domain is `vetara.com` | Verify this is the correct production domain. The `robots.ts` also references `vetara.com`. |

### 2.3 Blog SEO Details

**5 articles exist:**
1. `outgrown-spreadsheets-vet` -- generalist
2. `arabic-software-dubai-vet` -- UAE-specific
3. `ai-veterinary-triage` -- AI feature
4. `whatsapp-integration-dubai-vet` -- WhatsApp feature
5. `veterinary-software-uae-guide` -- UAE guide

**Good**: Each article has metaTitle, metaDescription, canonical URL, OG tags, Twitter cards, BlogPosting JSON-LD, tags, reading time, related articles, table of contents.

**Missing**: No article about Breeding (the killer feature). No article targeting France market. No article in Arabic or French (blog is English-only).

### 2.4 Page Speed Considerations

**Good**:
- Using `next/font/google` (Manrope) -- font is self-hosted, no CLS
- Hero image has `priority` flag (preloaded)
- SVG images are lightweight
- `ScrollReveal` uses intersection observer (not scroll events)
- Sticky CTA uses `{ passive: true }` scroll listener
- `output: "standalone"` in next.config for optimized builds

**Concerns**:
- `unoptimized` flag on images (bypasses Next.js image optimization)
- No WebP/AVIF images in `/public/` (all SVG placeholders)
- `dangerouslySetInnerHTML` for blog content (no lazy loading of article bodies)

---

## 3. Onboarding / Signup Flow Audit

### 3.1 Current State

**Single-page form** at `/{locale}/signup` with 5 fields:
1. Clinic Name (text)
2. Email (email)
3. Phone (tel)
4. Password (password with show/hide toggle)
5. Confirm Password (password with show/hide toggle)

**Single CTA**: "Create My Clinic"

### 3.2 Strengths

- Minimal fields (5) -- low friction
- Password strength indicator (real-time, 3 criteria: length, uppercase, number)
- Client-side validation with Zod + react-hook-form
- Proper `aria-invalid`, `aria-describedby`, `role="alert"` on errors
- Form shake animation on validation errors
- Loading state with spinner
- Server error handling (email taken, connection error)
- Language switcher available on signup page
- "30-day free trial. No credit card required." subtitle

### 3.3 Issues

| # | Severity | Issue | Detail |
|---|----------|-------|--------|
| 1 | HIGH | No progress indicator / step counter | Even though it is a single step, there is no visual indication of "1 of 1" or progress bar. Users do not know if more steps follow. |
| 2 | HIGH | No social login (Google, Apple) | For a SaaS in 2026, the absence of "Sign up with Google" significantly increases friction. Many UAE professionals expect this. |
| 3 | HIGH | No onboarding wizard after signup | After successful signup, the user is redirected directly to `/dashboard`. There is no guided setup (add first patient, configure clinic hours, invite team, etc.). New users land on an empty dashboard with no guidance. |
| 4 | MEDIUM | Phone field has no format validation | The phone field accepts any string with `min(1)`. No validation for UAE format (+971), French format (+33), or international format. |
| 5 | MEDIUM | No Terms of Service / Privacy Policy checkbox | Users are not asked to accept ToS or privacy policy during signup. This is legally required in UAE and EU (GDPR). |
| 6 | MEDIUM | Clinic timezone/country not collected | The system needs to know if it is a UAE or France clinic for VAT rules, currency, and business hours. This is not asked during signup. |
| 7 | LOW | No email verification step shown | After signup, the user goes straight to dashboard. No email verification prompt visible (may be handled server-side but there is no UI indication). |
| 8 | LOW | "Confirm Password" field adds friction | Modern best practice is to use a single password field with show/hide toggle instead of a confirm field. |

---

## 4. Recommendations

### Quick Wins (< 1 day each)

1. **[CRITICAL] Fix blog middleware** -- Add `/{locale}/blog` to the `isPublicPath` function in `middleware.ts`. Without this, Google cannot index any blog content. Estimated: 15 minutes.

2. **[CRITICAL] Fix help page middleware** -- Same as above for `/{locale}/help`. Estimated: 5 minutes.

3. **[CRITICAL] Add hreflang tags** -- In `[locale]/layout.tsx`, add `<link rel="alternate" hreflang="en" href="https://vetara.com/en">` etc. for all 3 locales + `x-default`. Estimated: 30 minutes.

4. **[HIGH] Add OG image** -- Create a proper 1200x630 PNG showing the Vetara dashboard. Replace the SVG placeholder in OG meta and hero section. Estimated: 2 hours (design + implementation).

5. **[HIGH] Add logo.png** -- Create and add `/public/logo.png` so the JSON-LD publisher logo does not 404. Estimated: 15 minutes.

6. **[HIGH] Add Breeding to feature cards** -- Add a 10th feature card for Breeding (heat cycle tracking, litter management, pregnancy monitoring). Use the Baby or Heart icon from Lucide. Estimated: 1 hour.

7. **[HIGH] Add testimonials disclaimer** -- Either show the existing disclaimer text on the page, or replace the fake testimonials with a "beta tester" framing. Estimated: 15 minutes.

8. **[MEDIUM] Include CompetitiveTableSection** -- The component and translations already exist. Just add it to the landing page between Trust Signals and Testimonials. Estimated: 15 minutes.

9. **[MEDIUM] Fix root URL redirect** -- The middleware redirects `/` to `/login` for unauthenticated users. It should redirect to `/en` (the landing page). The `app/page.tsx` already does this but middleware intercepts first. Estimated: 15 minutes.

10. **[MEDIUM] Add ToS/Privacy checkbox to signup** -- Legal requirement for UAE and EU. Estimated: 30 minutes.

### Medium-term (1-5 days each)

11. **[HIGH] Create France-specific landing content** -- Adapt hero copy, pricing (EUR), compliance mentions (RGPD, Ordre des Veterinaires), and testimonials for the French market. The `fr.json` file exists but copy is UAE-focused.

12. **[HIGH] Build post-signup onboarding wizard** -- 3-5 step guided setup: configure clinic (timezone, country, currency), add first patient, invite team member, configure business hours. Show accumulated value.

13. **[HIGH] Write Breeding blog post** -- SEO article targeting "veterinary breeding management software" and related keywords. This supports the new killer feature.

14. **[MEDIUM] Expand sitemap** -- Include AR and FR locale pages, all blog slugs in all locales, help pages.

15. **[MEDIUM] Add social login** -- Google Sign-In at minimum. Apple Sign-In for iOS users.

16. **[MEDIUM] Add phone validation** -- Use a library like `libphonenumber-js` to validate UAE (+971) and French (+33) phone formats.

17. **[MEDIUM] Replace placeholder dashboard image** -- Either with a real screenshot or a high-quality mockup.

### Long-term (> 1 week)

18. **[HIGH] Move blog to CMS or MDX** -- Current hardcoded TypeScript articles are not scalable. Consider a headless CMS (Contentful, Sanity) or MDX files for easier content creation.

19. **[MEDIUM] Add Arabic and French blog content** -- Either translated versions of existing articles or original content targeting each market.

20. **[MEDIUM] Add video demo** -- Self-serve video walkthrough embedded on the landing page (reduces dependency on "Book a Demo" form).

21. **[LOW] Add BreadcrumbList JSON-LD** -- On blog pages for richer search results.

22. **[LOW] Create legal pages** -- Privacy Policy, Terms of Service, and DPA pages. Currently all footer links point to `#`.

23. **[LOW] Add LocalBusiness JSON-LD** -- If Vetara has a physical office in UAE, add LocalBusiness schema for local search visibility.

---

## 5. Summary Scorecard

| Area | Score | Notes |
|------|-------|-------|
| Landing Page Copy | 7/10 | Strong CTA, good structure. Missing Breeding, France targeting. |
| CRO / Conversion | 8/10 | Excellent: sticky CTA, exit intent, demo form, multiple CTAs, analytics. |
| Mobile Responsiveness | 7/10 | Responsive classes present. Needs real-device testing. |
| SEO Infrastructure | 4/10 | Blog gated behind auth is a dealbreaker. No hreflang. Incomplete sitemap. |
| Blog / Content | 6/10 | 5 solid articles with proper SEO. Hardcoded, English-only, no Breeding content. |
| Structured Data | 6/10 | SoftwareApplication + BlogPosting JSON-LD present. Missing logo, BreadcrumbList. |
| Signup Flow | 6/10 | Lean and functional but no social login, no onboarding wizard, missing legal consent. |
| Internationalization | 5/10 | 3 locales configured. AR has RTL. But no hreflang, FR copy is UAE-focused, blog is EN-only. |

**Top 3 priorities:**
1. Fix middleware to allow blog/help crawling (15 min fix, massive SEO impact)
2. Add hreflang tags across all locales (30 min fix, prevents duplicate content penalties)
3. Add Breeding module to landing page (1 hour, highlights the killer feature)
