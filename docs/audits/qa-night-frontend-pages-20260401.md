# QA Audit: Frontend Pages Inventory & Dead Route Detection

**Date**: 2026-04-01
**Scope**: `src/frontend/src/app/[locale]/` — all page.tsx files
**Total pages found**: 53 (including root redirect)

---

## 1. Full Page Inventory

### 1.1 Public / Marketing Pages

| # | URL Pattern | Linked From | data-testid | i18n |
|---|---|---|---|---|
| 1 | `/` | Root redirect to `/en` | None (redirect) | No |
| 2 | `/[locale]` | Landing page (entry point) | 26 | Yes (getTranslations) |
| 3 | `/[locale]/pricing` | **NOT LINKED** (only `#pricing` anchor on landing) | 3 | Yes |
| 4 | `/[locale]/blog` | Landing nav, footer | 7 | Yes |
| 5 | `/[locale]/blog/[slug]` | Blog listing cards | 14 | Yes |
| 6 | `/[locale]/pet-owners` | Landing nav | 22 | Yes |
| 7 | `/[locale]/developers` | Landing nav, footer | 21 | Yes |
| 8 | `/[locale]/help` | Footer | 20 | Yes (useTranslations) |
| 9 | `/[locale]/privacy` | Signup form, footer, terms page | 8 | Yes (useTranslations) |
| 10 | `/[locale]/terms` | Signup form, footer, privacy page | 8 | Yes (useTranslations) |
| 11 | `/[locale]/shared/[token]` | Shared via link (pet owner share feature) | 0 (delegated to SharedRecordView: 13) | Yes (via component) |

### 1.2 Auth Pages

| # | URL Pattern | Linked From | data-testid | i18n |
|---|---|---|---|---|
| 12 | `/[locale]/(auth)/login` | Landing nav ("Sign in") | 0 (delegated to LoginForm: 10) | Yes (via component) |
| 13 | `/[locale]/(auth)/signup` | Landing nav CTA | 0 (delegated to SignupForm: 25) | Yes (via component) |

### 1.3 Dashboard Pages (authenticated)

| # | URL Pattern | Linked From | data-testid in page | i18n |
|---|---|---|---|---|
| 14 | `/[locale]/(dashboard)/dashboard` | Sidebar + Header nav | 4 | Yes (via components) |
| 15 | `/[locale]/(dashboard)/appointments` | Sidebar + Header nav | 1 | Yes (via components) |
| 16 | `/[locale]/(dashboard)/appointments/new` | Appointments page button | 3 | Yes |
| 17 | `/[locale]/(dashboard)/appointments/[id]` | Appointments list row click | 2 | Yes |
| 18 | `/[locale]/(dashboard)/patients` | Sidebar + Header nav | 0 (delegated to PatientsPageClient: 15) | Yes (via component) |
| 19 | `/[locale]/(dashboard)/patients/new` | Patients page button | 3 | Yes |
| 20 | `/[locale]/(dashboard)/patients/[id]` | Patients list row click | 41 | Yes (useTranslations) |
| 21 | `/[locale]/(dashboard)/patients/[id]/records/new` | Patient detail page | 6 | Yes |
| 22 | `/[locale]/(dashboard)/billing` | Sidebar + Header nav | 2 | Yes (via components) |
| 23 | `/[locale]/(dashboard)/billing/new` | Billing page button | 3 | Yes |
| 24 | `/[locale]/(dashboard)/billing/[id]` | Billing list row click | 3 | Yes |
| 25 | `/[locale]/(dashboard)/messages` | Sidebar + Header nav | 0 (delegated to MessagesPage: 18) | Yes (via component) |
| 26 | `/[locale]/(dashboard)/messages/[id]` | Messages list click | 0 (delegated to ConversationDetailPage: 13) | Yes (via component) |
| 27 | `/[locale]/(dashboard)/breeding` | Sidebar (VET/ADMIN only) | 21 | Yes |
| 28 | `/[locale]/(dashboard)/stock` | Sidebar + Header nav | 0 (delegated to StockPageClient: 5) | Yes (via component) |
| 29 | `/[locale]/(dashboard)/stock/drugs` | Stock page navigation | 9 | Yes (useTranslations) |
| 30 | `/[locale]/(dashboard)/stock/drugs/[id]` | Drug list row click | 5 | Yes (useTranslations) |
| 31 | `/[locale]/(dashboard)/stock/history` | Stock page navigation | 6 | Yes (useTranslations) |
| 32 | `/[locale]/(dashboard)/profile` | Sidebar bottom nav | 0 (delegated to ProfilePageClient: 9) | Yes (via component) |
| 33 | `/[locale]/(dashboard)/settings` | Sidebar bottom nav + Header shortcut | 0 (delegated to SettingsPageClient: 2) | Yes (via component) |
| 34 | `/[locale]/(dashboard)/settings/team` | Sidebar (ADMIN only) | 5 | Yes (useTranslations) |
| 35 | `/[locale]/(dashboard)/settings/notifications` | Settings page navigation | 19 | Yes (useTranslations) |
| 36 | `/[locale]/(dashboard)/settings/preferences` | Settings page navigation | 4 | Yes (useTranslations) |
| 37 | `/[locale]/(dashboard)/settings/working-hours` | Settings page navigation | 9 | Yes (useTranslations) |
| 38 | `/[locale]/(dashboard)/settings/messaging/templates` | Sidebar (ADMIN only) | 0 (delegated to TemplatesPage: 12) | Yes (via component) |
| 39 | `/[locale]/(dashboard)/settings/messaging/whatsapp` | Messaging settings sub-nav | 0 (delegated to WhatsAppSettingsPage: 10) | Yes (via component) |
| 40 | `/[locale]/(dashboard)/settings/messaging/hours` | Messaging settings sub-nav | 0 (delegated to MessagingHoursPage: 4) | Yes (via component) |
| 41 | `/[locale]/(dashboard)/settings/messaging/stats` | Messaging settings sub-nav | 0 (delegated to TriageStatsPage: 11) | Yes (via component) |

### 1.4 Portal Pages (pet owner facing)

| # | URL Pattern | Linked From | data-testid | i18n |
|---|---|---|---|---|
| 42 | `/[locale]/portal/[clinicSlug]` | Portal landing (conversations) | 0 (delegated to PortalLanding: 15) | Yes (via component) |
| 43 | `/[locale]/portal/[clinicSlug]/new` | Portal landing (new message button) | 0 (delegated to NewMessageForm: 13) | Yes (via component) |
| 44 | `/[locale]/portal/[clinicSlug]/consent` | Consent flow before portal access | 0 (delegated to ConsentScreen: 7) | Yes (via component) |
| 45 | `/[locale]/portal/[clinicSlug]/export` | Portal menu | 0 (delegated to ExportPage: 6) | Yes (via component) |
| 46 | `/[locale]/portal/[clinicSlug]/conversations/[id]` | Conversation list click | 0 (delegated to PortalConversation: 14) | Yes (via component) |
| 47 | `/[locale]/portal/[clinicSlug]/pets` | Portal bottom tab nav | 0 (delegated to PortalPetsList: 7) | Yes (via component) |
| 48 | `/[locale]/portal/[clinicSlug]/pets/[animalId]` | Pets list click | 0 (delegated to PortalAnimalDetail: 6) | Yes (via component) |
| 49 | `/[locale]/portal/[clinicSlug]/animals/[id]` | **DUPLICATE** of pets/[animalId]? | 34 | Yes (useTranslations) |
| 50 | `/[locale]/portal/[clinicSlug]/book` | Portal bottom tab nav (appointments) | 0 (delegated to BookingLanding: 12) | Yes (via component) |
| 51 | `/[locale]/portal/[clinicSlug]/book/new` | Booking landing CTA | 0 (delegated to BookingWizard: 7) | Yes (via component) |
| 52 | `/[locale]/portal/[clinicSlug]/book/appointments` | Booking flow | 0 (delegated to MyAppointments: 16) | Yes (via component) |
| 53 | `/[locale]/portal/[clinicSlug]/book/appointments/[id]` | Appointments list click | 0 (delegated to AppointmentDetail: 23) | Yes (via component) |

---

## 2. Dead Routes (Orphaned Pages)

### 2.1 Confirmed Orphaned

| Page | Issue | Severity |
|---|---|---|
| `/[locale]/pricing` | Exists as standalone page but no link points to it. Landing page only uses `#pricing` anchor (section on landing). Footer also uses `#pricing`. | **Medium** — SEO page exists but is unreachable from navigation. |

### 2.2 Potential Duplicates

| Page A | Page B | Issue |
|---|---|---|
| `/portal/[clinicSlug]/animals/[id]` | `/portal/[clinicSlug]/pets/[animalId]` | Two separate routes for animal detail in portal. `animals/[id]` has 34 data-testid + useTranslations directly in page. `pets/[animalId]` delegates to PortalAnimalDetail component. Likely legacy duplication. | **Medium** |

### 2.3 Missing Portal Profile Page

The `PortalLayout.tsx` navigation includes a "profile" tab pointing to `${basePath}/profile`, but **no `page.tsx` exists** at `portal/[clinicSlug]/profile/`. This will result in a 404 for portal users.

**Severity**: **High** — portal users see a "Profile" tab that leads to a 404.

---

## 3. Missing Frontend Pages (Backend Features Without UI)

### 3.1 Staff Scheduling
- **Backend**: Working hours endpoints exist (`/api/v1/working-hours`) and have a settings page.
- **Frontend**: Only `settings/working-hours` page exists (for configuring clinic hours).
- **Missing**: No dedicated staff scheduling/shift management page. The working-hours page covers clinic operating hours, not individual staff shift scheduling.
- **Severity**: Low — may be intentional (MVP scope).

### 3.2 Clinic Group Dashboard
- **Backend**: Full ClinicGroup CRUD exists (create group, add/remove clinics, list clinics, switch clinic).
- **Frontend**: `ClinicSwitcher` component exists in the header for switching between clinics.
- **Missing**: No dedicated admin dashboard for managing clinic groups (add/remove clinics, view group-level stats). The ClinicSwitcher handles switching but there is no management UI.
- **Severity**: Medium — backend supports multi-clinic groups but admin has no way to manage group membership from the UI.

### 3.3 Monthly Analytics / Reporting
- **Backend**: E-Reporting endpoints exist (`/api/v1/billing/e-reporting/periods`, `/submit`). Dashboard has `AnalyticsSection` component.
- **Frontend**: Dashboard page shows some analytics via `AnalyticsSection` and `StatsCards`.
- **Missing**: No dedicated reporting/analytics page. No e-reporting UI for UAE government reporting. No monthly report generation page.
- **Severity**: Medium — e-reporting is a regulatory feature (UAE) with backend support but no frontend.

### 3.4 Drug Alternatives / Interaction Checker
- **Backend**: Endpoints exist for `check-interactions` and there is an `AlternativeSuggestions.tsx` component.
- **Frontend**: `AlternativeSuggestions` component exists but is likely embedded in patient/prescription flow, not a standalone page.
- **Missing**: No standalone drug interaction checker or alternatives browser page.
- **Severity**: Low — feature exists inline within prescription workflow, standalone page may not be needed.

### 3.5 Waitlist Management
- **Backend**: Full waitlist CRUD (`AddToWaitlist`, `RemoveFromWaitlist`, `ListWaitlistEntries`).
- **Frontend**: No waitlist page or component found.
- **Missing**: No UI for viewing/managing the appointment waitlist.
- **Severity**: Medium — backend feature with no frontend access.

### 3.6 No-Show Prediction
- **Backend**: Endpoints for `no-show-prediction/{appointmentId}` and batch predictions.
- **Frontend**: `AnalyticsSection` references no-show but no dedicated UI.
- **Missing**: No UI surface for viewing/acting on no-show predictions.
- **Severity**: Low — could be shown inline on appointment detail.

### 3.7 Referral System
- **Backend**: `GetOrCreateReferralCode` endpoint exists.
- **Frontend**: No referral page or component found.
- **Missing**: No UI for viewing or sharing referral codes.
- **Severity**: Low.

### 3.8 Patient Import (CSV/FHIR)
- **Backend**: `ImportPatients`, `ImportPatientFhir`, `GetImportTemplate` endpoints exist.
- **Frontend**: No import page or component found.
- **Missing**: No UI for bulk patient import.
- **Severity**: Medium — important for onboarding new clinics.

### 3.9 Patient Photo Upload
- **Backend**: `UploadPatientPhoto`, `GetPatientPhoto`, `DeletePatientPhoto` endpoints exist.
- **Frontend**: Not verified if patient detail page includes photo upload widget.
- **Severity**: Low — may be embedded in patient detail.

---

## 4. data-testid Coverage

### 4.1 Summary

Pages are split into two categories:
- **Rich pages**: contain UI directly in page.tsx (testids counted directly)
- **Thin wrappers**: delegate to a component (testids counted in the component)

### 4.2 Pages With Good Coverage (5+ testids, counting delegated components)

| Page | testid Count (page + component) |
|---|---|
| Patients [id] | 41 |
| Portal animals [id] | 34 |
| Signup | 25 (via SignupForm) |
| Pet Owners landing | 22 |
| Breeding | 21 |
| Developers | 21 |
| Help | 20 |
| Settings/Notifications | 19 |
| Messages list | 18 (via MessagesPage) |
| My Appointments (portal) | 16 (via MyAppointments) |
| Patients list | 15 (via PatientsPageClient) |
| Portal Landing | 15 (via PortalLanding) |
| Portal Conversation | 14 (via PortalConversation) |
| Blog [slug] | 14 |
| Conversation detail | 13 (via ConversationDetailPage) |
| New Message (portal) | 13 (via NewMessageForm) |
| Shared Record | 13 (via SharedRecordView) |
| Booking Landing | 12 (via BookingLanding) |
| Messaging Templates | 12 (via TemplatesPage) |
| Triage Stats | 11 (via TriageStatsPage) |
| Login | 10 (via LoginForm) |
| WhatsApp Settings | 10 (via WhatsAppSettingsPage) |
| Booking Success | 10 (via BookingSuccess) |

### 4.3 Pages With Weak Coverage (0-4 testids total)

| Page | testid Count | Issue |
|---|---|---|
| Settings index | 2 (via SettingsPageClient) | Settings hub page with navigation links likely needs more testids |
| Appointments list | 1 | Relies heavily on CalendarContainer component — need to verify sub-component coverage |
| Pricing | 3 | Standalone pricing page, interactive elements may be in PricingPageClient |
| Stock index | 5 (via StockPageClient) | Acceptable for a hub page |
| Dashboard | 4 | Hub page delegates to multiple components (StatsCards, TodayAppointments, etc.) |
| Messaging Hours | 4 (via MessagingHoursPage) | Low for a settings page with time pickers |
| Profile | 9 (via ProfilePageClient) | Acceptable |

### 4.4 Pages With Zero testid (page.tsx only, no delegation counted)

These pages are all thin wrappers that delegate to components. The delegated components DO have testids (counts shown in sections above). This pattern is acceptable — the page.tsx files correctly delegate all rendering.

---

## 5. i18n Coverage

### 5.1 Pages Using i18n Directly (useTranslations or getTranslations in page.tsx)

12 pages use i18n directly: patients/[id], settings/notifications, settings/preferences, settings/team, settings/working-hours, stock/drugs, stock/drugs/[id], stock/history, help, privacy, terms, portal/animals/[id].

### 5.2 Pages Using i18n Via Delegation

All other dashboard and portal pages delegate to components that use `useTranslations`. **All rendered content is internationalized.**

### 5.3 Pages Without i18n

| Page | Notes |
|---|---|
| `/` (root) | Just a redirect, no content to translate |
| `/[locale]` (landing) | Uses `getTranslations` server-side — covered |

**All content pages have i18n coverage**, either directly or via delegated components.

---

## 6. Recommendations

### Critical (fix before next release)
1. **Create portal profile page**: Add `src/app/[locale]/portal/[clinicSlug]/profile/page.tsx` — the PortalLayout nav links to it but it does not exist (404).

### High Priority
2. **Resolve portal animal detail duplication**: Either remove `/portal/[clinicSlug]/animals/[id]` or `/portal/[clinicSlug]/pets/[animalId]` and redirect one to the other.
3. **Add e-reporting UI**: Backend supports UAE e-reporting but no frontend exists.
4. **Add waitlist management page**: Backend CRUD exists with no frontend surface.
5. **Add patient import page**: Important for clinic onboarding flow.

### Medium Priority
6. **Link pricing page from navigation**: The standalone `/pricing` page exists but is unreachable. Either link it from the landing nav or remove the page.
7. **Add clinic group management page**: Backend supports group operations but no admin UI exists.

### Low Priority
8. **Consider dedicated analytics/reporting page**: Dashboard has some stats but no comprehensive reporting view.
9. **Consider referral code UI**: Backend supports it but no frontend surface exists.
