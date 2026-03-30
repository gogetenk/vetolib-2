# AI Features Expansion Study -- Vetolib UAE Market

**Date:** 2026-03-30
**Author:** Technical Research Agent
**Status:** Study -- awaiting PO validation
**Scope:** New AI features targeting UAE veterinary clinics

---

## Table of Contents

1. [Current State Summary](#1-current-state-summary)
2. [Proposed Feature 1: Falcon Health Monitoring](#2-falcon-health-monitoring)
3. [Proposed Feature 2: Breeding Outcome Prediction](#3-breeding-outcome-prediction)
4. [Proposed Feature 3: Smart Appointment Scheduling](#4-smart-appointment-scheduling)
5. [Proposed Feature 4: Drug Interaction Checking Improvements](#5-drug-interaction-checking-improvements)
6. [Proposed Feature 5: Automated Medical Record Summarization](#6-automated-medical-record-summarization)
7. [Proposed Feature 6: Client Communication AI](#7-client-communication-ai)
8. [Priority Matrix](#8-priority-matrix)
9. [Implementation Roadmap](#9-implementation-roadmap)

---

## 1. Current State Summary

### Existing AI Module (`Vetolib.AI`)

The AI module is already scaffolded and operational with the following features:

| Feature | Status | Technology |
|---|---|---|
| AI Triage (symptom classification) | Built | LLM via `Microsoft.Extensions.AI` (IChatClient) |
| No-Show Prediction | Built | ML.NET (logistic regression via PredictionEnginePool) |
| SOAP Notes Generation | Built | LLM (Claude via ClaudeSoapNotesGenerator + template fallback) |
| Drug Interaction Checking | Built | Rule-based (species contraindications, drug-drug, dosage range) |
| Message Triage | Contract defined | LLM (IMessageTriageService interface exists) |
| Predictive Health Alerts | Built | Rule-based engine (11 rules, background job, 6 endpoints) |

### Existing Breeding Module (`Vetolib.Breeding`)

A full breeding module was recently built with:

- **HeatCycle** tracking with next-heat prediction (rule-based, species-specific cycle lengths)
- **Pregnancy** management with checks, delivery recording, loss tracking
- **Litter** management with offspring tracking
- **Lineage/Pedigree** (SetLineage, GetPedigree, GetDescendants)
- **GestationPeriods** static data (species-specific gestation windows)

### Species Support

The `Species` enum currently covers: `Dog`, `Cat`, `Bird`, `Rabbit`, `Horse`, `Exotic`, `Camel`, `Falcon`, `Reptile`. Falcon and Reptile were added recently (commit 8df45b4).

### Relevant Market Context (from MARKET-RESEARCH-UAE-2026.md)

- Abu Dhabi Falcon Hospital: 11,000 falcons/year -- world's largest
- Dubai Camel Hospital: $11M facility, 65 staff
- Zero competitors offer falcon/camel-specific AI features
- WhatsApp is the dominant messaging platform in UAE
- 2M+ pets in UAE, cats outnumber dogs 2:1
- No VPMS has Arabic AI features

---

## 2. Falcon Health Monitoring

### Description

Species-specific health alert rules for avian patients (falcons, parrots, raptors), extending the existing Predictive Health Alerts engine. Falcons have unique health patterns not covered by the current 11 rules (which focus on dogs and cats).

### Business Value

- **Abu Dhabi Falcon Hospital alone handles 11,000 cases/year.** This single clinic is a potential anchor customer worth premium pricing.
- Falconry is a core cultural practice in the UAE (UNESCO Intangible Cultural Heritage). Falcon owners are typically high-net-worth individuals willing to pay for premium care.
- **Zero competitors** offer falcon-specific health monitoring. First-mover advantage is absolute.
- Positions Vetolib as the only VPMS that understands avian medicine in the Gulf region.

### Technical Feasibility

**What already exists:**
- `Species.Falcon` is in the enum
- `IHealthAlertRule` interface and rules engine are built and extensible
- `PatientAlertContext` already carries Species, Breed, WeightKg, medical history
- `BreedRiskData` has a `SeniorAgeThresholdYears` entry for `Bird` (8 years) but nothing falcon-specific
- Background job (`HealthAlertGeneratorJob`) runs nightly and evaluates all rules

**What needs to be built:**
- 6-8 new `IHealthAlertRule` implementations for falcon-specific conditions:
  - `FALCON-ASPERGILLOSIS-001`: Aspergillosis screening (most common falcon disease, fungal respiratory infection). Trigger: falcon with no respiratory exam in 6+ months.
  - `FALCON-BUMBLEFOOT-001`: Pododermatitis check. Trigger: falcon with previous bumblefoot diagnosis, no follow-up in 3+ months.
  - `FALCON-ENDOSCOPY-001`: Annual endoscopy recommendation. Trigger: falcon with no endoscopy in 12+ months.
  - `FALCON-FEATHER-001`: Feather condition assessment. Trigger: imping records older than 6 months during hunting season (Oct-Mar in UAE).
  - `FALCON-WEIGHT-001`: Falcon weight monitoring (falconers track weight daily for flight fitness). Trigger: weight deviation >5% from baseline (tighter than the 15% general rule).
  - `FALCON-QUARANTINE-001`: Post-import quarantine reminder. Trigger: new falcon patient with import date within 30 days, no quarantine completion recorded.
  - `FALCON-PARASITE-001`: Trichomonas screening. Trigger: falcon with no fecal exam in 6+ months.
- Extend `BreedRiskData` with falcon breed data (Peregrine, Saker, Gyrfalcon, Shaheen, hybrid breeds)
- Add falcon-specific keyword lists for medical record scanning (endoscopy, aspergillosis, imping, bumblefoot, trichomoniasis, PBFD)

**No architectural changes required.** The existing rules engine was designed to be extended by adding new `IHealthAlertRule` implementations. All rules are auto-discovered via DI.

### Estimated Effort

**3-4 days**

| Task | Days |
|---|---|
| Research falcon veterinary medicine (validate rules with vet sources) | 0.5 |
| Implement 7 falcon-specific rules | 1.5 |
| Extend BreedRiskData with falcon breeds | 0.25 |
| Unit tests for each rule (edge cases, keyword matching) | 0.5 |
| Integration test (falcon patient triggers correct alerts) | 0.25 |

### Priority: **P0**

Rationale: Minimal effort, maximum market differentiation. The Abu Dhabi Falcon Hospital is a potential lighthouse customer. The rules engine is already built -- this is pure content addition.

---

## 3. Breeding Outcome Prediction

### Description

Use heat cycle history, pregnancy records, and lineage data to predict breeding outcomes: optimal mating windows, expected litter sizes, pregnancy complication risk, and genetic diversity warnings.

### Business Value

- Racing camels and purebred falcons are **high-value assets** (racing camels: $1M+, prize falcons: $100K+). Breeders invest heavily in maximizing reproductive success.
- Purebred dog and cat breeding is a growing business in UAE. Breeders want data-driven decisions.
- Breeding prediction is a premium feature that justifies higher-tier pricing ("Falcon/Equine" custom tier from pricing strategy).
- No competitor offers AI-assisted breeding management at all.

### Technical Feasibility

**What already exists:**
- Full breeding module with HeatCycle, Pregnancy, Litter, and Lineage entities
- `PredictNextHeat` query handler already computes next heat date (rule-based)
- `GestationPeriods` static data per species
- Pregnancy history with outcomes (normal delivery, C-section, loss, stillborn)
- Litter data with offspring count and individual offspring records
- Lineage/pedigree tree traversal (GetPedigree up to configurable depth)

**What needs to be built:**

1. **Optimal Mating Window Calculator** (rule-based, no ML):
   - Extend `PredictNextHeat` to output a fertility window (not just date)
   - Factor in cycle regularity (variance across past cycles)
   - Species-specific: dogs (11-15 days post-estrus onset), cats (induced ovulators -- different logic), camels (seasonal breeders Nov-Mar), falcons (spring breeders)
   - New query: `GetOptimalMatingWindow(patientId)` returning date range + confidence

2. **Litter Size Predictor** (ML.NET or rule-based):
   - Input features: species, breed, dam age, dam weight, sire data (if lineage exists), number of previous litters, average previous litter size, mating method (natural vs AI)
   - Rule-based Phase 1: breed average lookup table + adjustment for dam age/history
   - ML Phase 2: train on litter history data once enough clinics provide data (cold start problem)

3. **Pregnancy Complication Risk Score** (rule-based):
   - Input: species, breed, dam age, pregnancy history (previous C-sections, losses, complications)
   - Output: risk score 0-100 with specific risk factors flagged
   - Brachycephalic breeds: auto-flag C-section risk
   - Advanced maternal age by species: flag

4. **Inbreeding Coefficient Calculator** (graph algorithm):
   - Use pedigree tree (already traversable via GetPedigree)
   - Calculate Wright's coefficient of inbreeding (COI) from the pedigree
   - Alert if COI exceeds breed-specific thresholds
   - New query: `GetInbreedingCoefficient(damId, sireId)` for prospective mating analysis

**Architecture:** All features fit in the existing Breeding module as new queries/commands. The inbreeding calculator is a graph algorithm on the existing PatientLineage data. No new module needed. The AI module could later consume breeding data via a new contract (`IBreedingDataReader` in `Breeding.Contracts`) for cross-module health alerts (e.g., "this pregnant patient is high-risk based on breeding history").

### Estimated Effort

**8-10 days**

| Task | Days |
|---|---|
| Optimal mating window (4 species, rule-based) | 2 |
| Litter size predictor (rule-based Phase 1, breed lookup tables) | 1.5 |
| Pregnancy complication risk score | 2 |
| Inbreeding coefficient (COI algorithm + pedigree traversal) | 2 |
| Contract in Breeding.Contracts for cross-module consumption | 0.5 |
| Unit tests (all 4 sub-features) | 1.5 |
| Integration tests | 0.5 |

### Priority: **P1**

Rationale: High value for premium customers (breeders, racing stables, falcon hospitals) but substantial effort. The breeding module is freshly built and needs to stabilize before adding prediction layers. Ship falcon health monitoring first (P0), then this.

---

## 4. Smart Appointment Scheduling

### Description

Enhance the existing scheduling optimization (Feature 2 in AI-FEATURES-SPEC) with no-show-aware slot optimization, dynamic buffer times, and demand forecasting.

### Business Value

- Reduces revenue loss from no-shows (industry average: 10-15% no-show rate).
- Optimizes vet utilization -- UAE clinics report vet time as their most expensive resource.
- Differentiator: no regional competitor (vetPMS, MEDAS, kumoVet) has any scheduling intelligence.
- Ramadan-aware demand forecasting is unique to UAE (clinics see different patterns during Ramadan).

### Technical Feasibility

**What already exists:**
- `SuggestSlot` query with scoring algorithm (5 weighted criteria) in Agenda module
- `DurationEstimator` with rolling average of past 20 appointments
- No-show prediction model (ML.NET) in AI module returning probability per appointment
- `PredictNoShowBatch` command for all appointments on a given date

**What needs to be built:**

1. **No-Show-Aware Overbooking** (extension of SuggestSlot):
   - When a high-risk no-show appointment exists in a slot, suggest it as partially available
   - Calculate overbooking capacity: if P(no-show) > 0.4 for existing appointment, allow one additional booking
   - Risk-balanced: never overbook more than 1 per slot to avoid vet overload
   - Requires: AI module to expose no-show predictions to Agenda module via `AI.Contracts`

2. **Dynamic Buffer Times**:
   - Current: fixed duration per appointment type
   - Proposed: adjust buffer based on appointment type + vet speed history
   - Example: if Dr. Ahmad consistently runs 5 min over on surgery follow-ups, auto-add 5 min buffer
   - Extension of existing `DurationEstimator`

3. **Demand Forecasting** (rule-based + seasonal patterns):
   - Predict busy/quiet days based on: day of week, month, Ramadan, public holidays, historical patterns
   - Output: daily demand heatmap for the next 2 weeks
   - Use case: suggest opening extra slots on predicted busy days, reducing availability on quiet days
   - New query: `ForecastDemand(startDate, endDate)` in Agenda module

4. **Smart Reminders** (integration with no-show prediction):
   - For high-risk no-show appointments: trigger extra reminder 48h before + 2h before
   - For medium-risk: trigger reminder 24h before
   - Requires integration with a future Notifications module (event-based)

**Cross-module communication:** The AI module already has no-show prediction. The Agenda module needs to read predictions via a new contract `INoShowPredictionReader` in `AI.Contracts`. This follows the existing pattern (AI module reads from MedicalRecords.Contracts; Agenda would read from AI.Contracts).

### Estimated Effort

**6-7 days**

| Task | Days |
|---|---|
| No-show-aware overbooking (extend SuggestSlot) | 1.5 |
| Dynamic buffer times (extend DurationEstimator) | 1 |
| Demand forecasting (seasonal patterns, Ramadan calendar) | 2 |
| Smart reminders (event publication for high-risk slots) | 1 |
| Unit + integration tests | 1.5 |

### Priority: **P1**

Rationale: Good ROI for clinics (directly reduces revenue loss), builds on existing infrastructure. But the Agenda module's SuggestSlot must be stabilized first, and the Notifications module needs to exist for smart reminders.

---

## 5. Drug Interaction Checking Improvements

### Description

Extend the existing drug interaction checker with species-specific dosing for UAE exotic species (falcons, camels, reptiles), LLM-assisted interaction detection for drugs not in the catalog, and formulary compliance checking.

### Business Value

- Patient safety is always-on (PO decision: drug interactions are never gated by preferences).
- Falcon and camel drug dosing is specialized and poorly documented in global databases. Vets currently rely on personal experience or textbook lookup.
- Regulatory value: UAE's new Federal Decree-Law on Veterinary Medical Products (2025) requires traceability of drug dispensing. Automated interaction checking supports MOCCAE compliance.
- Liability reduction for clinics.

### Technical Feasibility

**What already exists:**
- `CheckInteractionsHandler` with 4 checks: species contraindications, drug-drug interactions, reverse drug interactions, dosage out-of-range
- `DrugCatalogEntry` with `DosageGuidelines` (per species: min/max dose per kg)
- `SpeciesContraindication` entity with alternative drug suggestions
- The handler uses MediatR to read drug catalog and patient data from MedicalRecords module

**What needs to be built:**

1. **Exotic Species Dosage Guidelines** (data entry, not code):
   - Add dosage guidelines for Falcon, Camel, Reptile species to existing `DosageGuideline` entity
   - Source: published avian/exotic formularies (Carpenter's Exotic Animal Formulary, Samour's Avian Medicine)
   - This is primarily a seed data task, not a code task

2. **LLM-Assisted Interaction Detection** (for uncataloged drugs):
   - When a drug is prescribed that is NOT in the catalog (free-text entry), use LLM to check for known interactions with the patient's active prescriptions
   - New handler: `CheckFreeTextDrugInteractionsCommand`
   - Uses `IChatClient` (already configured in AI module) with a structured prompt
   - Returns interactions with confidence level and source citations
   - Always flagged as "AI-suggested -- verify with pharmacist" (never blocks prescription)

3. **Formulary Compliance Check**:
   - Verify that prescribed drugs are approved for use in UAE (MOCCAE/Emirates Drug Establishment)
   - Rule-based: check drug against an approved formulary list (seeded data)
   - Alert if drug is not on the UAE-approved list (informational, not blocking)

4. **Combination Therapy Alerts**:
   - Detect common dangerous combinations even when individual drugs are safe (e.g., NSAID + corticosteroid in dogs)
   - Rule-based: define combination rules as static data
   - Extend `DetectInteractionsAsync` with a new `CheckCombinationTherapy` method

### Estimated Effort

**5-6 days**

| Task | Days |
|---|---|
| Exotic species dosage data (seed Falcon, Camel, Reptile) | 1 |
| LLM-assisted free-text drug interaction check | 2 |
| Formulary compliance check (UAE approved list) | 1 |
| Combination therapy alerts (rule-based) | 1 |
| Tests | 1 |

### Priority: **P1**

Rationale: Patient safety feature with regulatory compliance value. The exotic species dosage data (especially falcon) directly supports the P0 Falcon Health Monitoring feature. However, the LLM-assisted piece requires careful prompt engineering and validation with veterinary pharmacologists before production use.

---

## 6. Automated Medical Record Summarization

### Description

LLM-powered summarization of a patient's complete medical history into structured, actionable summaries. Three modes: (1) visit summary for client communication, (2) clinical summary for vet handoff, (3) chronological timeline for record review.

### Business Value

- Vets spend 1+ hours/day on documentation (industry pain point #1 per market research).
- Visit summaries sent to owners via WhatsApp increase perceived care quality and justify premium pricing.
- Clinical handoff summaries reduce errors when a different vet sees the patient.
- Directly extends the existing SOAP Notes Generation feature -- uses the same LLM infrastructure.
- Digitail and Covetrus already offer AI summaries -- Vetolib must match to compete.

### Technical Feasibility

**What already exists:**
- `ISoapNotesGenerator` with `GenerateAsync` (Claude-based + template fallback)
- `IConversationSummaryService` contract (already defined in AI.Contracts)
- `IChatClient` pipeline with telemetry, caching, and function invocation
- `MedicalRecord` entity with diagnosis, treatment, examination data
- `WeightEntry` history
- `PatientAlertContext` (used by health alerts) already aggregates recent records and weight history

**What needs to be built:**

1. **Patient History Summarizer**:
   - New handler: `SummarizePatientHistoryCommand(patientId, mode)`
   - Modes: `ClientFriendly`, `ClinicalHandoff`, `Timeline`
   - Reads all medical records via `IPatientAlertDataReader` (already exists)
   - Constructs a mode-specific prompt and calls `IChatClient`
   - `ClientFriendly`: plain language, no jargon, suitable for WhatsApp
   - `ClinicalHandoff`: SOAP-structured, includes differentials and pending tests
   - `Timeline`: chronological bullet points with dates and key findings

2. **Visit Summary Auto-Generation**:
   - Triggered after a medical record is saved (domain event from MedicalRecords)
   - Generates a client-friendly summary of today's visit
   - Stores summary in AI module (linked to record ID)
   - Available for one-click send via WhatsApp/SMS (when communication module exists)

3. **Bilingual Summary** (EN + AR):
   - Generate summaries in both English and Arabic
   - Arabic summaries for Emirati clients (no competitor does this)
   - LLM prompt includes language instruction; Arabic output validated for RTL display

**Architecture:** Fits entirely in the AI module. Reads from MedicalRecords via existing contracts. Publishes summaries as DTOs. The communication module (future) would consume summaries for sending.

### Estimated Effort

**4-5 days**

| Task | Days |
|---|---|
| Patient history summarizer (3 modes, prompt engineering) | 2 |
| Visit summary auto-generation (domain event trigger) | 1 |
| Bilingual support (EN + AR prompts, validation) | 1 |
| Tests (mocked LLM responses, edge cases) | 1 |

### Priority: **P0**

Rationale: Directly addresses the #1 vet pain point (documentation burden). Infrastructure is already in place (IChatClient, SOAP generator). Bilingual Arabic summaries are a unique differentiator that no global competitor offers. Relatively low effort for high impact.

---

## 7. Client Communication AI

### Description

AI-powered auto-drafting of client messages via WhatsApp (UAE's dominant messaging platform). Includes: appointment reminders, post-visit follow-ups, vaccination reminders, prescription refill alerts, and emergency triage responses.

### Business Value

- WhatsApp is the primary communication channel in UAE (used by 98% of smartphone users).
- Market research identifies "client communication module with WhatsApp" as a Priority 2 feature.
- Reduces receptionist workload: auto-drafted messages just need one-click approval.
- Post-visit follow-ups increase client retention and encourage return visits.
- Vaccination/prescription reminders generate recurring revenue for clinics.
- No competitor integrates WhatsApp with AI-drafted veterinary messages.

### Technical Feasibility

**What already exists:**
- `IMessageTriageService` with `TriageAsync` and `GenerateSuggestedRepliesAsync` (contract defined)
- LLM infrastructure (IChatClient, prompt engineering patterns from triage and SOAP)
- Health alerts with specific alert types that could trigger client messages
- Predictive health alerts background job (could be extended to generate client reminders)

**What needs to be built:**

1. **WhatsApp Business API Integration**:
   - New infrastructure service: `WhatsAppMessageSender` using WhatsApp Business API (cloud-hosted)
   - Template-based messages (WhatsApp requires pre-approved templates for business-initiated messages)
   - Templates: appointment reminder, follow-up, vaccination due, prescription refill, lab results ready
   - This is an infrastructure concern, not AI -- but it's the delivery channel for AI-drafted content

2. **AI Message Drafter**:
   - New handler: `DraftClientMessageCommand(patientId, messageType, context)`
   - Uses LLM to generate personalized, contextual messages
   - Message types: `AppointmentReminder`, `PostVisitFollowUp`, `VaccinationDue`, `PrescriptionRefill`, `HealthAlertNotification`, `Custom`
   - Bilingual: generates both EN and AR versions
   - Tone: professional but warm, culturally appropriate (formal Arabic, avoid overly casual English)

3. **Automated Trigger Rules**:
   - Post-visit: auto-draft follow-up message 24h after visit (configurable per clinic)
   - Vaccination: auto-draft reminder 7 days before due date (from health alerts)
   - No-show: auto-draft "we missed you" message with rebooking link
   - Health alert: when a new health alert is generated, auto-draft owner notification

4. **Approval Workflow**:
   - All AI-drafted messages go to a staff review queue before sending
   - Staff can: approve as-is, edit, or discard
   - Track: `WasEdited`, `OriginalDraft`, `SentContent` (same pattern as message triage)

**Dependencies:** This feature requires a Notifications/Communication module that does not yet exist. The AI drafting can be built now, but the WhatsApp delivery channel is a separate infrastructure task.

### Estimated Effort

**12-15 days** (full feature including WhatsApp integration)

| Task | Days |
|---|---|
| WhatsApp Business API integration | 3 |
| AI message drafter (6 message types, bilingual) | 3 |
| Automated trigger rules (4 triggers) | 2 |
| Approval workflow (review queue, tracking) | 2 |
| Frontend: message queue UI, preview, approve/edit/discard | 3 |
| Tests | 2 |

**AI drafting only (without WhatsApp delivery):** 5-6 days

### Priority: **P2**

Rationale: High value but high effort. Requires a Notifications module and WhatsApp Business API setup (business verification, template approval process with Meta takes 1-2 weeks). The AI drafting component can be built ahead of the delivery channel, but the value is only realized when messages actually reach clients. Best to ship after P0 and P1 features.

---

## 8. Priority Matrix

| # | Feature | Priority | Effort (days) | Business Value | Technical Risk | Dependencies |
|---|---|---|---|---|---|---|
| 1 | Falcon Health Monitoring | **P0** | 3-4 | Very High (UAE-unique) | Very Low | None -- extends existing rules engine |
| 2 | Medical Record Summarization | **P0** | 4-5 | Very High (pain point #1) | Low | None -- uses existing LLM infra |
| 3 | Breeding Outcome Prediction | **P1** | 8-10 | High (premium customers) | Medium | Breeding module stabilization |
| 4 | Smart Appointment Scheduling | **P1** | 6-7 | High (revenue protection) | Low | Agenda SuggestSlot, Notifications module for reminders |
| 5 | Drug Interaction Improvements | **P1** | 5-6 | High (safety + compliance) | Medium (LLM validation) | Exotic species dosage data sourcing |
| 6 | Client Communication AI | **P2** | 12-15 | Very High (WhatsApp) | High (external API dependency) | Notifications module, WhatsApp Business API |

**Total estimated effort: 38-47 days**

---

## 9. Implementation Roadmap

### Phase 1: Quick Wins (Q2 2026, weeks 1-2)

- **Falcon Health Monitoring** (P0, 3-4 days)
  - Directly extends existing rules engine
  - No new modules, no new infrastructure
  - Immediate value for falcon hospital outreach

- **Medical Record Summarization** (P0, 4-5 days)
  - Uses existing IChatClient and SOAP infrastructure
  - Bilingual summaries are a unique selling point
  - Can demo immediately to prospective clinics

### Phase 2: Premium Features (Q2 2026, weeks 3-5)

- **Drug Interaction Improvements** (P1, 5-6 days)
  - Exotic species dosage data first (supports falcon health monitoring)
  - LLM-assisted free-text checking second
  - Combination therapy alerts third

- **Smart Appointment Scheduling** (P1, 6-7 days)
  - No-show-aware overbooking and dynamic buffers first
  - Demand forecasting second (requires historical data)
  - Smart reminders deferred until Notifications module exists

### Phase 3: Breeding Intelligence (Q3 2026, weeks 6-8)

- **Breeding Outcome Prediction** (P1, 8-10 days)
  - Optimal mating window and pregnancy risk first
  - Inbreeding coefficient second
  - Litter size predictor last (needs more data)

### Phase 4: Communication Platform (Q3-Q4 2026)

- **Client Communication AI** (P2, 12-15 days)
  - Build AI message drafter first (can test with mock delivery)
  - WhatsApp Business API integration requires separate business verification process
  - Approval workflow frontend is the largest single task

### Key Risks

| Risk | Mitigation |
|---|---|
| Falcon veterinary rules accuracy | Validate with avian vet specialist before production. Phase 1 rules are conservative (screening reminders, not diagnoses). |
| LLM hallucination in drug interactions | Always flag as "AI-suggested". Never block prescriptions. Require vet confirmation. |
| Arabic LLM quality | Test Arabic output with native speakers. GPT-4o has strong Arabic capability. Ollama fallback may have weaker Arabic -- test. |
| WhatsApp Business API approval delays | Start Meta verification process 4+ weeks before planned launch. Have SMS fallback ready. |
| Breeding prediction cold start | Rule-based Phase 1 works without historical data. ML Phase 2 only activates when sufficient litter data exists per clinic. |
| Cross-module coupling creep | Maintain strict contract-based communication. New features expose data via `.Contracts` interfaces only. |

---

## Appendix: Technical Architecture Notes

### Module Placement

| Feature | Module | Rationale |
|---|---|---|
| Falcon Health Monitoring | `Vetolib.AI` (Rules/) | Extension of existing health alert rules |
| Medical Record Summarization | `Vetolib.AI` (Commands/) | Uses existing LLM infrastructure |
| Breeding Outcome Prediction | `Vetolib.Breeding` (Queries/) | Domain logic belongs with breeding data |
| Smart Scheduling | `Vetolib.Agenda` (Services/) | Extension of existing SuggestSlot |
| Drug Interaction Improvements | `Vetolib.AI` (Queries/CheckInteractions/) | Extension of existing handler |
| Client Communication AI | `Vetolib.AI` + new `Vetolib.Notifications` | AI drafting in AI module, delivery in Notifications |

### New Contracts Needed

| Contract | In | Consumed By |
|---|---|---|
| `INoShowPredictionReader` | `AI.Contracts` | Agenda (for overbooking) |
| `IBreedingDataReader` | `Breeding.Contracts` | AI (for breeding-aware health alerts) |
| `IPatientSummaryReader` | `AI.Contracts` | Future Communication/Notifications module |
| `IMessageDeliveryService` | `Notifications.Contracts` | AI (for sending drafted messages) |

### LLM Cost Estimation

Based on GPT-4o pricing ($2.50/1M input tokens, $10/1M output tokens) for a clinic with 30 patients/day:

| Feature | Calls/day | Avg tokens/call | Monthly cost |
|---|---|---|---|
| Medical Record Summarization | 30 | ~2,000 | ~$6 |
| Drug Interaction (LLM-assisted) | 5 (free-text only) | ~1,500 | ~$1 |
| Client Communication AI | 30 | ~800 | ~$3 |
| **Total per clinic** | | | **~$10/month** |

This cost is negligible relative to subscription pricing ($199-399/month) and can be absorbed or passed through as part of the AI features tier.
