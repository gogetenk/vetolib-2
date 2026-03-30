# Mobile Responsiveness Audit - 2026-03-30

**Auditor:** Claude Opus 4.6 (static code review)
**Context:** UAE vets use tablets heavily. This audit checks every major page component for mobile/tablet usability.
**Methodology:** Static analysis of Tailwind classes, breakpoints, fixed widths, touch targets, and layout patterns across all page-level components and their children.

---

## Executive Summary

The codebase has **reasonable mobile foundations** -- most layouts use responsive grid breakpoints and the calendar auto-switches to day view on mobile. However, there are **several significant issues** that would degrade the tablet/mobile experience, particularly around data tables that only use `grid-cols-1 md:grid-cols-N` without any stacking UX, touch target sizing, and some hardcoded widths.

**Severity legend:** CRITICAL = broken on mobile, HIGH = poor UX, MEDIUM = suboptimal, LOW = minor polish.

---

## 1. Dashboard (`/dashboard`)

**File:** `src/frontend/src/app/[locale]/(dashboard)/dashboard/page.tsx`

### What works
- `PageContainer` applies `p-6 lg:p-8` -- appropriate padding at both sizes.
- `StatsCards` uses `grid-cols-1 sm:grid-cols-2 lg:grid-cols-4` -- proper responsive grid.
- All sections stack vertically via `space-y-6` in PageContainer.

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 1.1 | LOW | `text-[22px]` on the greeting title is a hardcoded font size. On very small screens (320px) this could be slightly large combined with a long user name. No `text-lg md:text-[22px]` scaling. |
| 1.2 | MEDIUM | `AnalyticsSection` charts use `h-40` and `h-48` fixed heights. On a 7" tablet in portrait, the pie chart with `outerRadius={60}` may render small and the legend may overlap. The `ResponsiveContainer` helps, but the hardcoded outer height is not responsive. |
| 1.3 | MEDIUM | `AnalyticsSection` grid is `grid-cols-1 lg:grid-cols-3`. On a standard 10" tablet in portrait (~768px), this stays single-column, wasting horizontal space. A `md:grid-cols-2` breakpoint would improve tablet layout. |
| 1.4 | LOW | `TodayAppointments` row items use `hidden sm:inline-block` for consultation type badge and `hidden md:inline` for vet name -- good progressive disclosure. No issues. |

---

## 2. Calendar / Appointments (`/appointments`)

**Files:**
- `src/frontend/src/app/[locale]/(dashboard)/appointments/page.tsx`
- `src/frontend/src/components/features/calendar/CalendarContainer.tsx`
- `src/frontend/src/components/features/calendar/WeekCalendarBody.tsx`
- `src/frontend/src/components/features/calendar/CalendarHeader.tsx`

### What works
- CalendarContainer detects `window.innerWidth < 768` and forces day view on mobile -- good.
- WeekCalendarBody dynamically adjusts column count: 1 on mobile, 3 on tablet, 7 on desktop.
- CalendarHeader hides vet filter and view toggle on mobile (`hidden md:flex`).
- "New Appointment" button text hidden on mobile, icon-only (`hidden md:inline`).

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 2.1 | HIGH | **Week view on tablet (768px-1024px) shows only 3 columns** but the user sees a "week" label. The UX is confusing -- they selected "week" but see 3 days. There is no horizontal scroll/swipe or pager to see the other 4 days. `visibleStartIndex` is hardcoded to `0` with no UI to change it. On a 10" tablet, this means Fri/Sat are never visible. |
| 2.2 | MEDIUM | **Calendar time slots have 8px tall touch targets** (`h-8` = 32px). Apple HIG recommends 44px minimum. Each half-hour slot is 32px -- a vet with thick gloves or on a small tablet will misfire taps. The full hour slot is 64px (`h-16`) which is fine, but the individual clickable halves are undersized. |
| 2.3 | MEDIUM | **Week view column minimum width** is `min-w-28` (112px). On a narrow 768px tablet with 3 visible columns plus the time column, this leaves very tight space for appointment text inside `AppointmentBlock`. Text will likely truncate aggressively. |
| 2.4 | LOW | The date label in CalendarHeader has `min-w-0 md:min-w-[200px]` -- on mobile the date text can get squished between the prev/next buttons if the day name is long (e.g., "Wednesday, March 30, 2026"). |
| 2.5 | MEDIUM | **`CalendarContainer` uses `overflow-x-auto`** on the week view wrapper, but without `scroll-snap` or touch momentum hints. A swipe gesture would feel janky on a tablet. |
| 2.6 | LOW | The appointments page forces `!max-w-screen-2xl` which allows very wide layouts. On an ultra-wide monitor this is fine, but on a tablet in landscape it is unused (no issue, just a note). |

---

## 3. Patient Detail (`/patients/[id]`)

**File:** `src/frontend/src/app/[locale]/(dashboard)/patients/[id]/page.tsx`

### What works
- Patient header uses `flex flex-col md:flex-row` -- stacks on mobile, side-by-side on tablet.
- Owner info panel uses `md:w-[280px]` with `border-t md:border-t-0 md:border-l` -- proper responsive border switching.
- Tab bar has `overflow-x-auto` -- scrollable on small screens.

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 3.1 | HIGH | **Tabs are scrollable but have no scroll indicator.** Six tabs ("Medical Records", "Prescriptions", "Vaccinations", "Health Alerts", "Weight", "Breeding") with `whitespace-nowrap` and `px-5 py-3`. On a 375px phone, only ~2-3 tabs are visible. There is no visual cue (gradient fade, scroll arrows, or visible scrollbar) that more tabs exist to the right. Users may never discover the "Breeding" or "Weight" tabs. |
| 3.2 | HIGH | **Vaccinations and Prescriptions tables use `grid-cols-1 md:grid-cols-2 lg:grid-cols-4` (vaccinations) and `grid-cols-1 md:grid-cols-3 lg:grid-cols-5` (prescriptions).** On mobile (`grid-cols-1`), each cell stacks vertically with no labels. A row showing vaccine name, date, next due, and vet becomes 4 unlabeled stacked text blocks -- the user cannot tell which is which. There are no per-cell labels or `<dt>/<dd>` patterns for the mobile view. |
| 3.3 | MEDIUM | **Tab buttons have `px-5 py-3`** -- the horizontal padding is generous but the touch target height is `py-3` = ~44px total including text. This is borderline OK. |
| 3.4 | LOW | The edit patient sheet uses `w-full sm:max-w-xl` -- good, covers full screen on mobile. |
| 3.5 | MEDIUM | **Owner info panel has a fixed width `md:w-[280px]`**. On a narrow tablet (768px) this takes ~36% of the width, leaving only ~488px for the patient info. If the patient name or breed is long, it may feel cramped. |

---

## 4. Appointments Table (via TodayAppointments on Dashboard)

**File:** `src/frontend/src/components/features/dashboard/TodayAppointments.tsx`

### What works
- Uses a flat list layout (`<ul>` with `<li>`) rather than a table -- naturally stacks.
- Progressive disclosure: consultation type badge hidden below `sm:`, vet name hidden below `md:`.
- Check-in button uses `size="sm"` -- reasonable touch target.

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 4.1 | MEDIUM | **Row items use `flex items-center justify-between`** with a lot of inline elements. On a 375px screen, the left side (type dot + time + patient link) and right side (status badge + check-in button) may collide or cause horizontal overflow. The patient name has `truncate` but the combined width of fixed elements (dot 10px + time 48px + gap + badge + button ~120px) leaves very little for the patient name. |
| 4.2 | LOW | Check-in button is `size="sm"` with `rounded-xl` -- actual rendered height is likely ~32px. Below the 44px touch target recommendation. |

---

## 5. Breeding Dashboard (`/breeding`)

**File:** `src/frontend/src/app/[locale]/(dashboard)/breeding/page.tsx`

### What works
- Summary cards use `grid-cols-1 md:grid-cols-3` -- stacks on mobile.
- Uses `PageContainer` for consistent padding.

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 5.1 | HIGH | **All data tables (pregnancies, litters, heat cycles) use `grid-cols-1 md:grid-cols-5` with no mobile card layout.** Same problem as patient detail: on mobile, 5 unlabeled fields stack vertically. A pregnancy row shows mating date, expected due, days left, checks -- all as plain text with no identifying labels in the mobile stacked view. |
| 5.2 | MEDIUM | The "View All Patients" button at the bottom has default `size` which renders at standard height -- should be fine for touch. |

---

## 6. Breeding Tab (Patient Detail sub-tab) and Pedigree Tree

**Files:**
- `src/frontend/src/components/features/patients/BreedingTab.tsx`
- `src/frontend/src/components/features/breeding/PedigreeTree.tsx`

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 6.1 | CRITICAL | **PedigreeTree has `min-w-[500px]`** inside an `overflow-x-auto` container. On a phone (375px), the tree requires horizontal scrolling. The tree grows wider with each generation (parents side-by-side with `gap-6`). A 3-generation pedigree could easily be 700-800px wide. There is no zoom, pinch, or pan support -- just native horizontal scroll, which is awkward on mobile. On a 10" tablet in portrait (~768px), the tree fits but is tight for deep pedigrees. |
| 6.2 | MEDIUM | **Pedigree node buttons are `min-w-[140px]` with `px-4 py-2.5`** -- touch target height is approximately 40px, slightly under the 44px recommendation. |
| 6.3 | HIGH | **Breeding tab data tables** (litters, pregnancies, heat cycles) all use the same `grid-cols-1 md:grid-cols-5` / `grid-cols-1 md:grid-cols-4` pattern with no mobile labels. Same unlabeled stacking issue as #5.1. |

---

## 7. Messaging (`/messages`)

**File:** `src/frontend/src/components/features/messaging/MessagesPage.tsx`

### What works
- **Excellent mobile split-view pattern.** Conversation list panel uses `showMobileDetail ? 'hidden md:flex' : 'flex'` and detail panel uses `!showMobileDetail ? 'hidden md:flex' : 'flex'`. This is a proper mobile-first show/hide pattern.
- Back button in detail header is `md:hidden` -- only shown on mobile.
- ConversationFilters stack `flex-col sm:flex-row`.

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 7.1 | MEDIUM | **Status action buttons in ConversationDetail** (`flex flex-col gap-1 items-end`) show up to 3 small buttons (Open, InProgress, Resolved, Closed minus current). Each is `size="sm"` with `text-[12px]`. These stack vertically in the top-right corner alongside the subject and metadata. On mobile, these will be squished against the back button and subject title. The layout may need wrapping to a separate row on mobile. |
| 7.2 | HIGH | **ConversationFilters** render ALL status buttons (5: All + 4 statuses) AND ALL category buttons (8: All + 7 categories) in `flex-wrap` rows. That is 13 filter buttons total. On a phone screen, these wrap into 3-4 rows, consuming significant vertical space above the conversation list -- potentially 150-200px. On a phone with ~600px viewport height minus header (64px) and title area (~60px), the conversation list gets only ~280px of visible height. |
| 7.3 | LOW | Conversation list panel width is `w-full md:w-80 lg:w-96` -- appropriate. On mobile it goes full-width. |
| 7.4 | MEDIUM | **The messages page uses custom padding `p-6 lg:p-8`** instead of PageContainer. On a 375px phone, 24px padding on each side leaves only 327px for content. This is fine for conversations but worth noting for consistency. |

---

## 8. Forms

### Appointment Form
**File:** `src/frontend/src/components/features/appointments/AppointmentForm.tsx`

| # | Severity | Description |
|---|----------|-------------|
| 8.1 | LOW | Form uses `grid-cols-1 md:grid-cols-2` -- properly goes full-width on mobile. Good. |
| 8.2 | MEDIUM | **Sticky submit bar** uses `-mx-6 px-6` to extend edge-to-edge. This works but on very small screens the "Cancel" and "Save" buttons may be close together. The buttons use default size which is ~40px height -- slightly under 44px touch target. |
| 8.3 | LOW | `Input` components use `text-[13px]` which is readable but small. iOS will zoom the page on inputs with font-size < 16px unless the viewport meta tag has `maximum-scale=1`. This may cause unwanted zoom-in when tapping input fields on iOS Safari. |

### Patient Form
**File:** `src/frontend/src/components/features/patients/PatientForm.tsx`

| # | Severity | Description |
|---|----------|-------------|
| 8.4 | LOW | Same `grid-cols-1 md:grid-cols-2` pattern. Full-width on mobile. |
| 8.5 | MEDIUM | Same iOS zoom issue with `text-[13px]` inputs (see 8.3). |
| 8.6 | LOW | Owner name field spans `md:col-span-2` -- full width on both mobile and desktop. Good. |

### Invoice Form
**File:** `src/frontend/src/app/[locale]/(dashboard)/billing/new/page.tsx`

| # | Severity | Description |
|---|----------|-------------|
| 8.7 | LOW | Uses `PageContainer variant="narrow"` (`max-w-4xl`) -- good constraint for forms. |

---

## 9. Patients List (`/patients`)

**File:** `src/frontend/src/app/[locale]/(dashboard)/patients/PatientsPageClient.tsx`

### What works
- Patient grid uses `grid-cols-1 md:grid-cols-2 lg:grid-cols-3` -- single column on mobile, 2 on tablet.
- Search input is `w-full` -- always full-width.
- Action buttons (Import CSV, Add Patient) stack in a flex row.

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 9.1 | MEDIUM | **Title bar** uses `flex items-center justify-between` with the title on the left and two buttons on the right. On a narrow phone (375px), the "Import CSV" button with icon + text (~140px) and "Add Patient" button (~130px) together are ~270px + gaps. Combined with the title (~100px), this overflows on phones below ~400px. Buttons should stack or the Import CSV button could be icon-only on mobile. |
| 9.2 | LOW | `PatientCard` uses `min-h` implicitly through content -- no fixed height issues. The card is touch-friendly as the entire card is a link. |
| 9.3 | LOW | Pagination buttons have `h-9` (36px) -- under the 44px touch recommendation. |

---

## 10. Billing / Invoice Table (`/billing`)

**File:** `src/frontend/src/components/features/billing/InvoiceTable.tsx`

### What works
- **Excellent pattern:** separate mobile card layout (`md:hidden`) and desktop table (`hidden md:table`). The mobile cards show invoice number, patient, status, date, and total in a card format.
- Mobile cards have `min-h-[44px]` -- explicitly meeting touch target requirements.

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 10.1 | MEDIUM | **Search input has a fixed `w-64`** (256px). On a 375px phone with 48px padding, only 327px is available. The 256px input fits but leaves the status filter dropdown pushed to the next line. This is handled by `flex-wrap gap-3`, so it works but looks slightly awkward. Making the search `w-full sm:w-64` would be cleaner. |
| 10.2 | LOW | Status filter dropdown uses `w-48` (192px) -- fine on mobile since it wraps to next line. |

---

## 11. Stock Page

**File:** `src/frontend/src/app/[locale]/(dashboard)/stock/StockPageClient.tsx` and `StockTable.tsx`

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 11.1 | HIGH | **StockTable uses `<Table>` (HTML table) without a mobile card layout alternative.** Unlike InvoiceTable which has dual layouts, StockTable renders a standard table on all screen sizes. On mobile, a table with columns for name, category, quantity, reorder level, expiry, status, and action buttons will cause horizontal overflow. |

---

## 12. Layout and Navigation

**Files:**
- `src/frontend/src/app/[locale]/(dashboard)/layout.tsx`
- `src/frontend/src/components/features/shell/Header.tsx`
- `src/frontend/src/components/features/shell/Sidebar.tsx`

### What works
- Dashboard layout is `flex h-screen flex-col` with header + scrollable main content -- correct mobile pattern.
- Header shows hamburger menu on mobile (`lg:hidden`), desktop nav tabs on large screens (`hidden lg:flex`).
- Mobile sidebar uses `Sheet` (slide-out drawer) -- proper mobile navigation pattern.
- Sidebar nav items have `px-3 py-2.5` -- approximately 40px height, close to 44px target.
- Header is `sticky top-0 z-50` -- stays visible during scroll, important for navigation on tablets.

### Issues

| # | Severity | Description |
|---|----------|-------------|
| 12.1 | MEDIUM | **Desktop nav breaks to hamburger at `lg:` (1024px).** A 10" iPad in landscape is ~1024px, which means it gets the hamburger menu rather than the tab bar. Since tablets are heavily used, the breakpoint should arguably be `md:` (768px) to show the tab bar on tablets in landscape. |
| 12.2 | LOW | Header height is `h-16` (64px) which is appropriate for tablet/mobile. |

---

## 13. Cross-cutting Issues

| # | Severity | Description |
|---|----------|-------------|
| 13.1 | HIGH | **iOS input zoom:** Multiple form inputs use `text-[13px]`. On iOS Safari, input fields with `font-size < 16px` trigger automatic page zoom on focus. This affects ALL forms: appointment, patient, billing, messaging search, patient search. The fix is either making input font-size 16px or adding `maximum-scale=1` to the viewport meta tag (the latter harms accessibility). |
| 13.2 | HIGH | **Unlabeled mobile grid stacking is a recurring pattern.** At least 6 data tables use `grid-cols-1 md:grid-cols-N` where the mobile single-column view stacks cells without any label. Affected components: VaccinationsTab, PrescriptionsTab, breeding dashboard (pregnancies, litters, heat cycles), BreedingTab (litters, pregnancies, heat cycles). The fix is either adding `<span className="md:hidden font-bold">Label:</span>` prefixes to each cell on mobile or switching to a card layout like InvoiceTable does. |
| 13.3 | MEDIUM | **Small touch targets are widespread.** Many `size="sm"` buttons render at ~32-36px height. Affected: check-in buttons, pagination buttons, status action buttons, filter buttons. The Apple HIG and WCAG recommend 44x44px minimum. |
| 13.4 | LOW | **No `scroll-behavior: smooth` or snap** on any horizontally scrollable container (tab bar, pedigree tree, week calendar). Touch scrolling works but feels mechanical. |

---

## Priority Fix Recommendations

### Immediate (before tablet deployment)
1. **Add mobile card layouts to all data tables** (or at minimum, add inline labels for the `grid-cols-1` mobile view). Pattern to follow: `InvoiceTable.tsx` which already implements `md:hidden` cards + `hidden md:table`. (#3.2, #5.1, #6.3, #13.2)
2. **Fix iOS input zoom** by increasing input font-size to 16px on mobile or using `text-[16px] md:text-[13px]`. (#13.1)
3. **Add scroll indicator to patient detail tabs** -- a gradient fade-out on the right edge, or scroll arrows. (#3.1)
4. **Fix week calendar tablet UX** -- either add swipe/pager for hidden days, or show all 7 narrow columns on tablet. (#2.1)

### Short-term
5. **Add mobile card layout to StockTable** (follow InvoiceTable pattern). (#11.1)
6. **Make PedigreeTree responsive** -- consider a vertical/list layout for mobile, or add pinch-zoom. (#6.1)
7. **Increase touch targets** on frequently-used buttons to minimum 44px (`h-11` or `min-h-[44px]`). (#13.3)
8. **Collapse filter buttons in messaging** on mobile (e.g., behind a dropdown or accordion). (#7.2)

### Nice-to-have
9. Lower the desktop-nav breakpoint from `lg:` to `md:` for tablet landscape. (#12.1)
10. Make patient list action buttons responsive (icon-only on mobile). (#9.1)
11. Add `scroll-snap` to horizontal containers. (#13.4)
12. Use responsive chart heights in AnalyticsSection. (#1.2)

---

## Components with Good Mobile Patterns (reference implementations)

These components demonstrate patterns that should be replicated across the app:

1. **`InvoiceTable.tsx`** -- dual layout with `md:hidden` card view + `hidden md:table` table view. Best mobile data table pattern in the codebase.
2. **`MessagesPage.tsx`** -- show/hide split-view with `showMobileDetail` state toggle. Proper mobile conversation pattern.
3. **`CalendarContainer.tsx`** -- responsive view forcing (day view on mobile) with `window.innerWidth` detection.
4. **`TodayAppointments.tsx`** -- progressive disclosure with `hidden sm:` and `hidden md:` for secondary information.
5. **`StatsCards.tsx`** -- `grid-cols-1 sm:grid-cols-2 lg:grid-cols-4` proper 3-tier responsive grid.
