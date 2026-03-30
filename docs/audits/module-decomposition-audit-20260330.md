# Module Decomposition Audit -- 2026-03-30

## 1. Module Inventory

| Module | Entities (DbSets) | Handlers | Endpoints | Runtime LOC (excl. migrations/obj) | Contracts LOC |
|---|---|---|---|---|---|
| **Auth** | 6 (User, RefreshToken, Clinic, OnboardingState, ClinicGroup, ClinicGroupMember) | 22 | 5 | 3,259 | 352 |
| **MedicalRecords** | 7 (Patient, Owner, PatientOwner, MedicalRecord, Prescription, DrugCatalogEntry, WeightEntry) | 21 | 5 | 4,398 | 554 |
| **Messaging** | 10 (Conversation, Message, ReplyAudit, MessageAttachment, ResponseTemplate, OwnerPortalToken, MessagingHours, WhatsAppBusinessAccount, WhatsAppPhoneMapping, PendingUpload) | 33 | 4 | 6,743 | 496 |
| **AI** | 2 (TriageResult, HealthAlert) | 13 | 2 | 3,884 | 236 |
| **Agenda** | 2 (Appointment, ConsultationType) | 15 | 2 | 2,477 | 361 |
| **Billing** | 4 (Invoice, InvoiceItem, EReportingPeriod, EReportingTaxBreakdown) | 14 | 2 | 3,028 | 350 |
| **Breeding** | 6 (HeatCycle, Litter, LitterOffspring, Pregnancy, PregnancyCheck, PatientLineage) | 19 | 5 | 2,332 | 252 |
| **Notifications** | 2 (ReminderLog, ReminderConfig) | 3 | 1 | 1,505 | 183 |
| **Stock** | 2 (StockItem, StockMovement) | 8 | 1 | 1,061 | 190 |
| **Preferences** | 3 (UserPreference, ClinicPreferenceDefault, ConsentAuditEntry) | 0 | 0 | 678 | 191 |

### Summary statistics

- Total entities: 44
- Total handlers: 148
- Total endpoints: 27
- Total runtime LOC: ~29,365

---

## 2. Dependency Graph (Runtime -> Contracts references)

```
Auth.Contracts  <-- (no external Contracts deps, leaf)
    ^
    |--- Agenda.Contracts depends on Auth.Contracts
    |--- Auth Runtime depends on Agenda.Contracts, Billing.Contracts, MedicalRecords.Contracts

MedicalRecords.Contracts  <-- (no external Contracts deps, leaf)
    ^
    |--- Breeding.Contracts depends on MedicalRecords.Contracts
    |--- Breeding Runtime depends on MedicalRecords.Contracts
    |--- Stock Runtime depends on MedicalRecords.Contracts
    |--- MedicalRecords Runtime depends on Stock.Contracts
    |--- AI Runtime depends on MedicalRecords.Contracts
    |--- Messaging Runtime depends on MedicalRecords.Contracts
    |--- Auth Runtime depends on MedicalRecords.Contracts

Agenda.Contracts
    ^
    |--- AI Runtime depends on Agenda.Contracts
    |--- Messaging Runtime depends on Agenda.Contracts
    |--- Notifications Runtime depends on Agenda.Contracts
    |--- Auth Runtime depends on Agenda.Contracts

Billing.Contracts
    ^
    |--- Notifications Runtime depends on Billing.Contracts
    |--- Auth Runtime depends on Billing.Contracts

Messaging.Contracts
    ^
    |--- Notifications Runtime depends on Messaging.Contracts

AI.Contracts
    ^
    |--- Messaging Runtime depends on AI.Contracts

Preferences.Contracts
    ^
    |--- AI Runtime depends on Preferences.Contracts
    |--- Notifications Runtime depends on Preferences.Contracts

Stock.Contracts
    ^
    |--- MedicalRecords Runtime depends on Stock.Contracts
```

### Contracts-to-Contracts dependencies (transitive coupling risk)

Only two Contracts assemblies reference other Contracts assemblies:
1. **Agenda.Contracts -> Auth.Contracts** (for VeterinarianDto or similar shared types)
2. **Breeding.Contracts -> MedicalRecords.Contracts** (for patient-related types)

These are acceptable since Auth and MedicalRecords are foundational modules.

### Circular dependency analysis

**Near-circular found: MedicalRecords <-> Stock**
- MedicalRecords Runtime -> Stock.Contracts (to decrement stock on prescription)
- Stock Runtime -> MedicalRecords.Contracts (to reference drug catalog entries)

This is technically not a circular dependency (both go through Contracts, not Runtime), but it indicates tight semantic coupling between prescriptions and stock management. These two modules share the concept of "drug" and "prescription fulfillment."

**No true circular dependencies exist.** All references go through Contracts assemblies as intended.

---

## 3. Analysis by Module

### 3.1 Modules that are too fine-grained

#### Preferences -- MERGE CANDIDATE (Critical)

- **0 handlers, 0 endpoints, 678 LOC**
- Acts purely as a data store + `IPreferenceChecker` service consumed by other modules
- Has a `PreferenceChangedConsumer` (MassTransit) but no API surface
- Its own DbContext with 3 tables is infrastructure overhead for what is essentially a key-value store
- **Recommendation**: Merge into Auth. User preferences and clinic defaults are naturally part of identity/tenant configuration. The `IPreferenceChecker` interface would remain in Auth.Contracts so consumers are unaffected.

#### Notifications -- KEEP but monitor

- **3 handlers, 1 endpoint, 1,505 LOC**
- Has real domain logic: reminder scheduling (ReminderSchedulerService), configurable reminder configs, 9 MassTransit consumers for cross-module events
- Acts as the system's notification hub -- this is a legitimate bounded context
- References 5 other modules' Contracts (Auth, Agenda, Billing, Messaging, Preferences) which is expected for a notification aggregator
- **Recommendation**: Keep as-is. The high fan-in is appropriate for this role. However, if Preferences is merged into Auth, the dependency count drops to 4.

#### Stock -- KEEP but watch growth

- **8 handlers, 1 endpoint, 1,061 LOC, 2 entities**
- Below the 3-entity / 5-handler threshold but has clear domain logic: stock movements, alerts (low/expiring), prescription-driven decrement
- Semantic coupling with MedicalRecords via drug catalog is real but managed through Contracts
- **Recommendation**: Keep separate. Inventory management has different lifecycle/stakeholders than medical records. Will grow as batch ordering, suppliers, and expiry tracking evolve.

### 3.2 Modules that may be too coarse-grained

#### MedicalRecords -- MONITOR, do not split yet

The module handles 6 distinct sub-domains:
1. **Patient registry** (Patient, Owner, PatientOwner) -- 7 handlers
2. **Medical records** (MedicalRecord) -- 2 handlers
3. **Prescriptions** (Prescription) -- 3 handlers
4. **Drug catalog** (DrugCatalogEntry, DosageGuideline, DrugInteraction, SpeciesContraindication) -- 3 handlers
5. **Weight tracking** (WeightEntry) -- 4 handlers
6. **Import** (CSV import) -- 1 handler

At 7 entities and 21 handlers, this is the second largest module by entity count. However:
- All sub-domains revolve around the same aggregate root: **Patient**
- A prescription references a patient, a drug from the catalog, and generates a medical record entry
- Weight history feeds into drug dosage calculations
- Splitting "Patient" into its own module would create massive cross-cutting: every medical action needs patient context

**Recommendation**: Keep unified for now. If the drug catalog grows significantly (formulary management, supplier pricing, regulatory compliance), consider extracting a **Pharmacy/Formulary** module. The existing `Stock.Contracts` dependency on `MedicalRecords.Contracts` already hints at this future boundary.

**Risk of premature split**: Extracting Patient into its own module would require 6+ other modules to depend on Patient.Contracts (Agenda, Breeding, AI, Messaging, Billing, MedicalRecords-remainder). This would create a "God module" at the Contracts level -- worse than the current state.

#### Messaging -- MONITOR, potential future split

At 10 entities, 33 handlers, and 6,743 LOC, this is the largest module by every metric. It contains:
1. **Core messaging** (Conversation, Message, MessageAttachment, ReplyAudit) -- ~15 handlers
2. **WhatsApp integration** (WhatsAppBusinessAccount, WhatsAppPhoneMapping, WhatsAppSender) -- 3 handlers
3. **Owner portal** (OwnerPortalToken, portal endpoints, magic link auth) -- ~5 handlers
4. **Response templates** (ResponseTemplate) -- 3 handlers
5. **AI triage orchestration** (TriageOrchestrator, ClassificationFeedback) -- ~3 handlers
6. **SSE real-time** (MessagingEventBroadcaster) -- infrastructure
7. **Admin settings** (MessagingHours) -- 2 handlers

The WhatsApp integration and Owner Portal could be candidates for extraction, but:
- WhatsApp is a delivery channel for the same conversations -- splitting would mean cross-module writes on the same aggregate
- Owner Portal creates conversations that land in the same inbox -- same data, different access path
- AI triage is already delegated to the AI module via AI.Contracts

**Recommendation**: Keep unified for now. The complexity is inherent to the domain (multi-channel communication hub). If WhatsApp grows into a full integration platform (multiple channels: SMS, Telegram, etc.), extract a **Channels** or **Integrations** module that handles delivery, while Messaging remains the inbox/conversation manager.

### 3.3 Well-sized modules

| Module | Assessment |
|---|---|
| **Auth** | 6 entities, 22 handlers. Clean bounded context: identity, authentication, authorization, subscription management, onboarding. Appropriately sized. |
| **Agenda** | 2 entities, 15 handlers. Focused and cohesive. Appointment scheduling is a clear bounded context. |
| **Billing** | 4 entities, 14 handlers. Invoicing + UAE e-reporting/e-invoicing compliance. Well-scoped. |
| **Breeding** | 6 entities, 19 handlers. Niche veterinary domain (heat cycles, pregnancy, lineage, litters). Clear bounded context with no overlap. |
| **AI** | 2 entities, 13 handlers. Despite few entities, has substantial logic (18 health alert rules, ML no-show prediction, SOAP notes generation, triage). Legitimate bounded context. |

---

## 4. Cross-Module Coupling Analysis

### Auth as an orchestrator -- CONCERN

Auth Runtime references 3 other modules' Contracts:
- `Agenda.Contracts` (GetAppointmentCountQuery for onboarding)
- `Billing.Contracts` (GetInvoiceCountQuery for onboarding)
- `MedicalRecords.Contracts` (GetPatientCountQuery for onboarding + subscription checking)

This coupling exists in two places:
1. **GetOnboardingStateHandler** -- queries other modules to auto-complete onboarding steps
2. **SubscriptionChecker** -- counts patients across modules for plan limit enforcement

This is acceptable because:
- Auth uses MediatR queries (not direct DB access) -- loose coupling
- The queries are simple counts, not complex joins
- Onboarding is inherently cross-cutting

**However**, if more cross-module queries accumulate in Auth, consider extracting an **Onboarding** module or using domain events instead of synchronous queries.

### Notifications as a sink -- CORRECT pattern

Notifications consumes events from 5 modules but no module depends on Notifications. This is the correct "event sink" pattern for a notification service.

### PatientId ownership -- CLEAR

**Owner**: MedicalRecords module (Patient entity lives there)
**Consumers via ID reference only**: Agenda, Breeding, AI, Messaging, Billing

All consumers reference PatientId as a Guid -- no module duplicates the Patient entity. This is correct bounded context integration via identity reference.

---

## 5. Data Ownership Audit

| Concept | Owner Module | Referenced By (via Guid ID) |
|---|---|---|
| Patient | MedicalRecords | Agenda, Breeding, AI, Messaging, Billing |
| Owner (pet owner) | MedicalRecords | Messaging |
| User / Clinic | Auth | Agenda, all modules via IClinicContext |
| Appointment | Agenda | AI, Messaging, Notifications |
| Invoice | Billing | Notifications |
| Conversation | Messaging | Notifications, AI |
| DrugCatalogEntry | MedicalRecords | Stock |
| StockItem | Stock | MedicalRecords (via Contracts) |
| HeatCycle / Pregnancy | Breeding | (none) |
| Preferences | Preferences | AI, Notifications |

**No entity ownership conflicts detected.** Each entity has a single owning module.

---

## 6. Recommendations Summary

| # | Recommendation | Priority | Risk | Effort |
|---|---|---|---|---|
| R1 | **Merge Preferences into Auth** | High | Low -- only 678 LOC, 0 endpoints, minimal blast radius | Small (1-2 days) |
| R2 | **Monitor Messaging size** -- set a threshold at 40 handlers to trigger split evaluation | Medium | N/A (observation) | None now |
| R3 | **Monitor MedicalRecords** -- if Drug Catalog exceeds 10 handlers, evaluate Pharmacy module extraction | Medium | N/A (observation) | None now |
| R4 | **Evaluate Auth cross-module queries** -- if onboarding grows beyond 3 module queries, extract to its own module | Low | Low | Small |
| R5 | **Resolve MedicalRecords <-> Stock circular Contracts coupling** -- consider moving shared drug types to a `Vetolib.Pharmacy.Contracts` or `Shared.Kernel` | Low | Medium -- requires careful migration | Medium (3-5 days) |

### R1 Detail: Merge Preferences into Auth

**Current state**: Preferences has its own DbContext, 3 tables, 0 handlers, 0 endpoints. It is consumed as a service (`IPreferenceChecker`) and via MassTransit events (`PreferenceChangedIntegrationEvent`).

**Proposed state**:
- Move `UserPreference`, `ClinicPreferenceDefault`, `ConsentAuditEntry` entities into Auth's DbContext
- Move `IPreferenceChecker` interface into `Auth.Contracts`
- Move `PreferenceChecker` implementation into Auth Runtime
- Move `PreferenceChangedIntegrationEvent` into `Auth.Contracts`
- Delete `Vetolib.Preferences` and `Vetolib.Preferences.Contracts` projects
- Update consumers (AI, Notifications) to reference `Auth.Contracts` instead of `Preferences.Contracts`

**Risk**: Low. No endpoints to migrate. No handlers to move. The migration is mostly mechanical (move files, update namespaces, consolidate DbContext).

**Risk mitigation**: The `PreferenceChangedIntegrationEvent` contract must remain backward-compatible for MassTransit consumers.

### R5 Detail: MedicalRecords <-> Stock coupling

The bidirectional reference pattern is:
- MedicalRecords needs Stock to decrement inventory when a prescription is created
- Stock needs MedicalRecords to identify drugs by DrugCatalogEntryId

**Options**:
- **Option A**: Extract `DrugCatalogEntryId` and related types into `Shared.Kernel` -- but Shared is frozen
- **Option B**: Create `Vetolib.Pharmacy.Contracts` with drug-related DTOs, referenced by both modules -- adds a project but cleanly breaks the cycle
- **Option C**: Use domain events (MedicalRecords publishes `PrescriptionCreatedEvent`, Stock consumes it) instead of synchronous Contracts reference -- this already partially exists

**Recommendation**: Option C is already in progress (`PrescriptionCreatedEvent` exists). Complete the transition so MedicalRecords Runtime no longer needs `Stock.Contracts` directly.

---

## 7. Module Cohesion Score

Rating: 1 (low cohesion) to 5 (high cohesion)

| Module | Score | Notes |
|---|---|---|
| Auth | 4/5 | Slight sprawl with onboarding cross-module queries |
| Agenda | 5/5 | Tight, focused |
| MedicalRecords | 3/5 | 6 sub-domains but unified by Patient aggregate |
| Billing | 5/5 | Focused: invoicing + compliance |
| Breeding | 5/5 | Niche, no overlap |
| AI | 4/5 | Cohesive but depends on 3 other modules' data |
| Messaging | 3/5 | Multi-concern (conversations + WhatsApp + portal + SSE + triage) |
| Notifications | 4/5 | Clear "event sink" role, high fan-in is by design |
| Stock | 5/5 | Focused inventory management |
| Preferences | 2/5 | Too thin to justify isolation -- no behavior, just data |

---

## 8. Risk Assessment

### If we do nothing (status quo)

- Preferences remains a hollow module -- minor tech debt, 20 assembly overhead
- Messaging continues to grow -- could become 10K+ LOC within 6 months
- MedicalRecords <-> Stock coupling is manageable but creates implicit ordering constraints for migrations
- **Overall risk: LOW** -- the architecture is sound, no blocking issues

### If we merge Preferences into Auth (R1)

- Reduces project count by 2 (from 20 to 18 module assemblies)
- Simplifies dependency graph (2 fewer Contracts references)
- EF migration consolidation needed (move 3 tables to Auth schema)
- **Risk: LOW** -- mostly mechanical refactoring

### If we prematurely split MedicalRecords

- Would create a "Patient.Contracts" that becomes a dependency for 6+ modules
- Increases coupling surface area, not decreases it
- Migrations become complex (split DbContext, move tables)
- **Risk: HIGH** -- avoid unless there is a clear business driver

### If we prematurely split Messaging

- Conversations and WhatsApp messages share the same aggregate (Message entity)
- Split would require cross-module writes or distributed transactions
- **Risk: HIGH** -- avoid, not worth the complexity yet

---

## 9. Conclusion

Vetolib's module decomposition is **fundamentally sound**. The Ardalis modular monolith pattern is correctly applied: Contracts/Runtime separation is consistent, dependencies flow through Contracts only, and entity ownership is clear.

The single actionable recommendation is **R1: merge Preferences into Auth**. All other observations are monitoring items that should be re-evaluated in 3-6 months or when specific growth thresholds are hit.

The architecture follows bounded context principles well. The temptation to split further should be resisted until there is a clear scaling, team-ownership, or deployment-independence driver.
