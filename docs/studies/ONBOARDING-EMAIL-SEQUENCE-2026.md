# Vetolib Onboarding Email Sequence (2026)

> **Target**: New sign-ups (Free tier + Pro trial 14 days)
> **Goal**: Activate users toward 3 milestones: first patient created, first appointment booked, first invoice sent.
> **Tone**: Warm, helpful, non-pushy. Written in English (UAE market).
> **Logic**: Each email is skipped if the user has already completed the action it promotes.

---

## Email 1 — Welcome

| Field | Value |
|---|---|
| **Trigger** | Immediately after sign-up (J+0) |
| **Skip condition** | Never skipped — always sent |
| **Subject** | Welcome to Vetolib — let's get your clinic running |
| **Preview text** | Your account is ready. Here's what to do first. |

### Body

Hi {{first_name}},

Welcome to Vetolib! Your clinic account is live, and your 14-day Pro trial has started.

Vetolib helps veterinary clinics in the UAE manage appointments, medical records, billing, and client communication — all in one place.

**Here's what happens next:**

1. Set up your clinic profile (hours, team, consultation types)
2. Add your first patient
3. Book your first appointment

It takes about 10 minutes to get fully operational. We've prepared a quickstart guide that walks you through each step.

**[Get started now -> {{quickstart_url}}]**

If you have questions at any point, just reply to this email — a real person reads every message.

Best,
The Vetolib Team

---

## Email 2 — Setup Checklist

| Field | Value |
|---|---|
| **Trigger** | J+1 (24 hours after sign-up) |
| **Skip condition** | Skip if clinic profile is complete (operating hours set AND at least 1 team member added AND at least 1 consultation type created) |
| **Subject** | Quick wins: 3 things to set up today |
| **Preview text** | Operating hours, your team, consultation types — 5 minutes each. |

### Body

Hi {{first_name}},

Getting the basics in place makes everything else faster. Here are 3 things you can set up right now:

**1. Operating hours**
Tell Vetolib when your clinic is open. If you follow the UAE Sunday-Thursday schedule, we've pre-filled that for you — just confirm or adjust.

**[Set operating hours -> {{settings_hours_url}}]**

**2. Your team**
Add your veterinarians and staff so you can assign appointments and track who does what.

**[Add team members -> {{settings_team_url}}]**

**3. Consultation types**
Define the types of visits you offer (general consultation, vaccination, surgery, dental, etc.). This powers your booking calendar and AI triage suggestions.

**[Create consultation types -> {{settings_consultation_types_url}}]**

Each one takes about 2 minutes. Once these are done, you're ready to see patients.

Best,
The Vetolib Team

---

## Email 3 — First Patient

| Field | Value |
|---|---|
| **Trigger** | J+3 (72 hours after sign-up) |
| **Skip condition** | Skip if at least 1 patient exists in the account |
| **Subject** | Add your first patient (or import them all at once) |
| **Preview text** | One patient manually, or hundreds via CSV — your call. |

### Body

Hi {{first_name}},

A clinic management tool is only useful when your patients are in it. Let's fix that.

**Option A — Add one patient manually**
Click "New Patient," fill in the owner details and pet info, and you're done in under a minute. Great for getting a feel for the system.

**[Add a patient -> {{new_patient_url}}]**

**Option B — Import your existing database**
If you have patient records in a spreadsheet, you can import them all at once using our CSV import tool. We support exports from most common practice management systems.

**[Import via CSV -> {{csv_import_url}}]**

Need help formatting your file? Reply to this email and we'll prepare a template matched to your current system.

**Pro tip**: Once a patient exists, their full medical history, prescriptions, and invoices all live in one place — accessible from any device.

Best,
The Vetolib Team

---

## Email 4 — First Appointment

| Field | Value |
|---|---|
| **Trigger** | J+5 (5 days after sign-up) |
| **Skip condition** | Skip if at least 1 appointment has been created |
| **Subject** | Book your first appointment — see the calendar in action |
| **Preview text** | Drag, drop, done. Your schedule, organized. |

### Body

Hi {{first_name}},

The calendar is the heart of Vetolib. Here's how to book your first appointment:

**From the calendar view:**
1. Click on an empty time slot (or click "+ New Appointment")
2. Search for a patient (or create one on the fly)
3. Pick the consultation type and assign a vet
4. Confirm — done

**What you'll notice:**
- **Day, week, and month views** — switch between them instantly
- **Color-coded consultations** — each type gets its own color so you can scan your day at a glance
- **Conflict detection** — Vetolib warns you if you're double-booking a vet
- **Client notifications** — if WhatsApp is configured, your client gets an automatic confirmation

**[Open the calendar -> {{calendar_url}}]**

Once you've created an appointment, try completing it: open the consultation, write a SOAP note, and mark it as done. That's a full clinical workflow in under 5 minutes.

Best,
The Vetolib Team

---

## Email 5 — AI Features

| Field | Value |
|---|---|
| **Trigger** | J+7 (7 days after sign-up) |
| **Skip condition** | Skip if user has already used AI triage or AI SOAP notes at least once |
| **Subject** | Let AI handle the paperwork |
| **Preview text** | AI triage and SOAP notes — included in your Pro trial. |

### Body

Hi {{first_name}},

You've been using Vetolib for a week now. Time to meet your AI assistant.

**AI Triage**
When a client books online, Vetolib's AI analyzes the reported symptoms and suggests the right consultation type and urgency level. You review and confirm — the AI never decides alone.

This means fewer mis-categorized appointments and faster intake for emergencies.

**[See AI Triage in action -> {{ai_triage_url}}]**

**AI SOAP Notes**
After a consultation, describe what happened in plain language. The AI structures it into a proper SOAP note (Subjective, Objective, Assessment, Plan) that you can review, edit, and save to the medical record.

Less typing, more time with patients.

**[Try AI SOAP Notes -> {{ai_soap_url}}]**

Both features are available during your Pro trial. They remain available on the Pro plan after the trial ends.

*Note: AI suggestions are decision-support tools. All clinical decisions remain yours.*

Best,
The Vetolib Team

---

## Email 6 — WhatsApp Setup

| Field | Value |
|---|---|
| **Trigger** | J+10 (10 days after sign-up) |
| **Skip condition** | Skip if WhatsApp integration is already configured and active |
| **Subject** | Automate client reminders via WhatsApp |
| **Preview text** | Reduce no-shows with appointment reminders your clients actually read. |

### Body

Hi {{first_name}},

In the UAE, WhatsApp is how people communicate. Vetolib uses it to keep your clients informed — automatically.

**What gets sent automatically once configured:**
- Appointment confirmation (immediately after booking)
- Reminder 24 hours before the appointment
- Follow-up message after the visit (with care instructions if applicable)
- Vaccination reminders when boosters are due

**Setup takes about 5 minutes:**
1. Go to Settings > Integrations > WhatsApp
2. Connect your clinic's WhatsApp Business number
3. Review the message templates (you can customize them)
4. Activate

**[Set up WhatsApp -> {{whatsapp_setup_url}}]**

Clinics using WhatsApp reminders see a measurable reduction in no-shows. Your clients get the information they need, and your front desk spends less time on the phone.

Best,
The Vetolib Team

---

## Email 7 — Trial Ending

| Field | Value |
|---|---|
| **Trigger** | J+12 (12 days after sign-up — 2 days before trial expires) |
| **Skip condition** | Skip if user has already upgraded to a paid Pro plan |
| **Subject** | Your Pro trial ends in 2 days |
| **Preview text** | Here's exactly what changes — and what stays. |

### Body

Hi {{first_name}},

Your 14-day Pro trial ends on **{{trial_end_date}}**. Here's what you need to know.

**What stays on the Free plan:**
- Unlimited patients and medical records
- Calendar and appointment management
- Basic invoicing
- Mobile access

**What you'll lose without Pro:**
- AI Triage and AI SOAP Notes
- WhatsApp automated reminders
- Advanced reporting and analytics
- Multi-vet calendar with conflict detection
- CSV import and data export
- Priority support

**Your data is safe.** Nothing gets deleted. If you upgrade later, everything is exactly where you left it.

**[Upgrade to Pro -> {{upgrade_url}}]**

If you're not sure yet, that's okay. You can continue on the Free plan and upgrade whenever you're ready. No pressure, no hidden countdowns.

Have questions about pricing or what's included? Reply here — happy to help.

Best,
The Vetolib Team

---

## Implementation Notes

### Sending infrastructure
- Use a transactional email provider (e.g., Postmark, Resend, or AWS SES)
- All emails sent from `hello@vetolib.com` with reply-to monitored by support
- Unsubscribe link required in every email (CAN-SPAM / UAE TRA compliance)

### Tracking events for skip conditions
| Email | Skip if event exists |
|---|---|
| E1 (Welcome) | Never skipped |
| E2 (Setup) | `clinic_profile_complete` |
| E3 (First patient) | `patient_created` |
| E4 (First appointment) | `appointment_created` |
| E5 (AI features) | `ai_triage_used` OR `ai_soap_used` |
| E6 (WhatsApp) | `whatsapp_integration_active` |
| E7 (Trial ending) | `pro_plan_subscribed` |

### Metrics to track
- **Open rate** per email (benchmark: 40-60% for onboarding)
- **CTA click rate** per email (benchmark: 15-25%)
- **Activation rate**: % of users who complete all 3 milestones within trial
- **Conversion rate**: % of trial users who upgrade to Pro
- **Time to first milestone**: median days to first patient, first appointment, first invoice

### A/B testing candidates
- E1 subject line: "Welcome to Vetolib" vs. "Your clinic is live — here's your first step"
- E4 CTA: "Open the calendar" vs. "Book your first appointment now"
- E7 tone: loss-aversion framing vs. neutral summary
- E7 timing: J+12 vs. J+13 (2 days vs. 1 day before expiry)
