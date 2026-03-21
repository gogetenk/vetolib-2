# Post-MVP Product Owner Decisions

**Date**: 2026-03-10
**Author**: Product Owner Vetolib
**Status**: Validated -- ready for architect consumption
**Context**: MVP complete (174 tasks done). These decisions guide post-MVP priorities.

---

## Table of Contents

1. [Analytics / PostHog](#1-analytics--posthog)
2. [Preference Management](#2-preference-management-opt-in--opt-out)
3. [Multi-Market Readiness](#3-multi-market-readiness-uae--eu--us)

---

## 1. Analytics / PostHog

### Priority: HIGH

**Why**: Without usage analytics, we are blind. We cannot measure feature adoption, identify churn signals, or prioritize the roadmap with data. For a SaaS, this is not optional -- it is foundational.

### 1.1 Key Behaviors to Track

Ordered by business impact:

| Behavior | Why it matters | Event name suggestion |
|----------|---------------|----------------------|
| Appointment creation flow completion | Core value loop. If users don't book, nothing else matters. | `appointment_created` |
| Time from login to first action | Measures onboarding friction. UAE vets are busy, 30+ seconds = churn risk. | `first_action_after_login` |
| Medical record creation | Proves the vet trusts the system with clinical data (highest commitment). | `medical_record_created` |
| Invoice sent (DRAFT to SENT) | Revenue generation action. Measures billing adoption. | `invoice_sent` |
| CSV import usage | Feature adoption signal. Clinics importing data are committing to the platform. | `csv_import_completed` |
| AI triage acceptance rate | Already tracked in DB (`WasAccepted`). Surface in PostHog for product analytics. | `ai_triage_accepted` / `ai_triage_overridden` |
| No-show prediction viewed | Measures trust in AI features. | `noshow_prediction_viewed` |
| Messaging conversation opened | Measures owner engagement and clinic responsiveness. | `conversation_opened` |
| User invited (team growth) | Expansion signal within a clinic. More users = stickier account. | `user_invited` |
| Feature page visits (Agenda, Billing, Medical, Messaging) | Navigation heatmap. Identifies which modules are actually used. | `page_viewed` with `module` property |

### 1.2 Business KPIs

| KPI | Definition | Target (6 months post-launch) |
|-----|-----------|-------------------------------|
| **Weekly Active Clinics (WAC)** | Clinics with at least 1 appointment created per week | 80% of registered clinics |
| **Feature Adoption Depth** | Average number of modules used per clinic (out of 6: Agenda, Medical, Billing, Messaging, Stock, AI) | >= 3 modules |
| **Time to Value** | Days from registration to first appointment created | < 1 day |
| **Billing Activation Rate** | % of clinics that send at least 1 invoice within 14 days of registration | > 50% |
| **Retention (D30)** | % of clinics active 30 days after registration | > 70% |
| **Expansion Rate** | Average users per clinic over time | Growing (2 to 4+ in 90 days) |
| **AI Trust Score** | % of AI triage suggestions accepted without override | > 60% (below = prompt tuning needed) |
| **NPS** | Net Promoter Score via in-app survey | > 40 |

### 1.3 Privacy and Transparency (UAE market)

**UAE vets are NOT particularly sensitive to analytics tracking**, compared to EU markets. The UAE has no GDPR equivalent with the same enforcement level. The Federal Decree-Law No. 45/2021 on Personal Data Protection exists but is less restrictive than GDPR, and veterinary practice data (animal health) is largely outside its scope.

However, best practices still apply:

- **Cookie banner**: Yes, with simple accept/decline. No granular cookie settings needed for UAE MVP.
- **Privacy policy**: Must exist, must mention analytics. Plain English, no legalese.
- **No PII in analytics events**: Never send patient names, owner names, email addresses, or medical data to PostHog. Only send anonymized IDs, event types, and aggregate counts.
- **Server-side tracking preferred**: Avoids ad-blockers and is more reliable. PostHog supports server-side SDKs.
- **Clinic-level analytics visible to ADMIN**: The clinic admin should see their own usage stats (how many appointments this month, active users, etc.). This is not PostHog -- this is the existing dashboard.

### 1.4 Decision: Simple PostHog First, Admin Dashboard Later

**Phase 1 (immediate)**: Integrate PostHog with server-side SDK. Track the 10 behaviors listed above. No custom analytics dashboard for clinic admins yet.

**Phase 2 (3 months)**: Build a PostHog insights dashboard for the Vetolib product team (internal). Define alerts on churn signals (clinic inactive > 7 days).

**Phase 3 (6 months)**: Consider exposing anonymized usage stats to clinic ADMINs in the existing dashboard module. Only if clinics request it.

**Rationale**: Building a custom admin analytics dashboard before we even know what metrics matter is premature. PostHog gives us the data. We decide what to surface to clinics based on actual usage patterns.

### 1.5 User Stories

```
US-ANALYTICS-1: As a product owner, I want to see weekly active clinics and feature adoption
  so that I can prioritize the roadmap based on real usage data.

US-ANALYTICS-2: As a product owner, I want to receive an alert when a clinic has been
  inactive for 7+ days so that I can trigger a re-engagement email.

US-ANALYTICS-3: As a clinic admin, I want to see how many appointments, invoices, and
  patients my team processed this month so that I can track productivity.
  [DEFERRED to Phase 3 -- use existing dashboard KPIs first]
```

### 1.6 Acceptance Criteria (Phase 1)

- [ ] PostHog server-side SDK integrated in the .NET backend
- [ ] All 10 key events tracked with correct properties (clinicId anonymized, no PII)
- [ ] PostHog dashboard with WAC, Feature Adoption Depth, and Time to Value visible
- [ ] No client-side PostHog JS (server-side only for now)
- [ ] Privacy policy page updated to mention analytics
- [ ] No performance degradation (events sent async, fire-and-forget)

---

## 2. Preference Management (Opt-in / Opt-out)

### Priority: MEDIUM

**Why**: Users need control over their experience. But over-engineering preferences is a trap. We start with the preferences that actually matter and add granularity only when users request it.

### 2.1 Preferences Users Must Control

| Preference | Granularity | Default | Who sets it |
|-----------|-------------|---------|-------------|
| **Email notifications: appointment reminders** | Per user | ON | Individual user |
| **Email notifications: invoice sent** | Per user | ON | Individual user |
| **Email notifications: messaging (new message received)** | Per user | ON | Individual user |
| **SMS notifications** | Per clinic | OFF (not implemented yet) | Admin only |
| **Push notifications** | Per user | OFF (not implemented yet) | Individual user |
| **Language** | Per user | EN | Individual user (EN or AR) |
| **Timezone** | Per clinic | Asia/Dubai | Admin only |
| **AI triage suggestions visible in appointment form** | Per clinic | ON | Admin only |
| **AI no-show predictions visible in agenda** | Per clinic | ON | Admin only |
| **AI messaging suggestions** | Per clinic | ON | Admin only |
| **Drug interaction alerts** | NOT configurable | ALWAYS ON | Nobody -- safety feature, never disable |
| **Operating hours** | Per clinic | Sun-Thu 8:00-18:00 | Admin only |
| **Ramadan hours** | Per clinic | Not set | Admin only |
| **Analytics tracking** | Per clinic | ON | Admin only (opt-out at clinic level) |

### 2.2 Granularity Rules

**Per clinic (Admin decides for all):**
- Timezone, operating hours, Ramadan hours
- AI feature toggles (triage, no-show, messaging AI)
- SMS channel (enabled/disabled)
- Analytics tracking opt-out

**Per user (individual choice):**
- Language
- Email notification channels (per category)
- Push notification channels (when implemented)

**Not configurable (safety):**
- Drug interaction alerts: ALWAYS ON. A vet cannot disable drug interaction warnings. This is a patient safety feature. If a vet disagrees with an interaction alert, they can override it (already implemented with `OverrideReason`), but the alert itself always fires.

### 2.3 Key Use Case: Vet Disables AI Suggestions but Keeps Drug Alerts

This is explicitly supported by the granularity model above:

- AI triage suggestions = clinic-level toggle, Admin can turn OFF
- AI no-show predictions = clinic-level toggle, Admin can turn OFF
- Drug interaction alerts = NOT configurable, ALWAYS ON

So a vet who finds AI triage suggestions unhelpful can ask their Admin to disable them. Drug interaction alerts will continue to fire regardless. This is the correct behavior.

However, if a single vet wants to disable AI suggestions while other vets in the same clinic want to keep them, the current model does NOT support this (it is per-clinic, not per-vet). This is an intentional simplification for the first version.

**If user feedback shows that per-vet AI toggles are needed**, we add a `UserPreference` entity with per-user overrides. But not before we have evidence of the need.

### 2.4 Defaults Philosophy

**Opt-in by default for everything that adds value and has no cost:**
- Email notifications: ON (users expect them)
- AI suggestions: ON (this is a differentiator)
- Drug interaction alerts: ALWAYS ON (safety)
- Analytics: ON (clinic-level opt-out available)

**Opt-out by default for everything that has a cost or is not yet implemented:**
- SMS: OFF (cost per message, needs Twilio integration)
- Push notifications: OFF (not implemented)
- Ramadan hours: NOT SET (not every clinic observes)

### 2.5 User Stories

```
US-PREF-1: As a clinic staff member, I want to choose which email notifications I receive
  (reminders, invoices, messages) so that I am not overwhelmed by emails.

US-PREF-2: As a clinic admin, I want to enable or disable AI features (triage, no-show,
  messaging AI) for my entire clinic so that I can control our technology adoption pace.

US-PREF-3: As a clinic admin, I want to configure operating hours and Ramadan hours
  so that appointments and messaging hours reflect our actual schedule.

US-PREF-4: As a vet, I want drug interaction alerts to ALWAYS fire regardless of any
  preference setting so that patient safety is never compromised.

US-PREF-5: As a clinic admin, I want to opt my clinic out of analytics tracking
  so that we have control over our data.
```

### 2.6 Acceptance Criteria

- [ ] `UserPreference` entity with per-user notification preferences (email categories)
- [ ] `ClinicSettings` entity with clinic-level AI toggles, timezone, operating hours
- [ ] Settings page in frontend (per-user section + admin-only clinic section)
- [ ] Drug interaction alerts are not affected by any preference toggle
- [ ] Default values applied correctly on user/clinic creation
- [ ] RBAC enforced: only ADMIN can change clinic-level settings

### 2.7 Data Model (High Level)

```
UserPreference (ClinicId, UserId)
  - EmailAppointmentReminders: bool (default: true)
  - EmailInvoiceSent: bool (default: true)
  - EmailNewMessage: bool (default: true)
  - Language: string (default: "en")

ClinicSettings (ClinicId) -- extends existing Clinic entity or separate table
  - Timezone: string (default: "Asia/Dubai")
  - OperatingHoursStart: TimeOnly (default: 08:00)
  - OperatingHoursEnd: TimeOnly (default: 18:00)
  - WorkDays: string[] (default: ["Sun","Mon","Tue","Wed","Thu"])
  - RamadanHoursStart: TimeOnly? (nullable)
  - RamadanHoursEnd: TimeOnly? (nullable)
  - AiTriageEnabled: bool (default: true)
  - AiNoShowEnabled: bool (default: true)
  - AiMessagingEnabled: bool (default: true)
  - AnalyticsEnabled: bool (default: true)
```

---

## 3. Multi-Market Readiness (UAE -> EU -> US)

### Priority: LOW (for now) -- revisit at +12 months post-launch

### 3.1 Next Market After UAE

**Recommendation: Saudi Arabia (KSA), then wider GCC, then EU (UK first).**

Rationale:
1. **KSA shares the same language (Arabic), same timezone region, same cultural context** (camels, falcons, exotic pets). The product is 90% ready.
2. **GCC markets (Kuwait, Bahrain, Qatar, Oman)** are small but low-effort to enter from UAE.
3. **UK** is the logical EU entry point: English-speaking, strong vet market, RCVS regulations are well-documented.
4. **US** is last: extremely competitive market (Vetsource, ezyVet, Shepherd), complex regulatory landscape (50 states), different insurance model. Not worth the effort before product-market fit is proven in GCC+UK.

**Do NOT pursue EU continental (France, Germany) before UK.** The localization effort (language + regulations + tax systems) is too high for the expected return.

### 3.2 UAE-Specific vs Universal Features

| Feature | UAE-Specific? | Adaptation needed for other markets |
|---------|--------------|--------------------------------------|
| AED currency | Yes | Add currency per clinic (already a string field) |
| 5% VAT | Yes | Tax rate per clinic/country. UK = 20%, KSA = 15%, US = state-level sales tax |
| Asia/Dubai timezone | Yes | Already configurable per clinic via ClinicSettings |
| Sun-Thu work week | Yes (and KSA) | Already configurable per clinic |
| Ramadan hours | Yes (and GCC) | Already implemented as optional |
| Camel/Falcon species | Yes (and GCC) | See section 3.4 |
| Arabic language | Yes (and GCC) | Already implemented (EN+AR) |
| Microchip ISO 11784 | Universal | Same standard worldwide |
| Drug catalog | Partially UAE | Drug names/formulations vary by country. Need per-country catalogs. |
| RBAC model | Universal | Same roles apply everywhere |
| Multi-tenant isolation | Universal | Same architecture |
| AI triage | Universal | Prompts need localization (language, common species per market) |
| WhatsApp integration | UAE/GCC priority | Less critical in EU/US (email/SMS preferred) |

### 3.3 Pricing by Market

**Yes, pricing must vary by market.** UAE clinics are high-revenue (average clinic revenue 2-5M AED/year). European clinics are more price-sensitive. US clinics expect bundled pricing with integrations.

Recommendation:
- **UAE/GCC**: Premium tier. AED pricing. Annual billing preferred (common in UAE B2B).
- **UK/EU**: Mid tier. GBP/EUR. Monthly billing. Free trial 30 days.
- **US**: Competitive tier. USD. Monthly billing. Integration with existing tools expected.

**Do not implement multi-currency billing in the product itself yet.** Use Stripe with multi-currency support externally. The invoice module (AED, 5% VAT) is for the clinic's patients, not for Vetolib's own billing to clinics.

### 3.4 Species by Market

**Camel is relevant for UAE, KSA, and wider GCC. Not for EU/US.**

Current species handling: free text field on Patient entity. This is actually fine for now. The species field is not an enum and does not constrain input.

**Recommendation: species catalog, not species restriction.**

When we build the species catalog (for AI triage accuracy and reporting), it should be structured as:

```
SpeciesCatalog (global, no ClinicId)
  - Id
  - CommonName: string (e.g., "Camel")
  - ScientificName: string (e.g., "Camelus dromedarius")
  - AvailableInMarkets: string[] (e.g., ["UAE", "KSA", "QA", "KW", "BH", "OM"])
  - IsDefault: bool (true for Dog, Cat, Horse -- shown in all markets)
```

This way:
- UAE clinics see: Dog, Cat, Horse, Camel, Falcon, Exotic
- UK clinics see: Dog, Cat, Horse, Rabbit, Exotic
- The vet can always type a custom species (free text fallback)
- AI triage prompts adapt based on market-relevant species

**Do NOT implement this catalog now.** The free text field works. Build the catalog when we expand to KSA (first market expansion).

### 3.5 Owner Expectations by Market

| Aspect | UAE owners | EU/UK owners | US owners |
|--------|-----------|-------------|-----------|
| Communication channel | WhatsApp (primary), email | Email, SMS | Email, text, patient portal |
| Payment | Cash + card, no insurance culture | Card, growing pet insurance | Insurance-dominant (claims integration expected) |
| Language | Arabic + English (bilingual) | Local language + English | English |
| Appointment booking | Phone call or WhatsApp | Online booking expected | Online booking + mobile app expected |
| Medical records access | Low expectation (trust the vet) | Growing expectation (GDPR awareness) | High expectation (patient portal standard) |
| Price sensitivity | Low (affluent pet owners in Dubai) | Medium | High (comparison shopping) |

**Key insight for product**: UAE owners are the easiest to serve (low digital expectations, high willingness to pay). UK/EU owners expect more digital self-service. US owners expect insurance integration, which is a major feature development.

### 3.6 User Stories (Market Expansion)

```
US-MARKET-1: As a Vetolib product team member, I want to configure tax rate and currency
  per clinic so that we can onboard clinics in KSA (15% VAT, SAR) without code changes.

US-MARKET-2: As a KSA clinic admin, I want the species selector to show Camel and Falcon
  by default so that I do not have to type them manually every time.

US-MARKET-3: As a UK clinic admin, I want to configure Mon-Fri work week and GBP currency
  so that the system reflects British veterinary practice.

US-MARKET-4: As a Vetolib product owner, I want per-country drug catalogs so that
  prescriptions reference locally approved medications.
```

### 3.7 Acceptance Criteria (Market Expansion Phase 1: KSA)

- [ ] Currency configurable per clinic (AED, SAR to start)
- [ ] Tax rate configurable per clinic (5%, 15% to start)
- [ ] Species catalog with market-based defaults (optional, can defer)
- [ ] Drug catalog supports per-country entries (or at minimum, GCC-wide catalog)
- [ ] Arabic RTL confirmed working (already implemented)
- [ ] No hardcoded "AED" or "5%" in frontend or backend

### 3.8 What NOT to Build Now

- Multi-currency Vetolib subscription billing (use Stripe)
- US insurance claims integration (years away)
- Per-country regulatory compliance engine (handle manually per market)
- Species catalog (free text works fine)
- Per-country drug catalogs (UAE catalog covers GCC for now)

---

## Summary of Priorities

| Subject | Priority | First Action | Timeline |
|---------|----------|-------------|----------|
| Analytics (PostHog) | HIGH | Integrate server-side SDK, track 10 key events | Next sprint |
| Preference Management | MEDIUM | Build ClinicSettings + UserPreference entities | Sprint +2 |
| Multi-Market (KSA) | LOW | Parameterize currency + tax rate | +6 months |
| Multi-Market (UK) | LOW | Add GBP, Mon-Fri defaults, English drug catalog | +12 months |
| Multi-Market (US) | VERY LOW | Insurance integration study | +18 months minimum |

---

## Cross-References

- AI features pipeline: `docs/AI-FEATURES-SPEC.md`
- RBAC matrix: `docs/RBAC-MATRIX.md`
- Multi-tenant study: `docs/MULTITENANT-STUDY.md`
- Messaging spec: `docs/MESSAGING-SPEC.md`
- WhatsApp decision: `questions/whatsapp-integration-001.md`
- Drug catalog placement: `questions/drug-catalog-placement-001.md`
