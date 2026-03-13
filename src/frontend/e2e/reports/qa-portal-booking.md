# QA Report - Portal and Booking

**Date**: 2026-03-12
**Status**: QA_FAIL
**Branch**: develop

---

## Test Results

- Passed: 102 (project-wide)
- Skipped: 34 (project-wide)
- Failed: 0

The only dedicated E2E spec for this zone is e2e/integration/messaging/portal.spec.ts
(13 messaging scenarios). Zero E2E tests exist for the booking sub-zone
(BookingLanding, MyAppointments, AppointmentDetail, SlotGrid, WeekNavigator, RecommendedSlots).

---

## Coverage Analysis

### Pages covered by existing tests

| Page | Component | Scenarios |
|---|---|---|
| /portal/[slug] | PortalLanding | Open portal, expired link, conversation list |
| /portal/[slug]/consent | ConsentScreen | Display and accept consent |
| /portal/[slug]/conversations/[id] | PortalConversation | View thread (no reply test) |
| /portal/[slug]/export | ExportPage | Navigation and download button visible |
| /portal/[slug]/new | NewMessageForm | Pet/category selectors, char counter, photo upload, daily limit |

### Pages NOT covered by tests

| Page | Component |
|---|---|
| /portal/[slug]/book | BookingLanding |
| /portal/[slug]/book/appointments | MyAppointments |
| /portal/[slug]/book/appointments/[id] | AppointmentDetail |
| (sub-components) | SlotGrid, WeekNavigator, RecommendedSlots |

### Untested scenarios within covered components

- PortalConversation: Reply send flow, 429 on reply, closed state, 404 state, Ctrl+Enter shortcut
- PortalLanding: Empty conversations state, language toggle EN/AR
- NewMessageForm: Subject/body validation errors, success state after send
- ExportPage: Blob download click, API error state
- ConsentScreen: API failure error state

---

## Bugs Found

### [BUG-001] Missing i18n key portal.booking.landing.link_expired - blank error on expired token

- **Severity**: High
- **Files**: src/components/features/portal/booking/MyAppointments.tsx:135
  and src/components/features/portal/booking/AppointmentDetail.tsx:99
- **Description**: Both use useTranslations(portal.booking) then render t(landing.link_expired),
  resolving to portal.booking.landing.link_expired. This key does not exist in en.json or ar.json.
  The key link_expired only exists at portal.landing.link_expired (messaging namespace).
  When a magic link is expired the error text will be blank or display a raw key string.
- **Expected**: Add portal.booking.landing.link_expired to both locale files,
  or reference the existing key at portal.landing.link_expired.

### [BUG-002] Hardcoded English string bypasses i18n in PortalLanding export link

- **Severity**: Medium
- **File**: src/components/features/portal/PortalLanding.tsx:150
- **Description**: The export link renders the literal string "Download all my conversations"
  in JSX without t(). The key portal.export.download exists in both locale files.
- **Expected**: Use t from the portal.export namespace so Arabic users see translated text.

### [BUG-003] Hardcoded "Conversation not found." in PortalConversation not-found state

- **Severity**: Medium
- **File**: src/components/features/portal/PortalConversation.tsx:77
- **Description**: Not-found state renders a hardcoded English string. useTranslations is
  already imported. No not_found key exists in portal.conversation in either locale file.
- **Expected**: Add not_found to portal.conversation in both locale files and render via t.

### [BUG-004] Hardcoded English strings in SlotGrid and RecommendedSlots

- **Severity**: Medium
- **Files**: SlotGrid.tsx:47, RecommendedSlots.tsx:132 and :145
- **Description**: Three user-visible strings are hardcoded with no useTranslations call.
  portal.booking.slotGrid.noSlots already exists in both locale files for the SlotGrid empty case.
  RecommendedSlots has no translation keys at all.
- **Expected**: SlotGrid must use slotGrid.noSlots. RecommendedSlots must add and use keys.

### [BUG-005] PortalConversation catch block swallows 401 and 404 showing identical UI

- **Severity**: Medium
- **File**: src/components/features/portal/PortalConversation.tsx:35-38
- **Description**: The catch block body is empty and does not inspect the error type.
  Both 401 (expired token) and 404 (conversation missing) result in conversation === null
  and the same not-found UI. The expired/notFound distinction present in MyAppointments
  and AppointmentDetail is absent here.
- **Expected**: 401 sets expired state, 404 sets notFound state, each with distinct UI.

### [BUG-006] canCancelOrReschedule uses browser local time instead of Asia/Dubai time

- **Severity**: Low
- **File**: src/components/features/portal/booking/AppointmentDetail.tsx:30-36
- **Description**: new Date() returns browser local timezone. For users whose device timezone
  differs significantly from +04:00 the 24h boundary check may show or hide action buttons
  inconsistently versus the server decision.
- **Expected**: Remove the client guard and rely on server 422, or compute time in +04:00.

### [BUG-007] Disabled Book New Appointment card has no accessible semantics

- **Severity**: Low
- **File**: src/components/features/portal/booking/BookingLanding.tsx:58-75
- **Description**: Plain div with aria-disabled=true but no role attribute. aria-disabled has
  no semantic effect on a div without role=button. No accessible explanation that feature is coming soon.
- **Expected**: Add role=button to make aria-disabled meaningful plus a Coming Soon explanation.

---

## Missing data-testid

| Component | Element | Status |
|---|---|---|
| AppointmentDetail | White detail info card container | MISSING |
| PortalConversation | Message body paragraph element | MISSING - text assertion hard |
| RecommendedSlots | Section label span | MISSING |
| PortalLanding | Export link | OK |
| NewMessageForm | All interactive elements | OK |
| MyAppointments | Tab buttons and appointment cards | OK |
| AppointmentDetail | Cancel dialog | OK |
| WeekNavigator | Navigation buttons and day cells | OK |
| SlotGrid | Slot buttons | OK |

---

## i18n Issues

### Blocking

- portal.booking.landing.link_expired missing from both en.json and ar.json (BUG-001)

### Non-blocking hardcoded strings (5 occurrences)

- PortalLanding.tsx:150 - "Download all my conversations" not using i18n (BUG-002)
- PortalConversation.tsx:77 - "Conversation not found." not using i18n (BUG-003)
- SlotGrid.tsx:47 - "No available slots for this day" despite matching key existing (BUG-004)
- RecommendedSlots.tsx:132 - "No suggestions available right now." (BUG-004)
- RecommendedSlots.tsx:145 - "Recommended slots" (BUG-004)

### Minor

- portal.new_message.pet_placeholder declared in both locale files but not rendered in PetSelector.tsx.
  PetSelector uses t(no_pet) for the blank option instead. Dead translation key.

---

## a11y Issues

### Missing aria-live on dynamic error states

The following error containers appear dynamically without aria-live or role=alert.
Screen reader users will not be notified when they appear:

- NewMessageForm data-testid=submit-error
- ConsentScreen data-testid=consent-error
- ExportPage data-testid=export-error
- PortalConversation data-testid=reply-error

### Missing aria-live on character counter

NewMessageForm char counter (data-testid=char-counter) updates on every keystroke with no
aria-live region. Screen readers will not announce remaining character count.

### Correct accessibility patterns found (no action needed)

- WeekNavigator day buttons: role=tab, aria-selected, descriptive aria-label with closed/past state - correct
- SlotGrid slot buttons: aria-pressed and descriptive aria-label including availability - correct
- RecommendedSlots chips: aria-pressed and descriptive aria-label with vet name and datetime - correct
- PhotoUpload remove buttons: aria-label including the photo filename - correct
- PortalConversation send button: span.sr-only for screen reader text - correct
- BookingLanding My Appointments card: handles Enter and Space keyboard events - correct
- MyAppointments AppointmentCard: handles Enter and Space keyboard events - correct

---

## MSW Handler Coverage

All API endpoints called by portal messaging and booking components have corresponding
MSW handlers in portal.ts and booking.ts. No missing handlers detected.

Covered in portal.ts: GET and POST /api/v1/portal/conversations,
GET /api/v1/portal/conversations/:id, POST /api/v1/portal/conversations/:id/messages,
POST /api/v1/portal/consent, GET /api/v1/portal/export, GET /api/v1/portal/pets.

Covered in booking.ts: GET /api/v1/booking/slots, GET /api/v1/booking/suggest,
GET and POST /api/v1/portal/booking/appointments,
GET /api/v1/portal/booking/appointments/:id,
POST /api/v1/portal/booking/appointments/:id/cancel,
POST /api/v1/portal/booking/appointments/:id/reschedule.

### Handler quality issues (non-blocking)

- booking.ts checkPortalAuth() uses TypeScript cast via as unknown as that could mask type errors.
- getMockOwnerAppointments() hardcodes now = new Date("2026-03-11T10:00:00+04:00").
  As real time advances upcoming vs past bucketing shifts in MyAppointments.
  Future E2E tests will fail flakily without any code changes.

---

## Recommendations

### Blocking - must fix before writing or merging booking E2E tests

1. Add portal.booking.landing.link_expired to both messages/en.json and messages/ar.json (BUG-001)
2. Replace all five hardcoded English strings with i18n keys (BUG-002, BUG-003, BUG-004)
3. Fix PortalConversation error handling to distinguish 401 from 404 with separate UI (BUG-005)

### Non-blocking - should fix

4. Write E2E Playwright tests for BookingLanding, MyAppointments, and AppointmentDetail
   (tab switching, cancel dialog flow, reschedule-coming-soon toast, expired/not-found states)
5. Add aria-live=polite or role=alert to all four dynamic error containers
6. Add aria-live=polite to the char counter in NewMessageForm
7. Add the three missing data-testid attributes noted in the Missing data-testid section
8. Replace fixed date in getMockOwnerAppointments() with new Date() to prevent test drift
9. Resolve BookingLanding disabled card accessibility issue (BUG-007)
10. Remove or make timezone-aware the 24h cancel guard in AppointmentDetail (BUG-006)
11. Either use pet_placeholder in PetSelector.tsx or remove it from both locale files
