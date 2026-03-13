# QA Report — Calendar Agenda Redesign

**Date**: 2026-03-12
**Tester**: Automated QA (Playwright MCP)
**Environment**: Next.js dev (localhost:3000) + MSW mocks
**User**: Dr. Sarah Johnson (VET, Desert Paws Clinic)

---

## Screenshots Index

| # | File | View | Status |
|---|---|---|---|
| 01 | `01-week-view-en.png` | Week view (EN, LTR) | PASS |
| 02 | `02-day-view-en.png` | Day view (EN, LTR) | PASS |
| 03 | `03-month-view-en.png` | Month view (EN, LTR) | PASS |
| 04 | `04-vet-filter-dropdown-en.png` | Vet filter dropdown | PASS |
| 05 | `05-week-view-ar-RTL-BUG.png` | Week view (AR, RTL) | PASS (layout) / BUG (i18n) |
| 06 | `06-week-view-notification-toast.png` | Week view with notification toast | PASS |

---

## Validated Features

### Week View (EN)
- [x] 7-day grid Sun-Sat (UAE work week)
- [x] Today (Thu 12) highlighted with blue circle on date number
- [x] Weekend columns (Fri 13, Sat 14) greyed out with `bg-muted/40`
- [x] Off-hours (before 08:00, after 18:00) have subtle grey background
- [x] Time column 07:00-21:00 on the left
- [x] 10 appointments displayed with correct positioning by time
- [x] Each appointment shows: pet emoji + name, owner name, consultation type badge
- [x] Consultation type colors are distinct and readable:
  - General Checkup = green (Max)
  - Vaccination = pink (Cleo)
  - Follow-up = peach/orange (Luna)
  - Emergency = cyan (Rocky)
  - Exotic Animal = green (Mango)
  - Grooming = gold/yellow (Buddy)
  - Laboratory/Diagnostics = blue/lavender (Simba)
  - Dental = purple (Sultan)
  - Surgery = red/pink (Kira)
  - Boarding = blue (Oreo)

### Day View (EN)
- [x] Single day "Thursday, March 12, 2026" with full date label
- [x] Red "now" indicator line at current time (~13:00)
- [x] 2 appointments displayed: Sultan (Dental, purple, 07:00) and Kira (Surgery, red, 10:00)
- [x] Appointments show full details: pet + owner + type + vet + time + description

### Month View (EN)
- [x] Full month grid March 2026
- [x] 7-column layout Sun-Sat
- [x] Today (12) highlighted with blue circle
- [x] Appointments shown as compact pills with time + pet name
- [x] Up to 2 appointments per day visible (Max+Cleo on 8th, Luna+Rocky on 9th, etc.)
- [x] Weekend columns (Fri/Sat) slightly greyed

### Navigation
- [x] "Previous" button navigates to prior week (Mar 1-7)
- [x] "Today" button returns to current week
- [x] "Next" button navigates to next week
- [x] Day/Week/Month view toggle buttons work correctly
- [x] Active view button has highlighted state

### Vet Filter
- [x] Dropdown lists 4 vets + "All vets" option
- [x] Dr. Sarah Johnson, Dr. Omar Al-Rashid, Dr. Layla Al-Mansoori, Dr. Khalid Ibrahim

### Real-time Notifications
- [x] Toast notifications appearing from MSW mock (new messages from clients)
- [x] "View" button on notification toasts
- [x] Messages badge in sidebar updates dynamically

---

## Bugs Found

### BUG-1: Arabic (RTL) layout shows English text [MEDIUM]
**Screenshot**: `05-week-view-ar-RTL-BUG.png`
**Description**: When navigating to `/ar/appointments`, the RTL layout is correctly mirrored (sidebar on right, days reversed Sat→Sun), but ALL text remains in English:
- Page title: "Appointments" instead of Arabic
- Day names: "Sat", "Fri", etc. instead of Arabic
- Buttons: "Today", "New Appointment", "Filter by vet" in English
- Sidebar navigation labels: all English
- Sidebar links point to `/en/` instead of `/ar/`

**Root cause**: The sidebar and page text use `useTranslations()` but the sidebar `Link` components use hardcoded `/en/` paths instead of using the current locale. The calendar day names likely use `Intl.DateTimeFormat` with the correct locale but the page-level translations aren't loading in Arabic context.

**Impact**: Medium — RTL visual layout works, but Arabic users see English text.

### BUG-2: Appointment click does nothing in CalendarContainer [LOW]
**Description**: Clicking on an appointment block in the week/month view does nothing. The `CalendarContainer` renders `WeekCalendarBody` and `MonthCalendarBody` which don't pass `onClick` handlers to `AppointmentBlock`. The standalone `WeekCalendar.tsx` has this feature (quick create + detail sheet) but `CalendarContainer` doesn't.

**Impact**: Low for current sprint — appointments are display-only. Quick create and detail sheet need to be wired into CalendarContainer.

### BUG-3: No clickable empty slots in CalendarContainer views [LOW]
**Description**: Empty time slots are not clickable in the CalendarContainer's week view (`WeekCalendarBody`). The standalone `WeekCalendar.tsx` has clickable half-hour slots with hover effect and plus icon, but this wasn't ported to the extracted body component.

**Impact**: Low — the "New Appointment" button at the top is available as an alternative.

---

## Summary

| Category | Result |
|---|---|
| Visual design | EXCELLENT — professional, clean, colored by type |
| Week view | PASS |
| Day view | PASS |
| Month view | PASS |
| Navigation | PASS |
| Vet filter | PASS |
| RTL layout | PASS (layout) / FAIL (i18n text) |
| Interactivity | PARTIAL (display-only, no click handlers) |

**Recommendation**: Ship the calendar views as-is for EN. Create follow-up tasks for:
1. Wire `onClick` handlers into CalendarContainer (quick create + detail sheet)
2. Fix Arabic i18n text loading on calendar page
3. Fix sidebar link locale prefixes in AR mode
