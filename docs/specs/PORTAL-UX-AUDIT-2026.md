# Portal Owner UX Audit -- Vetara vs Doctolib

**Date**: 2026-03-22
**Auditor**: UX Agent (portail de reservation veterinaire)
**Scope**: Owner-facing portal (`/portal/{clinicSlug}/`)
**Benchmark**: Doctolib (gold standard de la prise de RDV sante)

---

## Executive Summary

The Vetara owner portal is **functionally complete** with messaging, booking, appointments, and conversation export. The code quality is high: consistent `data-testid` attributes, proper ARIA roles, loading skeletons, error states, and i18n support (EN/AR). However, compared to Doctolib, several **structural UX gaps** exist that would reduce conversion and owner confidence. The most critical issues are: (1) no clinic information visible on the portal landing, (2) the "Book New Appointment" card is disabled and unreachable, (3) no clear primary CTA to book an appointment, and (4) the landing page is messaging-centric rather than appointment-centric.

**Scoring**: Vetara portal = 6.5/10 | Doctolib = 9/10

---

## 1. PORTAL LANDING (`/portal/{clinicSlug}`)

### Current State

The landing page (`PortalLanding.tsx`) shows **only conversations**. The title says "Your Conversations" and lists existing messaging threads with status badges and unread counts. The only CTA is "New Message".

### Findings

| # | Finding | Severity | Description |
|---|---------|----------|-------------|
| L1 | **No clinic information visible** | CRITIQUE | The owner arrives and sees zero information about the clinic: no address, no phone number, no opening hours, no clinic logo/photo. On Doctolib, the practitioner page shows name, photo, address, map, specialties, and availability -- all above the fold. The owner has no confirmation they are on the right clinic's portal. |
| L2 | **No CTA to book an appointment** | CRITIQUE | The landing page has zero path to booking. The "Appointments" tab exists in the nav but leads to `BookingLanding.tsx` where the "Book New Appointment" card is **disabled** (`opacity-60`, `cursor-not-allowed`, `aria-disabled="true"`). There is no functional way for an owner to reach the booking wizard from the landing page. Doctolib's entire homepage IS a booking interface. |
| L3 | **Messaging-first architecture** | IMPORTANT | The portal is organized around conversations, not appointments. For a veterinary booking portal, the primary user intent is "book an appointment for my pet." Messaging should be secondary. Doctolib puts booking front and center; messaging is secondary. |
| L4 | **No visual hierarchy for key action** | IMPORTANT | The "+ New Message" button is small and placed in the header row. On Doctolib, the "Book Appointment" button is large, prominent, and impossible to miss. |
| L5 | **Magic link auth has no grace period indicator** | NICE-TO-HAVE | When the link expires, the owner sees "Link expired" with no way to request a new one. Doctolib sends auto-reminders and offers resend. |

### Proposed Solutions

**L1 -- Clinic Info Card (CRITIQUE)**
```
+------------------------------------------+
| [Clinic Logo]  Desert Paws Clinic        |
|              Al Wasl Road, Dubai         |
|              +971 4 XXX XXXX            |
|              Sun-Thu: 8:00-18:00         |
|              [Map link]                  |
+------------------------------------------+
```
Add a `ClinicInfoCard` component at the top of the portal, fetching clinic metadata from API. Always visible on landing, collapsible on inner pages.

**L2 -- Enable Booking CTA (CRITIQUE)**
Remove `aria-disabled` and `cursor-not-allowed` from the "Book New Appointment" card in `BookingLanding.tsx`. Wire the `onClick` to navigate to `/portal/{slug}/book/new`. Add a prominent "Book Appointment" button on the landing page itself.

**L3 -- Restructure Landing (IMPORTANT)**
Reorganize the landing page:
1. Clinic info card (top)
2. "Book Appointment" primary CTA (large button)
3. Next upcoming appointment card (if any)
4. Quick links: My Appointments | Messages | My Pets

---

## 2. BOOKING WIZARD (`/portal/{clinicSlug}/book/new`)

### Current State

The wizard has **4 steps**: Pet Selection -> Consultation Type -> Slot Selection -> Confirmation. This is well-architected with proper state management, slide animations, step indicators, and a success screen with iCal download.

### Findings

| # | Finding | Severity | Description |
|---|---------|----------|-------------|
| W1 | **4 steps vs Doctolib's 3** | IMPORTANT | Doctolib's flow: Specialty/Motive -> Slot -> Confirmation. Vetara adds "Pet Selection" as step 1, which is domain-specific and justified. However, Step 2 combines consultation type + vet preference + reason, which is heavy. Consider merging pet selection with consultation type if the pet determines available types. |
| W2 | **No "slot reservation" reassurance** | CRITIQUE | On Doctolib, once you click a slot, it shows "This slot is reserved for you for 15 minutes." Vetara has no such indicator. On mobile with slow connections, the owner may worry the slot will be taken while they confirm. |
| W3 | **Recommended slots are excellent** | POSITIVE | The `RecommendedSlots` component with AI-powered suggestions (sparkle icon, top 3 suggestions) is a differentiator. Doctolib does not have this. This is a competitive advantage. |
| W4 | **WeekNavigator swipe support** | POSITIVE | Touch swipe support for week navigation is well-implemented. Doctolib also has this. Parity achieved. |
| W5 | **Step 2 is overloaded** | IMPORTANT | Step 2 asks for: consultation type (cards), vet preference (dropdown), and reason (textarea, 500 chars). That's 3 decisions on one screen. On mobile (375px), this scrolls significantly. Doctolib keeps each step to one decision. |
| W6 | **No price indicator** | IMPORTANT | Vetara shows duration (e.g. "30 min") for each consultation type but no price. Doctolib shows the price. For UAE market, price transparency is important. Even if pricing is not yet implemented, a placeholder or "Price on request" should be shown. |
| W7 | **Terms checkbox blocks confirmation** | NICE-TO-HAVE | The confirmation step requires checking a terms box before the "Confirm" button activates. The button appears grayed out (`bg-muted-foreground/30`) until terms are accepted. On Doctolib, terms acceptance is inline with less friction. Consider: pre-check the box with a link to terms, or use implicit consent with a notice. |
| W8 | **Success screen auto-redirects after 5 seconds** | IMPORTANT | `BookingSuccess.tsx` auto-redirects to portal home after 5 seconds. This is too fast for users who want to: download the iCal file, read the summary, or screenshot the confirmation. Doctolib does NOT auto-redirect. The user stays on the confirmation until they choose to leave. |
| W9 | **iCal download is a strong feature** | POSITIVE | The iCal generation with proper RFC 5545 formatting is well done. Doctolib offers "Add to calendar" similarly. Parity achieved. |
| W10 | **SlotGrid shows vet name per slot** | POSITIVE | When no vet is pre-selected, each slot chip shows the vet name. This is helpful and not standard on Doctolib. |

### Proposed Solutions

**W2 -- Slot Reservation Indicator (CRITIQUE)**
After slot selection, show:
```
"This slot is reserved for you for 15 minutes.
Complete your booking to confirm."
```
With a visual countdown timer (subtle, not stressful). This reduces abandonment anxiety.

**W5 -- Split Step 2 (IMPORTANT)**
Option A: Move "reason" field to step 4 (confirmation) as an optional note.
Option B: Make vet preference a sub-step that slides in only if user taps "Choose a specific vet."

**W6 -- Add Price (IMPORTANT)**
Add `price?: number` to `ConsultationTypeDto`. Display "AED 150" or "Price on consultation" next to duration on each card.

**W8 -- Remove Auto-Redirect (IMPORTANT)**
Remove the `REDIRECT_DELAY_MS` timer. Let the user stay on the success screen. Add a prominent "Back to My Appointments" button as the only navigation. Keep the iCal download button visible.

---

## 3. MY APPOINTMENTS (`/portal/{clinicSlug}/book/appointments`)

### Current State

`MyAppointments.tsx` shows upcoming/past tabs with appointment cards. Each card shows status badge, consultation type, pet name, vet name, date/time. Clicking navigates to `AppointmentDetail.tsx` which shows full details with cancel/reschedule actions.

### Findings

| # | Finding | Severity | Description |
|---|---------|----------|-------------|
| A1 | **Tabs for upcoming/past are well-done** | POSITIVE | Clean tab UI with counts. Upcoming sorted chronologically (soonest first), past sorted reverse. Matches Doctolib. |
| A2 | **Cancel requires 24h notice** | POSITIVE | `canCancelOrReschedule()` checks `hoursUntil >= 24`. This is clear business logic. But the rule is not communicated to the user until they try to cancel. |
| A3 | **Reschedule is a placeholder** | IMPORTANT | The reschedule button shows `toast.info("Coming soon")`. This is frustrating for users who expect the feature to work. Either hide the button or implement it. On Doctolib, reschedule is fully functional. |
| A4 | **No reminder indicators** | IMPORTANT | There are no visual reminders (e.g., "Tomorrow at 10:00 AM", "In 3 days"). Doctolib shows time-relative labels ("In 2 hours", "Tomorrow"). This adds urgency and context. |
| A5 | **No cancel reason prompt** | NICE-TO-HAVE | The cancel action hardcodes `reason: 'Cancelled by owner'`. Doctolib asks for a reason (dropdown: "Schedule conflict", "Found another practitioner", etc.). This data is valuable for the clinic. |
| A6 | **Cancel confirmation dialog is good** | POSITIVE | Uses shadcn Dialog with proper title, description, and two-button footer. Prevents accidental cancellation. |
| A7 | **24h cancel rule not communicated** | IMPORTANT | If the appointment is <24h away, the cancel/reschedule buttons silently disappear. The user sees no explanation. Add a notice: "Cancellation is no longer available less than 24 hours before the appointment." |

### Proposed Solutions

**A3 -- Reschedule: Hide or Implement (IMPORTANT)**
Short term: Replace with `Button` that is `disabled` with tooltip "Reschedule coming soon." Long term: Implement reschedule flow that re-enters the booking wizard at step 3 (slot selection) with pre-filled context.

**A4 -- Relative Time Labels (IMPORTANT)**
Add relative labels to appointment cards:
- "Tomorrow at 10:00 AM" (amber highlight)
- "In 3 days"
- "Today at 2:30 PM" (green highlight, urgent)

**A7 -- Communicate Cancel Policy (IMPORTANT)**
Show a notice below the appointment detail card when `hoursUntil < 24`:
```
"This appointment can no longer be cancelled or rescheduled
(less than 24 hours away). Contact the clinic directly for changes."
```

---

## 4. MESSAGING (`/portal/{clinicSlug}/conversations`)

### Current State

Messaging includes: conversation list (landing), conversation detail with chat bubbles, new message form with pet selector/category/subject/body/photo upload, consent screen, and export page.

### Findings

| # | Finding | Severity | Description |
|---|---------|----------|-------------|
| M1 | **Chat UI is clean and modern** | POSITIVE | Owner messages right-aligned (primary color), clinic messages left-aligned (white). Timestamps visible. Sender name shown for clinic messages. WhatsApp-like feel. |
| M2 | **New message form is comprehensive** | POSITIVE | Pet selector, category picker (6 categories including MedicalUrgency), subject, body with char counter (2000), photo upload (3 max, 5MB each). Well-validated. |
| M3 | **Emergency warning on consent screen** | POSITIVE | The `ConsentScreen.tsx` shows an amber alert: "If your pet is experiencing an emergency, call the clinic directly." This is responsible UX and a regulatory best practice. |
| M4 | **No real-time updates** | IMPORTANT | Messages are fetched once on mount. No polling or WebSocket for new messages. If the clinic replies while the owner has the conversation open, they won't see it until they refresh. Doctolib has real-time notifications. |
| M5 | **Rate limiting is handled** | POSITIVE | 429 responses show "Too many messages" error. Good protection against spam. |
| M6 | **Ctrl+Enter to send** | POSITIVE | `onKeyDown` handler supports Ctrl+Enter / Cmd+Enter to send. Power user friendly. |
| M7 | **Conversation closed notice is clear** | POSITIVE | When `status === 'Closed'`, the reply area is replaced with a notice. No confusion. |
| M8 | **No read receipts** | NICE-TO-HAVE | The owner cannot see if the clinic has read their message. Doctolib shows read status. |
| M9 | **Export page is a bonus feature** | POSITIVE | Downloading all conversations as .txt is a GDPR/data portability feature. Not common in competitors. |

### Proposed Solutions

**M4 -- Polling for New Messages (IMPORTANT)**
Add a 30-second polling interval in `PortalConversation.tsx`:
```typescript
useEffect(() => {
  const interval = setInterval(() => {
    getPortalConversation(conversationId).then(conv => {
      setMessages(conv.messages ?? [])
    })
  }, 30000)
  return () => clearInterval(interval)
}, [conversationId])
```
Long term: implement WebSocket/SSE for real-time updates.

---

## 5. MOBILE (375px)

### Current State

The portal uses a responsive layout with: sticky header, bottom tab bar (`md:hidden`), sidebar nav on desktop (`hidden md:flex`), `max-w-2xl` content area, and `flex-col-reverse sm:flex-row` button layouts.

### Findings

| # | Finding | Severity | Description |
|---|---------|----------|-------------|
| B1 | **Bottom tab bar is well-implemented** | POSITIVE | 4 tabs: Conversations, Appointments, My Pets, Profile. Icons + labels. `min-w-[64px]` and `min-h-[44px]` for touch targets. Active state uses primary color. Matches iOS/Android native patterns. |
| B2 | **Touch targets meet 44x44px minimum** | POSITIVE | All buttons and interactive elements specify `min-h-[44px]`. Slot chips, day buttons, pet cards all meet this. Well done. |
| B3 | **Step labels hidden on mobile** | POSITIVE | Step indicator: numbers always visible, labels hidden below `sm` breakpoint. Prevents overflow. |
| B4 | **pb-20 for bottom bar overlap** | POSITIVE | `main` has `pb-20 md:pb-6` to prevent content hiding behind fixed bottom bar. |
| B5 | **Wizard nav buttons stack on mobile** | POSITIVE | `flex-col-reverse sm:flex-row` means "Next" appears above "Back" on mobile (primary action first). Good pattern. |
| B6 | **No safe area insets for notch phones** | IMPORTANT | The bottom tab bar uses `fixed bottom-0` without `env(safe-area-inset-bottom)`. On iPhone with home indicator, the tab bar may overlap with the system gesture area. |
| B7 | **Slot grid 3 columns on mobile** | POSITIVE | `grid-cols-3 sm:grid-cols-4 md:grid-cols-6` -- 3 columns at 375px gives ~115px per slot. Comfortable. |
| B8 | **No pull-to-refresh** | NICE-TO-HAVE | Mobile users expect pull-to-refresh to reload appointments/conversations. Currently only the initial fetch occurs. |
| B9 | **No horizontal scroll for week nav** | POSITIVE | The 7-day grid at 375px is tight but functional. Each day gets ~48px. The swipe gesture for week change is a smart alternative. |
| B10 | **Chat input area on mobile** | IMPORTANT | The reply textarea (3 rows) + send button at the bottom of a conversation may be partially hidden by the mobile keyboard. No `position: sticky` at bottom. Doctolib's chat input sticks to the bottom viewport. |

### Proposed Solutions

**B6 -- Safe Area Insets (IMPORTANT)**
Add to the bottom tab bar:
```css
padding-bottom: env(safe-area-inset-bottom, 0px);
```
Or in Tailwind: `pb-[env(safe-area-inset-bottom)]` with a fallback.

**B10 -- Sticky Chat Input (IMPORTANT)**
Make the reply area `sticky bottom-0` with proper background. Use `visualViewport` API to handle keyboard appearance:
```css
.reply-area {
  position: sticky;
  bottom: env(safe-area-inset-bottom, 0px);
  background: white;
  border-top: 1px solid var(--border);
  padding: 8px;
}
```

---

## 6. COMPARISON MATRIX: VETARA vs DOCTOLIB

| Feature | Doctolib | Vetara | Gap |
|---------|----------|--------|-----|
| Clinic info (name, address, hours, phone) | Full profile page | Name only in header | CRITIQUE |
| Primary booking CTA | Homepage IS booking | Disabled card, no CTA | CRITIQUE |
| Booking steps | 3 (motive -> slot -> confirm) | 4 (pet -> type -> slot -> confirm) | Justified (pet domain) |
| Slot reservation timer | 15-min reserve with indicator | None | CRITIQUE |
| Price display | Yes | No | IMPORTANT |
| AI-powered slot suggestions | No | Yes (RecommendedSlots) | ADVANTAGE Vetara |
| Calendar download (iCal) | Yes | Yes | Parity |
| Reschedule | Full flow | Placeholder ("coming soon") | IMPORTANT |
| SMS/Push reminders | Yes | No visible reminder system | IMPORTANT |
| Real-time messaging | Yes | No (fetch-once) | IMPORTANT |
| Read receipts | Yes | No | NICE-TO-HAVE |
| Consent / emergency warning | Not applicable | Yes (amber warning) | ADVANTAGE Vetara |
| Photo upload in messages | Limited | Yes (3 photos, 5MB each) | ADVANTAGE Vetara |
| Data export (GDPR) | Yes | Yes (.txt export) | Parity |
| Multi-language (RTL) | No | Yes (EN/AR with RTL) | ADVANTAGE Vetara |
| Mobile bottom nav | App-only | Web bottom tabs | Parity |
| Safe area (notch) | Yes (native app) | No | IMPORTANT |
| Pet-specific context | N/A | Yes (pet selector in booking + messaging) | ADVANTAGE Vetara (domain) |
| Auto-redirect after booking | No | Yes (5s) | REGRESSION |

---

## 7. PRIORITIZED ACTION PLAN

### P0 -- CRITIQUE (Ship blockers)

| # | Action | Effort | Impact |
|---|--------|--------|--------|
| L1 | Add `ClinicInfoCard` to portal landing | 1 day | Trust + orientation |
| L2 | Enable "Book New Appointment" card + wire to wizard | 0.5 day | Core user flow unblocked |
| W2 | Add slot reservation indicator with 15-min timer | 1 day | Reduces abandonment |

### P1 -- IMPORTANT (Next sprint)

| # | Action | Effort | Impact |
|---|--------|--------|--------|
| L3 | Restructure landing: booking-first, messaging-second | 2 days | Aligns with user intent |
| W5 | Lighten step 2: move reason to step 4 | 0.5 day | Reduces cognitive load |
| W6 | Add price display on consultation type cards | 0.5 day | Price transparency |
| W8 | Remove 5s auto-redirect on success screen | 0.5 hour | Prevents missed info |
| A3 | Hide or implement reschedule | 1-3 days | Removes broken UX |
| A4 | Add relative time labels to appointment cards | 0.5 day | Better context |
| A7 | Communicate 24h cancel policy | 0.5 day | Sets expectations |
| M4 | Add 30s polling for conversation messages | 0.5 day | Near real-time |
| B6 | Add safe-area-inset-bottom to bottom tab bar | 0.5 hour | iPhone compat |
| B10 | Make chat reply area sticky at bottom | 0.5 day | Mobile keyboard |

### P2 -- NICE-TO-HAVE (Backlog)

| # | Action | Effort | Impact |
|---|--------|--------|--------|
| L5 | Add "Request new link" on expired screen | 1 day | Reduces support load |
| W7 | Reduce terms friction (pre-check or inline) | 0.5 day | Micro-conversion |
| A5 | Add cancel reason dropdown | 0.5 day | Data collection |
| M8 | Add read receipts to messages | 2 days | Owner peace of mind |
| B8 | Pull-to-refresh on mobile | 1 day | Native app feel |

---

## 8. WHAT VETARA DOES BETTER THAN DOCTOLIB

1. **AI-Powered Slot Suggestions** (`RecommendedSlots.tsx`): Sparkle-marked AI suggestions with vet name, date, time. Doctolib does not have this. This is a genuine competitive advantage.

2. **Pet-Specific Context Throughout**: Every flow (booking, messaging) is pet-aware. The owner selects which pet the appointment or message is about. This is domain-appropriate and not applicable to human healthcare.

3. **Photo Upload in Messaging**: Owners can attach up to 3 photos (JPEG/PNG, 5MB each) to messages. Useful for "my cat has a rash" pre-consultation triage.

4. **Emergency Warning on Consent**: The amber alert guiding owners to call the clinic for emergencies is responsible and reduces liability.

5. **RTL Support (Arabic)**: Full EN/AR toggle with proper RTL layout. Doctolib is EU-only (no Arabic).

6. **Data Export**: GDPR-compliant conversation export as .txt. Clean implementation.

7. **UAE Localization**: Dubai timezone (`Asia/Dubai`), AED currency context, Sunday-Thursday work week, Ramadan-aware scheduling.

---

## Sources

- [Doctolib Booking Help](https://doctolibpatient.zendesk.com/hc/de/articles/14141893264924-How-can-I-book-an-appointment-online)
- [Doctolib User Flow Analysis (Medium)](https://caroline-graver.medium.com/lets-see-how-doctolib-s-app-user-flow-works-14d41cd5453d)
- [Doctolib UX Case Study Redesign (Medium)](https://medium.com/@katkiani/ux-ui-case-study-redesigning-ui-of-doctolib-app-82cd26d18361)
- [Booking UX Best Practices 2025 (Ralabs)](https://ralabs.org/blog/booking-ux-best-practices/)
- [Doctolib Features & Reviews 2026 (GetApp)](https://www.getapp.com/healthcare-pharmaceuticals-software/a/doctolib/)
- [Doctor Appointment Booking UX Case Study (Bootcamp)](https://bootcamp.uxdesign.cc/ux-case-study-doctor-easy-doctors-appointment-booking-app-a2bd7042dc66)
- [Build an App Like Doctolib (DevTechnoSys)](https://devtechnosys.com/insights/build-an-app-like-doctolib/)
- [Doctolib Appointment Management (ICT&health)](https://www.icthealth.org/news/how-this-platform-mastered-the-art-of-appointment-management)
