# UX Audit -- Vetara (ex-Vetolib) -- March 2026

**Auditor**: UX Designer Agent
**Date**: 2026-03-22
**Scope**: All main screens + navigation shell
**Persona**: Veterinarian standing in exam room, gloves on, one hand free, 15-second attention budget.

---

## Summary

The app is functional and visually polished. The design system (shadcn/ui + Tailwind tokens, rounded-xl cards, consistent 13px body text) creates a professional baseline. However, the interface was built feature-by-feature, and it shows: cognitive overload on the dashboard, navigation inconsistencies between desktop/mobile, missing empty states in key places, and several screens that require too many clicks for the vet's most urgent tasks.

**Findings by severity**:
- CRITIQUE: 8
- IMPORTANT: 11
- NICE-TO-HAVE: 7

---

## 1. Navigation (Header + Sidebar)

**Files**: `src/frontend/src/components/features/shell/Header.tsx`, `Sidebar.tsx`

### CRITIQUE -- N1: Dual navigation architecture creates confusion

The app has TWO separate navigation systems with DIFFERENT items:
- **Desktop header** (top bar): Dashboard, Appointments, Messages, Medical Records, Patients, Billing, Stock (7 items, no Settings, no role filtering)
- **Mobile sidebar** (Sheet): Dashboard, Appointments, Patients, Medical Records, Billing, Messages, Stock, Team, Messaging Settings, Settings, Profile (up to 11 items, role-filtered)

Problems:
- The **order differs** between desktop and mobile (Messages is 3rd in header, 6th in sidebar)
- **Settings is missing** from the desktop header -- the vet must click the tiny user menu avatar to reach it
- The sidebar has **Team** and **Messaging Settings** as separate items that don't exist in the header
- Medical Records appears in both navigations but is just a redirect page saying "go to Patients"

**Solution**: Unify to ONE navigation model. The header tabs should match the mobile sidebar items in the same order. Remove Medical Records from nav entirely (it's not a real page). Move Settings to a visible icon in the header, not buried in a dropdown. Cap at 6 primary items + overflow.

### CRITIQUE -- N2: 7 top-level nav items is too many for a vet in a hurry

Seven tabs across the header is borderline. On a 1024px laptop, text gets cramped. The vet's actual daily flow touches 3 screens: **Appointments, Patients, Dashboard**. Billing, Stock, Messages, and Medical Records are secondary.

**Solution**: Primary tabs: Dashboard, Appointments, Patients, Messages (with badge). Secondary: Billing, Stock, Settings -- accessible via a "More" dropdown or a clearly grouped secondary area. This reduces scanning from 7 items to 4.

### IMPORTANT -- N3: Medical Records page is a dead end

`/medical-records/page.tsx` is a full-screen placeholder saying "Access patient records from each patient's profile page" with a single button "Go to Patients." This page shouldn't exist in navigation -- it wastes a click and confuses users.

**Solution**: Remove Medical Records from all navigation. Medical records are accessed contextually from each patient's detail page, which is the correct UX.

### IMPORTANT -- N4: Settings is invisible on desktop

On desktop, there is NO settings link in the header navigation. The only way to reach Settings is through the UserMenu dropdown (click avatar > "Settings"). This is a 2-click path to a primary admin function.

**Solution**: Add a gear icon to the header's right side, next to the UserMenu avatar.

### NICE-TO-HAVE -- N5: Logo link goes to /appointments, not /dashboard

The logo and sidebar logo both link to `/appointments`. This is unconventional -- most apps link the logo to the dashboard/home. If Appointments is truly the "home" for vets, fine, but it should be consistent with the fact that Dashboard exists as a separate page.

### NICE-TO-HAVE -- N6: "Vetolib" branding inconsistency

The header shows "Veto" (without "lib"), the mobile sidebar Sheet title says "Vetolib", and the desktop sidebar says "Vetolib". The product is now called "Vetara." All three should match the current brand name.

---

## 2. Dashboard

**File**: `src/frontend/src/app/[locale]/(dashboard)/dashboard/page.tsx`

### CRITIQUE -- D1: Dashboard is overloaded -- 6 sections compete for attention

The dashboard stacks vertically:
1. WelcomeBanner (onboarding)
2. Greeting + date
3. StatsCards (up to 5 cards in a 4-col grid)
4. SetupChecklist (onboarding)
5. TodayAppointments + RecentActivity (2-col grid)
6. AnalyticsSection (no-show rate + species pie chart + revenue bar chart)

For a vet who opens the app at 8am, the question is: "What do I have today?" That answer (TodayAppointments) is buried below 3 other sections. The vet must scroll past stats, greeting, and possibly onboarding to reach the most important information.

**Solution**: Reorder. The hero section should be TodayAppointments (the vet's next actions). Stats cards go second. Analytics goes behind a "Show analytics" toggle or a separate Analytics page. Onboarding elements should overlay/float, not push content down.

### IMPORTANT -- D2: StatsCards show up to 5 cards -- too many for ADMIN role

An ADMIN sees: Appointments Today, Pending Check-in, Unpaid Invoices, Total Patients, Today's Revenue. That's 5 cards in a 4-column grid, so one wraps to a new row on most screens. This breaks the visual rhythm.

**Solution**: Cap at 4 cards max. "Today's Revenue" can be merged into the Analytics section below. Or show 4 most relevant cards based on time of day (morning: appointments + check-in; afternoon: billing + revenue).

### IMPORTANT -- D3: Today's Revenue is hardcoded to "AED 0"

`StatsCards.tsx` line 187: `<LtrText>AED 0</LtrText>` -- this is a hardcoded string, not connected to actual data. Showing "AED 0" every day erodes trust.

**Solution**: Either wire it to real data or remove it until the backend provides it.

### NICE-TO-HAVE -- D4: Greeting uses browser locale detection, not user preference

`getInitialLocale()` reads `document.documentElement.lang`, which works but could be out of sync with the user's actual locale preference stored in the URL segment `[locale]`.

---

## 3. Appointments (Calendar)

**Files**: `CalendarContainer.tsx`, `CalendarHeader.tsx`

### CRITIQUE -- C1: CalendarHeader is too complex for mobile -- and mobile is forced to day view

The CalendarHeader has two rows:
- Row 1: Personnel filter (pill buttons)
- Row 2: Today button, prev/next navigation, view toggle (Day/Week/Month), New Appointment button

On mobile (<768px), the component forces `activeView = 'day'` and the view toggle is presumably hidden. But the entire CalendarHeader with its two rows still renders at mobile width. The vet filter dropdown, "Team" button, "Today" pill, navigation arrows, and "New Appointment" button must all fit. This is very cramped.

**Solution**: On mobile, collapse the header to a single row: date label + prev/next arrows + a "+" FAB (floating action button) for new appointment. Vet filter moves behind a filter icon. The "Today" button can be a tap on the date label.

### IMPORTANT -- C2: "New Appointment" button has decorative glow shadow but no urgency hierarchy

The new appointment button has `shadow-[0_4px_14px_0_rgba(48,62,245,0.39)]` -- a purple glow. This is visually heavy. On the calendar page, creating a new appointment is important, but the PRIMARY action for a vet is usually to VIEW/CHECK-IN an existing one. The glow draws attention to the wrong action.

**Solution**: Use a standard primary button for "New Appointment." Reserve visual emphasis (glow, animation) for the most urgent action (e.g., "1 patient waiting for check-in").

### IMPORTANT -- C3: Error handling is silent

`CalendarContainer.tsx` line 103: `catch { // Silently handle }`. If the appointment data fails to load, the vet sees an empty calendar with no indication that something went wrong. No error state, no retry button.

**Solution**: Add an ErrorState component similar to what Patients and Billing pages use. Show "Failed to load appointments" with a retry button.

### NICE-TO-HAVE -- C4: Slot click for quick appointment could pre-fill more context

When clicking an empty time slot, only date and time are passed to `QuickAppointmentForm`. Pre-filling the selected vet (if filtered) would save one interaction.

---

## 4. Patients

**File**: `PatientsPageClient.tsx`, `PatientCard.tsx`

### IMPORTANT -- P1: No pagination -- all patients loaded at once

`getPatients` returns all patients. For a clinic with 500+ patients, this means a long initial load and rendering 500 PatientCard components. The 3-column grid will be very long.

**Solution**: Add pagination or infinite scroll. Show 20-30 patients per page with a "Load more" button.

### IMPORTANT -- P2: Hardcoded colors remain in Patients page

Despite the recent color token refactoring (commit 476cb0d3), `PatientsPageClient.tsx` still uses:
- `text-[#061e44]` (line 69)
- `bg-[#303ef5]` (line 70, 88)
- `hover:bg-[#2530c4]` (line 88)
- `hover:bg-[#f4f6f9]` (line 80)
- `focus:ring-[#303ef5]/20` (line 106)

These bypass the design token system and will break if the theme changes.

**Solution**: Replace all hardcoded hex values with Tailwind tokens (`text-foreground`, `bg-primary`, etc.).

### IMPORTANT -- P3: "Owner:" label is hardcoded in English

`PatientCard.tsx` line 67: `<span className="text-muted-foreground">Owner:</span>`. Same for "Last visit:" (line 76) and "Next:" (line 78). These are not using the i18n system (`useTranslations`), so they won't translate for Arabic users.

**Solution**: Use translation keys for all visible text in PatientCard.

### NICE-TO-HAVE -- P4: Patient count text doesn't pluralize correctly for i18n

Line 110: `{patients.length} {patients.length === 1 ? 'patient' : 'patients'}` -- manual pluralization instead of using next-intl's pluralization support.

---

## 5. Billing

**File**: `InvoiceTable.tsx`

### CRITIQUE -- B1: Status filter options are hardcoded in English

Lines 35-41: `STATUS_OPTIONS` uses hardcoded English labels ("All statuses", "Draft", "Sent", etc.) instead of translation keys. The invoice table header also hardcodes "Invoices" (line 105) and "+ New Invoice" (line 107).

**Solution**: Use `useTranslations('billing')` for all visible text, including filter options and button labels.

### IMPORTANT -- B2: StatusBadge shows raw enum value

`StatusBadge` renders `{status}` directly (line 57), showing "DRAFT", "SENT", "PAID", "CANCELLED" in uppercase. This is developer jargon, not user-friendly labels.

**Solution**: Map status to user-friendly, translated labels: "Draft" instead of "DRAFT", etc.

### IMPORTANT -- B3: Search placeholder is hardcoded English

Line 116: `placeholder="Search invoice # or patient..."` -- not translated.

### NICE-TO-HAVE -- B4: Desktop table has 7 columns -- consider simplifying

Columns: Invoice #, Patient, Date, Subtotal, VAT, Total, Status. For most vets, Subtotal and VAT are secondary information. They could be shown on the invoice detail page instead.

---

## 6. Messages

**File**: `MessagesPage.tsx`, `ConversationFilters.tsx`

### CRITIQUE -- M1: Filter bar has 13 buttons visible simultaneously

The ConversationFilters component renders:
- Search input
- Status group: All, Open, InProgress, Resolved, Closed (5 buttons)
- Category group: All, MedicalUrgency, PostOperativeFollowUp, MedicalQuestion, AppointmentRequest, Administrative, Feedback, Other (8 buttons)

That is 13 filter buttons + 1 search input, all visible at once. On a 1024px screen, these wrap to 3+ rows, pushing the actual message list below the fold. This is severe cognitive overload.

**Solution**: Replace category buttons with a single Select dropdown. Keep only the status buttons (5 is acceptable as pill filters). This reduces the filter area from 13 interactive elements to 6.

### IMPORTANT -- M2: Conversation detail has "messages coming soon" placeholder

`MessagesPage.tsx` line 343: `{t('messages_coming_soon')}` -- the actual message thread is not implemented. The detail panel shows only metadata and a placeholder. This means the Messages page is essentially a read-only list with no reply functionality.

**Solution**: This is a feature gap, not a UX issue per se, but users who navigate here expecting to read/reply to messages will be confused. Consider adding an explicit "Under construction" state or hiding the Messages nav item until the feature is complete.

### IMPORTANT -- M3: Status change shows 3 buttons stacked vertically in the detail header

The detail header shows up to 3 status change buttons (all statuses except current) stacked vertically in the top-right corner. This takes significant vertical space and creates visual noise for what is a secondary action.

**Solution**: Use a single "Change status" dropdown button instead of 3 separate buttons.

---

## 7. Stock

**File**: `StockPageClient.tsx`, `StockTable.tsx`

### IMPORTANT -- S1: Hardcoded colors throughout Stock page

Same issue as Patients: `text-[#061e44]`, `bg-[#303ef5]`, `hover:bg-[#2530c4]`, `hover:bg-[#f4f6f9]` are used instead of tokens.

### IMPORTANT -- S2: No empty state for Stock

When `StockTable` has zero items (and no filters active), it shows a simple text: `{t('no_items')}` -- a single line of text centered on the page. This is a missed onboarding opportunity.

**Solution**: Use the same `EmptyState` component used by Patients and Billing, with an icon, description, and a CTA to "Add your first stock item."

### NICE-TO-HAVE -- S3: Desktop table actions are invisible until hover

`StockTable.tsx` line 249: `opacity-0 group-hover:opacity-100`. The Edit and Movement buttons on each row are invisible until the user hovers over the row. This is a discoverability problem -- new users won't know they can edit items.

**Solution**: Show actions at reduced opacity (e.g., `opacity-50 group-hover:opacity-100`) or always visible. On touch devices, hover doesn't exist.

---

## 8. Settings

**File**: `SettingsPageClient.tsx`

### IMPORTANT -- ST1: Hardcoded colors

Same pattern: `text-[#061e44]`, `bg-[#303ef5]`, `hover:border-[#303ef5]/30`, `bg-[#eef2fd]`, `group-hover:bg-[#303ef5]/15`.

### NICE-TO-HAVE -- ST2: Only 4 settings cards -- page feels sparse

The settings index shows 4 cards (Preferences, Team, Messaging, Notifications) in a 3-column grid. This looks fine but is a very shallow settings area for a veterinary practice management tool. No clinic profile, no working hours, no integrations, no billing settings.

---

## 9. Cross-cutting issues

### CRITIQUE -- X1: Inconsistent use of PageContainer

Some pages use `PageContainer` (Dashboard, Appointments, Billing, Medical Records), while others use raw `div className="p-6 lg:p-8 space-y-6"` (Patients, Stock, Settings, Messages). This creates inconsistent max-width constraints and padding behavior across pages.

- Dashboard: `PageContainer` (max-w-screen-xl)
- Appointments: `PageContainer` with `!max-w-screen-2xl` override
- Billing: `PageContainer`
- Patients: raw div (no max-width -- stretches full width)
- Stock: raw div (no max-width)
- Settings: raw div (no max-width)
- Messages: raw div (no max-width)

**Solution**: ALL pages should use `PageContainer`. The pages without it will stretch to fill ultra-wide monitors, making text lines unreadably long.

### CRITIQUE -- X2: Hardcoded colors across 4+ pages despite recent token refactoring

The recent commits (476cb0d3, 59b405e6) were supposed to centralize colors to CSS tokens. However, Patients, Stock, and Settings still use hardcoded hex values (`#061e44`, `#303ef5`, `#2530c4`, `#f4f6f9`, `#eef2fd`). The Billing page also hardcodes English strings. This undermines theme consistency.

Affected files:
- `src/frontend/src/app/[locale]/(dashboard)/patients/PatientsPageClient.tsx`
- `src/frontend/src/app/[locale]/(dashboard)/stock/StockPageClient.tsx`
- `src/frontend/src/app/[locale]/(dashboard)/settings/SettingsPageClient.tsx`
- `src/frontend/src/components/features/billing/InvoiceTable.tsx`

**Solution**: Complete the color token migration for all remaining pages.

### IMPORTANT -- X3: Mobile touch targets need verification

Most buttons use `h-10` (40px) which meets the 44px minimum touch target only if padding is counted. The stock mobile cards correctly use `min-h-[44px]` on buttons. Other pages should follow this pattern.

### NICE-TO-HAVE -- X4: Loading skeletons are inconsistent

- Dashboard stats: `Skeleton` component
- Patients: colored `div` with `animate-pulse`
- Calendar: custom skeleton with grid layout
- Stock: plain `div` with `animate-pulse`
- Billing: `Skeleton` component

The visual appearance during loading varies significantly across pages. Standardizing on the `Skeleton` component everywhere would create a more polished feel.

---

## Priority action plan

### Immediate (before next release)

| # | Finding | Severity | Effort |
|---|---------|----------|--------|
| X2 | Complete color token migration (Patients, Stock, Settings, Billing) | CRITIQUE | S |
| X1 | Wrap all pages in PageContainer | CRITIQUE | S |
| B1 | Translate all hardcoded English in Billing | CRITIQUE | S |
| D3 | Fix hardcoded "AED 0" in Today's Revenue | IMPORTANT | XS |
| C3 | Add error state to Calendar when fetch fails | IMPORTANT | S |
| P3 | Translate hardcoded labels in PatientCard | IMPORTANT | S |

### Next sprint

| # | Finding | Severity | Effort |
|---|---------|----------|--------|
| N1 | Unify desktop header and mobile sidebar navigation | CRITIQUE | M |
| N2 | Reduce to 4 primary nav items + overflow | CRITIQUE | M |
| M1 | Replace 13-button filter bar with dropdown | CRITIQUE | S |
| D1 | Reorder dashboard: TodayAppointments first | CRITIQUE | M |
| C1 | Simplify CalendarHeader for mobile | CRITIQUE | M |
| N3 | Remove Medical Records from navigation | IMPORTANT | XS |

### Backlog

| # | Finding | Severity | Effort |
|---|---------|----------|--------|
| P1 | Add pagination to Patients list | IMPORTANT | M |
| S2 | Add EmptyState to Stock | IMPORTANT | S |
| M3 | Replace status buttons with dropdown | IMPORTANT | S |
| D2 | Cap StatsCards at 4 | IMPORTANT | S |
| N4 | Add Settings icon to desktop header | IMPORTANT | S |
| B2 | Translate StatusBadge labels | IMPORTANT | XS |
| S3 | Make table actions visible without hover | NICE-TO-HAVE | XS |
| X4 | Standardize loading skeletons | NICE-TO-HAVE | M |
| N6 | Fix brand name consistency | NICE-TO-HAVE | XS |

---

## Appendix: Screen-by-screen click count analysis

| Task | Current clicks | Target | Gap |
|------|---------------|--------|-----|
| See today's appointments | 1 (nav) + scroll past 3 sections | 1 | Reorder dashboard |
| Create new appointment | 1 (nav) + 1 (button) = 2 | 2 | OK |
| Check in a patient | 1 (nav) + scroll + 1 (button) = 2-3 | 2 | OK with reorder |
| Find a patient by name | 1 (nav) + 1 (type in search) = 2 | 2 | OK |
| Create an invoice | 1 (nav) + 1 (button) = 2 | 2 | OK |
| Change message status | 1 (nav) + 1 (select msg) + 1 (click status btn) = 3 | 3 | OK |
| Access Settings | 1 (avatar) + 1 (dropdown) + 1 (link) = 3 | 1 | Add header icon |
| Add stock item | 1 (nav) + 1 (button) = 2 | 2 | OK |
| View patient medical record | 1 (nav to med records) + 1 (redirect to patients) + 1 (search) + 1 (click patient) = 4 | 2 | Remove redirect |
