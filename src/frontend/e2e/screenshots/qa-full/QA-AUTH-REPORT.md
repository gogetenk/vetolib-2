# QA Auth Screenshots Report

**Date**: 2026-03-13
**Tester**: QA Agent (automated + visual inspection)
**Browser**: Chromium (headless)
**Viewport**: 1440x900

---

## 1. Login Page (EN) - Initial State

- **URL**: `/en/login`
- **Screenshot**: `01-login-initial.png`
- **Console errors**: None
- **Console warnings**: None
- **Status**: PASS
- **Notes**:
  - Clean centered card layout with "Vetolib" branding and "Veterinary Management" subtitle
  - Email field with placeholder `vet@clinic-dubai.com` (UAE-appropriate)
  - Password field with show/hide toggle icon
  - "Sign In" CTA button (teal/green, full-width)
  - "Forgot password?" link properly positioned next to Password label
  - "Don't have an account? Sign Up" link at bottom
  - EN/AR language switcher present in top-right area
  - No accessibility issues visible

## 2. Login Page - Error State (invalid credentials)

- **URL**: `/en/login`
- **Screenshot**: `02-login-error.png`
- **Console errors**: 1 (`401 Unauthorized` - expected from MSW mock)
- **Console warnings**: None
- **Status**: PASS
- **Notes**:
  - Red-tinted error banner displayed: "Invalid email or password"
  - Error message is clearly visible with pink/red background
  - Form fields retain the entered values (good UX - user doesn't have to re-type email)
  - Password field shows dots (masked) - correct behavior
  - No information leak (generic error, doesn't say "user not found" vs "wrong password")

## 3. Login - After Valid Credentials

- **URL**: `/en/login` -> redirected to dashboard
- **Screenshot**: `03-login-success.png`
- **Console errors**: None
- **Console warnings**: None
- **Status**: PASS
- **Notes**:
  - Successfully logged in as "Dr. Sarah Johnson" (Vet role) at "Desert Paws Clinic"
  - Redirected to Agenda/Appointments dashboard (week view)
  - Sidebar navigation visible: Agenda, Patients, Medical Records, Billing, Messages (with badge "3"), Stock, Settings
  - Calendar shows week view (Mar 8 - Mar 14, 2026) with color-coded appointments
  - Cookie consent banner at bottom ("We use analytics to improve your experience")
  - User avatar "DS" in top-right with name and role
  - All MSW mock data rendered correctly with realistic UAE names (Hamdan Al-Maktoum, Mariam Al-Suwaidi, etc.)

## 4. Signup Page (EN) - Initial State

- **URL**: `/en/signup`
- **Screenshot**: `04-signup-initial.png`
- **Console errors**: None
- **Console warnings**: None
- **Status**: BUG - shows dashboard instead of signup
- **Findings**:
  - **BUG [AUTH-001]**: Navigating to `/en/signup` while logged in shows the dashboard (Appointments page with calendar week view) instead of the signup page. The app should either (a) redirect to dashboard with a message "already logged in", or (b) show the signup page anyway. Currently it silently shows the dashboard which is confusing.
  - The signup page IS accessible when not logged in (see AR screenshot 07 which was captured in a separate browser context without a prior login).

## 5. Forgot Password Page

- **URL**: `/en/forgot-password`
- **Screenshot**: `05-forgot-password.png`
- **Console errors**: 1 (`404 Not Found`)
- **Console warnings**: None
- **Status**: CRITICAL BUG
- **Findings**:
  - **BUG [AUTH-002] CRITICAL**: The `/en/forgot-password` page shows a **Next.js Runtime Error**: "Missing `<html>` and `<body>` tags in the root layout." This is a framework-level error that completely breaks the page.
  - The page is non-functional -- no forgot-password form is rendered at all.
  - The 404 console error suggests the route or layout file may be missing or misconfigured.
  - Next.js version shown: 16.1.6 Turbopack.
  - This needs immediate investigation of `app/[locale]/forgot-password/` layout and page files.

## 6. Login Page (AR/RTL)

- **URL**: `/ar/login`
- **Screenshot**: `06-login-ar-rtl.png`
- **Console errors**: None
- **Console warnings**: None
- **Status**: PASS with minor RTL issues
- **Notes**:
  - Language switcher correctly shows "AR | EN" with EN as the switch target
  - Labels ("Email", "Password") are right-aligned -- correct for RTL
  - Placeholder text is right-aligned inside inputs -- correct
  - "Forgot password?" link shows with leading question mark: "?Forgot password" -- this is correct RTL punctuation behavior
  - **ISSUE [AUTH-003] MINOR**: Labels and UI text remain in English ("Email", "Password", "Sign In", "Forgot password?", "Don't have an account? Sign Up"). For a true AR locale, these should be translated to Arabic. This may be by design if Arabic translations haven't been added yet, but it's inconsistent with the `/ar/` URL prefix which implies Arabic content.
  - The form card is still centered (not shifted) -- acceptable for a login form
  - Password show/hide icon is on the left side of the input in RTL -- correct mirroring
  - Overall RTL layout direction appears correct

## 7. Signup Page (AR/RTL)

- **URL**: `/ar/signup`
- **Screenshot**: `07-signup-ar-rtl.png`
- **Console errors**: None
- **Console warnings**: None
- **Status**: PASS with RTL issues
- **Notes**:
  - Signup form renders correctly with fields: Clinic Name, Email, Phone, Password, Confirm Password
  - "Create My Clinic" CTA and "Already have an account? Sign In" link present
  - Subtitle: "Start your 14-day free trial. No credit card required." with a leading period (RTL punctuation)
  - Phone placeholder: `+971 50 123 4567` (UAE format -- correct)
  - Password hint: "Min. 8 chars, 1 uppercase, 1 number" -- good
  - **ISSUE [AUTH-004] MINOR**: Same as AUTH-003 -- all text remains in English despite `/ar/` locale. Labels should be Arabic.
  - **ISSUE [AUTH-005] MINOR**: The subtitle has a leading period (`.Start your 14-day free trial...`) which is the RTL rendering of the trailing period. This looks slightly off -- the period appears before the text visually. Consider using proper RTL punctuation marks or ensuring the `dir` attribute handles this correctly.
  - RTL alignment of labels (right-aligned) is correct
  - Input fields and placeholders are right-aligned -- correct

---

## Summary of Findings

### Critical Bugs (must fix before release)

| ID | Screen | Severity | Description |
|---|---|---|---|
| AUTH-002 | Forgot Password | **CRITICAL** | `/en/forgot-password` crashes with Next.js Runtime Error: "Missing `<html>` and `<body>` tags in the root layout." Page is completely non-functional. |

### Bugs (should fix)

| ID | Screen | Severity | Description |
|---|---|---|---|
| AUTH-001 | Signup (EN) | **MEDIUM** | `/en/signup` shows dashboard when user is already logged in, instead of redirecting properly or showing the signup form. |

### RTL / i18n Issues (cosmetic, lower priority)

| ID | Screen | Severity | Description |
|---|---|---|---|
| AUTH-003 | Login (AR) | **MINOR** | All labels remain in English on `/ar/login`. Arabic translations missing. |
| AUTH-004 | Signup (AR) | **MINOR** | All labels remain in English on `/ar/signup`. Arabic translations missing. |
| AUTH-005 | Signup (AR) | **MINOR** | Leading period on subtitle text (`.Start your 14-day free trial...`) looks awkward in RTL. |

### What Works Well

- Login flow (EN): clean, professional design, proper error handling
- Login success: correct redirect to dashboard, all mock data renders
- Error messages: non-leaking, generic "Invalid email or password"
- Password field: show/hide toggle present and functional
- Language switcher: present and functional on all auth pages
- UAE-contextual data: clinic names, phone numbers, currency all UAE-appropriate
- RTL direction: form alignment is correct for RTL, inputs right-aligned
- Calendar/Dashboard: renders beautifully with color-coded appointments after login

### Recommended Actions

1. **P0**: Fix `/en/forgot-password` layout crash (AUTH-002) -- investigate missing layout.tsx or root layout tags
2. **P1**: Add route guard for `/en/signup` when authenticated (AUTH-001)
3. **P2**: Add Arabic translations for auth screens (AUTH-003, AUTH-004) -- coordinate with i18n task
4. **P2**: Review RTL punctuation handling for subtitles (AUTH-005)
