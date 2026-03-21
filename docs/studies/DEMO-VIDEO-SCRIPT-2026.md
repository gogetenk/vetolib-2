# Vetolib Demo Video Script -- 2026

> Production-ready script for a 90-second product demo video.
> Target channels: Product Hunt, landing page hero, investor pitches, LinkedIn/X ads.
> Last updated: 2026-03-21

---

## 1. FULL SCRIPT -- 90 Seconds (Voice-Over)

### 0-10s -- HOOK

**Voice-over:**
> "What if your vet clinic ran itself? No more phone tag. No more paper charts. No more late nights doing admin instead of medicine."

**Tone:** Calm, confident. Not a question -- a promise.

---

### 10-25s -- THE PROBLEM

**Voice-over:**
> "Right now, veterinarians spend over an hour a day on paperwork. They juggle five, six, sometimes eight different tools -- for scheduling, records, billing, communication. And none of them talk to each other. The software was built in 2010. The vets using it deserve better."

**Tone:** Empathy. Slight tension. The viewer should think: "That's me."

---

### 25-50s -- THE SOLUTION TOUR

**Voice-over:**
> "Meet Vetolib. One platform. Everything your clinic needs."
>
> "See your entire day at a glance -- appointments color-coded by type, drag-and-drop rescheduling, conflict detection built in."
>
> "Pull up any patient in two clicks. Full history, vaccinations, prescriptions -- all linked to the owner."
>
> "Need SOAP notes? Our AI drafts them for you in seconds. Review, approve, done."
>
> "Invoices generate automatically with UAE VAT. And your clients? They book online and get WhatsApp confirmations -- no phone calls needed."

**Tone:** Energetic but controlled. Let the product speak. Each feature gets exactly one breath.

---

### 50-70s -- DIFFERENTIATORS

**Voice-over:**
> "Built for the Middle East from day one. Full Arabic interface, right-to-left layout, Ramadan-aware scheduling, AED billing."
>
> "AI-powered triage tells pet owners if it's routine or urgent -- before they even walk in."
>
> "Running multiple clinics? Every location is fully isolated, fully managed, from one dashboard."

**Tone:** Authoritative. These are not features -- they are moats.

---

### 70-85s -- SOCIAL PROOF + PRICING

**Voice-over:**
> "680 automated tests. Built on enterprise-grade infrastructure -- .NET Aspire, PostgreSQL, Next.js. This isn't a weekend project."
>
> "Transparent pricing from 299 AED per vet per month. No hidden fees. No 'contact sales.' 30-day free trial, cancel anytime."

**Tone:** Trust. Factual. No hype.

---

### 85-90s -- CTA

**Voice-over:**
> "Vetolib. Less admin. More medicine. Start your free trial today."

**Tone:** Warm, final. Logo + URL hold for 3 seconds.

---

## 2. STORYBOARD -- Screen by Screen

### Scene 1: Hook (0-10s)

| Element | Detail |
|---|---|
| **Screen** | Dark background, slow reveal of Vetolib logo (white on dark). Subtle particle animation. |
| **Action** | Logo fades in, tagline types itself: "Less admin. More medicine." |
| **Voice-over** | "What if your vet clinic ran itself?..." |
| **Transition** | Hard cut to a vet staring at a cluttered desk (stock footage or illustration). |

---

### Scene 2: The Problem (10-25s)

| Element | Detail |
|---|---|
| **Screen** | Split montage: (left) frustrated vet surrounded by paper files and a clunky legacy UI mockup. (right) clock spinning, calendar chaos, phone ringing icons. |
| **Action** | Quick-cut montage: paper stacking up, notification overload, Excel hell. Text overlays: "5-8 tools", "1+ hour/day on admin", "$400/mo for outdated software". |
| **Voice-over** | "Right now, veterinarians spend over an hour a day..." |
| **Transition** | Zoom into the legacy screen, which dissolves into the Vetolib dashboard. |

---

### Scene 3A: Calendar (25-35s)

| Element | Detail |
|---|---|
| **Screen** | `/en/appointments` -- Appointments calendar view (week view). |
| **Action** | Mouse scrolls through a busy week. Color-coded blocks (blue = checkup, orange = surgery, green = vaccination). User drags an appointment from 10am to 2pm -- smooth animation. A conflict toast appears, resolves itself. |
| **Voice-over** | "See your entire day at a glance..." |
| **Transition** | Slide left to patient view. |

---

### Scene 3B: Patients + Medical Records (35-42s)

| Element | Detail |
|---|---|
| **Screen** | `/en/patients/[id]` -- Patient detail page. |
| **Action** | Click on a patient card ("Luna -- Persian Cat"). Medical history expands: vaccinations timeline, past consultations, linked owner ("Ahmed Al-Rashid"). Quick scroll through record entries. |
| **Voice-over** | "Pull up any patient in two clicks..." |
| **Transition** | Zoom into a medical record entry, which morphs into the SOAP panel. |

---

### Scene 3C: AI SOAP Notes (42-47s)

| Element | Detail |
|---|---|
| **Screen** | `SoapNotesPanel.tsx` component within `/en/patients/[id]` -- medical record form. |
| **Action** | User clicks "Generate SOAP Notes." Loading shimmer for 1.5s. AI-generated subjective/objective/assessment/plan fields populate with realistic vet content. User reviews, clicks "Approve." Green checkmark. AI disclaimer visible at bottom. |
| **Voice-over** | "Need SOAP notes? Our AI drafts them for you in seconds..." |
| **Transition** | Slide right to billing. |

---

### Scene 3D: Billing + WhatsApp (47-50s)

| Element | Detail |
|---|---|
| **Screen** | `/en/billing` -- Invoice table, then quick flash to `/en/messages` showing a WhatsApp conversation thread. |
| **Action** | Invoice auto-generates: line items, 5% VAT calculated, total in AED. Quick cut to the messaging view: a WhatsApp confirmation message sent to the pet owner in Arabic. |
| **Voice-over** | "Invoices generate automatically... WhatsApp confirmations..." |
| **Transition** | Fade to Arabic UI. |

---

### Scene 4A: Arabic + RTL (50-58s)

| Element | Detail |
|---|---|
| **Screen** | `/ar/dashboard` -- Full dashboard in Arabic, RTL layout. |
| **Action** | Language switcher (`LanguageSwitcher.tsx`) toggles from EN to AR. The entire interface flips smoothly: sidebar moves to the right, text aligns right, calendar reads right-to-left. Show the settings page with Ramadan hours toggle. |
| **Voice-over** | "Full Arabic interface, right-to-left layout, Ramadan-aware scheduling..." |
| **Transition** | Zoom into a search bar, where the triage demo starts. |

---

### Scene 4B: AI Triage (58-64s)

| Element | Detail |
|---|---|
| **Screen** | Online booking portal (public-facing) or triage component. |
| **Action** | Pet owner types: "My cat hasn't eaten in 3 days and is vomiting." AI processes (brief loading). Result: red "Urgent" badge, recommended action: "Book as emergency appointment." AI disclaimer clearly visible. |
| **Voice-over** | "AI-powered triage tells pet owners if it's routine or urgent..." |
| **Transition** | Pull back to show multi-clinic selector. |

---

### Scene 4C: Multi-Clinic (64-70s)

| Element | Detail |
|---|---|
| **Screen** | `/en/settings` or clinic switcher in `Header.tsx`. |
| **Action** | Dropdown opens showing 3 clinics: "Dubai Marina Vet", "Abu Dhabi Pet Care", "Sharjah Animal Hospital." Click to switch -- dashboard data changes instantly. |
| **Voice-over** | "Running multiple clinics? Every location is fully isolated..." |
| **Transition** | Fade to trust section. |

---

### Scene 5: Social Proof + Pricing (70-85s)

| Element | Detail |
|---|---|
| **Screen** | Clean graphic layout (not in-app). Dark background. |
| **Action** | Animated counters: "680+ automated tests", "10 modules", "Enterprise-grade stack". Tech logos fade in: .NET, PostgreSQL, Next.js. Then pricing card appears: "Starter: 299 AED/vet/month" with feature checklist. "30-day free trial" badge pulses gently. |
| **Voice-over** | "680 automated tests... Transparent pricing from 299 AED..." |
| **Transition** | Fade to final card. |

---

### Scene 6: CTA (85-90s)

| Element | Detail |
|---|---|
| **Screen** | Vetolib logo centered on brand-colored background. URL below: `vetolib.com`. Button: "Start Free Trial". |
| **Action** | Logo holds. Tagline fades in below: "Less admin. More medicine." Subtle shimmer on the CTA button. |
| **Voice-over** | "Vetolib. Less admin. More medicine. Start your free trial today." |
| **Transition** | Hold 3 seconds. End. |

---

## 3. SHORT VERSION -- 30 Seconds (Social Media)

For Instagram Reels, TikTok, X/Twitter, LinkedIn video ads.

### Script

> **[0-3s]** "Vets spend an hour a day on paperwork."
> *(Text on screen: "1+ hour/day on admin". Frustrated vet stock footage.)*
>
> **[3-8s]** "What if one platform handled everything?"
> *(Quick montage: calendar, patient card, invoice -- 1.5s each, fast cuts.)*
>
> **[8-18s]** "Vetolib: scheduling, records, billing, AI SOAP notes, WhatsApp -- all in one."
> *(Screen recording: dashboard overview, click through patient, AI generates notes, invoice appears.)*
>
> **[18-25s]** "Arabic. AI triage. Multi-clinic. Built for the UAE."
> *(RTL toggle, triage result, clinic switcher -- rapid cuts.)*
>
> **[25-30s]** "Start free. vetolib.com"
> *(Logo + URL + CTA button. Clean, bold.)*

### Specs
- Aspect ratio: 9:16 (vertical) for Reels/TikTok, 1:1 (square) for LinkedIn/X
- Captions burned in (80% of social video is watched on mute)
- No voice-over required -- text overlays carry the message
- Background music: upbeat lo-fi or electronic (see Section 4)

---

## 4. MUSIC RECOMMENDATIONS

| Mood | Style | Where to Source | Notes |
|---|---|---|---|
| **Primary pick** | Upbeat electronic, clean, modern. Think Stripe or Linear product videos. Mid-tempo (110-120 BPM). | Artlist, Epidemic Sound, Musicbed | Avoid vocals. Subtle bass, crisp hi-hats, rising energy in the solution section. |
| **Alternative A** | Inspiring orchestral-lite. Piano + subtle strings. | Artlist "Corporate Inspiring" | Good for investor pitch version. Builds emotion. |
| **Alternative B** | Lo-fi ambient. Warm, techy, minimal. | Epidemic Sound "Tech / Startup" | Best for social media short cuts. Unobtrusive. |
| **Alternative C** | Middle Eastern fusion -- subtle oud or percussion blended with modern electronic. | Custom commission or Artlist "World / Middle East" | Bold choice. Signals UAE-first identity. Use sparingly -- 10-15s max for the Arabic section. |

### Music Arc (90s version)
- **0-10s:** Low, ambient pad. Tension. Minimal.
- **10-25s:** Subtle percussive build. Slight unease (the problem).
- **25-50s:** Drop into full groove. Upbeat, confident. Energy peak at 40s.
- **50-70s:** Shift to a slightly different texture (Arabic fusion hint at 50-58s, then back to main groove).
- **70-85s:** Groove continues but cleaner, more spacious. Room for facts.
- **85-90s:** Music resolves. Final chord or gentle fade. Satisfying close.

---

## 5. LIVE DEMO TALKING POINTS -- 10 Minutes

For investor meetings, clinic owner demos, conference booths.

### Recommended Feature Order (10 min)

| Time | Feature | Screen | What to Show |
|---|---|---|---|
| 0-1 min | **Opening + Dashboard** | `/en/dashboard` | "This is what your clinic looks like at 8am. Everything in one view: today's appointments, pending invoices, unread messages. No switching between apps." |
| 1-3 min | **Appointments** | `/en/appointments` | Create an appointment live. Show conflict detection ("Dr. Sara already has a surgery at 10am"). Drag-and-drop reschedule. Color-coded types. Toggle to week/month view. |
| 3-4 min | **Patients + Owners** | `/en/patients` then `/en/patients/[id]` | Search for "Luna." Show linked owner, vaccination history, medical timeline. "Every piece of data is connected -- you never lose context." |
| 4-5.5 min | **AI SOAP Notes** | `SoapNotesPanel` in medical record | Start a consultation. Click "Generate SOAP." Watch AI fill in the fields. Edit one line manually. Approve. "This saves 15 minutes per consultation. That's an extra hour every day." |
| 5.5-6.5 min | **AI Triage** | Booking portal / triage input | Type symptoms: "My dog ate chocolate 2 hours ago." Show urgency result. "This is the first thing pet owners see. It saves lives and reduces your no-shows." |
| 6.5-7.5 min | **Messaging + WhatsApp** | `/en/messages` | Show a conversation thread. Appointment reminders sent via WhatsApp automatically. "95% of UAE residents use WhatsApp. We meet your clients where they are." |
| 7.5-8.5 min | **Billing** | `/en/billing` | Generate an invoice from a completed consultation. Show VAT calculation (5%), AED currency. "One click. Fully compliant." |
| 8.5-9 min | **Arabic + Multi-clinic** | Language switcher + clinic switcher | Toggle to Arabic. Let the audience react to the full RTL flip. Switch clinics. "Every clinic is isolated. Your Dubai data never touches your Abu Dhabi data." |
| 9-10 min | **Close + Pricing** | Pricing page or slide | "299 AED per vet per month. 30-day free trial. No contracts, no hidden fees. You can be onboarded by tomorrow." |

---

### Questions to Ask the Prospect (Discovery)

Use these during or after the demo to qualify and personalize:

1. "How many vets do you have on staff?"
   *(Sizes the deal: pricing = per vet.)*

2. "What software are you using today? How much are you paying?"
   *(Anchors against current pain. Most pay $300-500/mo for less.)*

3. "How do your clients book appointments -- phone, WhatsApp, walk-in?"
   *(Sets up the online booking + WhatsApp differentiator.)*

4. "Do you have Arabic-speaking staff or clients?"
   *(If yes: "We're the only platform with native Arabic RTL.")*

5. "Do you manage more than one location?"
   *(If yes: multi-tenant becomes the killer feature.)*

6. "How long does it take your vets to write up their notes after a consultation?"
   *(Quantifies the AI SOAP value: 15 min saved x 20 consultations = 5 hours/day reclaimed.)*

7. "Have you ever had a no-show ruin your schedule?"
   *(Introduces no-show prediction as a bonus feature.)*

---

### Common Objections and Responses

| Objection | Response |
|---|---|
| **"We already have software that works fine."** | "Does it support Arabic? WhatsApp? AI notes? If you're happy with 2010 software, we respect that. But your competitors are modernizing -- and their clients notice." |
| **"It looks great, but migration is painful."** | "We have a data migration wizard. Import your patient records from CSV or your existing system. Most clinics are fully onboarded in 48 hours, not 3 months." |
| **"How do I know my data is safe?"** | "Multi-tenant architecture with strict isolation. Each clinic's data is separated at the database level. We run 680+ automated tests on every release. Enterprise-grade infrastructure: PostgreSQL, .NET Aspire, deployed on Azure." |
| **"Is the AI reliable? I don't trust AI with medical decisions."** | "The AI suggests -- it never decides. Every AI output has a clear disclaimer, and the vet always reviews and approves before anything is saved. It's a time-saver, not a replacement." |
| **"299 AED/month sounds expensive for a small clinic."** | "That's less than 10 AED per day. If AI SOAP notes save your vet 1 hour a day, that hour is worth far more than 10 dirhams. Plus: 30-day free trial, no commitment." |
| **"Can I try it first?"** | "Absolutely. 30 days free, full access, no credit card required. You'll know within a week." |
| **"What about French/Polish support?"** | "English and Arabic are live today. French and Polish are in our roadmap for Q4 2026. The architecture is built for multi-language from day one -- adding a language is a configuration change, not a rewrite." |

---

## 6. PRODUCTION NOTES

### Recording the Product Screens

- Use a **1920x1080** browser window, 100% zoom, no browser chrome (use fullscreen or a screen recording tool that crops)
- Cursor movements should be smooth and intentional -- use a tool like **Cursor Highlighter** or record with a Wacom tablet for fluid motion
- All demo data must use **realistic UAE names**: Ahmed Al-Rashid, Fatima Al-Maktoum, Dr. Sara Al-Nuaimi. Pet names: Luna, Simba, Coco, Nala
- Currency: AED. Dates: DD/MM/YYYY. Timezone: Asia/Dubai
- Record at **60fps** for smooth UI transitions, export at 30fps for final delivery

### Delivery Formats

| Platform | Format | Duration | Aspect Ratio |
|---|---|---|---|
| Product Hunt | MP4 / embedded | 90s | 16:9 |
| Landing page hero | MP4 (autoplay, muted, looped) | 90s or 30s loop | 16:9 |
| Instagram Reels | MP4 | 30s | 9:16 |
| TikTok | MP4 | 30s | 9:16 |
| LinkedIn | MP4 | 30s or 90s | 1:1 or 16:9 |
| X/Twitter | MP4 | 30s | 16:9 or 1:1 |
| Investor pitch (in-person) | Embedded in Keynote/PPT | 90s | 16:9 |

### Voiceover Talent

- **Accent:** Neutral international English (think: Bloomberg anchor, not Valley startup bro)
- **Gender:** Male or female -- test both. The voice should feel trustworthy and calm, not salesy
- **Sources:** Fiverr Pro, Voices.com, or ElevenLabs for AI voice (test "Rachel" or "Adam" voices)
- **Script reading time check:** 90s script at ~150 words/min = ~370 words. Current script is ~340 words. Buffer is comfortable.
