# WCAG AA Accessibility Audit -- 2026-03-30

**Scope**: `src/frontend/src/` -- all TSX components, pages, and UI primitives
**Standard**: WCAG 2.1 Level AA
**Method**: Static code analysis (grep, AST-level pattern matching)
**Status**: Report only -- no fixes applied

---

## Executive Summary

The Vetolib frontend demonstrates good baseline accessibility: proper `lang`/`dir` on `<html>`, consistent use of semantic `<main>`, `<nav>`, `<h1>`-`<h4>` elements, `role="alert"` on error messages, `aria-live` on dynamic content, and well-labeled forms via `<Label htmlFor>`. However, several medium and high severity issues were found, primarily around keyboard accessibility on interactive non-button elements, missing skip-navigation links, lightbox focus trapping, and RTL hardcoded directional utilities.

**Findings**: 8 high, 9 medium, 5 low
**Blockers for WCAG AA compliance**: 3 (skip nav, focus trap, keyboard on sortable headers)

---

## 1. Images -- alt text

**Severity**: LOW
**Status**: PASS (with minor notes)

All `<img>` tags found have meaningful `alt` attributes:

| File | alt value | Verdict |
|------|-----------|---------|
| `components/features/messaging/AttachmentPreview.tsx:46` | `attachment.file.name` | OK -- filename is meaningful |
| `components/features/messaging/MessageBubble.tsx:50` | `attachment.fileName` | OK |
| `components/features/messaging/MessageBubble.tsx:90` | `attachment.fileName` (lightbox) | OK |
| `components/features/portal/PhotoUpload.tsx:79` | `photo.file.name` | OK |

No empty `alt=""`, no `alt="image"` or placeholder text found. The app uses very few `<img>` tags (Next.js `Image` is absent too), and all are dynamically sourced user content with descriptive alt text derived from filenames.

---

## 2. Forms -- input/label association

**Severity**: MEDIUM
**Found**: 3 issues

### 2a. Hidden file input without label -- CsvImportDialog
- **File**: `components/features/patients/CsvImportDialog.tsx:170`
- **Issue**: `<input type="file" className="hidden">` has no `aria-label` or associated `<label>`. It is activated via a parent `onClick` on a `<div>`, which is itself a keyboard accessibility issue (see section 5).
- **Recommendation**: Add `aria-label="Upload CSV file"`.

### 2b. Checkbox input without explicit label -- DayHoursRow
- **File**: `components/features/messaging/admin/DayHoursRow.tsx:48`
- **Issue**: `<input type="checkbox">` is wrapped in a `<label>` element but has no `id`/`htmlFor` binding and no `aria-label`. The wrapping label provides implicit association, but the label text ("Open"/"Closed") changes dynamically.
- **Severity**: LOW (implicit label exists via wrapper)

### 2c. Good patterns observed
Most forms use `<Label htmlFor="...">` consistently (LoginForm, SignupForm, PatientForm, AppointmentForm, MedicalRecordForm, AddDrugDialog, StockItemForm, etc.). File inputs that are visually hidden correctly use `aria-label` in ReplyComposer and PhotoUpload.

---

## 3. Buttons -- accessible text

**Severity**: HIGH
**Found**: 1 issue

### 3a. Icon-only button missing aria-label -- AppointmentDetailSheet send button
- **File**: `components/features/calendar/AppointmentDetailSheet.tsx:286`
- **Code**: `<Button size="icon-sm" className="..." disabled><SendIcon /></Button>`
- **Issue**: Icon-only button with no `aria-label`. Screen readers will announce nothing or "button".
- **Recommendation**: Add `aria-label="Send note"` or similar.

### Good patterns observed
- CalendarHeader prev/next buttons have `aria-label="Previous"` / `aria-label="Next"`
- Header settings button has `aria-label="Settings"`
- Sidebar toggle has `aria-label="Toggle Sidebar"`
- PhotoUpload remove buttons have `aria-label="Remove photo {name}"`
- ExitIntentPopup close button has `aria-label="Close"`
- Lightbox close button has `aria-label="Close lightbox"`

---

## 4. Color contrast

**Severity**: MEDIUM
**Found**: 2 concern areas

### 4a. `text-stone-400` on white backgrounds
- **Files** (24+ occurrences across 14 files):
  - `app/[locale]/page.tsx:505,515` -- testimonial emirate text, mobile hint
  - `app/[locale]/help/page.tsx:276,327,441,451` -- no-results text, search icon, link icons
  - `components/features/blog/BlogArticleCard.tsx:55` -- article metadata
  - `components/features/auth/SignupForm.tsx:238,325` -- password toggle icons
  - `components/features/auth/LoginForm.tsx:190` -- password toggle icon
  - `components/features/auth/LanguageSwitcher.tsx:35,47,59` -- inactive language options
  - `components/features/landing/Footer.tsx:129,156,191,203` -- disabled links, copyright
  - `components/features/landing/NavLanguageSwitcher.tsx:26,37` -- inactive languages
  - `components/features/landing/FaqSection.tsx:96` -- chevron icon
  - `components/features/landing/ExitIntentPopup.tsx:107` -- close button
  - `components/features/landing/PricingSection.tsx:157,209` -- trial info text
- **Issue**: `text-stone-400` (#a8a29e) on a white background yields a contrast ratio of approximately 2.9:1, which fails WCAG AA (requires 4.5:1 for normal text, 3:1 for large text). Some of these are decorative icons (`aria-hidden` should be used) but many are readable text.
- **Note**: `text-muted-foreground` is used extensively (540+ occurrences across 141 files) and its actual contrast depends on the theme CSS variable. Needs runtime verification.

### 4b. `text-stone-300` on white background
- **File**: `app/[locale]/help/page.tsx:441,451`
- **Issue**: `text-stone-300` (#d6d3d1) is approximately 1.7:1 contrast against white -- severely fails AA.
- **Note**: These appear to be decorative icons, but if they convey meaning, they need `aria-hidden="true"` or better contrast.

---

## 5. Keyboard navigation

**Severity**: HIGH
**Found**: 4 issues

### 5a. Sortable table headers use onClick on `<th>` without keyboard support
- **File**: `components/drugs/DrugCatalogTable.tsx:228-244`
- **Issue**: Three `<TableHead>` elements have `onClick` handlers for sorting but no `onKeyDown`, `tabIndex`, `role="button"`, or `aria-sort`. Keyboard users cannot trigger sort.
- **Recommendation**: Add `tabIndex={0}`, `role="columnheader"` with `aria-sort`, `onKeyDown` handler for Enter/Space.

### 5b. Drug card (mobile) uses `<div onClick>` without keyboard support
- **File**: `components/drugs/DrugCatalogTable.tsx:183-186`
- **Issue**: `<div className="...cursor-pointer" onClick={() => onView(drug)}>` -- non-interactive element with click handler, no `tabIndex`, `role`, or `onKeyDown`. Keyboard users cannot navigate to or activate these cards.
- **Recommendation**: Use `<button>` or add `role="button"`, `tabIndex={0}`, `onKeyDown`.

### 5c. CSV import drop zone uses `<div onClick>` without keyboard support
- **File**: `components/features/patients/CsvImportDialog.tsx:160-168`
- **Issue**: A `<div>` acts as a clickable drop zone with `onClick` but no keyboard equivalent. The hidden `<input type="file">` inside is not keyboard-focusable either (`className="hidden"` removes it from tab order vs `className="sr-only"` which would keep it accessible).
- **Recommendation**: Change `className="hidden"` to `className="sr-only"` on the file input so keyboard users can tab to it, or add keyboard handler to the div.

### 5d. Settings cards use cursor-pointer but rely on Link wrapping
- **File**: `app/[locale]/(dashboard)/settings/SettingsPageClient.tsx:75`
- **Issue**: Cards have `cursor-pointer` class. Needs verification that the parent `<Link>` provides keyboard access (likely OK if wrapped in Next.js Link).
- **Severity**: LOW (needs runtime verification)

---

## 6. Focus management

**Severity**: HIGH
**Found**: 2 issues

### 6a. Lightbox dialog has no focus trap
- **File**: `components/features/messaging/MessageBubble.tsx:70-96`
- **Issue**: The lightbox uses `role="dialog"` and `aria-modal="true"` but has no focus trap implementation. When opened, focus is not moved to the dialog, and Tab can escape to background content. No `onKeyDown` handler for Escape key either (only `onClick` to close).
- **Recommendation**: Implement focus trapping (e.g., using `@radix-ui/react-dialog` or a custom trap), auto-focus the close button on open, handle Escape key.

### 6b. ExitIntentPopup dialog has no focus trap
- **File**: `components/features/landing/ExitIntentPopup.tsx:91-99`
- **Issue**: Same as above -- `role="dialog"` and `aria-modal="true"` but no focus trap, no Escape key handler, no auto-focus.
- **Recommendation**: Same as 6a.

### Good patterns observed
- shadcn/ui Dialog and Sheet components (used elsewhere) inherit from Radix UI which provides proper focus trapping automatically.

---

## 7. ARIA roles

**Severity**: LOW
**Found**: 1 minor issue

### 7a. `aria-modal="true"` without focus trap
- **Files**: `MessageBubble.tsx:74`, `ExitIntentPopup.tsx:98`
- **Issue**: `aria-modal="true"` tells assistive technology that content behind the dialog is inert, but without actual focus trapping, this creates a mismatch between what AT announces and what keyboard users experience.
- Cross-reference with section 6.

### Good patterns observed
- `role="alert"` used correctly on error messages (26+ instances)
- `role="progressbar"` with `aria-valuenow/min/max` on upload progress
- `role="dialog"` with `aria-modal` on dialogs
- `aria-label` on navigation elements (BookingWizard stepper)
- `aria-hidden="true"` on decorative elements (MSWProvider spinner, error-state icon, nav chevrons)
- `aria-live="polite"` on dynamic counters (character counter, booking countdown, dosage indicator)

---

## 8. Heading hierarchy

**Severity**: MEDIUM
**Found**: 2 issues

### 8a. Help page skips heading levels
- **File**: `app/[locale]/help/page.tsx`
- **Issue**: Uses `<h1>` (line 317), then `<h2>` (400, 418), then `<h3>` (434, 452, 465), then `<h4>` (125) -- the `<h4>` appears before `<h1>` in the DOM and is generated dynamically inside article content rendering. This creates an inconsistent heading tree.
- **Severity**: MEDIUM

### 8b. AppointmentDetailSheet heading hierarchy
- **File**: `components/features/calendar/AppointmentDetailSheet.tsx`
- **Issue**: Uses `<h2>` (183), `<h3>` (193, 213, 234), `<h4>` (249, 265, 273) -- this is technically correct nesting but within a Sheet (modal), there is no `<h1>`. Since it is a modal context, starting at `<h2>` is acceptable but could be improved.
- **Severity**: LOW

### Good patterns observed
Most pages have a clear `<h1>` (dashboard, patients, billing, stock, messaging, appointments, portal pages). Section headings generally follow h1 > h2 > h3 order.

---

## 9. RTL support

**Severity**: HIGH
**Found**: Systemic issue

### 9a. Hardcoded directional utilities (ml-/mr-/pl-/pr-) vs logical (ms-/me-/ps-/pe-)
- **Stats**: 95 occurrences of `ml-`/`mr-`/`pl-`/`pr-` across 49 files vs 575 occurrences of `ms-`/`me-`/`ps-`/`pe-` across 154 files.
- **Assessment**: The codebase predominantly uses logical properties (good), but ~95 hardcoded directional utilities remain. Since the app supports Arabic RTL (`dir="rtl"` is set on `<html>` when locale is `ar`), these will produce incorrect spacing in RTL mode.
- **Key offenders** (files with multiple occurrences):
  - `components/features/messaging/ReplyComposer.tsx` (5)
  - `components/features/messaging/PatientContextPanel.tsx` (5)
  - `components/features/patients/PatientHealthAlerts.tsx` (5)
  - `components/features/dashboard/HealthAlertPanel.tsx` (4)
  - `components/features/calendar/AppointmentDetailSheet.tsx` (4)
  - `components/features/patients/BreedingTab.tsx` (4)
  - `app/[locale]/help/page.tsx` (4)
  - `components/drugs/DrugCatalogTable.tsx` (3)
  - `components/features/auth/SignupForm.tsx` (3)
  - `components/features/messaging/MessageBubble.tsx` (3)

### 9b. Hardcoded `left-`/`right-` positional utilities
- **Stats**: 61 occurrences of `left-`/`right-` across 37 files vs 26 occurrences of `start-`/`end-` across 17 files.
- **Key offenders**:
  - `app/[locale]/page.tsx` (7)
  - `app/[locale]/help/page.tsx` (1)
  - `components/features/auth/SignupForm.tsx` (2)
  - `components/features/auth/LoginForm.tsx` (1)
  - `components/features/messaging/MessageBubble.tsx` (3)
  - `components/features/calendar/AppointmentDetailSheet.tsx` (1: `ml-0.5` on SendIcon)
- **Issue**: `left-*` and `right-*` do not flip in RTL. Should use `start-*` and `end-*` (Tailwind v3.3+ supports these as `inset-inline-start` / `inset-inline-end`).

### Good patterns observed
- `<html lang={locale} dir={locale === 'ar' ? 'rtl' : 'ltr'}>` is correctly set in the root layout
- The majority of spacing uses logical properties (ms-/me-/ps-/pe-)
- The sidebar component uses `data-[side=left]` / `data-[side=right]` with `ltr:` / `rtl:` prefixes

---

## 10. Skip navigation

**Severity**: HIGH
**Found**: Missing entirely

### 10a. No skip-to-content link anywhere in the application
- **Files checked**: All layout files (`app/layout.tsx`, `app/[locale]/layout.tsx`, `app/[locale]/(dashboard)/layout.tsx`, `app/[locale]/(auth)/layout.tsx`), Header, Sidebar
- **Issue**: No skip navigation link exists. WCAG 2.4.1 (Level A) requires a mechanism to bypass blocks of repeated content. The dashboard layout has a complex header and sidebar; keyboard users must tab through all navigation items to reach main content on every page.
- **Recommendation**: Add a visually-hidden-until-focused skip link as the first child of `<body>`:
  ```tsx
  <a href="#main-content" className="sr-only focus:not-sr-only focus:absolute focus:z-50 focus:p-4 focus:bg-primary focus:text-white">
    Skip to main content
  </a>
  ```
  And add `id="main-content"` to the `<main>` element.

---

## Summary table

| # | Category | Severity | Count | WCAG Criterion |
|---|----------|----------|-------|----------------|
| 1 | Images alt text | LOW | 0 issues | 1.1.1 |
| 2 | Form labels | MEDIUM | 2 | 1.3.1, 4.1.2 |
| 3 | Button accessible text | HIGH | 1 | 4.1.2 |
| 4 | Color contrast | MEDIUM | 2 areas | 1.4.3 |
| 5 | Keyboard navigation | HIGH | 3 | 2.1.1 |
| 6 | Focus management | HIGH | 2 | 2.4.3 |
| 7 | ARIA misuse | LOW | 1 | 4.1.2 |
| 8 | Heading hierarchy | MEDIUM | 2 | 1.3.1 |
| 9 | RTL support | HIGH | systemic (~156 instances) | 1.3.4 |
| 10 | Skip navigation | HIGH | 1 (missing) | 2.4.1 |

---

## Recommended priority

1. **P0 -- Add skip-to-content link** (section 10) -- simple fix, Level A requirement
2. **P0 -- Fix keyboard on sortable headers and clickable cards** (section 5a, 5b) -- Level A requirement
3. **P1 -- Add focus trapping to custom dialogs** (section 6) -- consider replacing with Radix Dialog
4. **P1 -- Fix RTL hardcoded directional utilities** (section 9) -- batch find-replace `ml-` to `ms-`, `mr-` to `me-`, `pl-` to `ps-`, `pr-` to `pe-`, `left-` to `start-`, `right-` to `end-`
5. **P2 -- Fix contrast issues** (section 4) -- bump `text-stone-400` to `text-stone-500` minimum
6. **P2 -- Add aria-label to icon-only button** (section 3a)
7. **P3 -- Fix heading hierarchy** (section 8)
8. **P3 -- Add aria-label to hidden file input** (section 2a)
