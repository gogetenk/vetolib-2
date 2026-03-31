# Platform Strategy Backlog 2026

**Date**: 2026-03-30
**Author**: Product Owner
**Status**: Draft -- awaiting founder validation
**Context**: MVP complete (174 tasks). Post-MVP features (messaging, AI, prescriptions, breeding spec) shipped or specified. This backlog covers the next 3-6 months of strategic product development across 5 epics.

---

## Table of Contents

1. [Strategic Context](#strategic-context)
2. [Epic 1 -- Pet Owner Landing & Acquisition](#epic-1--pet-owner-landing--acquisition)
3. [Epic 2 -- Owner Portal Enhancement](#epic-2--owner-portal-enhancement)
4. [Epic 3 -- Shared Medical Record / Public API](#epic-3--shared-medical-record--public-api)
5. [Epic 4 -- Positioning & B2B Landing](#epic-4--positioning--b2b-landing)
6. [Epic 5 -- Growth Hacking](#epic-5--growth-hacking)
7. [Priority Matrix](#priority-matrix)
8. [Dependency Map](#dependency-map)

---

## Strategic Context

### What exists today

The Vetara platform currently includes:

**B2B (clinic-facing)**:
- Agenda with conflict detection, recurring appointments, scheduling preferences
- Medical records (SOAP notes, vaccinations, attachments, PDF export)
- Billing with UAE VAT 5%, PDF invoices, payment tracking
- Stock/inventory with drug catalog, batch tracking, expiry alerts
- AI: symptom triage, no-show prediction, SOAP generation, drug interactions, predictive health alerts, message triage
- Messaging: staff-owner conversations, AI classification, WhatsApp integration, SSE real-time
- Team management: RBAC (Admin/Vet/Assistant/Receptionist), user invitations
- Preferences: clinic hours, Ramadan scheduling, notification settings
- Breeding: specified (F1-F8), frontend page scaffold exists

**B2C (owner-facing)**:
- Owner portal: magic link auth, booking wizard (4 steps), appointment history, messaging, conversation export
- Portal UX: rated 6.5/10 vs Doctolib (9/10) -- structural gaps identified in audit

**Marketing**:
- Landing page exists (locale-based, blog section)
- Pricing page specified (299/449/649 AED per vet/month)
- AI marketing strategy written (6 features live, 6 in pipeline)

### What is missing

1. **No B2C acquisition funnel** -- owners discover the portal only if their clinic uses Vetara. No organic owner traffic.
2. **No medical record visibility for owners** -- the portal shows appointments and messages, not the pet's health history.
3. **No interoperability** -- records are locked inside each clinic. No export, no sharing, no API.
4. **Landing page is generic** -- does not reflect the "modern + connected + intelligent" positioning.
5. **No growth loops** -- no referral, no SEO content, no partnership integrations.

### Naming note

The product was renamed from "Vetolib" to "Vetara" for marketing purposes. Internal code and repo still use "Vetolib." This backlog uses "Vetara" for customer-facing references and "Vetolib" for codebase references.

---

## Epic 1 -- Pet Owner Landing & Acquisition

### Why this epic

Today, owners arrive at the portal only through a direct link from their clinic. There is zero organic acquisition. To build network effects (owner tells friend, friend finds their own clinic on Vetara), we need a B2C entry point. The Doctolib model proves this works: owners search for a practitioner, discover the platform, book an appointment, and become recurring users.

### Persona: Pet Owner (Fatima, 32, Dubai, 2 cats)

Fatima searches Google for "veterinaire Dubai" or "mon chat ne mange plus." She lands on Vetara, finds her clinic, sees her pet's vaccination history, and books an appointment. Later, she shares the health record with a specialist when her cat is referred.

---

### E1-T1: B2C Landing Page (SEO-optimized)

**What**: A public landing page targeting pet owners, separate from the B2B landing (which targets clinic owners). Optimized for search queries like "veterinaire Dubai", "vet near me Dubai", "cat vaccination Dubai", "falcon vet Abu Dhabi."

**Why**: Organic traffic from owners is the cheapest acquisition channel. Today, zero pages on Vetara target owner search intent.

**Persona**: Pet owner searching for a vet or pet health information.

**Priority**: P0 (before launch)

**Dependencies**: None

**Gherkin scenarios to write**:
- Scenario: Owner lands on the B2C page and sees a search bar for clinics
- Scenario: Owner searches for a clinic by name and sees results
- Scenario: Owner searches for a clinic by location and sees nearby results
- Scenario: Owner clicks a clinic and lands on the clinic's portal
- Scenario: Page renders correctly in English and Arabic
- Scenario: Page loads in under 3 seconds on mobile (performance budget)

---

### E1-T2: Clinic Directory / Search

**What**: A searchable directory of clinics using Vetara. Each clinic has a public profile page showing: name, address, phone, opening hours, services offered, accepted species, vet team (names only), and a "Book Appointment" CTA.

**Why**: This is the core of the B2C acquisition funnel. An owner finds a clinic, sees it looks professional, and books. The clinic profile also provides SEO juice (one indexed page per clinic). This is exactly how Doctolib built its marketplace.

**Persona**: Pet owner looking for a vet.

**Priority**: P0 (before launch)

**Dependencies**: Requires clinics to opt-in to public listing (privacy setting).

**Gherkin scenarios to write**:
- Scenario: Owner searches for clinics in "Dubai" and sees a list sorted by distance
- Scenario: Owner filters clinics by species accepted (falcon, cat, dog)
- Scenario: Owner views a clinic profile with address, hours, team, and services
- Scenario: Clinic admin enables public listing from settings
- Scenario: Clinic admin disables public listing and the clinic disappears from search
- Scenario: Clinic profile shows a "Book Appointment" button linking to the portal
- Scenario: Search returns zero results with a friendly message and suggestion

---

### E1-T3: "Ask Your Vet About Vetara" Page

**What**: A page where a pet owner can enter their vet's name and email. Vetara sends a personalized email to the vet explaining the platform, with a pitch tailored to "your client [owner name] wants to use Vetara to manage their pet's health -- here is why you should join."

**Why**: This is a low-cost, high-signal growth loop. The owner does the sales outreach for us. The vet receives a warm lead (their own client asking for it), which converts far better than cold outreach. Doctolib used exactly this mechanism in its early growth phase.

**Persona**: Pet owner whose vet is not yet on Vetara.

**Priority**: P1 (month 1 post-launch)

**Dependencies**: E1-T2 (owner must first search and not find their clinic). Email infrastructure (already exists -- done-back-email-001).

**Gherkin scenarios to write**:
- Scenario: Owner searches for their clinic, gets no results, and sees "Invite your vet" CTA
- Scenario: Owner fills the form with vet name, clinic name, and email
- Scenario: Vet receives a branded email with owner's name and a signup link
- Scenario: Rate limiting prevents spam (max 3 invitations per owner per day)
- Scenario: Owner cannot invite the same vet email twice within 30 days

---

### E1-T4: Owner Onboarding Flow -- "Find Your Clinic & Pet"

**What**: After an owner lands on the B2C page and finds their clinic, they need to authenticate and link to their existing pet records. The flow: (1) Enter phone/email, (2) Receive magic link, (3) If the clinic already has a patient record linked to that phone/email, auto-match and show "Is this your pet?", (4) Owner confirms, (5) Owner sees their pet's profile.

**Why**: Today, owners arrive at the portal via a direct clinic link and can only book. They cannot see their pet's existing records. This "claim your pet" flow is essential for the owner to feel ownership over the health data. It also validates the owner's identity against the clinic's database, preventing unauthorized access.

**Persona**: Pet owner who already has a record at a Vetara clinic.

**Priority**: P0 (before launch)

**Dependencies**: Owner portal auth (already exists -- magic link). Patient-Owner relationship in DB (already exists -- PatientOwners join table).

**Gherkin scenarios to write**:
- Scenario: Owner authenticates and sees a list of pets linked to their phone/email across clinics
- Scenario: Owner confirms pet ownership and gains access to the pet profile
- Scenario: Owner with no matching records is prompted to register a new pet
- Scenario: Owner sees pets from multiple clinics if they exist
- Scenario: Clinic receptionist can manually link an owner to a patient if auto-match fails

---

### E1-T5: Owner UX -- Ideal Experience Design

**What**: Define and implement the "golden path" for a pet owner on Vetara. Based on the Portal UX Audit (6.5/10 score), the current portal is messaging-centric, not appointment-centric. The redesigned experience should be:

1. **Landing**: Clinic info card + "Book Appointment" CTA + upcoming appointment + quick links
2. **My Pets**: List of pets with photo, species, breed, age. Tap to see pet profile.
3. **Pet Profile**: Vaccination status (green/yellow/red), next appointment, weight trend, recent visits. Tap to see full medical history (Epic 2).
4. **Book**: Streamlined 3-step wizard (pet + reason -> slot -> confirm). Remove the overloaded Step 2.
5. **Messages**: Secondary, accessible but not the landing page.

**Why**: The Portal UX Audit identified structural gaps that reduce conversion. An owner who cannot find the "Book" button or see clinic info will not return. Fixing these gaps is prerequisite for any B2C acquisition strategy.

**Persona**: Pet owner using the portal on mobile (80%+ of UAE traffic is mobile).

**Priority**: P0 (before launch)

**Dependencies**: Portal UX Audit recommendations (already documented). E1-T4 (pet linking).

**Gherkin scenarios to write**:
- Scenario: Owner lands on portal and sees clinic info (name, address, hours) above the fold
- Scenario: Owner sees a prominent "Book Appointment" button on the landing page
- Scenario: Owner sees their upcoming appointment on the landing page
- Scenario: Owner navigates to "My Pets" and sees all linked pets with photos
- Scenario: Owner taps a pet and sees a summary card (vaccination status, weight, next appointment)
- Scenario: Booking wizard completes in 3 steps or fewer
- Scenario: Confirmation screen does not auto-redirect (owner stays until they choose to leave)

---

### E1-T6: PWA / App Install Prompt

**What**: Make the owner portal installable as a Progressive Web App. When an owner visits the portal on mobile for the second time, show an "Add to Home Screen" prompt. The PWA should work offline for viewing cached pet data (vaccination list, upcoming appointments).

**Why**: A PWA icon on the phone keeps Vetara top-of-mind. Push notifications (Epic 2) require a PWA or native app. A PWA is 10x cheaper than a native app and sufficient for MVP.

**Persona**: Pet owner on mobile.

**Priority**: P1 (month 1)

**Dependencies**: E1-T5 (portal redesign must be done first -- no point installing a poor experience).

**Gherkin scenarios to write**:
- Scenario: Owner visiting portal on mobile for the second time sees an install prompt
- Scenario: Owner installs PWA and sees the Vetara icon on home screen
- Scenario: Owner opens PWA offline and sees cached pet data (vaccinations, next appointment)
- Scenario: Owner opens PWA online and data refreshes automatically

---

## Epic 2 -- Owner Portal Enhancement

### Why this epic

The portal today handles booking and messaging. But the real stickiness for owners comes from the medical record: vaccination history, prescriptions, visit summaries. When an owner can show a vet "here is my pet's complete history" from their phone, they will never leave the platform. This is the data moat.

### Persona: Pet Owner (Ahmed, 45, Abu Dhabi, 1 Saluki + 2 falcons)

Ahmed has been going to the same clinic for 5 years. He wants to see all vaccinations, know when the next rabies shot is due, and share the falcon's health record with the Abu Dhabi Falcon Hospital when his bird is referred for endoscopy.

---

### E2-T1: Medical Record Viewer (Owner-Facing)

**What**: A read-only view of the pet's medical history, accessible from the owner portal. Shows: visit timeline (date, reason, diagnosis, prescriptions), vaccination list with due dates, and attached documents (lab results, imaging).

**Why**: This is the single most valuable feature for owner retention. "See your pet's complete health history" is a message that resonates with every pet owner. No competitor in UAE offers this.

**Persona**: Pet owner.

**Priority**: P0 (before launch)

**Dependencies**: E1-T4 (owner must be linked to pet). Existing medical record endpoints (already built). RBAC: new "OWNER" read scope needed (owner sees a filtered view -- no internal clinical notes, no financial data).

**Important business rule**: The owner sees a **filtered** version of the medical record:
- YES: visit date, reason, diagnosis, prescriptions, vaccinations, weight, attachments marked as "shareable"
- NO: internal clinical notes, differential diagnosis, staff comments, financial data
- The vet controls what is shareable via a "visible to owner" toggle per record/attachment

**Gherkin scenarios to write**:
- Scenario: Owner views visit timeline for their pet showing date, reason, and diagnosis
- Scenario: Owner views vaccination list with names, dates, and next due dates
- Scenario: Owner views prescriptions with drug name, dosage, and duration
- Scenario: Owner cannot see internal clinical notes marked as staff-only
- Scenario: Owner sees only attachments marked as "shareable" by the vet
- Scenario: Vet toggles "visible to owner" on a medical record and owner can now see it
- Scenario: Owner with no medical records sees a friendly empty state

---

### E2-T2: Vaccination Reminders (Push + WhatsApp)

**What**: Automated reminders sent to pet owners when a vaccination is due or overdue. Channels: push notification (requires PWA -- E1-T6), WhatsApp (already integrated -- done-back-whatsapp-001), and email (already integrated).

**Why**: Vaccination reminders are the #1 driver of repeat visits for companion animal clinics. A reminder that says "Luna's rabies vaccine is due in 2 weeks -- book now" converts at very high rates. This is revenue generation for the clinic AND a genuine health benefit for the animal.

**Persona**: Pet owner.

**Priority**: P0 (before launch)

**Dependencies**: E2-T1 (vaccination data must be visible to owner). WhatsApp integration (done). Push notifications require PWA (E1-T6) or can start with WhatsApp/email only.

**Business rules**:
- Reminder schedule: 30 days before due, 7 days before due, on the day, 7 days overdue
- Clinic can customize the schedule per vaccine type
- Owner can opt out of reminders per channel (push, WhatsApp, email)
- Reminder includes a direct "Book Now" link to the portal booking wizard
- Language: owner's preferred language (EN or AR)

**Gherkin scenarios to write**:
- Scenario: Owner receives a WhatsApp reminder 30 days before rabies vaccine is due
- Scenario: Owner receives an email reminder 7 days before vaccine is due
- Scenario: Reminder includes a "Book Now" link that opens the booking wizard with pet pre-selected
- Scenario: Owner opts out of WhatsApp reminders and stops receiving them
- Scenario: Clinic customizes reminder schedule to 14 days and 3 days before due
- Scenario: Owner with overdue vaccination receives an urgent reminder
- Scenario: Reminder is sent in Arabic when owner's preferred language is Arabic

---

### E2-T3: Medical Record Sharing (QR Code / Link)

**What**: An owner can generate a shareable link or QR code for their pet's medical record. The link provides read-only access to the filtered medical record (same view as E2-T1) for a limited time (configurable: 24h, 7 days, 30 days, permanent). The recipient does not need a Vetara account.

**Why**: This is the entry point for the shared medical record vision (Epic 3). Before building a full FHIR API, we start with the simplest version: a link that shows the pet's health history. Use cases: (1) referral to specialist, (2) travel with pet (show vaccination proof), (3) new clinic (share history), (4) pet sitter needs medical info.

**Persona**: Pet owner sharing with a vet, a pet sitter, or border control.

**Priority**: P1 (month 1)

**Dependencies**: E2-T1 (medical record viewer).

**Business rules**:
- Only the owner can generate a share link (not the vet -- the owner controls their data)
- Share link includes: pet identity (name, species, breed, microchip), vaccination history, allergies, chronic conditions, recent visit summaries
- Share link does NOT include: financial data, internal clinical notes, owner personal contact info
- Expiry: owner chooses duration. Default 7 days.
- Revocable: owner can revoke a share link at any time
- Audit trail: every access to a shared record is logged (IP, timestamp, country)
- QR code: generated client-side, encodes the share link URL

**Gherkin scenarios to write**:
- Scenario: Owner generates a share link for their pet's medical record
- Scenario: Owner selects expiry duration (24h, 7 days, 30 days)
- Scenario: Recipient opens the link and sees the pet's health summary without logging in
- Scenario: Owner revokes a share link and the recipient gets "Link expired"
- Scenario: Owner generates a QR code that encodes the share link
- Scenario: Share link accessed after expiry shows "This record has expired"
- Scenario: Audit log shows all accesses to the shared record

---

### E2-T4: Online Payment (Deferred -- NOT MVP)

**What**: Allow owners to pay invoices online through the portal. Integrate with Stripe (international cards), Tabby/Tamara (BNPL).

**Why**: Important for convenience and revenue acceleration, but NOT blocking for launch. UAE clinics currently handle payments at the counter. BNPL is a differentiator but requires payment provider agreements and PCI compliance.

**Persona**: Pet owner.

**Priority**: P2 (month 3+)

**Dependencies**: Billing module (done). Payment provider agreements (business dependency, not technical).

**Decision**: Deferred. The Tabby/Tamara integration was identified in market research as a first-mover advantage, but the legal/business setup (merchant account, PCI, BNPL provider agreement) takes 2-3 months. Start the business process now, build the integration when agreements are signed.

**Gherkin scenarios to write** (for when implemented):
- Scenario: Owner views an unpaid invoice on the portal and sees a "Pay Now" button
- Scenario: Owner pays with credit card via Stripe
- Scenario: Owner chooses "Pay in 4 installments" via Tabby
- Scenario: Payment confirmation is reflected in the clinic's billing module in real-time
- Scenario: Owner receives a payment receipt by email

---

### E2-T5: Owner Notification Preferences

**What**: A settings page in the owner portal where the owner can manage their notification preferences: which channels (email, WhatsApp, push), which types (appointment reminders, vaccination alerts, messages from clinic), and language preference.

**Why**: Owners must control their communication preferences. This is both a UX requirement (avoid notification fatigue) and a compliance requirement (UAE PDPL and anti-spam rules). Also, the clinic should not be able to override owner preferences.

**Persona**: Pet owner.

**Priority**: P1 (month 1)

**Dependencies**: E1-T6 (push requires PWA). WhatsApp integration (done). Email (done).

**Gherkin scenarios to write**:
- Scenario: Owner accesses notification settings from the portal
- Scenario: Owner disables WhatsApp notifications and enables email only
- Scenario: Owner sets preferred language to Arabic for all communications
- Scenario: Clinic sends a reminder and the owner's channel preference is respected
- Scenario: New owner has all channels enabled by default (opt-out model)

---

## Epic 3 -- Shared Medical Record / Public API

### Why this epic

The interoperability study (2026-03-31) concluded that **no one in the world** does bidirectional, standardized veterinary medical record sharing. This is a blue ocean opportunity. IDEXX and Zoetis share only their own lab results. Instinct (Shareville) does unidirectional sharing within its ecosystem. The pain point is real: referrals and emergencies suffer from missing history. The microchip ISO 11784/11785 provides a universal identifier already implanted in hundreds of millions of animals.

The strategy: start simple (E2-T3: share link), then build the infrastructure (this epic), then open the API (developer ecosystem).

### Persona: Referral Vet (Dr. Khalid, emergency vet, Dubai)

Dr. Khalid receives an emergency case at 2 AM. The animal has a microchip. He scans it, Vetara returns the full medical history from the primary clinic. He sees the drug allergies, the chronic condition, the last blood panel. He makes an informed decision in 30 seconds instead of 30 minutes of phone calls and faxes.

---

### E3-T1: Portable Health Record -- Export

**What**: A clinic can export a patient's complete medical record as a structured document (FHIR R4 Bundle with Patient-Animal extension). The export is triggered manually by the vet or automatically when a referral is created.

**Why**: This is step 1 of interoperability. Before anyone can import, someone must export. By making export free and easy for all Vetara clinics, we build the supply side of the network. The interop study recommends this exact approach: "Export gratuit et automatique pour toutes les cliniques Vetolib existantes."

**Persona**: Referring vet.

**Priority**: P1 (month 1)

**Dependencies**: Medical record module (done). Microchip field on Patient (specified in BREEDERS-FEATURES-SPEC F2, not yet built). FHIR format definition (needs technical study).

**Business rules**:
- Export includes: patient identity (name, species, breed, microchip, DOB, sex), vaccinations, allergies, chronic conditions, encounter summaries, lab results, prescriptions
- Export does NOT include: financial data, internal clinical notes, staff-only attachments
- Export format: FHIR R4 JSON Bundle (with patient-animal extension)
- Export also available as human-readable PDF (for vets who do not support FHIR)
- Owner consent is recorded before the first export (one-time consent per pet)
- Audit trail: every export is logged

**Gherkin scenarios to write**:
- Scenario: Vet exports a patient record as FHIR JSON
- Scenario: Vet exports a patient record as a readable PDF
- Scenario: Export includes vaccination history, allergies, and encounter summaries
- Scenario: Export excludes financial data and internal notes
- Scenario: Export requires owner consent (first time only)
- Scenario: Export is logged in the audit trail with vet identity and timestamp

---

### E3-T2: Portable Health Record -- Import

**What**: A clinic can import a FHIR R4 Bundle to populate a patient's history. This is used when receiving a referral or when an animal changes primary clinic.

**Why**: This is the demand side. The referral/emergency vet receives the history instantly instead of spending 15-30 minutes on phone/fax. The interop study pricing model: import/full-history is the paid feature (Pro tier and above).

**Persona**: Receiving vet (specialist, emergency).

**Priority**: P2 (month 3)

**Dependencies**: E3-T1 (export must exist first). FHIR parser (technical).

**Business rules**:
- Imported records are clearly marked as "external" (from [Clinic Name])
- Imported records are read-only (the receiving clinic cannot modify another clinic's records)
- Conflict handling: if the patient already exists (matched by microchip), the imported records are merged into the timeline alongside local records
- If the patient does not exist, a new patient is created from the import
- Imported vaccinations contribute to the vaccination schedule/reminders

**Gherkin scenarios to write**:
- Scenario: Vet imports a FHIR Bundle and a new patient is created with full history
- Scenario: Vet imports records for an existing patient (matched by microchip) and records are merged
- Scenario: Imported records are displayed with "External -- from [Clinic Name]" label
- Scenario: Imported records cannot be edited by the receiving clinic
- Scenario: Import of an invalid FHIR Bundle shows a clear error message

---

### E3-T3: Microchip-Based Lookup

**What**: A vet scans or enters a microchip number. If any Vetara clinic has a record for that animal (and the owner has consented to sharing), the system returns the animal's identity and a "Request Full Record" option.

**Why**: This is the "magic moment" of the platform. Scan a chip, get the history. This is the experience that will make emergency vets say "I need Vetara." It is also the trigger for clinics to join the network: "150 records of your referred patients are waiting for you."

**Persona**: Emergency vet, specialist receiving a referral.

**Priority**: P2 (month 3)

**Dependencies**: E3-T1 (export). Microchip on Patient entity (BREEDERS-FEATURES-SPEC F2). Owner consent for sharing (E3-T5).

**Business rules**:
- Lookup is cross-clinic (searches all clinics where the owner consented to sharing)
- Lookup returns: animal name, species, breed, age, primary clinic name. NOT the full record.
- Full record access requires: (a) a valid relationship (referral, emergency) AND (b) owner consent
- Rate limiting: max 50 lookups per clinic per day (prevent scraping)
- Lookup is a Pro/Enterprise tier feature (free tier sees "Records available -- upgrade to access")

**Gherkin scenarios to write**:
- Scenario: Vet enters a microchip number and sees the animal's identity and primary clinic
- Scenario: Vet requests the full record and the owner is notified for consent
- Scenario: Owner approves the request and the vet receives the complete history
- Scenario: Microchip not found in the system shows a "No records found" message
- Scenario: Lookup for an animal whose owner has not consented shows "Records exist but sharing is not authorized"
- Scenario: Free tier clinic sees a teaser with upgrade prompt

---

### E3-T4: Public API Documentation (Scalar)

**What**: A public-facing API documentation page at `api.vetara.com` (or `/developers`) using Scalar (already used internally for OpenAPI docs). Documents the FHIR export/import endpoints, authentication, rate limits, and data model.

**Why**: To attract third-party PMS vendors, lab integrations, and insurance companies, we need developer-friendly documentation. The "Developers" page signals that Vetara is a platform, not just a product. This is how Stripe, Twilio, and Doctolib built their ecosystems.

**Persona**: Developer at a partner company (lab, insurance, other PMS).

**Priority**: P2 (month 3)

**Dependencies**: E3-T1 and E3-T2 (API endpoints must exist to document).

**Gherkin scenarios to write**:
- Scenario: Developer visits the API documentation page and sees endpoint descriptions
- Scenario: Developer uses the sandbox to make a test API call
- Scenario: API documentation includes authentication examples
- Scenario: API documentation includes FHIR data model with examples

---

### E3-T5: Owner Consent Management

**What**: A consent management screen in the owner portal where the owner controls who can access their pet's health record. Options: (a) "Share with any Vetara clinic" (global opt-in), (b) "Share only when I approve" (per-request), (c) "Do not share" (opt-out).

**Why**: Owner consent is both a legal requirement (UAE PDPL, GDPR for France expansion) and a trust requirement. Vets are territorial about "their" data. Positioning the owner as the one who controls sharing defuses the "my patients, my data" resistance. The interop study recommends: "Opt-in explicite du proprietaire pour chaque partage de dossier, audit trail complet."

**Persona**: Pet owner.

**Priority**: P1 (month 1)

**Dependencies**: E2-T1 (owner must be able to see what they are consenting to share).

**Business rules**:
- Default: "Share only when I approve" (opt-in per request)
- Consent is per-pet, not global (owner may consent for one pet but not another)
- Consent can be changed at any time
- Each consent change is logged (audit trail)
- When consent is revoked, existing shared links (E2-T3) are also revoked

**Gherkin scenarios to write**:
- Scenario: Owner sets sharing preference to "Share with any Vetara clinic"
- Scenario: Owner sets sharing preference to "Share only when I approve"
- Scenario: Owner revokes consent and all active share links are deactivated
- Scenario: Vet requests record access and owner receives an approval notification
- Scenario: Owner approves a specific clinic's access request
- Scenario: Owner denies a clinic's access request with a reason
- Scenario: Consent changes are visible in the audit trail

---

### E3-T6: Developer Sandbox

**What**: A sandbox environment where third-party developers can test the FHIR API with mock data. Includes test animals, test clinics, and test records. API keys are free for sandbox use.

**Why**: No developer will integrate a production API they cannot test first. A sandbox reduces integration time from weeks to days.

**Persona**: Developer at a partner company.

**Priority**: P2 (month 3)

**Dependencies**: E3-T4 (API documentation).

**Gherkin scenarios to write**:
- Scenario: Developer registers for a sandbox API key
- Scenario: Developer queries the sandbox and receives mock FHIR data
- Scenario: Developer imports a test record into the sandbox
- Scenario: Sandbox data is reset daily (no persistent test data)

---

## Epic 4 -- Positioning & B2B Landing

### Why this epic

The current landing page does not reflect the product's maturity or positioning. The AI marketing strategy (2026-03-30) recommends making AI the hero message. But the founder's direction adds two more pillars: "connected" (shared records) and "modern" (UX). The landing must communicate all three without being "UAE-only" or "AI-only."

### Persona: Clinic Owner / Practice Manager (Dr. Reem, 38, Dubai, 4-vet clinic)

Dr. Reem is evaluating PMS options. She visits the Vetara website. She needs to understand in 10 seconds: (1) what is this, (2) why is it different from ezyVet, (3) how much does it cost. If the landing page says "AI-powered" she is curious but skeptical. If it says "AI-powered + shared medical records + designed for the Gulf" she is intrigued.

---

### E4-T1: Hero Section Redesign

**What**: Redesign the landing page hero to communicate the three pillars:
1. **Intelligent** -- AI that predicts, triages, and documents (existing strength)
2. **Connected** -- Shared medical records, interoperability, owner portal (vision)
3. **Modern** -- Beautiful UX, bilingual, built for the Gulf (differentiator vs legacy PMS)

**Why**: The hero is the first and often only thing a visitor sees. It must instantly differentiate from "yet another clinic management software."

**Proposed hero structure**:
- Headline: "The veterinary platform your clinic deserves"
- Subheadline: "AI-powered clinical tools. Shared medical records. Built for veterinary teams in the Gulf."
- Primary CTA: "Start Free Trial" (30 days)
- Secondary CTA: "Watch Demo" (2-minute video)

**Priority**: P0 (before launch)

**Dependencies**: None (copy + design only).

**Gherkin scenarios to write**:
- Scenario: Visitor lands on the homepage and sees the hero with three value pillars
- Scenario: Visitor clicks "Start Free Trial" and is redirected to signup
- Scenario: Visitor clicks "Watch Demo" and sees a product video
- Scenario: Homepage loads in under 2 seconds on desktop
- Scenario: Homepage renders correctly in Arabic (RTL)

---

### E4-T2: AI Features Section (Position 2)

**What**: A dedicated section below the hero showcasing the 6 live AI features. Each feature gets a card with: name, one-sentence description, screenshot or animation, and "Learn more" link.

**Why**: AI is the strongest differentiator today (6 features shipped, zero competitors have any). The AI marketing strategy recommends: "Do not bury AI features in a Features page. They should be visible within the first scroll."

**Priority**: P0 (before launch)

**Dependencies**: None.

**Gherkin scenarios to write**:
- Scenario: Visitor scrolls past the hero and sees AI feature cards
- Scenario: Each AI feature card shows a title, description, and visual
- Scenario: Visitor clicks "Learn more" on a feature and sees detailed explanation

---

### E4-T3: Shared Records Vision Section (Position 3)

**What**: A section presenting the shared medical record vision. Not as a feature that exists today, but as a roadmap commitment. Message: "Your patients' records, available everywhere -- coming soon." Include: (1) the problem (records locked in silos), (2) the vision (microchip scan = instant history), (3) the timeline ("launching Q3 2026").

**Why**: This positions Vetara as a platform, not just a product. It signals long-term vision to clinic owners who are making a 3-5 year software commitment. The interop study shows this is a blue ocean -- no competitor can claim this.

**Important**: Be honest about timeline. Do not show it as a shipped feature. Vets respect transparency.

**Priority**: P0 (before launch)

**Dependencies**: None (marketing content only).

**Gherkin scenarios to write**:
- Scenario: Visitor sees the shared records section with a clear "Coming Soon" label
- Scenario: Section explains the problem (records locked in silos) and the solution
- Scenario: Section includes a "Notify me when available" email capture

---

### E4-T4: Pricing Page

**What**: Implement the pricing page per PRICING-FINAL-2026.md. Three tiers (Starter 299 AED, Pro 449 AED, Enterprise 649 AED per vet/month). Feature comparison table. Annual discount toggle. "Start Free Trial" CTA on each tier.

**Why**: Pricing transparency is a differentiator (market research: most competitors hide pricing behind "contact sales"). UAE clinic owners comparison-shop. A clear pricing page reduces sales friction.

**Priority**: P0 (before launch)

**Dependencies**: PRICING-FINAL-2026.md (already validated).

**Gherkin scenarios to write**:
- Scenario: Visitor sees three pricing tiers with AED prices
- Scenario: Visitor toggles to annual pricing and sees 15% discount
- Scenario: Visitor toggles currency to EUR and sees converted prices
- Scenario: Feature comparison table shows differences between tiers
- Scenario: Visitor clicks "Start Free Trial" on a tier and is redirected to signup with tier pre-selected

---

### E4-T5: Testimonials Section

**What**: A testimonials section on the landing page. For launch, use placeholder quotes attributed to "Dr. A., Dubai" (anonymized). Replace with real testimonials once pilot clinics provide them.

**Why**: Social proof is essential in B2B SaaS. A single quote from a known Dubai vet is worth more than 10 feature descriptions.

**Priority**: P1 (month 1 -- placeholders at launch, real quotes when available)

**Dependencies**: Pilot clinic relationships (business dependency).

**Gherkin scenarios to write**:
- Scenario: Visitor sees at least 3 testimonial cards with name, clinic, and quote
- Scenario: Testimonials rotate or are displayed in a carousel on mobile

---

### E4-T6: Demo Booking Flow

**What**: A "Book a Demo" page where a clinic owner/manager fills a short form (name, clinic name, email, number of vets, country). The form triggers an email to the Vetara sales team and a confirmation email to the prospect. Optionally, embed a Calendly-like scheduling widget.

**Why**: Enterprise and large clinics want a personalized demo before committing. This is standard in B2B SaaS and complements the self-service "Free Trial" path.

**Priority**: P1 (month 1)

**Dependencies**: None.

**Gherkin scenarios to write**:
- Scenario: Visitor fills the demo request form and receives a confirmation email
- Scenario: Sales team receives a notification with the prospect's details
- Scenario: Form validates required fields (name, email, clinic name)
- Scenario: Visitor from UAE sees "Book a Demo" CTA, visitor from France sees "Demander une demo"

---

## Epic 5 -- Growth Hacking

### Why this epic

Product quality alone does not drive adoption in a market with entrenched habits. UAE vets use paper, Excel, or legacy systems. Growth loops are needed: SEO content that brings owners organically, referral programs that viralize through word-of-mouth, partnerships that provide institutional credibility, and lab integrations that create switching costs.

---

### E5-T1: Blog Content Strategy (Owner SEO)

**What**: A content calendar of blog articles targeting pet owners in UAE. Published on the Vetara blog (already exists as a page scaffold). Each article targets a specific search query and includes a CTA to find a vet on Vetara or learn about the platform.

**Article plan (first 6 months)**:

| # | Title | Target Query | CTA |
|---|-------|-------------|-----|
| 1 | "Complete Vaccination Schedule for Dogs in UAE" | "dog vaccination schedule UAE" | Find a vet near you |
| 2 | "How to Read Your Cat's Health Record" | "cat health record" | See your pet's records on Vetara |
| 3 | "Falcon Health: 5 Signs of Aspergillosis" | "falcon aspergillosis symptoms" | Find a falcon vet |
| 4 | "Moving to UAE with Your Pet: MOCCAE Requirements" | "bring pet to UAE" | Generate your pet's travel document |
| 5 | "Microchipping Your Pet in Dubai: Everything You Need to Know" | "microchip pet Dubai" | Track your pet's records by microchip |
| 6 | "Pet Insurance in UAE: Is It Worth It?" | "pet insurance UAE" | Partner insurance links |
| 7 | "Ramadan Pet Care: Adjusting Your Pet's Routine" | "pet care Ramadan" | Book a check-up |
| 8 | "Breeding Your Saluki: A Complete Guide" | "Saluki breeding guide" | Vetara breeding module |

**Why**: SEO content is a long-term compounding asset. Each article brings organic traffic that converts to portal users (B2C) or clinic leads (B2B via "Ask Your Vet" -- E1-T3).

**Persona**: Pet owner searching for information.

**Priority**: P1 (start publishing month 1, compound over time)

**Dependencies**: Blog page (exists). Content creation (business/marketing task, not engineering).

**Gherkin scenarios to write**:
- Scenario: Blog article is published and accessible at /blog/{slug}
- Scenario: Blog article includes structured data (schema.org) for SEO
- Scenario: Blog article includes a contextual CTA linking to the portal or landing page
- Scenario: Blog articles are available in English and Arabic

---

### E5-T2: Referral Program (Owner-to-Owner)

**What**: An owner can invite another pet owner to Vetara via a referral link. When the referred owner books their first appointment through Vetara, both get a reward. Reward options: (a) credit toward next vet visit (requires payment integration -- deferred), (b) priority booking slot, (c) "early access" to new features.

**Why**: Word-of-mouth is the strongest acquisition channel for local services. Pet owner communities in Dubai (Facebook groups, building WhatsApp groups) are highly active. A referral program gives them a reason to share.

**Persona**: Pet owner.

**Priority**: P2 (month 3 -- requires payment integration for financial rewards, or launch with non-financial rewards)

**Dependencies**: E1-T4 (owner accounts). Payment integration (for financial rewards -- E2-T4).

**Decision**: Launch with non-financial rewards first (badge on profile, "VIP" status, early access). Add financial rewards when payment integration ships.

**Gherkin scenarios to write**:
- Scenario: Owner generates a referral link from the portal
- Scenario: Referred owner signs up via the link and both are credited
- Scenario: Owner sees referral stats (invitations sent, successful referrals)
- Scenario: Referral link tracks the source correctly across devices

---

### E5-T3: Referral Program (Vet-to-Vet)

**What**: A clinic already on Vetara can refer another clinic. When the referred clinic subscribes to a paid plan, the referring clinic gets 1 month free (per PRICING-FINAL-2026.md).

**Why**: Vets trust other vets. A recommendation from a colleague carries more weight than any marketing. The pricing document already defines this: "Parrainage: 1 mois gratuit pour le parrain quand la clinique referee convertit en paye."

**Persona**: Clinic admin/vet.

**Priority**: P1 (month 1)

**Dependencies**: Subscription/billing system for clinics (to track referral credits).

**Gherkin scenarios to write**:
- Scenario: Clinic admin generates a referral code from settings
- Scenario: Referred clinic enters the code during signup
- Scenario: When referred clinic converts to paid, referring clinic receives 1 month credit
- Scenario: Clinic admin sees referral stats (codes shared, clinics converted)

---

### E5-T4: IDEXX Lab Results Integration

**What**: Integration with IDEXX VetConnect Plus to automatically import lab results (blood work, urinalysis, fecal analysis) into the patient's medical record. IDEXX is the dominant lab equipment provider in UAE.

**Why**: Lab integration is "table stakes" for serious PMS adoption (per market research). More importantly, it is an adoption hook: once a clinic's lab results flow into Vetara automatically, the switching cost to another PMS becomes very high. IDEXX results also enrich the shared medical record (Epic 3), making the export more valuable.

**Persona**: Vet.

**Priority**: P2 (month 3)

**Dependencies**: Medical record module (done). IDEXX partnership/API agreement (business dependency). The interop study notes: "IDEXX pourrait vouloir controler" -- approach as a partnership, not a competitor.

**Gherkin scenarios to write**:
- Scenario: Lab results from IDEXX appear automatically in the patient's medical record
- Scenario: Vet reviews IDEXX results with reference ranges and abnormality flags
- Scenario: IDEXX results are included in the portable health record export (E3-T1)
- Scenario: Lab results sync fails gracefully with a retry mechanism and notification

---

### E5-T5: MOCCAE Partnership Exploration

**What**: Explore a partnership with MOCCAE (Ministry of Climate Change and Environment) to position Vetara as a compliant record-keeping system. Potential: generate MOCCAE-compliant health certificates for pet import/export directly from the medical record.

**Why**: MOCCAE compliance is a pain point for UAE vets (manual paperwork for health certificates). If Vetara auto-generates MOCCAE forms, it becomes indispensable for clinics that handle import/export cases. This also gives institutional credibility.

**Persona**: Vet handling pet import/export.

**Priority**: P2 (month 3 -- business exploration, not engineering)

**Dependencies**: Medical record with vaccination history (done). Microchip field (BREEDERS-FEATURES-SPEC F2).

**This is primarily a business/partnership task, not an engineering task.** Technical scope TBD after partnership discussions.

**Gherkin scenarios to write** (if partnership proceeds):
- Scenario: Vet generates a MOCCAE health certificate from a patient's record
- Scenario: Certificate includes microchip number, vaccination history, and vet signature
- Scenario: Certificate is generated as a PDF matching MOCCAE's official format

---

### E5-T6: I-CAD Integration (France Market)

**What**: Integration with I-CAD (French national animal identification database, 42M+ animals). Lookup a microchip number against I-CAD to auto-populate patient identity (name, species, breed, DOB, owner name).

**Why**: I-CAD is the authoritative source for animal identity in France. Auto-populating patient data from a microchip scan saves time and reduces errors. The interop study calls this a "partenariat strategique potentiel -- I-CAD a l'ID, Vetolib a le dossier medical."

**Persona**: French vet.

**Priority**: P2 (month 3+ -- France market is secondary to UAE)

**Dependencies**: Microchip field (BREEDERS-FEATURES-SPEC F2). I-CAD API access (business dependency -- requires formal partnership).

**Gherkin scenarios to write** (if partnership proceeds):
- Scenario: Vet scans a microchip and patient data is auto-populated from I-CAD
- Scenario: I-CAD lookup returns no match and the vet enters data manually
- Scenario: I-CAD data is used as default but the vet can override any field

---

## Priority Matrix

### P0 -- Before Launch (blocks go-to-market)

| Task | Epic | Effort Estimate | Justification |
|------|------|----------------|---------------|
| E1-T2: Clinic Directory / Search | 1 | Large | Core B2C acquisition funnel. Without this, owners cannot find clinics organically. |
| E1-T4: Owner Onboarding ("Find Your Pet") | 1 | Medium | Owners must link to existing pets to see value. |
| E1-T5: Owner UX Redesign | 1 | Large | Portal rated 6.5/10. Must fix before driving traffic to it. |
| E2-T1: Medical Record Viewer (Owner) | 2 | Medium | The #1 owner retention feature. "See your pet's history." |
| E2-T2: Vaccination Reminders | 2 | Medium | Direct revenue driver for clinics (repeat visits). |
| E4-T1: Hero Section Redesign | 4 | Small | First impression for every visitor. |
| E4-T2: AI Features Section | 4 | Small | Strongest differentiator must be visible. |
| E4-T3: Shared Records Vision Section | 4 | Small | Signals long-term platform vision. |
| E4-T4: Pricing Page | 4 | Small | Pricing transparency is a differentiator. |
| E1-T1: B2C Landing Page | 1 | Medium | SEO entry point for owners. |

### P1 -- Month 1 Post-Launch

| Task | Epic | Effort Estimate | Justification |
|------|------|----------------|---------------|
| E1-T3: "Ask Your Vet" Page | 1 | Small | Low-cost growth loop. |
| E1-T6: PWA Install Prompt | 1 | Small | Retention mechanism. |
| E2-T3: Medical Record Sharing (QR/Link) | 2 | Medium | Entry point for interoperability. |
| E2-T5: Owner Notification Preferences | 2 | Small | Compliance + UX. |
| E3-T1: Portable Health Record -- Export | 3 | Large | Foundation for shared records. Requires FHIR study. |
| E3-T5: Owner Consent Management | 3 | Medium | Legal prerequisite for sharing. |
| E4-T5: Testimonials | 4 | Small | Social proof. |
| E4-T6: Demo Booking | 4 | Small | Enterprise sales channel. |
| E5-T1: Blog Content (first 4 articles) | 5 | Medium | SEO compounds over time. Start early. |
| E5-T3: Vet-to-Vet Referral | 5 | Small | Already defined in pricing. |

### P2 -- Month 3 Post-Launch

| Task | Epic | Effort Estimate | Justification |
|------|------|----------------|---------------|
| E2-T4: Online Payment (Tabby/Tamara) | 2 | Large | Requires payment provider agreements. |
| E3-T2: Portable Health Record -- Import | 3 | Large | Demand side of network. Needs export first. |
| E3-T3: Microchip-Based Lookup | 3 | Medium | "Magic moment" but needs supply side first. |
| E3-T4: Public API Documentation | 3 | Medium | Developer ecosystem. |
| E3-T6: Developer Sandbox | 3 | Medium | Developer ecosystem. |
| E5-T2: Owner-to-Owner Referral | 5 | Medium | Needs owner accounts to be mature. |
| E5-T4: IDEXX Lab Integration | 5 | Large | Business dependency on IDEXX agreement. |
| E5-T5: MOCCAE Partnership | 5 | Unknown | Business exploration first. |
| E5-T6: I-CAD Integration | 5 | Medium | France market, secondary priority. |

---

## Dependency Map

```
E1-T1 (B2C Landing) ──────────────────────────────────────────────┐
E1-T2 (Clinic Directory) ─────────┐                               │
E1-T3 ("Ask Your Vet") ───────────┤ depends on E1-T2              │
E1-T4 (Owner Onboarding) ─────────┤                               │
E1-T5 (Owner UX Redesign) ────────┘                               │
E1-T6 (PWA) ──────────────────────── depends on E1-T5             │
                                                                    │
E2-T1 (Medical Record Viewer) ──── depends on E1-T4               │
E2-T2 (Vaccination Reminders) ──── depends on E2-T1               │
E2-T3 (Record Sharing QR/Link) ─── depends on E2-T1               │
E2-T4 (Online Payment) ─────────── independent (business dep.)     │
E2-T5 (Notification Preferences) ─ depends on E1-T6 (for push)    │
                                                                    │
E3-T1 (Export FHIR) ──────────────── depends on Microchip (F2)    │
E3-T2 (Import FHIR) ──────────────── depends on E3-T1             │
E3-T3 (Microchip Lookup) ─────────── depends on E3-T1 + E3-T5     │
E3-T4 (API Docs) ─────────────────── depends on E3-T1 + E3-T2     │
E3-T5 (Owner Consent) ────────────── depends on E2-T1              │
E3-T6 (Sandbox) ──────────────────── depends on E3-T4              │
                                                                    │
E4-T1 to E4-T6 ───────────────────── no technical dependencies ────┘
                                       (marketing/content tasks)

E5-T1 (Blog) ─────────────────────── no dependencies
E5-T2 (Owner Referral) ───────────── depends on E1-T4
E5-T3 (Vet Referral) ─────────────── depends on subscription system
E5-T4 (IDEXX) ────────────────────── depends on IDEXX agreement
E5-T5 (MOCCAE) ───────────────────── business exploration first
E5-T6 (I-CAD) ────────────────────── depends on I-CAD agreement
```

### Critical prerequisite (not in this backlog but blocking)

**BREEDERS-FEATURES-SPEC Phase 1 (F1: Sex, F2: Microchip, F3: Species)** must be built before:
- E3-T1 (Export needs microchip as identifier)
- E3-T3 (Microchip lookup)
- E5-T5 / E5-T6 (MOCCAE / I-CAD need microchip)

Recommend prioritizing BREEDERS Phase 1 immediately -- it is a small task (single migration) that unblocks multiple strategic features.

---

## What This Backlog Does NOT Cover (Explicitly Deferred)

| Topic | Why Deferred |
|-------|-------------|
| Native mobile app (iOS/Android) | PWA is sufficient for MVP. Native app is a P3 investment. |
| Multi-currency GCC (SAR, QAR) | UAE first. GCC expansion is a 2027 topic. |
| Equine module | Specified in breeders spec but secondary to companion animal focus. |
| Breeding AI (mating prediction, COI) | Breeding module (Phase 2) must ship first. AI is Phase 3+. |
| Voice AI scribe (ambient listening) | Covetrus has this. High effort, requires dedicated R&D. P3. |
| White-label / reseller model | Premature before product-market fit. |
| Insurance eClaims integration | Depends on insurance partnerships. Business exploration first. |

---

*This backlog is a PO recommendation. Effort estimates, timelines, and prioritization require founder validation before task creation. No technical decisions have been made -- architecture and implementation approach will be determined by the engineering team.*
