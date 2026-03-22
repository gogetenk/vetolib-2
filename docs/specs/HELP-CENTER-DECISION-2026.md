# Help Center -- Architecture Decision Record

> Date: 2026-03-22
> Status: DECIDED
> Decision maker: PO + Architect (solo founder context)

---

## Context

- 33 articles already written in `docs/studies/HELP-CENTER-CONTENT-2026.md` (468 lines)
- Solo founder, bootstrapped, total budget $160-200/mo for ALL infra
- Help center must be accessible from the app AND the public site
- Content in English now, French + Arabic later
- Product is a Next.js 15 SaaS (Vetara)

---

## Options Evaluated

### Option A -- Custom build in Next.js (Fumadocs or raw MDX)

| Criteria | Score |
|---|---|
| Monthly cost | $0 (hosted on same Vercel deployment) |
| Setup time | 1-2 days with Fumadocs, 3-5 days raw MDX |
| SEO | Excellent -- SSR/SSG, same domain, full control over meta tags |
| Branding | Total control -- same design system, same Tailwind tokens |
| Content updates | Edit MDX files, commit, auto-deploy. No WYSIWYG for non-devs. |
| App integration | Native -- same codebase, link directly from any page, shared auth context |
| Search | Fumadocs has built-in search. Raw MDX needs custom implementation. |
| i18n | next-intl already in the stack -- reuse same infrastructure |

### Option B -- GitBook

| Criteria | Score |
|---|---|
| Monthly cost | $0 (free plan) or $65/mo (custom domain) |
| Setup time | 2-4 hours |
| SEO | Good on paid plan (custom domain). Poor on free (gitbook.io subdomain). |
| Branding | Limited on free plan. Acceptable on paid. |
| Content updates | WYSIWYG editor, easy for non-devs. |
| App integration | External link or iframe. No native integration. |
| Search | Built-in, good quality. |
| i18n | Supported but clunky. Separate spaces per language. |

**Verdict:** Free plan = unprofessional subdomain. Paid plan = $65/mo for a help center is absurd at this stage.

### Option C -- Crisp Knowledge Base

| Criteria | Score |
|---|---|
| Monthly cost | $95/mo minimum (Essentials plan required for KB) |
| Setup time | 1-2 hours |
| SEO | Decent with custom domain. |
| Branding | Limited customization. |
| Content updates | WYSIWYG, easy. |
| App integration | Widget embed or external link. |
| Search | Built-in, integrated with chat. |
| i18n | Supported. |

**Verdict:** ELIMINATED. The knowledge base requires the Essentials plan at 95 EUR/mo. That alone would consume 50%+ of the total infra budget. The chat widget on the free/Mini plan does NOT include a knowledge base. This was a false lead -- Crisp is not viable for help center at bootstrap stage.

### Option D -- Notion Public

| Criteria | Score |
|---|---|
| Monthly cost | $0 |
| Setup time | 1 hour |
| SEO | Terrible -- Notion pages are poorly indexed, no custom domain. |
| Branding | Zero -- looks like Notion, not like Vetara. |
| Content updates | Easy (Notion editor). |
| App integration | External link only. Feels unprofessional. |
| Search | Notion's built-in, mediocre. |
| i18n | Manual duplication of pages. |

**Verdict:** ELIMINATED. Unacceptable for a SaaS product. Zero SEO value, zero brand consistency.

### Option E -- Mintlify

| Criteria | Score |
|---|---|
| Monthly cost | $0 (Hobby) or $300/mo (Pro) |
| Setup time | 2-4 hours |
| SEO | Excellent. |
| Branding | Beautiful defaults, customizable. |
| Content updates | MDX + GitHub, auto-deploy. |
| App integration | External link (separate subdomain). |
| Search | AI-powered, excellent. |
| i18n | Not natively supported. |

**Verdict:** Hobby plan is free WITH custom domain -- surprisingly generous. But it is developer-focused, not end-user help-center focused. The aesthetic is "API docs", not "how do I book an appointment". Also, adding i18n later would be painful. Overkill for 33 articles aimed at veterinary receptionists.

---

## Decision

### MVP (now): Option A -- Fumadocs in the existing Next.js app

**Rationale:**

1. **$0/mo additional cost.** The help center is just more pages in the same Next.js deployment. No new service, no new bill.

2. **Same-domain SEO.** `app.vetara.io/help/getting-started` is infinitely better for SEO than `vetara.gitbook.io/help`. Google treats it as part of the main site.

3. **Native integration.** From any page in the app, link to `/help/article-slug`. No iframe, no external redirect. The help center shares the same nav, the same design system, the same auth context. Contextual help buttons ("Need help? Read: How to create an appointment") become trivial.

4. **i18n is free.** next-intl is already configured. Adding `/ar/help/...` and `/fr/help/...` is the same mechanism as the rest of the app.

5. **Fumadocs is open source and free.** Built specifically for Next.js. Provides built-in full-text search, table of contents, breadcrumbs, and a clean reading layout. No vendor lock-in.

6. **Content is already written.** The 33 articles in `HELP-CENTER-CONTENT-2026.md` just need to be split into individual MDX files. That is a 2-hour task.

7. **Solo founder = solo editor.** The "no WYSIWYG" downside does not apply. The founder writes in Markdown anyway. If a content writer joins later, they can use any Markdown editor (Typora, StackEdit, even GitHub's web editor).

### Long-term (6-12 months): Stay on Option A, add search upgrades

When the article count exceeds 100+ and/or a non-technical content writer joins:
- Add Algolia DocSearch (free for docs sites) or Fumadocs' built-in search
- Consider a headless CMS (Contentlayer, Sanity) as a content source if the team grows
- The architecture does NOT need to change -- only the content source

There is no scenario in the foreseeable future where paying $65-300/mo for a hosted docs platform makes sense for 33-100 help articles when the same result can be achieved for $0 in the existing stack.

---

## Implementation Plan

### Phase 1 -- MVP Help Center (estimate: 1 day)

1. Install `fumadocs-core` + `fumadocs-ui` + `fumadocs-mdx` in the Next.js app
2. Create `/help` route group with Fumadocs layout
3. Split `HELP-CENTER-CONTENT-2026.md` into individual MDX files under `content/help/`
4. Structure:
   ```
   content/help/
     getting-started/
       set-up-your-clinic.mdx
       adding-first-patient.mdx
       creating-first-appointment.mdx
       sending-first-invoice.mdx
       whatsapp-reminders.mdx
     features/
       calendar-views.mdx
       medical-records.mdx
       ai-soap-notes.mdx
       ...
     troubleshooting/
       ...
     account/
       ...
   ```
5. Add "Help" link in the app sidebar/nav
6. Add contextual help links on key pages (agenda, patients, billing)
7. Style with existing Tailwind design tokens for brand consistency

### Phase 2 -- Polish (estimate: 0.5 day)

1. Add search (Fumadocs built-in or Algolia DocSearch)
2. Add "Was this helpful?" feedback widget (simple thumbs up/down, store in DB)
3. Add breadcrumbs and "Related articles" links
4. Public access: make `/help` accessible without authentication for SEO

---

## Summary Table

| | Cost | SEO | Brand | Integration | i18n | Winner? |
|---|---|---|---|---|---|---|
| **Fumadocs (A)** | $0 | Excellent | Total | Native | Free | YES |
| GitBook (B) | $0-65 | Poor-Good | Limited | External | Clunky | No |
| Crisp KB (C) | $95+ | Decent | Limited | Widget | OK | No |
| Notion (D) | $0 | Terrible | Zero | External | Manual | No |
| Mintlify (E) | $0-300 | Excellent | Good | External | Weak | No |
