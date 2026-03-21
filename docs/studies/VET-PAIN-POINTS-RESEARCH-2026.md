# Veterinary Software Pain Points Research 2026

> Research date: 2026-03-21
> Sources: Reddit, Quora, G2, Capterra, Software Advice, industry blogs, veterinary publications
> Purpose: Identify real pain points to drive Vetolib product roadmap and sales messaging

---

## Table of Contents

1. [User Interface & Usability](#1-user-interface--usability)
2. [Speed & Performance](#2-speed--performance)
3. [Documentation Burden (SOAP Notes)](#3-documentation-burden-soap-notes)
4. [Billing & Invoicing](#4-billing--invoicing)
5. [Inventory & Stock Management](#5-inventory--stock-management)
6. [Appointment Scheduling & No-Shows](#6-appointment-scheduling--no-shows)
7. [Integration & Interoperability](#7-integration--interoperability)
8. [Data Migration & Vendor Lock-In](#8-data-migration--vendor-lock-in)
9. [Client Communication](#9-client-communication)
10. [Multi-Location / Multi-Clinic](#10-multi-location--multi-clinic)
11. [Reporting & Analytics](#11-reporting--analytics)
12. [Support & Training](#12-support--training)
13. [Pricing & Hidden Costs](#13-pricing--hidden-costs)
14. [Telemedicine / Telehealth](#14-telemedicine--telehealth)
15. [Mobile Access](#15-mobile-access)
16. [AI & Automation](#16-ai--automation)
17. [Feature Requests ("I Wish My Vet Software Could...")](#17-feature-requests)
18. [Competitor-Specific Complaints](#18-competitor-specific-complaints)
19. [Vetolib Gap Analysis Summary](#19-vetolib-gap-analysis-summary)
20. [Sales Angle Recommendations](#20-sales-angle-recommendations)

---

## 1. User Interface & Usability

### The Problem
The #1 complaint across all forums, review sites, and Reddit threads. Veterinary software UIs are described as "clunky," "click-heavy," "last-century," and "making work harder instead of easier."

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Too many clicks to complete basic tasks (entering prescriptions, processing payments, navigating SOAP records) | G2/Capterra reviews for Cornerstone, Shepherd, NaVetor, Covetrus Pulse | **Strong consensus** -- mentioned across ALL major PIMS |
| Confusing terminology and navigation that makes learning the system "a nightmare" | ezyVet G2 reviews, Software Advice reviews | Multiple reviewers (5+) |
| Having to toggle between multiple tabs/screens to find patient records | VetSoftwareHub analysis, Shepherd reviews | **Strong consensus** |
| Unintuitive workflows that don't match how vets actually work | Cornerstone, Avimark, ezyVet reviews | **Strong consensus** |
| Interface looks "last-century" -- functional but visually outdated | Avimark reviews (Capterra/G2), NectarVet analysis | Multiple reviewers |
| Staff lose patience with systems that "make their work harder instead of easier" | VetSoftwareHub editorial, iLoveVeterinary analysis | Industry-wide observation |

### Quantified Impact
- A mid-sized practice handling 40-50 appointments daily loses an average of **15-20 minutes per day** simply searching for information across disconnected screens (VetSoftwareHub).

### Does Vetolib Solve This?
**Partially.** Vetolib uses shadcn/ui + Tailwind for a modern, clean interface. The agenda redesign (calendar view) is in progress. However:
- **GAP**: Need to audit click counts for common workflows (create appointment, enter SOAP, process payment) and ensure Vetolib beats competitors.
- **GAP**: Need user testing with actual vet staff to validate workflow intuitiveness.

### Recommendation
- Conduct a "click count audit" comparing Vetolib vs. Cornerstone/ezyVet for the 10 most common tasks.
- Market Vetolib as "designed for speed" -- every common task in 3 clicks or fewer.
- Highlight modern UI in demos as a differentiator against legacy systems.

---

## 2. Speed & Performance

### The Problem
Server-based legacy systems crash frequently. Cloud-based systems have latency issues. Both waste clinical time.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| System crashes during peak hours | Cornerstone reviews (G2, Capterra) | Multiple reviewers |
| Slow load times within clinical workflows and with diagnostics integrations | Covetrus Pulse reviews (Software Advice) | Multiple reviewers |
| Server replacements costing ~$17,000 on average | ezyVet blog, industry analysis | Industry data point |
| "Slow or difficult-to-navigate systems force team members to click through time-consuming menus instead of providing patient care" | IDEXX editorial | Industry-wide |
| Avimark "often has glitches and error codes" | VetPort comparison, G2 reviews | Multiple reviewers |

### Does Vetolib Solve This?
**Yes, architecturally.** Vetolib is cloud-native (Aspire + Next.js), eliminating server crashes and $17K replacement costs. However:
- **GAP**: Need performance benchmarks (page load < 1s, API response < 200ms) to make concrete speed claims.
- **GAP**: Need offline/degraded-mode capability for clinics with unstable internet (relevant in some UAE areas).

### Recommendation
- Add performance SLAs to sales materials: "99.9% uptime, <1s page loads."
- Build a "speed comparison" demo showing Vetolib vs. legacy system for the same workflow.

---

## 3. Documentation Burden (SOAP Notes)

### The Problem
The "documentation tax" is a leading cause of veterinary burnout. Vets spend hours after clinic closes finishing SOAP notes -- the "pajama time" phenomenon.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Vets spend 2+ hours daily on documentation after clinic hours | VetSoftwareHub AI Scribe Guide, HappyDoc, Scribenote | **Strong consensus** -- industry-wide |
| SOAP notes are tedious, repetitive, and distract from patient care | Multiple AI scribe vendors (VetRec, CoVet, ScribbleVet, VetGeni, VetSkribe) | **Very strong consensus** |
| Manual SOAP entry requires opening different tabs for each section | Shepherd reviews | Multiple reviewers |
| Documentation burden is the primary cause of administrative burnout | VetSoftwareHub editorial | Industry data |

### Quantified Impact
- AI scribes save **10 minutes per SOAP note** and **10 minutes per discharge** (VetGeni, peer-reviewed).
- AI scribes save up to **2 hours of documentation daily** per veterinarian.
- The AI vet scribe market is exploding: Scribenote, VetRec, CoVet, HappyDoc, ScribbleVet, VetGeni, VetSkribe -- all launched 2024-2026.

### Does Vetolib Solve This?
**Partially.** Vetolib has an AI module (Phase 2) with triage capabilities, but:
- **GAP**: No AI scribe / voice-to-SOAP feature currently planned.
- **GAP**: No AI-assisted note completion or templates.
- This is a **massive opportunity** -- standalone AI scribes cost $99-299/month per vet. An integrated AI scribe in Vetolib would be a killer feature.

### Recommendation
- **HIGH PRIORITY**: Add AI scribe (voice-to-SOAP) to the Vetolib.AI module roadmap.
- Integrate with existing AI scribe APIs (Scribenote, VetRec) as a short-term bridge.
- Position Vetolib as "the only PIMS with built-in AI scribe" -- saves clinics $100-300/month per vet.

---

## 4. Billing & Invoicing

### The Problem
Manual billing causes missed charges, revenue leakage, and slow checkout processes.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Practices lose up to **$60,000/year per full-time doctor** due to missed charges | IDEXX blog, industry studies | Industry data |
| Manual invoice building is time-consuming and error-prone | IDEXX, Digitail, Shepherd analyses | **Strong consensus** |
| Finance modules require toggling between tabs | Shepherd reviews (NectarVet) | Multiple reviewers |
| Payment processing involves too many steps/clicks | NaVetor reviews | Multiple reviewers |
| Outdated payment systems "frustrate clients, slow down teams, and hurt revenue" | PayJunction, Weave analyses | Industry observation |
| Reminder systems for payments can be difficult to use | DaySmart Vet reviews | Multiple reviewers |

### Quantified Impact
- **$60,000/year per FTE doctor** in missed charges (IDEXX).
- Hundreds or thousands of invoices monthly, each with potential for manual error.

### Does Vetolib Solve This?
**Partially.** Vetolib has a Billing module, but:
- **GAP**: Verify that charge capture is automated (services performed auto-populate invoice).
- **GAP**: Verify one-click checkout flow exists.
- **GAP**: Need integration with UAE payment processors (Network International, Tabby, Tamara for BNPL).

### Recommendation
- Ensure automatic charge capture: every service/medication administered auto-adds to invoice.
- Sales pitch: "Never miss a charge again -- Vetolib captures every service in real time."
- See also: BNPL-INTEGRATION-STUDY-2026.md for payment financing options.

---

## 5. Inventory & Stock Management

### The Problem
Veterinary clinics manage 500-2,000+ SKUs with complex tracking requirements (expiration dates, multiple dispensing pathways, supplier management). Most PIMS inventory modules are inadequate.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Inventory and prescription management issues | Digitail reviews (NectarVet) | Multiple reviewers |
| Overstocking leads to expiration waste; understocking means inability to treat | IDEXX, Shepherd, Inventory Ally blogs | **Strong consensus** |
| 25-35% of inventory-related expenses go to management overhead | Industry analysis | Industry data |
| Daily stock ordering, margin leakage, excess stock, wasted staff time | Digitail blog | Industry analysis |
| No real-time stock visibility across locations | VetPort, multi-location analyses | Multiple sources |

### Quantified Impact
- Inventory inefficiencies cost up to **$84,000 per vet per year** (Digitail).
- 500-800 SKUs for small clinics, 2,000+ for larger practices.
- 25-35% of inventory costs are pure management overhead.

### Does Vetolib Solve This?
**Partially.** Vetolib has a Stock module (implementation in progress). Key requirements:
- **GAP**: Verify expiration date tracking and alerts are implemented.
- **GAP**: Verify automated reorder points / low-stock alerts.
- **GAP**: Verify supplier integration capabilities.
- **GAP**: Verify stock consumed during procedure auto-deducts from inventory.

### Recommendation
- Ensure Stock module has: real-time tracking, expiration alerts, auto-reorder, supplier management, auto-deduction on dispensing.
- Sales pitch: "Stop losing $84K/year to inventory chaos."

---

## 6. Appointment Scheduling & No-Shows

### The Problem
No-shows cost clinics $50K/year. Manual scheduling causes double bookings and gaps. Front desk is overwhelmed with phone calls.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Average no-show rate of 10-11%, some practices higher | VetSoftwareHub, industry data | Industry data |
| Up to **$50,000/year** lost to cancellations and no-shows | VetSoftwareHub, AmeriVet | Industry data |
| Front desk overwhelmed with scheduling phone calls | Vetstoria, multiple PIMS vendors | **Strong consensus** |
| Double bookings and schedule gaps from manual booking | AmeriVet, VetPort analyses | **Strong consensus** |
| Waitlists are "underused in veterinary medicine" despite solving cancellation fill + client frustration | VetSoftwareHub editorial | Expert opinion |
| No deposits at booking to deter no-shows | Multiple scheduling analyses | Common gap |

### Does Vetolib Solve This?
**Yes, largely.** Vetolib has:
- Agenda module with scheduling
- AI Phase 1: scheduling optimization
- AI Phase 3: no-show prediction (ML.NET)
- Online booking portal
- Notifications module for reminders

**Remaining GAPs:**
- **GAP**: Verify waitlist management is implemented.
- **GAP**: Verify deposit collection at booking is supported.
- **GAP**: Verify SMS/WhatsApp reminders for UAE market (not just email).

### Recommendation
- The no-show prediction feature is a **strong differentiator** -- very few competitors have this.
- Sales pitch: "Vetolib predicts no-shows before they happen and auto-fills your schedule."
- Ensure booking deposits are supported for high-value appointments (surgery, dental).

---

## 7. Integration & Interoperability

### The Problem
Vet practices use a "constellation of systems" (labs, imaging, payment processors, communication tools). When PIMS doesn't integrate, staff do manual data entry across systems.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| "If new software does not integrate smoothly with existing tools, the clinic experiences friction" | VetSoftwareHub, iLoveVeterinary | **Very strong consensus** |
| Lab results require manual entry when integration is missing | IDEXX, Digitail, multiple vendors | **Strong consensus** |
| Diagnostic imaging systems disconnected from PIMS | AAHA, IDEXX comparisons | **Strong consensus** |
| Payment processors not integrated -- manual reconciliation | PayJunction, Weave analyses | Multiple sources |
| "Bad vendors blame the other company. Good vendors own the problem." | VetSoftwareHub editorial | Expert observation |

### Does Vetolib Solve This?
**Partially.** Vetolib is modular (Ardalis pattern) which makes integration architecturally clean, but:
- **GAP**: No lab integration (IDEXX, Heska/Mars, Abaxis) currently implemented.
- **GAP**: No imaging integration (DICOM, SignalPET).
- **GAP**: No payment processor integration for UAE (Network International).
- **GAP**: Need an API/webhook system for third-party integrations.

### Recommendation
- **HIGH PRIORITY**: Build IDEXX VetLab integration -- IDEXX is the #1 lab in veterinary.
- Plan a public API (already in post-MVP TODO: api-public-001).
- For UAE launch: integrate with Network International for payment processing.
- Long-term: marketplace for integrations (like Shopify apps).

---

## 8. Data Migration & Vendor Lock-In

### The Problem
Switching PIMS is terrifying for practices because of data hostage-taking, high migration costs, and fear of data loss.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Vendors charge $500-$3,000+ for data export/migration | VetSyCare pricing guide, Digitail blog | Industry data |
| Data exports in unusable formats (PDFs of individual records) | VetSoftwareHub contract guide | Expert analysis |
| "The lock-in was never about features -- it was about data hostage-taking" | VetSoftwareHub editorial | Expert observation |
| Medical record transfer is the #1 fear when switching | IDEXX blog, Digitail blog | **Strong consensus** |
| Migration timelines stretching to months | SoftwareSeni analysis, ezyVet blog | Industry observation |
| Practices accumulate "small frustrations until one day they add up to certainty" to switch | Digitail blog | Expert observation |

### Does Vetolib Solve This?
**Partially.** Being a new entrant, Vetolib needs to be the EASY system to switch TO:
- **GAP**: Need a migration tool / import wizard for common PIMS exports (Cornerstone, Avimark, ezyVet).
- **GAP**: Need to guarantee data portability -- full export in standard formats at any time, at no extra cost.
- **OPPORTUNITY**: "No lock-in guarantee" as a sales differentiator.

### Recommendation
- Build import wizards for top 3 competitor data formats.
- Offer free data migration as part of onboarding.
- Publish a "Data Freedom Guarantee" -- full export, standard formats, zero cost, anytime.
- This directly attacks the #1 switching barrier.

---

## 9. Client Communication

### The Problem
Pet owners expect modern communication (texting, portals, app notifications). Many PIMS still rely on phone calls and mailed postcards.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Reminder systems "can be difficult to use" | DaySmart Vet reviews | Multiple reviewers |
| No two-way texting with clients | Multiple PIMS comparisons | Common gap in older systems |
| Communication delays between front desk, techs, and vets | VetPort workflow analysis, AmeriVet | **Strong consensus** |
| No client-facing portal for records, appointments, prescriptions | Multiple vendor comparisons | Varies by vendor |
| Pet owners want WhatsApp/SMS, not just email | UAE market context, Emitrr analysis | **Strong consensus** for UAE/MENA |

### Does Vetolib Solve This?
**Partially.** Vetolib has:
- Messaging module (Phase 4, separate module)
- Notifications module
- Booking portal for clients

**GAPs:**
- **GAP**: Verify two-way SMS/WhatsApp messaging is planned.
- **GAP**: Verify client portal includes: medical records view, vaccination certificates, upcoming appointments, prescription refill requests.
- **GAP**: For UAE, WhatsApp Business API integration is essential.

### Recommendation
- WhatsApp Business API integration is non-negotiable for UAE market.
- Client portal should show: visit history, vaccination status, upcoming appointments, invoices, prescription refills.
- Sales pitch: "Your clients communicate in 2026. Your software should too."

---

## 10. Multi-Location / Multi-Clinic

### The Problem
Corporate consolidation is accelerating (32% increase in vet clinics in the past decade, many acquired by groups). Multi-location management is painful.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| No unified view of performance across locations | Digitail, VetPort multi-location analyses | **Strong consensus** |
| Patient records not accessible across locations | VetPort, Veterian analyses | **Strong consensus** |
| Time zone management across locations is "tedious" | VetPort analysis | Specific pain point |
| Financial reporting across locations requires manual consolidation | PayJunction, WooVet analyses | Multiple sources |
| User/permission management across locations is complex | Veterian, VIA analyses | Multiple sources |
| Different workflow needs per location not supported | WooVet best practices | Expert observation |

### Does Vetolib Solve This?
**Yes, architecturally.** Vetolib is multi-tenant by design (ClinicId global query filter). However:
- **GAP**: Multi-clinic management (one owner, multiple clinics) is in post-MVP TODO (multi-clinic-001).
- **GAP**: Need consolidated reporting across clinics.
- **GAP**: Need role-based access that spans clinics (regional manager role).

### Recommendation
- Prioritize multi-clinic-001 for corporate/group practices -- this is a growing segment.
- Build a "group dashboard" showing KPIs across all clinics.
- Time zone handling is already considered (UAE: Asia/Dubai; France later).

---

## 11. Reporting & Analytics

### The Problem
Practice owners need business intelligence but most PIMS offer basic or inflexible reporting.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| "Lacks substantial reporting capabilities" | DaySmart Vet reviews | Multiple reviewers |
| Reports are basic, not customizable | Multiple vendor comparisons | **Strong consensus** |
| No real-time dashboards for practice performance | Industry analyses | Common gap |
| Financial reporting requires manual export to Excel | Multiple practice manager complaints | **Strong consensus** |
| Can't easily answer: "How is my practice doing this month vs. last year?" | AmeriVet, VetPort analyses | Common frustration |

### Does Vetolib Solve This?
**Partially.** See ANALYTICS-STUDY.md for detailed analysis. Key gaps:
- **GAP**: Need real-time dashboard with KPIs (revenue, appointments, no-show rate, average transaction value).
- **GAP**: Need comparison reports (this month vs. last month, this year vs. last year).
- **GAP**: Need exportable reports (PDF, CSV, Excel).

### Recommendation
- Build a practice owner dashboard as a priority feature.
- Include: daily revenue, appointment fill rate, no-show rate, top services, inventory alerts.
- Sales pitch: "Know exactly how your practice is performing -- in real time."

---

## 12. Support & Training

### The Problem
Vet teams are not tech-savvy. When support is slow, inadequate, or doesn't understand veterinary workflows, frustration compounds.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Support staff don't understand vet terminology (CSRs, SOAP notes, etc.) | VetSoftwareHub editorial | Expert observation |
| Support "takes too long to respond" | Digitail reviews | Multiple reviewers |
| Implementation and data migration "can be time-consuming" | Provet Cloud reviews | Multiple reviewers |
| "Support teams are responsive but not always helpful" | Provet Cloud reviews | Multiple reviewers |
| Lack of hands-on training during onboarding | iLoveVeterinary analysis | Industry observation |
| Integration support -- "bad vendors blame the other company" | VetSoftwareHub editorial | Expert observation |

### Does Vetolib Solve This?
**Not yet -- operational concern, not software.** This is about building a support team. However:
- **OPPORTUNITY**: In-app help, contextual tooltips, video tutorials can reduce support load.
- **OPPORTUNITY**: See HELP-CENTER-CONTENT-2026.md for help center content plan.

### Recommendation
- Build comprehensive in-app help (tooltips, walkthroughs, video tutorials).
- Hire support staff with veterinary background (vet techs make excellent support agents).
- Offer white-glove onboarding with free data migration.

---

## 13. Pricing & Hidden Costs

### The Problem
Veterinary software pricing is opaque. Critical features are gated behind higher tiers. Total cost of ownership surprises practices.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| "Critical features only available at higher price tiers" | VetSoftwareHub analysis | **Strong consensus** |
| Workflows shown in demos were "curated, not representative of everyday usage" | VetSoftwareHub editorial | Expert observation |
| "Promised capabilities still in development or unreliable at scale" | VetSoftwareHub editorial | Multiple reports |
| Setup fees ($500-2,000+), migration fees ($500-3,000+) on top of monthly subscription | VetSyCare pricing guide | Industry data |
| Per-user pricing punishes growing practices | Multiple pricing analyses | Common complaint |

### Does Vetolib Solve This?
**Pricing strategy not yet finalized.** Recommendations:
- **OPPORTUNITY**: Transparent, all-inclusive pricing. No feature gating.
- **OPPORTUNITY**: Free migration. No setup fees.
- **OPPORTUNITY**: Per-clinic pricing (not per-user) to avoid punishing growth.

### Recommendation
- Price per clinic, not per user. Include all features.
- Offer a generous free trial (30 days, full features).
- Sales pitch: "One price. All features. No surprises."

---

## 14. Telemedicine / Telehealth

### The Problem
Post-COVID, pet owners expect telehealth options. The market is growing at 20.3% CAGR, reaching $921M by 2030. Most PIMS don't have integrated telehealth.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| Telemedicine is a separate system, not integrated with PIMS | Multiple vendor analyses | **Strong consensus** |
| Video consultation details not auto-recorded in patient chart | Acropolium case study, industry analyses | Common gap |
| Regulatory uncertainty (only 8 US states allow E-VCPR via telemedicine) | EliteLearning analysis | Regulatory reality |
| No integrated triage before booking | Industry analyses | Common gap |

### Does Vetolib Solve This?
**Partially.** See TELEMEDICINE-AND-LAB-STUDY-2026.md. The AI Triage feature (Phase 2) addresses pre-visit triage but:
- **GAP**: No video consultation feature currently planned.
- **GAP**: No telehealth integration.
- **NOTE**: UAE regulations on veterinary telehealth need research.

### Recommendation
- Short-term: integrate with a telehealth provider (Zoom for Healthcare, or build lightweight video).
- Long-term: built-in video consultation with auto-SOAP generation.
- Research UAE veterinary telehealth regulations before building.

---

## 15. Mobile Access

### The Problem
Vets move between exam rooms, surgery, and the field. Desktop-only PIMS forces them back to a workstation.

### Specific Complaints

| Complaint | Sources | Consensus Level |
|---|---|---|
| "Some customers wish systems had a mobile application" | Multiple PIMS reviews (Capterra, G2) | Multiple reviewers |
| Large animal / farm vets need field access | Industry context | Segment-specific need |
| Staff want to check schedule, patient info from personal devices | Multiple analyses | **Strong consensus** |
| Cloud systems help but responsive design is not the same as a native app | Industry observation | Common gap |

### Does Vetolib Solve This?
**Partially.** Next.js is responsive, so Vetolib works on mobile browsers. However:
- **GAP**: No native mobile app (iOS/Android).
- **GAP**: No offline mode for field use.

### Recommendation
- Short-term: ensure all Vetolib screens are fully responsive and mobile-optimized.
- Medium-term: PWA (Progressive Web App) with offline capability.
- Long-term: native mobile app for iOS and Android.

---

## 16. AI & Automation

### The Problem
AI is the hottest trend in vet software (2025-2026). Practices want automation for repetitive tasks. The market is flooded with AI scribe startups.

### Key AI Features Vets Want

| Feature | Market Demand | Competition |
|---|---|---|
| AI SOAP note generation (voice-to-text) | **Very high** | Scribenote, VetRec, CoVet, HappyDoc, ScribbleVet, VetGeni, VetSkribe |
| AI-powered scheduling optimization | High | Few competitors have this |
| No-show prediction | High | Almost no competitors have this |
| AI triage (symptom assessment) | High | CoVet, emerging |
| Automated reminders (smart timing) | Medium | Most modern PIMS have basic reminders |
| AI-powered inventory forecasting | Medium | Emerging |
| AI diagnostic assistance (image analysis) | Growing | SignalPET |

### Does Vetolib Solve This?
**Partially.** Vetolib AI roadmap:
- Phase 1: Scheduling optimization (algorithmic) -- IN PROGRESS
- Phase 2: AI Triage (LLM) -- SCAFFOLDED
- Phase 3: No-show prediction (ML.NET) -- PLANNED
- Phase 4: AI Messaging -- PLANNED

**GAPs:**
- **GAP**: No AI scribe (voice-to-SOAP) -- this is the #1 AI feature vets want.
- **GAP**: No AI diagnostic assistance.
- **GAP**: No AI inventory forecasting.

### Recommendation
- **CRITICAL**: Add AI scribe to Phase 2 or create Phase 2b. This is the single most requested AI feature.
- No-show prediction (Phase 3) is a genuine differentiator -- accelerate.
- AI triage is excellent for the UAE market (multilingual: EN/AR).

---

## 17. Feature Requests

### "I Wish My Vet Software Could..."

Compiled from Reddit, Quora, G2 reviews, Capterra reviews, and industry forums.

| Feature Request | Frequency | Vetolib Status |
|---|---|---|
| "One system for everything -- stop switching between 5 apps" | **Very common** | Vetolib is all-in-one by design |
| "Auto-generate SOAP notes from my voice" | **Very common** | GAP -- not planned |
| "Show me a real-time dashboard of how my practice is doing" | **Very common** | GAP -- needs dashboard |
| "Let clients book online without calling" | Common | Online booking portal exists |
| "Send automated text/WhatsApp reminders" | Common | Notifications module exists (verify WhatsApp) |
| "Predict which clients will no-show" | Emerging | Phase 3 planned |
| "Auto-deduct inventory when I dispense medication" | Common | Stock module in progress |
| "Generate treatment estimates clients can approve on their phone" | Common | GAP |
| "Built-in payment plans / BNPL" | Growing | See BNPL study |
| "Access patient records from any device, anywhere" | Common | Cloud-native, responsive |
| "Integrate with my lab equipment automatically" | **Very common** | GAP -- no lab integration |
| "Let me customize templates for common visit types" | Common | GAP -- needs template system |
| "Generate end-of-day reports automatically" | Common | GAP -- needs reporting |
| "Manage multiple clinics from one login" | Growing | Post-MVP (multi-clinic-001) |
| "Export my data easily if I want to switch" | Common | GAP -- need export tools |
| "Video call with clients for follow-ups" | Growing | GAP -- no telehealth |
| "Automatic vaccination reminders when they're due" | Common | Needs verification |
| "AI that helps me not forget anything during an exam" | Emerging | AI Triage partially covers this |
| "Waitlist management to fill cancellations" | Common | GAP -- not confirmed |
| "Client-facing app where owners can see their pet's records" | Growing | Booking portal exists; expand |

---

## 18. Competitor-Specific Complaints

### Cornerstone (IDEXX)
- **Market share**: 20% of server-based market
- **Complaints**: System crashes, click-heavy interface, difficult to learn, not user-friendly, limited functions
- **Strength to attack**: Legacy server-based = expensive hardware, no cloud flexibility
- **Vetolib angle**: "Cloud-native, zero server costs, modern UI"

### Avimark (Covetrus)
- **Market share**: 51% of server-based market
- **Complaints**: Glitches and error codes, "last-century" interface, limited for larger practices
- **Strength to attack**: Dominant but dated -- users are "collecting small frustrations"
- **Vetolib angle**: "The modern alternative for practices tired of Avimark"

### ezyVet (IDEXX)
- **Complaints**: "Worst system ever" (some users), "confusing, complicated," "extremely non-user-friendly," "learning the system a nightmare"
- **Counter**: Some users love it ("greatly improved hospital efficiency")
- **Strength to attack**: Steep learning curve, mixed reviews
- **Vetolib angle**: "Powerful without the complexity -- productive on day one"

### Covetrus Pulse
- **Complaints**: Navigation awkward, saving medical records difficult, too many clicks, slow load times
- **Vetolib angle**: "Fast, intuitive, designed by vets for vets"

### Shepherd
- **Complaints**: Click-heavy prescription entry, bugs in interface and finance modules
- **Vetolib angle**: "Bug-free, streamlined prescriptions"

### NaVetor
- **Complaints**: Click-heavy, too many steps for payments, UI geared for vets not techs
- **Vetolib angle**: "Designed for the whole team -- vets, techs, and front desk"

### DaySmart Vet
- **Complaints**: Difficult reminder system, lacks reporting
- **Vetolib angle**: "Smart reminders + real-time analytics built in"

### Digitail
- **Complaints**: Inventory/prescription management issues, slow support response
- **Vetolib angle**: "Reliable inventory + fast support"

---

## 19. Vetolib Gap Analysis Summary

### Already Solved (Competitive Advantages)

| Pain Point | Vetolib Solution | Differentiator Level |
|---|---|---|
| Modern UI / UX | shadcn/ui + Tailwind, calendar view redesign | Strong |
| Cloud-native / no server costs | .NET Aspire + Next.js, zero hardware | Strong |
| Multi-tenant architecture | ClinicId global query filter, built for groups | Strong |
| Online booking | Booking portal module | Table stakes |
| Automated reminders | Notifications module | Table stakes |
| No-show prediction | AI Phase 3 (ML.NET) | **Unique differentiator** |
| AI triage | AI Phase 2 (LLM) | Strong differentiator |
| Scheduling optimization | AI Phase 1 | Strong differentiator |
| All-in-one platform | Modular monolith, single login | Strong |

### Gaps to Close (Priority Order)

| Priority | Gap | Business Impact | Effort |
|---|---|---|---|
| **P0** | AI Scribe (voice-to-SOAP) | #1 feature request, reduces burnout, saves $100-300/vet/month | High |
| **P0** | Lab integration (IDEXX VetLab) | Blocking for serious practices, table stakes | High |
| **P1** | Practice owner dashboard / analytics | Revenue visibility, KPI tracking | Medium |
| **P1** | WhatsApp Business API (UAE) | Non-negotiable for UAE market | Medium |
| **P1** | Data import wizard (competitor migration) | Removes #1 switching barrier | Medium |
| **P1** | Auto charge capture (billing) | Prevents $60K/year revenue leakage | Medium |
| **P2** | Multi-clinic management | Growing corporate segment | Medium |
| **P2** | Treatment estimate generation (client-facing) | Improves client approval rates | Low |
| **P2** | Waitlist management | Fills cancellations, reduces no-show impact | Low |
| **P2** | Data export / portability guarantee | Trust builder, anti-lock-in | Low |
| **P3** | Video telehealth | Growing demand, regulatory dependent | High |
| **P3** | Native mobile app | Nice-to-have, PWA first | High |
| **P3** | Offline mode | Field vets, unstable internet | High |
| **P3** | AI diagnostic imaging | Emerging, specialized | Very High |

---

## 20. Sales Angle Recommendations

### Primary Messaging (Based on Pain Point Frequency)

**For practices on Avimark/Cornerstone (legacy server-based):**
> "Stop paying $17,000 for server replacements. Stop losing hours to crashes. Vetolib is cloud-native -- modern, fast, and accessible from anywhere. We'll migrate your data for free."

**For practices frustrated with click-heavy UIs:**
> "Every common task in 3 clicks or fewer. Vetolib was designed for speed -- not by engineers in a vacuum, but by watching vets work."

**For practices losing money to no-shows:**
> "Your practice loses $50,000/year to no-shows. Vetolib's AI predicts which clients will miss their appointment and auto-fills your schedule from the waitlist."

**For practices drowning in documentation:**
> "Two fewer hours of paperwork every day. Vetolib's AI scribe turns your voice into structured SOAP notes in minutes." *(when AI scribe is built)*

**For practices worried about vendor lock-in:**
> "Your data is yours. Export everything, anytime, in standard formats, at zero cost. We earn your business every month -- we don't trap you."

**For multi-location groups:**
> "One platform, all your clinics. Real-time performance across every location. Vetolib was built multi-tenant from day one."

### Competitor Displacement Playbook

| Current PIMS | Attack Vector | Vetolib Advantage |
|---|---|---|
| Avimark | Dated UI, glitches, server dependency | Modern cloud, zero hardware, clean UI |
| Cornerstone | Crashes, click-heavy, expensive | Stability, speed, transparent pricing |
| ezyVet | Complexity, steep learning curve | Intuitive from day one, same power |
| Covetrus Pulse | Slow loads, navigation issues | Fast cloud architecture, clean design |
| Shepherd | Click-heavy Rx, buggy finance | Streamlined workflows, reliable |
| Paper/Excel | Everything | Digital transformation partner |

---

## Appendix: Key Sources

### Reddit & Community
- r/veterinary, r/VetTech, r/veterinaryprofession -- recurring themes on software frustration
- Quora veterinary software threads

### Review Platforms
- [G2 ezyVet Reviews](https://www.g2.com/products/ezyvet/reviews)
- [Capterra Veterinary Software](https://www.capterra.com/veterinary-software/)
- [Software Advice Veterinary](https://www.softwareadvice.com/veterinary/)

### Industry Analysis
- [VetSoftwareHub - Why Teams Regret Their Software Choices](https://www.vetsoftwarehub.com/article/why-so-many-veterinary-teams-regret-their-software-choices)
- [VetSoftwareHub - Veterinary Software Contracts](https://www.vetsoftwarehub.com/article/veterinary-software-contracts-review-checklist)
- [VetSoftwareHub - Scheduling Software](https://www.vetsoftwarehub.com/article/veterinary-scheduling-software-reduce-no-shows-phone-calls)
- [VetSoftwareHub - AI Scribe Buyer's Guide](https://www.vetsoftwarehub.com/article/veterinary-ai-scribe-buyers-guide)
- [iLoveVeterinary - 9 Things Practices Get Wrong](https://iloveveterinary.com/blog/9-things-vet-practices-often-get-wrong-when-picking-software/)
- [IDEXX - Top Veterinary Software Solutions](https://software.idexx.com/top-veterinary-software-solutions-a-2025-comparison-guide)
- [IDEXX - Why Switching Software is Scary](https://software.idexx.com/neo/resources/blog/why-switching-veterinary-software-is-so-scary-and-why-it-shouldnt-be)
- [IDEXX - Automated Billing](https://software.idexx.com/resources/blog/7-ways-automated-veterinary-billing-can-save-your-practice-thousands)
- [Digitail - How to Switch PIMS](https://digitail.com/blog/how-to-switch-veterinary-practice-management-software-without-losing-your-mind-or-your-clients/)
- [Digitail - Inventory Management](https://digitail.com/blog/you-cant-manage-what-you-cant-track-a-better-way-to-handle-inventory/)
- [Provet - Hidden Costs of Outdated Software](https://www.provet.com/blog/real-cost-of-not-switching-veterinary-practice-management-software)
- [Shepherd - AI-Powered PIMS Comparison 2026](https://www.shepherd.vet/blog/8-best-ai-powered-veterinary-practice-management-software-platforms-2026-comparison-guide/)
- [NectarVet - Cornerstone Review](https://www.nectarvet.com/post/cornerstone-vet-software-pricing-reviews)
- [NectarVet - Cloud-Based Software Reviews](https://www.nectarvet.com/post/best-cloud-based-vet-software-prices-reviews)
- [ezyVet - Outdated Technology](https://www.ezyvet.com/blog/outdated-technology-is-slowing-your-clinic-down---heres-how-to-fix-it)
- [ezyVet - Features to Look For](https://www.ezyvet.com/blog/features-to-look-for-when-buying-veterinary-practice-management-software)
- [VetPort - Managing Clinic Chains](https://www.vetport.com/managing-veterinary-clinic-chains-with-pms)
- [VetPort - Challenges Faced by Practices](https://www.vetport.com/challenges-faced-by-veterinary-practices)
- [Asteris - 7 Challenges of Vet Practice Management](https://www.asteris.com/challenges/)
- [Vetstoria - 5 Challenges](https://www.vetstoria.com/blog/vetstoria-solves-these-common-veterinary-practice-issues/)
- [AmeriVet - Appointment Scheduling Headache](https://amerivet.com/blog/veterinary-appointment-scheduling)
- [VetSyCare - Pricing Guide 2026](https://vetsycare.com/blog/veterinary-software-pricing-guide)
- [AAHA - Choosing PIMS](https://www.aaha.org/trends-magazine/publications/view-from-the-board-considerations-for-choosing-veterinary-practice-management-software/)
- [Veterinary IT Services - Consequences of Bad IT](https://veterinaryit.services/8-consequences-of-bad-it-systems-in-the-veterinary-field/)

### Telemedicine
- [VetSoftwareHub - Best Veterinary Telemedicine 2026](https://www.vetsoftwarehub.com/category/telemedicine)
- [Akveo - State of Veterinary Telehealth 2025](https://www.akveo.com/pet-care/veterinary-software-development/telehealth)
- [IDEXX - 7 Digital Trends 2026](https://software.idexx.com/resources/blog/7-digital-veterinary-technology-trends-shaping-practices-in-2026)

### French Market
- [Appvizer - 6 Meilleurs Logiciels Veterinaire 2026](https://www.appvizer.com/health/veterinary)
- [La Fabrique du Net - Top 10 ERP Veterinaires](https://www.lafabriquedunet.fr/blog/comparatif-meilleurs-logiciels-veterinaires/)
- [e-Sante Animale - Logiciels pour veterinaires](https://esanteanimale.fr/logiciels-pour-veterinaires/)
- [VetoPartner](https://www.vetopartner.fr/)
- [dr.veto](https://www.drveto.com/)
- [Vetup](https://www.vetup.com/)

---

## Key Takeaways

1. **The #1 pain is UI/UX** -- click-heavy, unintuitive, outdated interfaces. Vetolib's modern stack is a genuine advantage here.

2. **The #1 feature request is AI scribe** (voice-to-SOAP). This is the most urgent gap in Vetolib's roadmap. Every competitor is either building this or integrating with a standalone scribe.

3. **Revenue leakage is massive** -- $60K/year in missed charges, $50K/year in no-shows, $84K/year in inventory waste. Vetolib can quantify the ROI of switching.

4. **Data lock-in is the #1 switching barrier**. A "Data Freedom Guarantee" with free migration would be a powerful sales tool.

5. **Lab integration (IDEXX)** is table stakes for any serious PIMS. This is Vetolib's biggest technical gap.

6. **No-show prediction is a unique differentiator** -- almost no competitor has this. Accelerate Phase 3.

7. **WhatsApp integration is non-negotiable for UAE**. Pet owners in the Gulf communicate via WhatsApp, not email.

8. **Per-clinic pricing (not per-user)** is the preferred model. Transparent, all-inclusive pricing attacks competitor opacity.

9. **The French market** has established players (VetoPartner 600+ clinics, dr.veto, Vetup) but none with AI features or modern cloud architecture. Vetolib can enter as the "next generation" option.

10. **Multi-location management** is a growing need due to corporate consolidation. Vetolib's multi-tenant architecture is ready for this.
