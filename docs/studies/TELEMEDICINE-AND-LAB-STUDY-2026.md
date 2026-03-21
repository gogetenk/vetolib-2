# Product Research: Telemedicine & Lab Integration

> Date: 2026-03-21
> Author: Product Research Agent
> Status: DRAFT -- pending PO review

---

## Table of Contents

1. [Part 1 -- Veterinary Telemedicine](#part-1--veterinary-telemedicine)
2. [Part 2 -- Lab Integration (IDEXX / Antech)](#part-2--lab-integration-idexx--antech)
3. [Comparative Priority Matrix](#comparative-priority-matrix)
4. [Sources](#sources)

---

# Part 1 -- Veterinary Telemedicine

## 1.1 Market Overview

| Metric | Value |
|---|---|
| Global market size (2025) | USD 6.48 billion |
| Global market size (2026) | USD 7.74 billion |
| Projected (2032) | USD 24.35 billion |
| CAGR (2026-2034) | 18.12% |
| Growth driver | Remote consultations, AI diagnostics, IoT health monitoring |

The veterinary telemedicine market is one of the fastest-growing segments in pet health. Post-COVID adoption stuck, and the trend accelerated with wearable pet devices and AI triage tools.

### Key competitors offering telemedicine

| Platform | Model | Telemedicine Approach |
|---|---|---|
| **Digitail** | All-in-one cloud PIMS | Built-in video telemedicine + 2-way messaging + AI SOAP notes |
| **PetDesk** | Patient journey platform | Telehealth bridge, 2-way texting, 2,500+ practices |
| **Vetster** | Marketplace | Pay-per-visit ($50+), 24/7, US/CA/UK, no subscription required |
| **ezyVet** | Cloud PIMS | Telemedicine via partner integrations |

**Differentiation opportunity for Vetolib:** None of these competitors targets the UAE/MENA market specifically. Vetolib can be first-to-market with a telemedicine solution adapted to UAE regulations, Arabic language support, and Ramadan-aware scheduling.

## 1.2 Regulatory Landscape

### UAE (Primary Market)

- The UAE Federal Law on Practicing the Veterinary Medicine Profession (2023) mandates licensing for all veterinary practices.
- Telehealth adoption in UAE has increased 30% in companion animal care.
- Human telemedicine is regulated by MOHAP (federal), DHA (Dubai), and DOH (Abu Dhabi). Veterinary telemedicine has **no specific prohibitive regulation** as of 2026 -- it falls under general veterinary practice licensing.
- **Prescription rules**: No explicit prohibition on remote prescription in UAE veterinary context. However, controlled substances likely require in-person VCPR (Veterinary-Client-Patient Relationship).
- **Recommendation**: Consult with MOHAP and relevant emirate authority before launch. Start with triage/follow-up consultations (lower regulatory risk), not initial diagnosis.

### US/International Reference

- Only 8 US states allow establishing a VCPR purely via telemedicine (AZ, CA, DC, FL, ID, NJ, VA, VT).
- AVMA position: in-person exam required before telemedicine, except for life-threatening emergencies.
- Trend: 4 additional states (MA, MI, NH, RI) have introduced E-VCPR legislation in 2026.

### Practical Implication for Vetolib

| Consultation Type | Regulatory Risk | MVP Scope |
|---|---|---|
| Follow-up after in-person visit | LOW | YES -- include in MVP |
| Triage / advice (non-prescribing) | LOW | YES -- include in MVP |
| Initial diagnosis + prescription | HIGH | NO -- Phase 2, per-country rules |
| Emergency guidance | LOW (most jurisdictions allow) | YES -- include in MVP |

## 1.3 Technical Implementation

### Video Technology Comparison

| Provider | Pricing | Free Tier | HIPAA/Privacy | Embed Support | Recommendation |
|---|---|---|---|---|---|
| **Twilio Video** | $0.004/participant/min | None | Yes (BAA available) | SDK (React, iOS, Android) | Best for full control |
| **Daily.co** | $0.004/participant/min | 10,000 min/month | Yes (BAA available) | iframe + SDK | Best for speed-to-market |
| **Whereby Embedded** | $0.004/min (after 2,000 min) | 2,000 min/month | Yes (HIPAA compliant) | iframe + API | Simplest integration |
| **Self-hosted WebRTC** | Infra cost only | N/A | Self-managed | Full custom | Most complex, cheapest at scale |

**Recommendation: Daily.co** for MVP.
- 10,000 free minutes/month covers ~330 consultations of 15 min each (2 participants).
- Same pricing as Twilio but simpler integration (iframe embed or prebuilt UI).
- HIPAA-grade privacy, recording support, and webhooks for call events.
- At scale (1,000 consultations/month): ~$120/month.

### Cost Projection

| Scale | Monthly Consultations | Avg Duration | Monthly Cost (Daily.co) |
|---|---|---|---|
| Early (1 clinic) | 50 | 15 min | FREE (within 10k tier) |
| Growth (10 clinics) | 500 | 15 min | ~$40 |
| Scale (50 clinics) | 2,500 | 15 min | ~$250 |
| Enterprise (200 clinics) | 10,000 | 15 min | ~$1,000 |

### User Flow

```
OWNER SIDE                              VET SIDE
    |                                       |
    | 1. Request teleconsultation           |
    |   (via portal or app)                 |
    |-------------------------------------->|
    |                                       |
    |       2. Vet reviews request          |
    |          (sees patient history)       |
    |                                       |
    |       3. Vet accepts + picks slot     |
    |<--------------------------------------|
    |                                       |
    | 4. Owner receives confirmation        |
    |    + link to waiting room             |
    |                                       |
    |============= SCHEDULED TIME ==========|
    |                                       |
    | 5. Owner joins waiting room           |
    |-------------------------------------->|
    |                                       |
    |       6. Vet joins call               |
    |       (sees patient record sidebar)   |
    |<--------------------------------------|
    |                                       |
    |======== VIDEO CALL IN PROGRESS =======|
    |                                       |
    |  - Owner shows pet on camera          |
    |  - Vet takes notes (SOAP format)      |
    |  - Vet can request photo upload       |
    |  - Screen share for lab results       |
    |                                       |
    |============ CALL ENDS ================|
    |                                       |
    |       7. Vet completes SOAP notes     |
    |       8. Vet creates follow-up        |
    |          or prescription (if allowed) |
    |       9. Invoice generated            |
    |                                       |
    |      10. Owner receives summary       |
    |          + invoice via email/portal    |
    |<--------------------------------------|
```

### Architecture Integration

```
Vetolib Modules Impacted:
==========================

[Agenda Module]
  |-- New ConsultationType: "Telehealth"
  |-- New fields: videoRoomUrl, videoProvider, callDuration
  |-- Scheduling rules: no physical room needed
  |-- Telehealth slots can overlap with in-person slots
  |     (vet does telehealth between physical appointments)

[MedicalRecords Module]
  |-- SOAP notes linked to telehealth consultation
  |-- Attachments: photos/videos uploaded during call
  |-- Flag: "Remote consultation" on record

[Billing Module]
  |-- New service type: "Teleconsultation"
  |-- Different pricing tier (typically 60-70% of in-person)
  |-- Auto-invoice on call completion

[Auth Module]
  |-- No changes (owner + vet already have roles)

[Notifications Module]
  |-- Reminder before teleconsultation (15 min, 1 hour)
  |-- "Vet is ready" push notification
  |-- Post-call summary email

[NEW: Telehealth Infrastructure]
  |-- Daily.co room management (create/destroy rooms)
  |-- Webhook handler for call events (started, ended, duration)
  |-- Recording storage (optional, consent-based)
  |-- Call quality metrics logging
```

### Backend Design (within Agenda module)

```
Agenda.Contracts/
  |-- Enums/
  |     |-- ConsultationType.cs  --> add "Telehealth" value
  |-- DTOs/
  |     |-- TelehealthSessionDto.cs
  |-- Events/
        |-- TelehealthCallStarted.cs
        |-- TelehealthCallEnded.cs

Agenda/
  |-- Entities/
  |     |-- TelehealthSession.cs  (VideoRoomId, StartedAt, EndedAt, Duration, RecordingUrl)
  |-- Handlers/
  |     |-- CreateTelehealthAppointmentHandler.cs
  |     |-- JoinTelehealthCallHandler.cs
  |     |-- EndTelehealthCallHandler.cs
  |-- Infrastructure/
        |-- DailyCoVideoService.cs  (implements IVideoRoomProvider from Contracts)
        |-- DailyCoWebhookHandler.cs
```

### Frontend Components

```
src/app/[locale]/(clinic)/agenda/
  |-- telehealth/
       |-- page.tsx                    -- Telehealth dashboard
       |-- [appointmentId]/
            |-- waiting-room.tsx       -- Pre-call screen (test camera/mic)
            |-- video-call.tsx         -- Daily.co embedded iframe + patient sidebar
            |-- post-call-summary.tsx  -- SOAP notes + invoice

src/app/[locale]/(portal)/
  |-- telehealth/
       |-- request.tsx                 -- Owner requests teleconsultation
       |-- waiting-room.tsx            -- Owner waiting room
       |-- video-call.tsx              -- Owner video view
```

## 1.4 Effort Estimation

| Work Package | Estimated Effort | Dependencies |
|---|---|---|
| Agenda: ConsultationType "Telehealth" + scheduling | 3 days | None |
| Daily.co integration service | 2 days | Daily.co account |
| Video call UI (vet side) | 3 days | Daily.co SDK |
| Video call UI (owner side) | 2 days | Daily.co SDK |
| Waiting room + call quality check | 1 day | None |
| SOAP notes integration post-call | 1 day | MedicalRecords module |
| Billing: teleconsultation service type | 1 day | Billing module |
| Notifications: reminders + post-call | 1 day | Notifications module |
| Webhooks for call events | 1 day | Daily.co account |
| BDD features + tests | 3 days | All above |
| **Total** | **~18 dev-days** | |

**Recommended team**: 2 developers in parallel = ~2 weeks calendar time.

---

# Part 2 -- Lab Integration (IDEXX / Antech)

## 2.1 How Veterinary Labs Work

### Industry Structure

| Lab Provider | Market Position | Coverage | Key Product |
|---|---|---|---|
| **IDEXX Laboratories** | #1 globally | Worldwide | VetConnect PLUS |
| **Antech Diagnostics** (Mars) | #2 in North America | 70+ labs, US/CA | HealthTracks |
| **Heska** (now part of Mars) | Mid-market | US | Element analyzers |
| **Zoetis (Reference Labs)** | Growing | Global | VETSCAN |

### Two Types of Lab Work

1. **In-house analyzers**: Blood chemistry, CBC, urinalysis done at the clinic. Results in minutes. Machines connect to IDEXX VetLab Station or equivalent.

2. **Reference labs**: Samples shipped to external lab. Results in 24-72 hours. Requires order submission + result retrieval via API.

### Data Exchange Standards

| Standard | Usage in Vet | Notes |
|---|---|---|
| **HL7 v2 (ORM/ORU)** | Common for in-house analyzers | ORM = order message, ORU = result message |
| **VetXML** | UK consortium standard | XML schemas for PIMS <-> service providers |
| **FHIR** | Emerging, not yet standard in vet | FHIR spec explicitly supports veterinary use |
| **Proprietary REST APIs** | IDEXX VetConnect PLUS, Antech HealthTracks | Most common for reference lab integration |
| **CSV/PDF** | Legacy clinics | Manual import, error-prone (73% discrepancy rate) |

**Key finding**: Unlike human healthcare (where HL7/FHIR are mandated), veterinary software has **no universally adopted data standard**. Integration requires custom mapping per lab provider.

## 2.2 IDEXX VetConnect PLUS Integration

### API Access

- IDEXX provides a **developer portal** at `developer.vetconnectplus.com`.
- IDEXX Data Services offers **OData V4 compliant APIs** for accessing practice data.
- Integration requires a formal partnership via the **IDEXX Practice Management Software Integration Request Form**.
- IDEXX provides an API for both in-house (IVLS) and reference lab results.

### Integration Capabilities

| Feature | Available | Protocol |
|---|---|---|
| Submit test orders | Yes | REST API |
| Receive results automatically | Yes | Webhooks / polling |
| View historical results | Yes | REST API (OData V4) |
| Result trending (graphs) | Yes (via VetConnect PLUS UI) | Embedded widget |
| In-house analyzer sync | Yes (via IDEXX VetLab Station) | HL7 / proprietary |
| Patient matching | Yes | API field mapping |

### Partnership Requirements

- **Formal application** required (integration request form).
- **Technical review** by IDEXX engineering team.
- **Certification process** before going live.
- **Timeline**: Typically 3-6 months from application to certified integration.
- **Cost**: No public pricing for API access; typically included in lab service contracts.

## 2.3 Antech Diagnostics Integration

### Integration Capabilities

| Feature | Available | Protocol |
|---|---|---|
| Submit test orders | Yes | REST API |
| Receive results automatically | Yes | Push to PIMS |
| Access via HealthTracks | Yes | Web portal + API |
| Result trending | Yes (HealthTracks) | Web UI |
| Integration partners | NectarVet, Vetspire, DaySmart Vet, ezyVet | REST API |

### Partnership Requirements

- Similar to IDEXX: formal partnership application.
- Antech is owned by **Mars Veterinary Health**, which also owns Heska and a large network of clinics.
- Integration typically handled through Antech's software partnership team.

## 2.4 Technical Implementation for Vetolib

### User Flow

```
VET WORKFLOW                                    LAB SIDE
    |                                               |
    | 1. Vet opens patient record                   |
    |    selects "Order Lab Test"                    |
    |                                               |
    | 2. Vet picks test panel                       |
    |    (CBC, Chemistry, Thyroid, etc.)             |
    |                                               |
    | 3. System creates LabOrder                    |
    |    status = "Pending"                         |
    |                                               |
    | 4. [IN-HOUSE] Vet runs sample on analyzer     |
    |    OR                                         |
    |    [REFERENCE] System sends order via API ---->|
    |                                               |
    |    [REFERENCE] Sample shipped to lab           |
    |                                               |
    |              ... 24-72 hours ...               |
    |                                               |
    |    5. Lab processes sample                    |
    |       Results available                       |
    |                                               |
    | 6. Results received via webhook/poll <---------|
    |    LabOrder status = "ResultsReceived"         |
    |                                               |
    | 7. System parses results into                 |
    |    structured LabResult entities              |
    |    (analyte, value, unit, ref range, flag)    |
    |                                               |
    | 8. Vet notified: "Lab results ready           |
    |    for [Patient Name]"                        |
    |                                               |
    | 9. Vet reviews results in patient record      |
    |    - Values outside range highlighted         |
    |    - Historical trending graph                |
    |    - Vet adds interpretation notes            |
    |                                               |
    | 10. Optional: share results with owner        |
    |     via portal (PDF or structured view)       |
```

### Data Model (MedicalRecords module)

```
NEW ENTITIES:

LabOrder
  |-- Id (Guid)
  |-- ClinicId (Guid)              -- multi-tenant
  |-- PatientId (Guid)             -- FK to Patient
  |-- MedicalRecordId (Guid?)      -- FK to MedicalRecord (optional until linked)
  |-- VeterinarianId (Guid)        -- who ordered
  |-- LabProvider (enum: IDEXX, Antech, InHouse, Other)
  |-- ExternalOrderId (string?)    -- ID from lab provider
  |-- Status (enum: Draft, Submitted, SampleReceived, Processing,
  |                  ResultsReceived, Reviewed, Cancelled)
  |-- OrderedAt (DateTimeOffset)
  |-- ResultsReceivedAt (DateTimeOffset?)
  |-- Notes (string?)

LabTestPanel
  |-- Id (Guid)
  |-- LabOrderId (Guid)            -- FK to LabOrder
  |-- PanelCode (string)           -- e.g., "CBC", "CHEM17", "T4"
  |-- PanelName (string)
  |-- ExternalPanelId (string?)    -- lab provider's code

LabResult
  |-- Id (Guid)
  |-- LabOrderId (Guid)            -- FK to LabOrder
  |-- LabTestPanelId (Guid)        -- FK to LabTestPanel
  |-- AnalyteName (string)         -- e.g., "ALT", "BUN", "WBC"
  |-- Value (decimal)
  |-- Unit (string)                -- e.g., "U/L", "mg/dL", "x10^3/uL"
  |-- ReferenceRangeLow (decimal?)
  |-- ReferenceRangeHigh (decimal?)
  |-- Flag (enum: Normal, Low, High, Critical)
  |-- Species (string)             -- ref ranges vary by species
  |-- RawData (string?)            -- original payload from lab (JSON)
```

### Architecture

```
MedicalRecords.Contracts/
  |-- Enums/
  |     |-- LabProvider.cs
  |     |-- LabOrderStatus.cs
  |     |-- LabResultFlag.cs
  |-- DTOs/
  |     |-- LabOrderDto.cs
  |     |-- LabResultDto.cs
  |     |-- LabTestPanelDto.cs
  |-- Events/
  |     |-- LabResultsReceived.cs      -- domain event, triggers notification
  |-- Interfaces/
        |-- ILabIntegrationService.cs  -- abstraction for lab providers

MedicalRecords/
  |-- Entities/
  |     |-- LabOrder.cs
  |     |-- LabTestPanel.cs
  |     |-- LabResult.cs
  |-- Handlers/
  |     |-- CreateLabOrderHandler.cs
  |     |-- SubmitLabOrderHandler.cs
  |     |-- ReceiveLabResultsHandler.cs
  |     |-- GetLabResultsForPatientHandler.cs
  |-- Infrastructure/
  |     |-- IdexxLabService.cs         -- implements ILabIntegrationService
  |     |-- AntechLabService.cs        -- implements ILabIntegrationService
  |     |-- LabResultParserService.cs  -- normalizes different formats
  |-- Endpoints/
        |-- LabOrderEndpoints.cs
        |-- LabResultEndpoints.cs
```

### Frontend Components

```
src/app/[locale]/(clinic)/patients/[patientId]/
  |-- lab/
       |-- page.tsx                -- Lab orders list for patient
       |-- order/
       |    |-- page.tsx           -- Create new lab order (select tests)
       |-- [orderId]/
            |-- page.tsx           -- View results with trending
            |-- components/
                 |-- result-table.tsx       -- Analytes with flags
                 |-- trending-chart.tsx     -- Historical values graph
                 |-- reference-range-bar.tsx -- Visual range indicator
```

## 2.5 Prerequisites and Partnership Timeline

| Step | Timeline | Blocker? |
|---|---|---|
| Apply to IDEXX partner program | Week 1 | No |
| Apply to Antech partner program | Week 1 | No |
| Receive API credentials (IDEXX) | 4-8 weeks | YES -- external dependency |
| Receive API credentials (Antech) | 4-8 weeks | YES -- external dependency |
| Build abstraction layer (ILabIntegrationService) | 1 week | No |
| Build "manual results entry" (no API needed) | 1 week | No |
| IDEXX integration implementation | 2 weeks | Needs API credentials |
| Antech integration implementation | 2 weeks | Needs API credentials |
| Certification by lab providers | 2-4 weeks | YES -- external review |
| **Total (critical path)** | **~3-4 months** | |

**Key insight**: The partnership application process is the bottleneck. Development work can start immediately with manual entry + mock lab responses, then plug in real APIs when credentials arrive.

## 2.6 Effort Estimation

| Work Package | Estimated Effort | Dependencies |
|---|---|---|
| Data model: LabOrder, LabTestPanel, LabResult | 2 days | None |
| Manual lab entry UI (no API needed) | 3 days | Data model |
| Lab order creation flow | 2 days | Data model |
| Results display with flags + trending | 3 days | Data model |
| ILabIntegrationService abstraction | 1 day | None |
| IDEXX VetConnect PLUS adapter | 3 days | API credentials |
| Antech HealthTracks adapter | 3 days | API credentials |
| Result parser / normalizer | 2 days | Lab adapters |
| Webhook endpoint for results push | 1 day | Lab adapters |
| Notifications on results received | 1 day | Notifications module |
| Owner portal: view lab results | 2 days | Results display |
| BDD features + tests | 3 days | All above |
| **Total** | **~26 dev-days** | |

**Note**: The 26 dev-days can be split into two phases:
- **Phase A (no lab partnership needed)**: Manual entry + UI = ~12 days
- **Phase B (after API credentials)**: Lab integrations = ~14 days

---

# Comparative Priority Matrix

| Criteria | Telemedicine | Lab Integration |
|---|---|---|
| **Market demand** | High (18% CAGR) | Very High (essential for clinical workflow) |
| **Revenue impact** | Medium (new revenue stream) | High (reduces manual entry, speeds diagnosis) |
| **Competitive moat** | High in UAE (no competitor) | Medium (IDEXX integrates with everyone) |
| **Dev effort** | 18 days | 26 days (12 without API) |
| **External dependencies** | Daily.co account (instant) | Lab partnerships (3-4 months) |
| **Regulatory risk** | Medium (UAE TBD) | Low |
| **Time to MVP** | ~2 weeks | Phase A: 2.5 weeks, Phase B: +3 months |
| **User retention impact** | Medium | Very High (daily use by vets) |

### Recommendation

**Start BOTH in parallel with a phased approach:**

1. **Immediate (Sprint 1-2)**: Telemedicine MVP -- Daily.co integration, basic video call flow. Ship in 2 weeks.

2. **Immediate (Sprint 1-2)**: Lab Integration Phase A -- manual lab entry, result display, trending charts. No external dependency.

3. **Background (Month 1)**: Submit IDEXX and Antech partnership applications.

4. **Sprint 3-4**: Lab Integration Phase B -- plug in real lab APIs as credentials arrive.

This approach delivers user-visible value in 2 weeks (telemedicine + manual lab entry) while the lab partnerships mature in the background.

---

# Sources

### Telemedicine Market
- [Veterinary Telemedicine Market - Fortune Business Insights](https://www.fortunebusinessinsights.com/veterinary-telemedicine-market-112176)
- [Veterinary Telemedicine Market - Markets and Markets](https://www.marketsandmarkets.com/Market-Reports/veterinary-telemedicine-market-84791668.html)
- [Veterinary Telehealth Market - Grand View Research](https://www.grandviewresearch.com/industry-analysis/veterinary-telehealth-market)
- [Veterinary Telemedicine Market - Future Market Insights](https://www.futuremarketinsights.com/reports/veterinary-telemedicine-market)

### Regulations
- [What States Allow Online Vet Prescriptions - Catster](https://www.catster.com/ask-the-vet/what-states-allow-online-vet-prescriptions/)
- [AVMA Telemedicine Policy Update](https://www.avma.org/news/avma-board-updates-telemedicine-guardianship-policies)
- [State Veterinary Telehealth Laws - AAHA](https://www.aaha.org/newstat/publications/the-patchwork-quilt-of-state-veterinary-telehealth-laws/)
- [VCPR by State - Otto](https://otto.vet/vcpr/)
- [UAE Federal Law on Veterinary Medicine](https://uaelegislation.gov.ae/en/legislations/1093)
- [UAE Telehealth Regulations - Muhami](https://muhami.ae/articles/how-is-telehealth-regulated-in-the-uae/)
- [Telemedicine Movement 2026 - APPA](https://americanpetproducts.org/blog/the-future-of-veterinary-care-telemedicine-movement-in-2026)

### Competitors
- [Digitail](https://digitail.com/)
- [PetDesk](https://petdesk.com/)
- [Vetster](https://www.vetster.com/)

### Video Providers
- [Twilio Video Pricing](https://www.twilio.com/en-us/video/pricing)
- [Daily.co Pricing](https://www.daily.co/pricing/)
- [Whereby Embedded Pricing](https://whereby.com/information/embedded/pricing)
- [Whereby Telehealth](https://whereby.com/information/embedded/healthcare)

### Lab Integration
- [IDEXX VetConnect PLUS](https://www.idexx.com/en/veterinary/software-services/vetconnect-plus/)
- [IDEXX Integration Request Form](https://www.idexx.com/en/veterinary/software-services/idexx-practice-management-software-integration-request-form/)
- [IDEXX Data Services API](https://io.datapointapi.com/documentation)
- [IDEXX Developer Portal](https://developer.vetconnectplus.com/)
- [Antech Diagnostics](https://www.antechdiagnostics.com/)
- [Antech HealthTracks](https://www.antechdiagnostics.com/reference-lab/healthtracks/)
- [Antech + ezyVet Integration](https://www.ezyvet.com/integration/antech-diagnostics)
- [Veterinary Data Interoperability Guide - Puppilot](https://www.puppilot.co/blog/veterinary-data-interoperability-the-complete-guide-to-connecting-pims-labs-insurers)

### Data Standards
- [HL7 FHIR Overview](https://www.hl7.org/fhir/overview.html)
- [HL7 ORU Message - InterfaceWare](https://www.interfaceware.com/hl7-oru)
