# Vetara Visual Branding -- Final Direction 2026

> Authored: 2026-03-22
> Status: APPROVED -- single source of truth for all visual decisions
> Scope: Landing page, dashboard app, blog, auth screens, portal

---

## 1. Problem Statement

Vetara currently suffers from a split personality:

| Surface | Color Used | Hue |
|---|---|---|
| Landing page (light) | Tailwind `emerald-700` | Green (~160) |
| Dashboard (light) | CSS var `--primary` = `oklch(0.46 0.25 264)` | Crayola Blue |
| Dashboard (dark) | CSS var `--primary` = `oklch(0.65 0.12 180)` | Teal/Cyan |
| Blog | Tailwind `emerald-700` | Green (~160) |
| Auth screens | Gradient `to-emerald-50/30` | Green tint |

Three different color identities across the same product. This destroys brand recognition, looks unprofessional, and confuses users who navigate between the landing page and the app.

---

## 2. Competitive Landscape Analysis

### What competitors use

| Competitor | Primary Color | Notes |
|---|---|---|
| ezyVet (IDEXX) | Blue | Standard IDEXX corporate blue, color-coded navigation |
| Digitail | Blue/Purple | Clean, modern, slightly purple-leaning blue |
| Vetspire | Blue | Traditional SaaS blue |
| Shepherd Vet | Warm orange/amber | Differentiated -- "joy" positioning |
| Provet Cloud | Blue | Nordic clean blue |
| PetDesk | Green | Bright lime-green accent |
| Vetter | Blue | Standard tech blue |

**Pattern:** The vet software market is overwhelmingly blue. A few use green (PetDesk, some clinic apps). Almost none use teal, amber, or warm tones as primary.

### What to avoid

- **Pure blue (#2563eb range):** Indistinguishable from ezyVet, Digitail, Vetspire, Provet -- looks like a clone
- **Bright green (#059669 range):** PetDesk territory, also overused in "eco/organic" branding
- **Pure emerald-700 (#047857):** The current landing color -- too close to generic "nature/health" green

---

## 3. Recommendation: TEAL as Primary Color

### The choice: Deep Teal (`oklch(0.47 0.12 195)` -- approximately #0D7377)

**Why teal:**

1. **Bridges blue and green** -- inherits trust (blue) and health/nature (green) without being either. Teal sits at the exact intersection of "professional technology" and "animal healthcare."

2. **WGSN 2026 Color of the Year** -- "Transformative Teal" is the color trend for 2026, signaling innovation, calm confidence, and forward-thinking. Vetara launches into this wave perfectly.

3. **Differentiated** -- No major vet software competitor uses teal as primary. Blue is saturated. Green is taken. Teal is the unclaimed territory.

4. **Works with "Vetara"** -- The name sounds modern, slightly exotic (Middle Eastern market resonance). Teal complements this: it evokes depth, sophistication, and warmth without being cold like blue.

5. **Excellent dark mode behavior** -- Teal lightens beautifully in dark mode (lighter teal on dark backgrounds) without losing saturation or becoming garish.

6. **Cultural fit for UAE market** -- Teal/turquoise has deep significance in Islamic art and architecture. It evokes trust and serenity in the target market.

7. **Accessibility** -- Deep teal achieves WCAG AA contrast ratios against white backgrounds when darkened slightly, and against dark backgrounds when lightened.

---

## 4. Complete Color Palette

All values in OKLCH (native CSS, used by Tailwind v4).

### 4.1 Light Mode (`:root`)

```css
:root {
  /* --- Brand --- */
  --primary: oklch(0.47 0.12 195);           /* Deep Teal #0D7377 -- THE brand color */
  --primary-foreground: oklch(0.98 0.01 195); /* Near-white on teal */

  /* --- Surfaces --- */
  --background: oklch(0.98 0.005 200);        /* Warm off-white with teal whisper #f5f7f8 */
  --foreground: oklch(0.18 0.02 200);         /* Near-black with teal undertone */
  --card: oklch(1 0 0);                       /* Pure white cards */
  --card-foreground: oklch(0.18 0.02 200);
  --popover: oklch(1 0 0);
  --popover-foreground: oklch(0.18 0.02 200);

  /* --- Secondary & Accent --- */
  --secondary: oklch(0.95 0.02 195);          /* Very light teal surface #eef6f6 */
  --secondary-foreground: oklch(0.25 0.08 195);
  --accent: oklch(0.93 0.03 195);             /* Light teal accent */
  --accent-foreground: oklch(0.47 0.12 195);  /* Primary on accent */

  /* --- Muted --- */
  --muted: oklch(0.96 0.005 200);             /* Neutral muted surface */
  --muted-foreground: oklch(0.55 0.02 200);   /* Gray text */

  /* --- Borders & Inputs --- */
  --border: oklch(0.91 0.01 200);             /* Light gray border */
  --input: oklch(0.91 0.01 200);
  --ring: oklch(0.47 0.12 195);               /* Focus ring = primary */

  /* --- Semantic --- */
  --destructive: oklch(0.58 0.22 27);         /* Red -- errors, delete actions */
  --success: oklch(0.60 0.17 155);            /* Green -- confirmations, checkmarks */
  --warning: oklch(0.75 0.15 75);             /* Amber -- warnings, caution */
  --info: oklch(0.58 0.12 240);               /* Blue -- informational */

  /* --- Charts --- */
  --chart-1: oklch(0.47 0.12 195);            /* Teal (primary) */
  --chart-2: oklch(0.58 0.22 27);             /* Coral/Red */
  --chart-3: oklch(0.60 0.17 155);            /* Green */
  --chart-4: oklch(0.75 0.15 75);             /* Amber */
  --chart-5: oklch(0.55 0.15 300);            /* Purple */

  /* --- Sidebar --- */
  --sidebar: oklch(1 0 0);
  --sidebar-foreground: oklch(0.18 0.02 200);
  --sidebar-primary: oklch(0.47 0.12 195);
  --sidebar-primary-foreground: oklch(0.98 0.01 195);
  --sidebar-accent: oklch(0.95 0.02 195);
  --sidebar-accent-foreground: oklch(0.47 0.12 195);
  --sidebar-border: oklch(0.91 0.01 200);
  --sidebar-ring: oklch(0.47 0.12 195);

  /* --- Layout --- */
  --radius: 0.5rem;
}
```

### 4.2 Dark Mode (`.dark`)

```css
.dark {
  /* --- Brand --- */
  --primary: oklch(0.72 0.12 195);            /* Lighter teal for dark bg */
  --primary-foreground: oklch(0.15 0.02 195); /* Dark text on light teal */

  /* --- Surfaces --- */
  --background: oklch(0.15 0.015 200);        /* Dark with subtle teal */
  --foreground: oklch(0.95 0.01 200);         /* Light text */
  --card: oklch(0.18 0.015 200);
  --card-foreground: oklch(0.95 0.01 200);
  --popover: oklch(0.18 0.015 200);
  --popover-foreground: oklch(0.95 0.01 200);

  /* --- Secondary & Accent --- */
  --secondary: oklch(0.25 0.025 200);
  --secondary-foreground: oklch(0.95 0.01 200);
  --accent: oklch(0.25 0.025 200);
  --accent-foreground: oklch(0.95 0.01 200);

  /* --- Muted --- */
  --muted: oklch(0.22 0.015 200);
  --muted-foreground: oklch(0.68 0.02 200);

  /* --- Borders & Inputs --- */
  --border: oklch(0.28 0.02 200);
  --input: oklch(0.28 0.02 200);
  --ring: oklch(0.72 0.12 195);

  /* --- Semantic --- */
  --destructive: oklch(0.60 0.18 27);
  --success: oklch(0.65 0.15 155);
  --warning: oklch(0.78 0.12 75);
  --info: oklch(0.62 0.10 240);

  /* --- Charts --- */
  --chart-1: oklch(0.65 0.10 195);
  --chart-2: oklch(0.70 0.08 200);
  --chart-3: oklch(0.72 0.10 155);
  --chart-4: oklch(0.78 0.08 75);
  --chart-5: oklch(0.75 0.06 300);

  /* --- Sidebar --- */
  --sidebar: oklch(0.18 0.015 200);
  --sidebar-foreground: oklch(0.95 0.01 200);
  --sidebar-primary: oklch(0.72 0.12 195);
  --sidebar-primary-foreground: oklch(0.15 0.02 195);
  --sidebar-accent: oklch(0.25 0.025 200);
  --sidebar-accent-foreground: oklch(0.95 0.01 200);
  --sidebar-border: oklch(0.28 0.02 200);
  --sidebar-ring: oklch(0.72 0.12 195);
}
```

### 4.3 Tailwind Mapping for Hardcoded Colors

All landing/blog/auth components currently use hardcoded Tailwind classes like `bg-emerald-700`. These MUST be replaced with CSS variable-based classes:

| Current Hardcoded Class | Replace With |
|---|---|
| `bg-emerald-700` | `bg-primary` |
| `bg-emerald-800` | `bg-primary/90` (or `hover:bg-primary/90`) |
| `text-emerald-700` | `text-primary` |
| `text-emerald-600` | `text-primary/85` |
| `text-emerald-800` | `text-primary` |
| `bg-emerald-100` | `bg-secondary` |
| `bg-emerald-50` | `bg-accent` or `bg-secondary/50` |
| `text-emerald-100` | `text-primary-foreground` |
| `text-emerald-200` | `text-primary-foreground/70` |
| `border-emerald-200` | `border-primary/20` |
| `border-emerald-800` | `border-primary/80` |
| `hover:text-emerald-700` | `hover:text-primary` |
| `hover:bg-emerald-50` | `hover:bg-accent` |
| `hover:bg-emerald-100` | `hover:bg-secondary` |
| `hover:bg-emerald-800` | `hover:bg-primary/90` |
| `shadow-emerald-700/25` | `shadow-primary/25` |
| `shadow-emerald-700/20` | `shadow-primary/20` |
| `bg-gradient-to-br from-emerald-50 via-white to-teal-50` | `bg-gradient-to-br from-secondary/50 via-white to-accent/50` |
| `bg-gradient-to-br from-emerald-100 to-teal-50` | `bg-gradient-to-br from-secondary to-accent/50` |
| `to-emerald-50/30` | `to-accent/30` |
| `bg-emerald-400` (ping animation) | `bg-primary/60` |
| `bg-emerald-500` (status dot) | `bg-primary` |

---

## 5. Typography

### Current: Manrope

**Verdict: KEEP Manrope.** It is an excellent choice for Vetara:

- Geometric sans-serif with friendly rounded terminals -- approachable yet professional
- Designed specifically for digital interfaces
- Excellent Arabic glyph support through variable font fallbacks
- Good weight range (400-800) for hierarchy
- Readable at small sizes (important for medical data tables)

No change needed.

---

## 6. Files To Modify

### 6.1 Core (1 file -- does everything for the dashboard)

| File | Action |
|---|---|
| `src/frontend/src/app/globals.css` | Replace all CSS variable values with the teal palette above |

### 6.2 Landing Page Components (15 files -- replace hardcoded emerald/teal classes)

| File | Hardcoded Colors |
|---|---|
| `src/frontend/src/app/[locale]/page.tsx` | ~30 instances of `emerald-*` |
| `src/frontend/src/components/features/landing/FeaturesSection.tsx` | `emerald-*`, `teal-*` gradients |
| `src/frontend/src/components/features/landing/PricingSection.tsx` | `emerald-*` buttons, badges, borders |
| `src/frontend/src/components/features/landing/Footer.tsx` | `emerald-*` text, buttons, hovers |
| `src/frontend/src/components/features/landing/FinalCtaSection.tsx` | `emerald-*` bg, text |
| `src/frontend/src/components/features/landing/StickyCtaBar.tsx` | `emerald-*` bg, border, text |
| `src/frontend/src/components/features/landing/MobileLandingNav.tsx` | `emerald-*` text, bg, hovers |
| `src/frontend/src/components/features/landing/NavLanguageSwitcher.tsx` | `emerald-*` text, bg |
| `src/frontend/src/components/features/landing/DemoFormSection.tsx` | `emerald-*` button |
| `src/frontend/src/components/features/landing/ExitIntentPopup.tsx` | `emerald-*` icon, button |
| `src/frontend/src/components/features/landing/CompetitiveTableSection.tsx` | `emerald-*` checkmarks, headers |
| `src/frontend/src/components/features/landing/TrustSignalsSection.tsx` | `emerald-*` gradients, icons |

### 6.3 Blog (4 files)

| File | Hardcoded Colors |
|---|---|
| `src/frontend/src/app/[locale]/blog/page.tsx` | `emerald-*` nav, text |
| `src/frontend/src/app/[locale]/blog/[slug]/page.tsx` | `emerald-*` nav, links, badges |
| `src/frontend/src/components/features/blog/LatestBlogSection.tsx` | `emerald-*` |
| `src/frontend/src/components/features/blog/BlogArticleCard.tsx` | `emerald-*` |
| `src/frontend/src/components/features/blog/BlogTagFilter.tsx` | `emerald-*` |
| `src/frontend/src/components/features/blog/BlogTableOfContents.tsx` | `emerald-*` |

### 6.4 Auth Screens (4 files)

| File | Hardcoded Colors |
|---|---|
| `src/frontend/src/app/[locale]/(auth)/layout.tsx` | `to-emerald-50/30` gradient |
| `src/frontend/src/app/[locale]/(auth)/loading.tsx` | `to-emerald-50/30` gradient |
| `src/frontend/src/components/features/auth/SignupForm.tsx` | `emerald-*` |
| `src/frontend/src/components/features/auth/LoginForm.tsx` | `emerald-*` |
| `src/frontend/src/components/features/auth/LanguageSwitcher.tsx` | `emerald-*` |

### 6.5 Dashboard Components Using Hardcoded emerald (semantic uses -- keep as success/green)

These files use `emerald` for semantic "success/positive" states (e.g., status badges, stock levels). These should be mapped to `--success` token, NOT to `--primary`:

| File | Context |
|---|---|
| `src/frontend/src/components/ui/status-badge.tsx` | Green = active/confirmed status |
| `src/frontend/src/components/ui/species-icon.tsx` | Species color coding |
| `src/frontend/src/components/features/dashboard/StatsCards.tsx` | Positive trend indicators |
| `src/frontend/src/components/features/billing/InvoiceTable.tsx` | Paid status |
| `src/frontend/src/components/features/stock/StockTable.tsx` | In-stock indicator |
| `src/frontend/src/components/features/stock/MovementTypeBadge.tsx` | Restock/positive movement |
| `src/frontend/src/components/features/calendar/AppointmentDetailSheet.tsx` | Confirmed status |
| `src/frontend/src/components/features/portal/booking/MyAppointments.tsx` | Confirmed status |

**Rule:** If emerald is used to mean "success/positive/confirmed" it stays as a semantic green color (mapped to `--success`). If it is used as "brand identity" it becomes `primary`.

---

## 7. Execution Priority

**Phase 1 -- Core identity (1 PR, ~2 files)**
1. Update `globals.css` with the new teal palette (both light and dark modes)
2. Verify the entire dashboard instantly adopts the new primary color via CSS variables

**Phase 2 -- Landing page unification (1 PR, ~15 files)**
1. Replace all hardcoded `emerald-*` and `teal-*` classes in landing components with `primary`/`secondary`/`accent` tokens
2. Result: landing page and dashboard now share the same teal identity

**Phase 3 -- Blog + Auth (1 PR, ~8 files)**
1. Same replacement in blog and auth components

**Phase 4 -- Semantic cleanup (1 PR, ~8 files)**
1. Ensure remaining emerald usage is intentionally "success green" and not brand-primary
2. Consider adding a `--success` CSS variable for explicit semantic usage

---

## 8. Visual Summary

```
BEFORE (3 identities):
  Landing:   -------[EMERALD GREEN]-------
  Dashboard: -------[CRAYOLA BLUE]--------
  Dark mode: -------[TEAL/CYAN]-----------

AFTER (1 identity):
  Everything: ------[DEEP TEAL]------------
  Dark mode:  ------[LIGHT TEAL]-----------
```

**Brand color: Deep Teal -- oklch(0.47 0.12 195)**

Not blue. Not green. The space between -- where trust meets nature, where technology meets animal care. Uniquely Vetara.

---

## Sources

- [Color Psychology in Veterinary Marketing (PetDesk)](https://petdesk.com/blog/the-psychology-of-color-in-veterinary-marketing)
- [Color Psychology for Veterinary Branding (LifeLearn)](https://www.lifelearn.com/2025/08/26/color-psychology-how-veterinary-practices-can-use-it-in-marketing-and-branding/)
- [Transformative Teal in Branding (Awesome Sauce Creative)](https://www.awesomesauce.in/insights/transformative-teal-in-branding)
- [The Art of Medical Colors in Healthcare Branding 2025 (ThinkPod)](https://thinkpodagency.com/the-art-of-medical-colors-in-healthcare-branding-in-2025/)
- [The Psychology of Colour in Branding: 2025 (Vivid Creative)](https://www.vividcreative.com/2025/07/25/the-psychology-of-colour-in-branding-2025s-mood-driven-palette/)
- [2026 Web Design Color Trends (Lounge Lizard)](https://www.loungelizard.com/web-design-color-trends/)
- [Why Color Psychology Still Works in 2026 (Seven Koncepts)](https://sevenkoncepts.com/blog/color-psychology-in-2026/)
- [B2B Brand Color Psychology (ACS Creative)](https://www.acscreative.com/insights/the-psychology-behind-color-in-b2b-branding-what-actually-converts/)
- [ezyVet Cloud Veterinary Software](https://www.ezyvet.com/)
- [Digitail All-in-one Cloud Veterinary Software](https://digitail.com/digitail-alternatives/)
- [Shepherd Veterinary Software](https://www.shepherd.vet/blog/8-best-ai-powered-veterinary-practice-management-software-platforms-2026-comparison-guide/)
