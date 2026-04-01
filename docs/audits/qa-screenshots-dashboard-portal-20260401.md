# QA Screenshots Audit - Portal & Dashboard Pages (Batch 2)

**Date**: 2026-04-01
**Environment**: localhost:3000 (Next.js dev server, MSW active)
**Browser**: Playwright Chromium (headless), 1280x900 viewport

## Summary

- **14 pages tested** (3 portal, 11 dashboard)
- **0 pages returned 404** -- all routes exist
- **1 page shows an error state** (portal landing / conversations)
- **11 dashboard pages redirect to login** (expected -- no authenticated session)
- **3 portal pages load without authentication** (expected -- public portal)

---

## Portal Pages (Public)

### /en/portal/demo-clinic -- Portal Landing (Conversations)
- **Screenshot**: `screenshots/portal-landing.png`
- **Status**: 200, no redirect
- **Result**: ERROR STATE -- displays "Something went wrong. Please try again later."
- **Layout**: Left sidebar with nav (Conversations, Appointments, My Pets, Profile) renders correctly. Clinic name "Desert Paws Clinic" shown. Arabic language toggle present.
- **data-testid count**: 20
- **Issue**: The Conversations tab (default landing) fails to load content. Likely MSW handler missing or conversation fetch failing. The error is displayed in a user-friendly way (not a crash), but this is the first thing a pet owner sees.
- **Cookie banner**: Present at bottom, functional appearance.

### /en/portal/demo-clinic/pets -- Portal Pets
- **Screenshot**: `screenshots/portal-pets.png`
- **Status**: 200, no redirect
- **Result**: OK -- page loads, shows "My Pets" heading with empty state "No pets found."
- **Layout**: Clean, sidebar nav highlights "My Pets". Empty state has a paw icon.
- **data-testid count**: 21
- **Issue**: No MSW mock data for pets. Empty state renders correctly but no way to verify the pet card UI without data.

### /en/portal/demo-clinic/book -- Portal Booking
- **Screenshot**: `screenshots/portal-booking.png`
- **Status**: 200, no redirect
- **Result**: OK -- page loads with "Appointments" heading, "Book via WhatsApp" CTA button (green), and two cards: "My Appointments" and "Select Your Pet".
- **Layout**: Well-structured. Green WhatsApp CTA is prominent. Cards have icons and descriptions.
- **data-testid count**: 24
- **Issue**: None. This is the best-rendered portal page.

---

## Dashboard Pages (Authenticated)

All 11 dashboard pages redirect to the login page at `/en/login?callbackUrl=...` as expected since there is no authenticated session. The login page renders consistently for all redirects.

### Login Page (shown for all dashboard redirects)
- **Screenshot**: `screenshots/dashboard-main.png` (representative)
- **Status**: 200 (after redirect)
- **Result**: OK -- clean login form with Email, Password, "Forgot password?", "Sign In" button, and "Sign Up" link.
- **Layout**: Centered card design. Language switcher (EN | FR | AR) top-right. Placeholder text "vet@clinic-dubai.com" is contextually appropriate for UAE market.
- **data-testid count**: 15 (consistent across all redirected pages)
- **Cookie banner**: Present at bottom.

### Redirect behavior by page:

| Page | Requested URL | Redirected To | Status |
|------|--------------|---------------|--------|
| Dashboard | /en/dashboard | /en/login?callbackUrl=%2Fen%2Fdashboard | OK |
| Patients | /en/patients | /en/login?callbackUrl=%2Fen%2Fpatients | OK |
| Appointments | /en/appointments | /en/login?callbackUrl=%2Fen%2Fappointments | OK |
| Billing | /en/billing | /en/login?callbackUrl=%2Fen%2Fbilling | OK |
| Messages | /en/messages | /en/login?callbackUrl=%2Fen%2Fmessages | OK |
| Breeding | /en/breeding | /en/login?callbackUrl=%2Fen%2Fbreeding | OK |
| Stock | /en/stock | /en/login?callbackUrl=%2Fen%2Fstock | OK |
| Settings | /en/settings | /en/login?callbackUrl=%2Fen%2Fsettings | OK |
| Settings/Team | /en/settings/team | /en/login?callbackUrl=%2Fen%2Fsettings%2Fteam | OK |
| Settings/Notifications | /en/settings/notifications | /en/login?callbackUrl=%2Fen%2Fsettings%2Fnotifications | OK |
| Settings/Preferences | /en/settings/preferences | /en/login?callbackUrl=%2Fen%2Fsettings%2Fpreferences | OK |

---

## Issues Found

### P1 - Portal Landing Error State
- **Page**: `/en/portal/demo-clinic`
- **Problem**: The default Conversations tab shows "Something went wrong. Please try again later." This is the first page a pet owner would see when visiting the clinic portal.
- **Likely cause**: Missing or broken MSW handler for conversations/messages endpoint.
- **Impact**: High -- this is the portal entry point.

### P3 - Portal Pets Empty State (No Mock Data)
- **Page**: `/en/portal/demo-clinic/pets`
- **Problem**: Shows "No pets found" -- no MSW mock data populates this page.
- **Impact**: Low -- empty state renders correctly, but unable to verify pet card rendering with data.

### P3 - Cookie Banner on All Pages
- **Observation**: Cookie consent banner appears on every page. It is functional (has "Necessary Only" and "Accept All" buttons) but takes up screen real estate on the portal where vertical space is valuable.

---

## Positive Observations

1. **Auth redirect works correctly** -- all 11 dashboard routes properly redirect to login with the correct `callbackUrl` parameter, ensuring users return to the intended page after login.
2. **Portal sidebar navigation** is consistent across all 3 portal pages with proper active-state highlighting.
3. **Login page** is clean, well-centered, and includes all expected elements (email/password fields, forgot password link, sign up link, language switcher).
4. **data-testid coverage** is good: 20-24 on portal pages, 15 on login page.
5. **Branding** is consistent ("Vetara", "Desert Paws Clinic") and appropriate for UAE market (Arabic toggle, Dubai email placeholder).
6. **No 404 errors** -- all routes resolve to a valid page.
7. **No JavaScript crashes** -- all pages render without unhandled exceptions.

---

## Screenshots Index

| File | Page |
|------|------|
| `portal-landing.png` | Portal landing (Conversations) - ERROR state |
| `portal-pets.png` | Portal pets - empty state |
| `portal-booking.png` | Portal booking - OK |
| `dashboard-main.png` | Dashboard -> Login redirect |
| `patients.png` | Patients -> Login redirect |
| `appointments.png` | Appointments -> Login redirect |
| `billing.png` | Billing -> Login redirect |
| `messages.png` | Messages -> Login redirect |
| `breeding.png` | Breeding -> Login redirect |
| `stock.png` | Stock -> Login redirect |
| `settings.png` | Settings -> Login redirect |
| `settings-team.png` | Settings/Team -> Login redirect |
| `settings-notifications.png` | Settings/Notifications -> Login redirect |
| `settings-preferences.png` | Settings/Preferences -> Login redirect |
