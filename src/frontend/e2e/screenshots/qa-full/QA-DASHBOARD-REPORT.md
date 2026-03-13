# QA Dashboard Report

**Date**: 2026-03-13
**Browser**: Chromium (headless, Playwright)
**Viewport**: 1440x900
**App version**: Next.js 16.1.6 Turbopack

## Summary

| Result | Count |
|---|---|
| OK - Fully functional | 8 |
| BROKEN - Runtime error / 404 | 2 |

## Page-by-Page Results

### 00 - Login (`/en/login`)
- **Verdict**: OK
- **Layout**: Centered card, clean design, EN/AR language switcher present
- **Features**: Email + password fields with placeholders, "Forgot password?" link, "Sign Up" link
- **MSW data**: Login works (redirects to `/en/appointments` after submit)
- **Screenshot**: `00-login.png`

### 01 - Dashboard / Appointments (`/en/appointments`)
- **Verdict**: OK
- **Layout**: Sidebar (left) + header (top-right user info) + main content - all correct
- **Features**: Weekly calendar view (Mar 8-14, 2026), Day/Week/Month toggle, "Today" button, "New Appointment" CTA, "Filter by vet" dropdown
- **MSW data**: 10 appointments displayed with pet names (Max, Cleo, Luna, Mango, Simba, Rocky, Oreo, Buddy, Sultan, Kira), owner names (Arabic: Mariam Al-Suwaidi, Noura Al-Ketbi, etc.), consultation types color-coded (Dental=purple, Surgery=pink, Grooming=yellow, Exotic Animal=green, Laboratory=blue)
- **Cookie banner**: Visible at bottom - "We use analytics to improve your experience"
- **Sidebar items**: Agenda, Patients, Medical Records, Billing, Messages (badge: 3), Stock, Settings, Profile
- **Screenshot**: `01-dashboard.png`

### 02 - Patients List (`/en/patients`)
- **Verdict**: OK
- **Layout**: Sidebar + header correct
- **Features**: Search bar, "Import CSV" button, "+ Add Patient" CTA, patient cards in grid layout
- **MSW data**: 5 patients displayed (Max, Luna, Simba, Bella, Zayed) with species, breed, age, sex, owner name, UAE phone numbers (+971), last visit and next appointment dates
- **Console warnings**: 2x Base UI button nativeButton warnings (non-critical, cosmetic)
- **Note**: Zayed is a Camel - Dromedary -- good UAE-realistic data
- **Screenshot**: `02-patients-list.png`

### 03 - Patient Create (`/en/patients/new`)
- **Verdict**: OK
- **Layout**: Sidebar + header correct, back arrow to Patients
- **Features**: Full form with Patient Name, Species (dropdown), Breed, Date of Birth, Sex (dropdown), Weight (optional), Owner section (Full Name, Phone, Email)
- **Placeholders**: UAE-realistic (e.g. "Ahmed Al-Mansoori", "+971 50 123 4567", "owner@email.ae")
- **Required fields**: Marked with red asterisk (Patient Name, Species, Date of Birth, Full Name, Phone)
- **Actions**: Cancel + Save Patient buttons
- **Console warnings**: 1x Base UI button nativeButton warning (non-critical)
- **Screenshot**: `03-patient-create.png`

### 04 - Medical Records (`/en/medical-records`)
- **Verdict**: OK
- **Layout**: Sidebar + header correct
- **Features**: Search bar ("Search by patient, diagnosis, or vet..."), record cards in vertical list
- **MSW data**: 4+ records visible - Max (Annual vaccination, Limping, Skin irritation), Luna (Skin condition follow-up). Each record shows: patient name, reason, date, veterinarian, weight, temp, HR, diagnosis
- **Data quality**: Realistic vitals (38.5-38.8C, 78-140 bpm), realistic diagnoses
- **Screenshot**: `04-medical-records.png`

### 05 - Billing (`/en/billing`)
- **Verdict**: OK
- **Layout**: Sidebar + header correct
- **Features**: Search bar, status filter dropdown (ALL), "+ New Invoice" CTA, data table with columns: Invoice#, Patient, Date, Subtotal, VAT (5%), Total AED, Status, Actions
- **MSW data**: 3 invoices (INV-2026-001 to 003), patients Max/Luna/Rocky, status badges (DRAFT/SENT/PAID) with color coding
- **Summary**: "3 invoices - Total filtered: AED 2,761.50"
- **UAE compliance**: AED currency, 5% VAT correctly displayed
- **Screenshot**: `05-billing.png`

### 06 - Stock (`/en/stock`)
- **Verdict**: OK
- **Layout**: Sidebar + header correct
- **Features**: Alert banners (low stock warning + expiring soon warning), search bar, category and status filters, data table with columns: Name, Category, Quantity, Min. Threshold, Expiry Date, Status, Actions (edit + restock icons)
- **MSW data**: 6 items (Meloxicam, Amoxicillin, Ketamine, Surgical Gloves, Syringes, Rabies Vaccine) with categories (Medication/Supply/Vaccine)
- **Status indicators**: "Low Stock" (red), "Expiring Soon" (orange), "OK" (green) - well color-coded
- **Alert banners**: "2 item(s) low on stock" (red bg), "1 item(s) expiring within 30 days" (orange bg)
- **Low stock quantities highlighted in red**: 5 bottles, 12 boxes
- **Screenshot**: `06-stock.png`

### 07 - Settings (`/en/settings`)
- **Verdict**: BROKEN - Runtime Error
- **Error**: "Missing `<html>` and `<body>` tags in the root layout." (Next.js 16.1.6 Turbopack)
- **HTTP Status**: 404
- **Root cause**: The `/en/settings` route does not have a proper page component; the sidebar links to it but the page does not exist or its layout is misconfigured
- **Screenshot**: `07-settings.png`

### 08 - Profile (`/en/profile`)
- **Verdict**: BROKEN - Runtime Error
- **Error**: Identical to Settings - "Missing `<html>` and `<body>` tags in the root layout."
- **HTTP Status**: 404
- **Root cause**: Same as Settings - route not implemented
- **Screenshot**: `08-profile.png`

### 09 - Patients AR/RTL (`/ar/patients`)
- **Verdict**: OK
- **RTL direction**: YES - correctly applied
- **Layout**: Sidebar on RIGHT, header user info on LEFT, content flows RTL - all correct
- **Features**: Same as EN patients but mirrored. "Add Patient" and "Import CSV" buttons on left side
- **MSW data**: Same 5 patients, cards flow RTL
- **Sidebar**: Labels and icons correctly right-aligned
- **Cookie banner text**: Arabic period at end (".We use analytics...")
- **Console warnings**: Same 2x Base UI nativeButton warnings
- **Note**: Patient card content is still in English (names, breeds) - expected since MSW data is EN. Labels like "Owner:", "Last visit:", "Next appt:" are also in English -- this may need i18n translation
- **Screenshot**: `09-patients-ar.png`

### 10 - Billing AR/RTL (`/ar/billing`)
- **Verdict**: OK
- **RTL direction**: YES - correctly applied
- **Layout**: Sidebar on RIGHT, table columns reversed (Invoice# on right, Actions on left), content flows RTL
- **Features**: Same billing data mirrored for RTL
- **MSW data**: Same 3 invoices, table properly mirrored
- **Summary line**: "AED 2,761.50" on left side (RTL-correct)
- **Note**: Column headers and data still in English - may need i18n
- **Screenshot**: `10-billing-ar.png`

## Critical Issues

### BUG-01: Settings page crashes with Runtime Error (SEVERITY: HIGH)
- **URL**: `/en/settings`
- **Error**: "Missing `<html>` and `<body>` tags in the root layout"
- **Impact**: Settings link in sidebar leads to a broken page
- **Fix**: Create proper page component at `app/[locale]/settings/page.tsx` with correct layout hierarchy, or remove the Settings link from sidebar until implemented

### BUG-02: Profile page crashes with Runtime Error (SEVERITY: HIGH)
- **URL**: `/en/profile`
- **Error**: Same as BUG-01
- **Impact**: Profile link in sidebar leads to a broken page
- **Fix**: Create proper page component at `app/[locale]/profile/page.tsx`, or remove the Profile link from sidebar until implemented

## Warnings (Non-Critical)

### WARN-01: Base UI nativeButton console warnings
- **Pages affected**: Patients (EN+AR), Patient Create
- **Message**: "A component that acts as a button expected a native `<button>` because the `nativeButton` prop is true"
- **Impact**: Cosmetic only, no visible UI impact
- **Fix**: Ensure Button components render as `<button>` elements, or set `nativeButton` to `false`

### WARN-02: AR locale - labels not translated
- **Pages affected**: Patients AR, Billing AR
- **Details**: Field labels ("Owner:", "Last visit:", "Next appt:", column headers) remain in English on AR pages
- **Impact**: UI is RTL-correct but not fully localized
- **Fix**: Ensure all UI strings go through `next-intl` translation keys

### WARN-03: Cookie consent banner overlaps content
- **Pages affected**: All pages
- **Details**: The analytics consent banner at the bottom overlaps with sidebar footer items (Settings, Profile links partially hidden)
- **Impact**: Minor UX issue - sidebar bottom items can be hard to click
- **Fix**: Add bottom padding to sidebar when consent banner is visible

## Overall Assessment

**8 of 10 pages functional and well-designed.** The application looks professional with:
- Clean, consistent layout across all working pages
- Proper sidebar navigation with active state highlighting
- UAE-realistic MSW data (Arabic names, AED currency, +971 phones, camels)
- Working RTL layout for Arabic locale
- Color-coded status indicators (stock alerts, invoice statuses, appointment types)
- Calendar view with appointment blocks is visually polished

**2 pages broken** (Settings, Profile) - these need page components created or sidebar links removed.

The RTL implementation is structurally correct (sidebar flips, table columns reverse, content flows right-to-left) but the AR locale needs i18n string translation for full localization.
