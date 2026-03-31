# Pet Owner UX Design -- Vetara B2C Portal

**Date**: 2026-03-30
**Author**: UX Designer Agent
**Status**: Draft -- Pending PO validation
**Scope**: Complete pet owner experience, from discovery to daily usage
**Market**: UAE (primary), France (secondary)
**Principle**: The owner is holding their phone in one hand, and possibly a leash or a pet carrier in the other.

---

## Table of Contents

1. [Audit of Existing Portal](#1-audit-of-existing-portal)
2. [Journey 1 -- New Owner Discovers Vetara](#2-journey-1--new-owner-discovers-vetara)
3. [Journey 2 -- Owner Consults Medical Record](#3-journey-2--owner-consults-medical-record)
4. [Journey 3 -- Owner Books an Appointment](#4-journey-3--owner-books-an-appointment)
5. [Journey 4 -- Owner Receives a Health Alert](#5-journey-4--owner-receives-a-health-alert)
6. [Wireframes -- Key Screens](#6-wireframes--key-screens)
7. [Recommendations Prioritized](#7-recommendations-prioritized)
8. [Open Questions for PO](#8-open-questions-for-po)

---

## 1. Audit of Existing Portal

### 1.1 What Exists Today

The portal lives at `/[locale]/portal/[clinicSlug]/` and is scoped to a single clinic. It is accessed via magic link (token in URL query string). The current feature set:

| Feature | Route | Component | Status |
|---------|-------|-----------|--------|
| Conversations list | `/portal/{slug}` | `PortalLanding.tsx` | Complete |
| Consent screen | `/portal/{slug}/consent` | `ConsentScreen.tsx` | Complete |
| New message form | `/portal/{slug}/new` | `NewMessageForm.tsx` | Complete |
| Conversation thread | `/portal/{slug}/conversations/{id}` | `PortalConversation.tsx` | Complete |
| Data export | `/portal/{slug}/export` | `ExportPage.tsx` | Complete |
| Booking landing | `/portal/{slug}/book` | `BookingLanding.tsx` | Complete |
| Booking wizard (4 steps) | `/portal/{slug}/book/new` | `BookingWizard.tsx` | Complete |
| My appointments list | `/portal/{slug}/book/appointments` | `MyAppointments.tsx` | Complete |
| Appointment detail | `/portal/{slug}/book/appointments/{id}` | `AppointmentDetail.tsx` | Complete |
| Layout (header + nav) | layout | `PortalLayout.tsx` | Complete |

**Navigation tabs (bottom bar mobile, sidebar desktop):**
1. Conversations
2. Appointments
3. My Pets (route does not exist -- dead link)
4. Profile (route does not exist -- dead link)

### 1.2 What is Missing

| Missing Feature | Impact | Notes |
|----------------|--------|-------|
| **B2C landing page** | CRITICAL | No public-facing page for owners to discover Vetara or search for a clinic. The current landing (`page.tsx`) is B2B only, targeting veterinarians. |
| **Clinic search / directory** | CRITICAL | No way for an owner to find their clinic on Vetara. |
| **Owner onboarding flow** | CRITICAL | No account creation, no pet linking. The portal relies solely on magic links sent by the clinic. The owner has zero agency to initiate. |
| **My Pets section** | HIGH | Nav tab exists but the route/page does not. Dead link. |
| **Pet medical record view** | HIGH | No vaccination history, no prescription list, no weight chart, no visit history visible to the owner. |
| **Profile / account section** | HIGH | Nav tab exists but the route/page does not. Dead link. |
| **PDF export of medical record** | MEDIUM | The export feature only covers conversations (.txt), not the actual medical dossier. |
| **Sharing (QR code / secure link)** | MEDIUM | No mechanism to share a pet's record with another vet. |
| **Push notifications / alerts** | MEDIUM | No notification system. Health alerts exist in the backend AI module but are not surfaced to owners. |
| **WhatsApp OTP auth** | MEDIUM | Auth is magic-link only. No WhatsApp OTP option (critical for UAE where WhatsApp penetration > 95%). |

### 1.3 Mobile-First Assessment

**Positive findings:**
- Bottom tab bar at 375px with 44px min touch targets
- Skeleton loading states on all lists
- `flex-col-reverse sm:flex-row` for button stacking on mobile
- `pb-20` to prevent content hiding behind fixed bottom bar
- Step indicator labels hidden below `sm` breakpoint
- Slot grid adapts: 3 columns at 375px
- Touch swipe for week navigation in calendar

**Gaps:**
- No `env(safe-area-inset-bottom)` for iPhone notch devices
- Chat reply area not sticky at bottom (keyboard overlap issue)
- No pull-to-refresh behavior
- Two nav tabs (My Pets, Profile) lead nowhere -- broken experience on mobile

### 1.4 data-testid Coverage

Excellent. Every interactive element has a `data-testid`. Examples: `portal-header`, `portal-landing`, `new-message-btn`, `booking-wizard`, `wizard-step-1`, `pet-card-{id}`, `appointment-card-{id}`, `cancel-dialog`, `send-reply-btn`. This is ready for Playwright.

### 1.5 i18n Status

Portal keys exist in all 3 locales (en.json, ar.json, fr.json) with matching top-level structure: `header`, `nav`, `landing`, `consent`, `new_message`, `conversation`, `export`, `booking`. The new features described below will need keys added to all 3 files.

---

## 2. Journey 1 -- New Owner Discovers Vetara

### 2.1 Context

A pet owner in Dubai searches Google for "dossier medical mon chat" or "my pet's vaccination record" or "vet appointment Dubai." They need to land on a page that makes them think: "This is where I manage my pet's health."

### 2.2 Flow

```
STEP 1: Google search
  |
  v
STEP 2: B2C Landing Page (NEW)
  URL: /[locale]/pet-owners   (or /[locale]/owners)
  Goal: Explain value prop in 5 seconds
  |
  v
STEP 3: Search for clinic
  Inline search bar: "Find your vet clinic"
  Type clinic name -> autocomplete results
  |
  +---> BRANCH A: Clinic found on Vetara
  |       |
  |       v
  |     STEP 4A: Clinic public profile page (NEW)
  |       Shows: name, address, hours, phone, vets, ratings
  |       CTA: "Create your free account"
  |       |
  |       v
  |     STEP 5A: Owner signup (NEW)
  |       Phone number (UAE: +971) -> WhatsApp OTP
  |       OR email -> magic link
  |       Name, email (if phone signup)
  |       |
  |       v
  |     STEP 6A: Link your pet
  |       "Does your vet already have your pet on file?"
  |       -> YES: Enter pet name + your phone -> match & link
  |       -> NO: Add pet manually (name, species, breed, DOB)
  |       |
  |       v
  |     STEP 7A: Portal home (existing, restructured)
  |
  +---> BRANCH B: Clinic NOT found on Vetara
          |
          v
        STEP 4B: "Recommend Vetara to your vet" (NEW)
          Pre-filled message template
          WhatsApp share button (clinic number field)
          OR email form to the clinic
          "We'll notify you when they join"
          Capture owner email for waitlist
```

### 2.3 Design Decisions

**Why a separate B2C landing, not the existing B2B landing?**
The current landing page (`page.tsx`, 35,000 lines) is a full B2B SaaS pitch with pricing tables, competitive comparisons, and demo forms. An owner landing there would be confused. The B2C message is entirely different: "Your pet's health in your pocket. Free for pet owners."

**Why WhatsApp OTP as primary auth for UAE?**
WhatsApp has 95%+ penetration in the UAE. Magic links via email require the owner to switch apps, open email, find the link. WhatsApp OTP: receive code in the app they already have open. 1-tap copy. No email needed. The magic link system remains as fallback.

**Why "link your pet" instead of "add your pet"?**
The vet clinic likely already has the pet in the system. Linking avoids duplicate records. The matching logic: pet name + owner phone number. If no match found, the owner creates a new pet record that the clinic can later merge.

---

## 3. Journey 2 -- Owner Consults Medical Record

### 3.1 Context

Fatima's cat Luna had a vaccination 6 months ago. Fatima wants to check when the next one is due. She also needs to show Luna's vaccine history to a groomer who requires proof of rabies vaccination.

### 3.2 Flow

```
STEP 1: Open Vetara (bookmark / WhatsApp notification link)
  Auto-login via saved session or quick WhatsApp OTP
  |
  v
STEP 2: Portal home
  Sees her pets as cards at the top
  Taps on "Luna" card
  |
  v
STEP 3: Pet profile (NEW)
  URL: /portal/{slug}/pets/{petId}

  SECTIONS (tabs or vertical scroll):

  [A] OVERVIEW
    - Pet photo (or species icon)
    - Name, species, breed, age, sex, weight, microchip
    - Next appointment card (if any)
    - Active alerts (vaccine due, weight trend)

  [B] VACCINATIONS
    - Timeline: vaccine name, date given, next due date
    - Status badges: "Up to date" (green), "Due soon" (amber), "Overdue" (red)
    - Each vaccine is tappable -> shows batch number, vet who administered

  [C] VISITS
    - Chronological list of consultations
    - Each shows: date, vet name, consultation type, brief diagnosis
    - Tappable -> shows full notes (what the vet wrote)
    - Excludes internal clinical notes (vet-only)

  [D] PRESCRIPTIONS
    - Active prescriptions at the top
    - Past prescriptions below
    - Each shows: drug name, dosage, frequency, start/end date
    - "Refill" button -> opens message to clinic pre-filled with refill request

  [E] WEIGHT
    - Line chart showing weight over time (last 12 months)
    - Current weight with trend arrow (up/down/stable)
    - Ideal weight range (if species+breed has reference data)
  |
  v
STEP 4: Download PDF (NEW)
  Button: "Download health record"
  Generates a branded PDF with:
    - Clinic logo + name
    - Pet info
    - Vaccination table
    - Recent visits summary
    - Active prescriptions
  Footer: "Generated by Vetara on {date}"
  |
  v
STEP 5: Share with another vet (NEW)
  Button: "Share record"
  Options:
    [A] QR code: Generate a time-limited QR code (valid 24h)
        Another vet scans it -> sees read-only record
    [B] Secure link: Copy a link (valid 7 days)
        Send via WhatsApp to the other vet
  Permissions: read-only, no editing, auto-expires
```

### 3.3 Design Decisions

**Why tabs vs. vertical scroll for the pet profile?**
Recommendation: vertical scroll with sticky section headers on mobile. Tabs work on desktop but add an extra tap on mobile. The vet in a hurry wants to scroll, not tap between sections. However, the PO should validate this -- if some owners have 50+ visits, tabs prevent overwhelming scroll length.

**What does the owner see vs. what the vet sees?**
The owner sees a curated, friendly version. Internal clinical notes (SOAP notes, differential diagnoses, internal observations) are NOT shown. The owner sees: diagnosis summary, treatment given, prescriptions made. This is the "patient-facing" view, like a hospital discharge summary vs. the full chart.

**PDF branding**
The PDF is branded with the clinic's logo and name, not Vetara's. The "Powered by Vetara" appears only in the footer. This reinforces the clinic's brand and makes the document feel official for presenting to other vets, groomers, or boarding facilities.

---

## 4. Journey 3 -- Owner Books an Appointment

### 4.1 Context

The existing booking wizard is well-built (4 steps, AI-suggested slots, iCal download). The gaps are upstream (getting to the wizard) and downstream (confirmation and reminders).

### 4.2 Flow (enhanced)

```
STEP 1: Portal home
  Primary CTA: "Book an Appointment" (large, impossible to miss)
  OR: taps "Appointments" in bottom nav
  |
  v
STEP 2: Booking landing (RESTRUCTURED)
  - Clinic info card (address, phone, hours) -- always visible
  - "Book Now" primary button
  - Next upcoming appointment preview (if any)
  - "My Appointments" link
  |
  v
STEP 3: Booking wizard (existing, with fixes)
  Step 1: Select pet (auto-select if only 1 pet)
  Step 2: Consultation type + vet preference
          (reason field MOVED to step 4)
  Step 3: Select time slot
          - AI recommended slots at top (existing -- competitive advantage)
          - Week grid below
          - "Slot reserved for 15 min" indicator after selection
  Step 4: Review + confirm
          - Summary card
          - Optional reason/notes field (moved from step 2)
          - Terms checkbox
          - "Confirm" button
  |
  v
STEP 4: Success screen (existing, with fixes)
  - NO auto-redirect (remove 5s timer)
  - iCal download (existing)
  - "Add to WhatsApp reminder" (NEW -- deep link)
  - Summary stays visible until owner navigates away
  |
  v
STEP 5: Confirmation via WhatsApp (NEW)
  Clinic sends automated WhatsApp message:
  "Your appointment is confirmed:
   Luna -- General Checkup
   Dr. Ahmed, Desert Paws Clinic
   Tuesday 2 April 2026, 10:00 AM

   Reply CANCEL to cancel (24h+ in advance)"
  |
  v
STEP 6: Reminder J-1 via WhatsApp (NEW)
  "Reminder: Luna's appointment is tomorrow at 10:00 AM.
   Desert Paws Clinic, Al Wasl Road.
   [Get directions]

   Reply CANCEL to cancel."
  |
  v
STEP 7: Check-in day of (NEW)
  Owner arrives at clinic
  Option A: Scan QR code at reception desk -> auto check-in
  Option B: Tap "I'm here" in the portal -> status changes to CheckedIn
  Clinic sees the owner on the arrivals board
```

### 4.3 Design Decisions

**Why move "reason" from step 2 to step 4?**
Step 2 currently asks 3 things: consultation type, vet preference, and reason. That is too much cognitive load for one screen on mobile. The reason is optional context -- it fits naturally in the review step where the owner is already confirming details. This reduces step 2 to 2 decisions (type + vet) which is closer to Doctolib's model.

**Why WhatsApp for reminders instead of email or push?**
In the UAE, WhatsApp open rates exceed 90%. Email open rates are around 20%. Push notifications require a native app or PWA permission. WhatsApp is the channel where UAE residents actually read messages. The architecture already supports an abstract channel model (per MESSAGING-SPEC.md) so adding WhatsApp is an implementation, not an architecture change.

**Why QR check-in?**
Reception desks in vet clinics are often hectic -- multiple owners, animals on leashes, cages being moved around. A QR code on a stand that the owner scans with their phone eliminates the need for the receptionist to manually mark arrivals. This is borrowed from airline self-check-in kiosks and Doctolib's QR check-in at French GP offices.

---

## 5. Journey 4 -- Owner Receives a Health Alert

### 5.1 Context

The backend AI module already generates health alerts (see `PREDICTIVE-HEALTH-ALERTS-SPEC.md`): vaccination overdue, weight trend anomaly, breed-specific screening due, senior wellness check, etc. These alerts currently exist only in the vet-facing dashboard. They need to reach the owner.

### 5.2 Flow

```
STEP 1: Alert generated (backend)
  Rule engine detects: "Luna's rabies vaccination expires in 30 days"
  Creates HealthAlert entity with severity, type, petId
  |
  v
STEP 2: WhatsApp notification (NEW)
  "Vetara Health Alert:
   Luna's rabies vaccination is due in 30 days.
   Book an appointment now to stay up to date.
   [Book Now]"
  |
  v
STEP 3: Owner taps "Book Now"
  Deep link: /portal/{slug}/book/new?petId={id}&type=vaccination
  |
  v
STEP 4: Booking wizard opens pre-filled
  - Pet: Luna (pre-selected, skips step 1)
  - Consultation type: Vaccination (pre-selected)
  - Goes directly to step 3 (slot selection)
  |
  v
STEP 5: Owner picks a slot and confirms
  (Standard booking flow from Journey 3)
  |
  v
STEP 6: Alert status updated
  Backend marks the alert as "Acted on -- appointment booked"
  Owner sees a green badge on the alert in their pet profile
```

### 5.3 Alert Types Surfaced to Owners

Not all vet-facing alerts should reach the owner. The following are owner-appropriate:

| Alert Type | Owner Message | Action |
|-----------|---------------|--------|
| Vaccination overdue | "{Pet}'s {vaccine} is due in {N} days" | Book vaccination appointment |
| Weight trend (gain/loss) | "{Pet} has gained/lost {X}kg in {N} months" | Book checkup |
| Senior wellness | "{Pet} is {age} -- time for a senior wellness check" | Book wellness exam |
| Prescription refill | "{Pet}'s {drug} prescription ends in {N} days" | Message clinic for refill |

**Not surfaced to owners** (vet-only):
- Breed-specific screening (complex medical context)
- Brachycephalic airway (requires clinical judgment)
- Cardiac breed risk (requires clinical judgment)

### 5.4 Design Decisions

**Why not push notifications?**
Push notifications require either a native app (not in MVP) or PWA service worker registration (lower reliability, requires HTTPS, and the user must explicitly grant permission in a browser prompt that most dismiss). WhatsApp is a guaranteed delivery channel for UAE. Push can be added later as a PWA enhancement.

**Why deep links with pre-filled context?**
The entire point of an alert is to convert it to an action with minimum friction. If the owner taps "Book Now" and arrives at an empty booking wizard, they have to re-select the pet and type from scratch -- 3 extra taps. Pre-filling skips those steps. The owner sees their pet + the right consultation type and only needs to pick a time slot. 1 decision instead of 4.

---

## 6. Wireframes -- Key Screens

### 6.1 B2C Landing Page (`/[locale]/pet-owners`)

```
+-----------------------------------------------+
| [Vetara logo]              [EN/AR]  [Log in]  |
+-----------------------------------------------+
|                                                |
|   YOUR PET'S HEALTH                           |
|   IN YOUR POCKET                              |
|                                                |
|   Free for pet owners. Always.                |
|                                                |
|   [  Find your vet clinic...  ] [Search]      |
|                                                |
+-----------------------------------------------+
|                                                |
|   [icon] Medical record   [icon] Appointments |
|   Access your pet's       Book online, get    |
|   full health history     reminders           |
|                                                |
|   [icon] Alerts           [icon] Share        |
|   Vaccination &           Send records to     |
|   health reminders        any vet instantly   |
|                                                |
+-----------------------------------------------+
|                                                |
|   TRUSTED BY 120+ CLINICS IN THE UAE          |
|                                                |
|   [Clinic logo] [Clinic logo] [Clinic logo]   |
|                                                |
+-----------------------------------------------+
|                                                |
|   "I never forget a vaccination anymore.       |
|    Everything is on my phone."                 |
|    -- Fatima, cat owner in Dubai              |
|                                                |
+-----------------------------------------------+
|                                                |
|   YOUR CLINIC ISN'T ON VETARA YET?            |
|   [Recommend Vetara to your vet]              |
|                                                |
+-----------------------------------------------+
|   Footer: About | Privacy | Terms | [Vetara]  |
+-----------------------------------------------+
```

**Notes:**
- The search bar is THE primary element. Everything else is secondary.
- No pricing, no feature comparison, no technical language. This is for pet owners, not vets.
- Social proof from other owners, not clinics.
- "Recommend to your vet" captures demand even when the clinic isn't onboarded.

### 6.2 Clinic Public Profile (`/[locale]/clinic/[slug]`)

```
+-----------------------------------------------+
| [Back]  [Vetara logo]             [EN/AR]     |
+-----------------------------------------------+
|                                                |
| [Clinic photo or map]                          |
|                                                |
| DESERT PAWS VETERINARY CLINIC                 |
| Al Wasl Road, Jumeirah, Dubai                 |
| +971 4 XXX XXXX                               |
| Sun-Thu: 8:00 AM - 6:00 PM                    |
| Fri: 9:00 AM - 1:00 PM                        |
| Sat: Closed                                    |
|                                                |
+-----------------------------------------------+
|                                                |
| OUR VETERINARIANS                              |
| [Photo] Dr. Ahmed   Small animals, surgery    |
| [Photo] Dr. Sarah   Exotic animals, birds     |
| [Photo] Dr. Omar    Equine, large animals     |
|                                                |
+-----------------------------------------------+
|                                                |
|  [====== Book an Appointment ======]           |
|  (primary CTA, full width, prominent)          |
|                                                |
|  [--- Create your free account ---]            |
|  (secondary, leads to signup)                  |
|                                                |
+-----------------------------------------------+
|                                                |
| Already a patient?  [Log in]                   |
|                                                |
+-----------------------------------------------+
```

**Notes:**
- No login required to view the public profile. SEO-friendly.
- The profile doubles as a Google Maps listing enhancer for the clinic (structured data).
- "Book an Appointment" leads to signup if not logged in, or directly to booking wizard if logged in.

### 6.3 Owner Signup (`/[locale]/signup`)

```
+-----------------------------------------------+
| [Vetara logo]                                  |
+-----------------------------------------------+
|                                                |
|   CREATE YOUR FREE ACCOUNT                    |
|                                                |
|   [WhatsApp icon]  Continue with WhatsApp     |
|   (Recommended -- fastest)                     |
|                                                |
|   ---- or ----                                 |
|                                                |
|   Email: [________________________]           |
|   [Continue with email]                        |
|                                                |
+-----------------------------------------------+
|   By creating an account you agree to our      |
|   Terms of Service and Privacy Policy          |
+-----------------------------------------------+

--- After WhatsApp OTP / magic link verified: ---

+-----------------------------------------------+
|   TELL US ABOUT YOU                           |
|                                                |
|   First name: [_______________]               |
|   Last name:  [_______________]               |
|   Phone:      [+971 _________]                |
|   (pre-filled if WhatsApp signup)              |
|                                                |
|   [Continue]                                   |
+-----------------------------------------------+
```

**Notes:**
- WhatsApp is first because it is the fastest path in UAE.
- Email is the fallback, not the primary.
- Minimal fields: name + phone. No address, no date of birth, no unnecessary fields.
- Phone is pre-filled if the user came via WhatsApp OTP.

### 6.4 Link Your Pet (`/[locale]/portal/{slug}/onboarding`)

```
+-----------------------------------------------+
|   LET'S ADD YOUR PET                          |
|                                                |
|   Does your vet already have your pet on file? |
|                                                |
|   [Yes, link my existing pet]                  |
|   [No, add a new pet]                         |
+-----------------------------------------------+

--- If "Yes, link my existing pet": ---

+-----------------------------------------------+
|   FIND YOUR PET                               |
|                                                |
|   Pet name: [_______________]                  |
|                                                |
|   We'll check with your clinic.                |
|   (If found, they'll confirm the link.)        |
|                                                |
|   [Search]                                     |
+-----------------------------------------------+

--- If "No, add a new pet": ---

+-----------------------------------------------+
|   ADD YOUR PET                                |
|                                                |
|   Name:     [_______________]                  |
|   Species:  [Dog v]                            |
|   Breed:    [_______________]                  |
|   Born:     [DD/MM/YYYY]                       |
|   Sex:      [Male / Female / Unknown]          |
|                                                |
|   [Add pet]                                    |
+-----------------------------------------------+
```

**Notes:**
- "Link" is the preferred path because it avoids duplicate records.
- The matching requires clinic-side approval to prevent unauthorized access to another owner's pet.
- After adding/linking, the owner lands on the pet profile.

### 6.5 Pet Profile (`/[locale]/portal/{slug}/pets/{petId}`)

```
+-----------------------------------------------+
| [Back]                          [Share] [PDF] |
+-----------------------------------------------+
|                                                |
| [Pet photo / species icon]                     |
| LUNA                                           |
| Persian cat, Female, 4 years                   |
| 4.2 kg | Microchip: 123456789012345            |
|                                                |
+-----------------------------------------------+
|                                                |
| [!] ALERTS                          [1 alert] |
| +-------------------------------------------+ |
| | Rabies vaccination due in 28 days         | |
| | [Book vaccination now]                    | |
| +-------------------------------------------+ |
|                                                |
+-----------------------------------------------+
|                                                |
| NEXT APPOINTMENT                               |
| +-------------------------------------------+ |
| | Tue 2 Apr 2026, 10:00 AM                 | |
| | General Checkup -- Dr. Ahmed              | |
| | [View details]                            | |
| +-------------------------------------------+ |
|                                                |
+-----------------------------------------------+
|                                                |
| VACCINATIONS                                   |
| +-------------------------------------------+ |
| | Rabies         12 Mar 2025   DUE SOON     | |
| | FVRCP          12 Mar 2025   Up to date   | |
| | FeLV           08 Jan 2024   Overdue      | |
| +-------------------------------------------+ |
| [See all vaccinations]                         |
|                                                |
+-----------------------------------------------+
|                                                |
| WEIGHT                                         |
| [Line chart: 3.8 -> 4.0 -> 4.2 kg over 6mo]  |
| Current: 4.2 kg (+0.2 in 3 months)            |
| Ideal range: 3.5-5.0 kg                       |
|                                                |
+-----------------------------------------------+
|                                                |
| RECENT VISITS                                  |
| +-------------------------------------------+ |
| | 12 Mar 2025  Annual vaccination           | |
| | Dr. Ahmed    Vaccines administered         | |
| +-------------------------------------------+ |
| | 15 Nov 2024  Skin issue                   | |
| | Dr. Sarah    Allergic dermatitis           | |
| +-------------------------------------------+ |
| [See all visits]                               |
|                                                |
+-----------------------------------------------+
|                                                |
| PRESCRIPTIONS                                  |
| +-------------------------------------------+ |
| | Apoquel 16mg  1x/day  Until 15 Apr 2025   | |
| | ACTIVE                                    | |
| | [Request refill]                          | |
| +-------------------------------------------+ |
| [See all prescriptions]                        |
|                                                |
+-----------------------------------------------+
```

**Notes:**
- Vertical scroll, not tabs. On mobile, scrolling is natural. Tapping between tabs adds friction.
- Alerts are at the top because they require action. If no alerts, this section is hidden.
- Each section shows the 2-3 most recent items with a "See all" link.
- "Request refill" opens the messaging flow pre-filled with the prescription details.
- Share and PDF buttons are in the top bar, always accessible.

### 6.6 Restructured Portal Home

```
+-----------------------------------------------+
| [Clinic avatar] Desert Paws Clinic   [EN/AR]  |
+-----------------------------------------------+
|                                                |
| MY PETS                                        |
| +--------+  +--------+  +--------+            |
| | [paw]  |  | [paw]  |  | [+]    |            |
| | Luna   |  | Rex    |  | Add    |            |
| | Cat    |  | Dog    |  | pet    |            |
| +--------+  +--------+  +--------+            |
|                                                |
+-----------------------------------------------+
|                                                |
| [!] 2 health alerts                           |
| Luna: rabies vaccination due in 28 days       |
| Rex: weight gain trend detected               |
| [View all alerts]                              |
|                                                |
+-----------------------------------------------+
|                                                |
| NEXT APPOINTMENT                               |
| +-------------------------------------------+ |
| | Tue 2 Apr, 10:00 AM                      | |
| | Luna -- General Checkup -- Dr. Ahmed     | |
| +-------------------------------------------+ |
|                                                |
+-----------------------------------------------+
|                                                |
| [======= Book an Appointment =======]         |
| (primary CTA, full width)                      |
|                                                |
+-----------------------------------------------+
|                                                |
| QUICK ACTIONS                                  |
| [Calendar icon] My Appointments               |
| [Message icon]  Message the clinic            |
| [Download icon] Download records              |
|                                                |
+-----------------------------------------------+
|                                                |
|           Powered by Vetara                    |
+-----------------------------------------------+

--- Bottom tab bar (mobile): ---
| Conversations | Appointments | My Pets | Profile |
```

**Notes:**
- Pets are first. The owner's mental model is "I'm here for my pet."
- Alerts are prominently displayed with action buttons.
- "Book an Appointment" is the unmissable primary CTA.
- Quick actions provide secondary paths.
- Conversations moved from primary position to quick actions -- booking and pet health are more frequent use cases than messaging.

### 6.7 Share Record

```
+-----------------------------------------------+
| [Back]  SHARE LUNA'S RECORD                   |
+-----------------------------------------------+
|                                                |
|   SCAN THIS QR CODE                           |
|                                                |
|   [=======QR CODE=======]                      |
|                                                |
|   Valid for 24 hours                           |
|   Anyone with this code can view               |
|   Luna's health record (read-only)             |
|                                                |
|   ---- or ----                                 |
|                                                |
|   [Copy secure link]                           |
|   Link valid for 7 days                        |
|                                                |
|   [Share via WhatsApp]                         |
|                                                |
+-----------------------------------------------+
```

---

## 7. Recommendations Prioritized

### P0 -- CRITICAL (Must have before B2C launch)

| # | Recommendation | Effort | Justification |
|---|---------------|--------|---------------|
| 1 | **Build B2C landing page** (`/pet-owners`) with clinic search | 3-5 days | Without this, pet owners have no entry point. The B2B landing confuses them. Zero discoverability. |
| 2 | **Build owner signup flow** (WhatsApp OTP + email fallback) | 3-5 days | Owners currently depend entirely on magic links from the clinic. They have zero agency. The clinic sends a link after a visit -- but the owner who has never visited cannot get in. |
| 3 | **Build "My Pets" section** with pet profile | 3-5 days | The nav tab exists but goes nowhere. This is the core value proposition: "See your pet's health in one place." Without it, the portal is just a booking + messaging tool. |
| 4 | **Build pet medical record view** (vaccinations, visits, prescriptions, weight) | 5-8 days | This is what owners search for on Google. This is the reason they create an account. Without it, there is nothing to retain the owner after booking. |
| 5 | **Fix dead nav links** (My Pets, Profile) | 1 day | Two out of four bottom nav tabs lead nowhere. On mobile, this is immediately visible and immediately frustrating. At minimum, show placeholder pages with "Coming soon." |
| 6 | **Restructure portal home** to be pet-first, booking-second, messaging-third | 2-3 days | Current home is messaging-only. The owner's primary needs are: see my pet's status, book an appointment, then maybe message the clinic. |

### P1 -- IMPORTANT (Next sprint after B2C launch)

| # | Recommendation | Effort | Justification |
|---|---------------|--------|---------------|
| 7 | **Surface health alerts to owners** via portal + WhatsApp | 3-5 days | Backend AI generates alerts. Owners never see them. This is the "magic moment" -- the system proactively tells you your pet needs care. High retention driver. |
| 8 | **PDF export of medical record** | 2-3 days | Owners need official documents for groomers, boarding facilities, travel, insurance, other vets. |
| 9 | **Share record via QR code / secure link** | 2-3 days | When changing vets or getting a second opinion, the owner needs to share the record instantly. |
| 10 | **Remove auto-redirect on booking success** | 0.5 hours | Current 5-second auto-redirect prevents the owner from reading the summary, downloading iCal, or taking a screenshot. Doctolib does not auto-redirect. |
| 11 | **Add slot reservation indicator** ("reserved for 15 min") | 1 day | Reduces abandonment anxiety, especially on slow mobile connections. |
| 12 | **Move "reason" field from step 2 to step 4** in booking wizard | 0.5 days | Reduces cognitive load on step 2. Reason is optional context, not a decision. |
| 13 | **WhatsApp booking confirmation + J-1 reminder** | 3-5 days | Reduces no-shows. UAE owners expect WhatsApp communication from businesses. |
| 14 | **Safe area insets** for bottom tab bar on iPhone | 0.5 hours | Prevents tab bar overlapping with iPhone home indicator. |
| 15 | **Sticky chat reply area** at bottom of conversation | 0.5 days | Prevents keyboard overlap on mobile. |

### P2 -- NICE TO HAVE (Backlog)

| # | Recommendation | Effort | Justification |
|---|---------------|--------|---------------|
| 16 | **QR check-in at clinic** | 2-3 days | Reduces reception workload on arrival. |
| 17 | **Deep links in alerts** (pre-fill booking wizard) | 1-2 days | Reduces taps from alert to booked appointment. |
| 18 | **Owner profile page** (edit name, phone, notification preferences) | 1-2 days | Completes the Profile nav tab. |
| 19 | **Pull-to-refresh** on mobile lists | 1 day | Native app feel. |
| 20 | **Polling / WebSocket for real-time messages** | 2-3 days | Currently messages are fetched once on mount. No live updates. |
| 21 | **Clinic public profile page** (SEO, vet list, map) | 3-5 days | Enhances discoverability and clinic credibility. |
| 22 | **"Recommend to your vet" flow** for clinics not on Vetara | 1-2 days | Captures demand, creates bottom-up growth loop. |
| 23 | **Read receipts in messaging** | 2 days | Owner peace of mind when messaging clinic. |
| 24 | **Cancel reason dropdown** (instead of hardcoded "Cancelled by owner") | 0.5 days | Valuable data for clinics. |

---

## 8. Open Questions for PO

These require PO decision before implementation. To be filed in `questions/` if unresolved.

### Q1: Owner account model
**Current**: Owners access the portal via magic links, scoped to a single clinic. There is no persistent owner account.
**Proposed**: Create a persistent owner account (phone + email) that can be linked to multiple clinics.
**Impact**: This is a fundamental architecture change. It means the owner entity lives outside any single clinic's tenant scope. It could live in the Auth module or a new module.
**Decision needed**: Do we build a cross-clinic owner identity now, or keep the single-clinic magic link model and add cross-clinic later?

### Q2: Medical record visibility rules
**Question**: What exactly can the owner see from their pet's medical record?
- Vaccinations: Yes (clear consensus)
- Visit dates + consultation type: Yes
- Diagnosis summary: Likely yes
- Treatment notes: Partial? (exclude internal observations)
- SOAP notes: No (vet-only)
- Prescriptions: Yes (active and past)
- Weight measurements: Yes
- Lab results: Need PO input
- Imaging (X-rays): Need PO input
**Decision needed**: Define the exact boundary between vet-facing and owner-facing data.

### Q3: Pet linking vs. pet creation
**Question**: When an owner claims "this is my pet Luna" and the clinic has a Luna in the system, what happens?
- Option A: Auto-link if name + owner phone match (fast but risk of mismatch)
- Option B: Send a link request to the clinic for approval (safe but adds friction)
- Option C: Clinic sends the magic link which is already scoped to the right pets (current model)
**Decision needed**: Which approach? Option B is recommended for data protection.

### Q4: WhatsApp channel for notifications
**Question**: The messaging spec describes an "abstract channel model" with WhatsApp planned for V2. Alerts and booking reminders need WhatsApp delivery.
**Decision needed**: Is WhatsApp Business API integration in scope for the B2C launch, or do we start with email notifications and add WhatsApp later? Budget/legal implications for WhatsApp Business API (Meta Business Verification required).

### Q5: Health alert opt-in
**Question**: Should health alerts be opt-in (owner explicitly subscribes) or opt-out (enabled by default, owner can mute)?
**Decision needed**: Opt-out is recommended for better engagement, but regulations may require opt-in for commercial communications. The health alerts are not commercial -- they are medical reminders. PO to confirm regulatory stance in UAE.

### Q6: Pricing display in booking
**Question**: The portal audit flagged the absence of prices on consultation types. Doctolib shows prices.
**Decision needed**: Should prices be visible to owners during booking? Some clinics may prefer not to show prices online. This could be a clinic-level toggle: "Show prices on portal: Yes/No."

---

## Appendix A: Competitive Advantage Summary

Features where Vetara is ahead of Doctolib (and should be preserved/amplified):

1. **AI-Powered Slot Suggestions** -- No competitor has this. Amplify with "Vetara suggests the best time for {consultation type}" messaging.
2. **Pet-Specific Context** -- Every flow is pet-aware. Doctolib is human-only. This is domain-native and should be celebrated in the B2C landing.
3. **Photo Upload in Messaging** -- "Show us what's wrong" pre-consultation triage. Saves time for the vet and the owner.
4. **RTL Arabic Support** -- Doctolib has zero Arabic. In UAE, this is table stakes.
5. **Health Alerts (once surfaced to owners)** -- Proactive care reminders. This is the "magic moment" that drives retention.
6. **Data Export** -- GDPR compliance and data portability. Not common in vet SaaS.

## Appendix B: Information Architecture (Proposed)

```
/[locale]/pet-owners                    NEW -- B2C landing with clinic search
/[locale]/clinic/[slug]                 NEW -- Clinic public profile (SEO)
/[locale]/signup                        NEW -- Owner account creation
/[locale]/portal/[clinicSlug]/          RESTRUCTURED -- Pet-first home
/[locale]/portal/[clinicSlug]/pets      NEW -- My pets list
/[locale]/portal/[clinicSlug]/pets/[id] NEW -- Pet profile (medical record)
/[locale]/portal/[clinicSlug]/pets/[id]/share    NEW -- Share record
/[locale]/portal/[clinicSlug]/profile   NEW -- Owner profile
/[locale]/portal/[clinicSlug]/book      EXISTS -- Booking landing
/[locale]/portal/[clinicSlug]/book/new  EXISTS -- Booking wizard
/[locale]/portal/[clinicSlug]/book/appointments      EXISTS -- My appointments
/[locale]/portal/[clinicSlug]/book/appointments/[id] EXISTS -- Appointment detail
/[locale]/portal/[clinicSlug]/consent   EXISTS -- Messaging consent
/[locale]/portal/[clinicSlug]/new       EXISTS -- New message
/[locale]/portal/[clinicSlug]/conversations/[id]     EXISTS -- Conversation thread
/[locale]/portal/[clinicSlug]/export    EXISTS -- Data export
/[locale]/portal/[clinicSlug]/onboarding NEW -- Pet linking after signup
```

## Appendix C: File References

Existing portal components analyzed:
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/PortalLayout.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/PortalLanding.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/ConsentScreen.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/NewMessageForm.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/PortalConversation.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/PhotoUpload.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/ExportPage.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/PetSelector.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/CategorySelector.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/booking/BookingWizard.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/booking/BookingLanding.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/booking/BookingSuccess.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/booking/StepPetSelection.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/booking/StepSlotSelection.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/booking/StepConfirmation.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/booking/MyAppointments.tsx`
- `C:/repos/vetolib-2/src/frontend/src/components/features/portal/booking/AppointmentDetail.tsx`

Portal routes:
- `C:/repos/vetolib-2/src/frontend/src/app/[locale]/portal/[clinicSlug]/layout.tsx`
- `C:/repos/vetolib-2/src/frontend/src/app/[locale]/portal/[clinicSlug]/page.tsx`

Previous audit:
- `C:/repos/vetolib-2/docs/specs/PORTAL-UX-AUDIT-2026.md`

Related specs:
- `C:/repos/vetolib-2/docs/specs/PREDICTIVE-HEALTH-ALERTS-SPEC.md`
- `C:/repos/vetolib-2/docs/specs/MESSAGING-SPEC.md`
- `C:/repos/vetolib-2/docs/specs/BREEDERS-FEATURES-SPEC.md`
