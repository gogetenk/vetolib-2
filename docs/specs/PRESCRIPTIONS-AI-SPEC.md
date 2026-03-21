# Prescriptions AI & Stock Integration -- Business Specification

> **Status**: Draft -- PO specification
> **Date**: 2026-03-10
> **Modules impacted**: MedicalRecords, Stock, AI
> **Prerequisites**: Existing Prescription entity, Stock module, AI module scaffold

---

## 1. Problem Statement

Today, prescriptions are free-text strings (`Medication`, `Dosage`). This creates three problems:

1. **No safety net**: a vet can prescribe ibuprofen to a cat (lethal) without any warning.
2. **No stock link**: prescriptions do not decrement inventory; stock counts diverge from reality.
3. **No searchability**: free-text medication names cannot be aggregated, matched, or analyzed.

This spec introduces two features that solve all three problems by adding a **Drug Catalog** as the shared reference point.

---

## 2. PO Decisions (Ambiguities Resolved)

### 2.1 Drug interaction data source

**Decision**: Embedded local database, seeded from open veterinary pharmacology data.

**Rationale**:
- No dependency on external APIs (DrugBank is human-focused, expensive, and adds latency).
- Vetolib ships with a curated seed of ~500 common veterinary drugs with known interactions and species contraindications.
- Clinics can add custom drugs to their catalog (clinic-scoped), but custom drugs have no interaction data by default.
- Phase 2: LLM-assisted interaction check for drugs not in the catalog (via the AI module).

### 2.2 Can a vet override a critical alert?

**Decision**: Yes, with mandatory justification and full audit trail.

**Rationale**:
- Veterinary medicine has legitimate cases where a contraindicated drug is the only option (e.g., chemotherapy agents).
- Blocking the vet entirely would push them to bypass the system (type a different drug name).
- The override is logged in the audit trail with: vet ID, vet license number, timestamp, severity level, justification text.
- Override is ONLY available for users with role VET or ADMIN. ASSISTANT and RECEPTIONIST cannot override.

### 2.3 Drug catalog vs. free text

**Decision**: Introduce a shared Drug Catalog. Prescriptions transition from free text to catalog reference + optional free text fallback.

**Rationale**:
- A catalog is mandatory for both interaction checking and stock matching to work reliably.
- The catalog is NOT a separate module. It lives in MedicalRecords.Contracts as a shared reference (DrugCatalogEntry).
- Stock items of category "Medication" or "Vaccine" can optionally link to a catalog entry via `DrugCatalogEntryId`.
- Prescriptions reference a catalog entry ID. Free-text fallback is kept for compounded/custom medications not in the catalog.

### 2.4 Dosage by animal weight

**Decision**: Yes, the catalog includes weight-based dosage ranges per species.

**Rationale**:
- Weight-based dosing is standard veterinary practice.
- The Patient entity needs a `Weight` field (decimal, kg) -- this is a new field.
- The catalog entry contains: `MinDosePerKg`, `MaxDosePerKg`, `Unit` per species.
- The system calculates the recommended range and warns if the prescribed dosage is outside it.
- Warning only, never blocking -- the vet has clinical judgment.

### 2.5 Stock decrement: automatic or manual?

**Decision**: Automatic on prescription creation, with vet confirmation step.

**Rationale**:
- Flow: Vet creates prescription -> system shows stock availability + recommended quantity -> vet confirms -> stock decremented.
- If the vet skips confirmation (e.g., medication given from personal stock, or owner buys elsewhere), no decrement occurs.
- The prescription is valid regardless of stock action. Stock link is optional.
- StockMovement.Reason records "Prescription #{prescriptionId}" for traceability.

### 2.6 Matching Prescription to StockItem

**Decision**: Via DrugCatalogEntryId (when both reference the same catalog entry). No fuzzy text matching.

**Rationale**:
- Free-text matching is unreliable ("Amoxicillin 250mg" vs "Amoxicilline 250" vs "Amox").
- Both Prescription and StockItem reference the same DrugCatalogEntryId.
- If a prescription uses free-text fallback (no catalog entry), no automatic stock matching occurs. The vet can manually select a stock item.

### 2.7 International scope

**Decision**: Drug catalog entries are language-neutral (INN/international names). Display names are localized.

**Rationale**:
- INN (International Nonproprietary Name) is the global standard for drug naming.
- Each catalog entry has: `InnName` (canonical), `DisplayNames` (dictionary of locale -> name for UI).
- Species contraindications are universal, not country-specific.
- Regulatory fields (e.g., controlled substance classification) are per-country and out of scope for MVP.

---

## 3. Data Model Changes

### 3.1 New Entity: DrugCatalogEntry (in MedicalRecords)

| Field | Type | Notes |
|---|---|---|
| Id | Guid | PK |
| ClinicId | Guid? | NULL = global seed, non-null = clinic-specific entry |
| InnName | string | International Nonproprietary Name (canonical) |
| DisplayName | string | Default display name (English) |
| Category | DrugCategory | Medication, Vaccine, Supplement |
| Species contraindications | List | e.g., [{Species: Cat, Severity: Critical, Reason: "Hepatotoxic"}] |
| Interactions | List | e.g., [{OtherDrugId: Guid, Severity: Moderate, Description: "..."}] |
| DosageGuidelines | List | e.g., [{Species: Dog, MinDosePerKg: 10, MaxDosePerKg: 25, Unit: "mg", Route: "oral"}] |
| IsActive | bool | Soft delete |

**Note on multi-tenancy**: Global entries (ClinicId = null) are read-only for all clinics. Clinic-specific entries are tenant-isolated as usual. Query: `WHERE ClinicId = @current OR ClinicId IS NULL`.

**This requires a modification to the standard multi-tenant query filter behavior.** The global catalog entries (ClinicId = null) must be visible to all clinics. This is a controlled exception to the standard `WHERE ClinicId = @current` filter. Implementation should use a separate DbSet with a custom query filter: `WHERE ClinicId = @current OR ClinicId IS NULL`.

### 3.2 Modified Entity: Prescription

| Field | Change |
|---|---|
| DrugCatalogEntryId | NEW -- Guid? (nullable, for free-text fallback) |
| Medication | KEPT -- serves as free-text fallback when no catalog entry |
| StockDecrementConfirmed | NEW -- bool (did the vet confirm stock decrement?) |
| OverrideJustification | NEW -- string? (non-null when a critical/moderate alert was overridden) |
| OverrideSeverity | NEW -- InteractionSeverity? (the severity level that was overridden) |

### 3.3 Modified Entity: Patient

| Field | Change |
|---|---|
| WeightKg | NEW -- decimal? (nullable, not all patients are weighed at every visit) |

### 3.4 Modified Entity: StockItem

| Field | Change |
|---|---|
| DrugCatalogEntryId | NEW -- Guid? (links medication/vaccine stock items to catalog) |

### 3.5 New Integration Events

| Event | Publisher | Consumer | Purpose |
|---|---|---|---|
| PrescriptionCreatedEvent | MedicalRecords | Stock | Triggers stock decrement if confirmed |
| StockInsufficientForPrescriptionEvent | Stock | MedicalRecords (or AI) | Notifies that stock is insufficient |

---

## 4. Feature 1: AI-Assisted Drug Interaction Checking

### 4.1 User Stories

**US-1**: As a vet, when I prescribe a medication, I want to see interaction warnings with the patient's active prescriptions so that I avoid harmful drug combinations.

**US-2**: As a vet, when I prescribe a medication to a cat, I want to be warned if the drug is contraindicated for cats so that I do not accidentally poison the animal.

**US-3**: As a vet, when an interaction is detected, I want to see alternative medications so that I can quickly choose a safe substitute.

**US-4**: As a vet, I want to override a critical alert with a written justification so that I can proceed when clinically necessary, with full traceability.

**US-5**: As a clinic admin, I want to add custom drugs to my clinic's catalog so that I can track medications specific to my practice.

### 4.2 Interaction Check Flow

```
1. Vet selects drug from catalog (or types free text)
2. System queries: active prescriptions for this patient (last 90 days)
3. System checks:
   a. Species contraindication (drug vs patient.Species)
   b. Drug-drug interaction (drug vs each active prescription's DrugCatalogEntryId)
   c. Dosage range (if patient.WeightKg is set)
4. System returns list of alerts, each with:
   - Severity: Critical | Moderate | Info
   - Type: SpeciesContraindication | DrugInteraction | DosageOutOfRange
   - Message (human-readable)
   - Alternatives (list of DrugCatalogEntryId suggestions)
5. If no Critical alerts -> prescription proceeds
6. If Critical alerts -> vet must provide justification to override
7. If Moderate alerts -> vet sees warning, can proceed without justification
8. If Info alerts -> displayed, no action required
```

### 4.3 Gherkin Scenarios

```gherkin
Feature: Drug Interaction Checking
  As a veterinarian
  I want the system to check drug interactions when I prescribe medication
  So that I avoid harmful drug combinations and species contraindications

  Background:
    Given I am logged in as a VET
    And a patient "Whiskers" of species "Cat" exists in my clinic
    And the drug catalog contains the following entries:
      | InnName      | Category   |
      | Amoxicillin  | Medication |
      | Metronidazole| Medication |
      | Ibuprofen    | Medication |
      | Meloxicam    | Medication |

  Scenario: Species contraindication detected -- critical alert
    Given "Ibuprofen" has a critical species contraindication for "Cat" with reason "Nephrotoxic and GI ulceration in cats"
    And "Meloxicam" is listed as an alternative to "Ibuprofen" for "Cat"
    When I create a prescription for patient "Whiskers" with drug "Ibuprofen"
    Then I should see a critical alert with message containing "Nephrotoxic"
    And I should see "Meloxicam" suggested as an alternative
    And the prescription should not be saved until I provide an override justification

  Scenario: Vet overrides a critical alert with justification
    Given "Ibuprofen" has a critical species contraindication for "Cat"
    When I create a prescription for patient "Whiskers" with drug "Ibuprofen"
    And I provide override justification "Only available NSAID, owner informed of risks, low dose protocol"
    Then the prescription should be saved with the override justification recorded
    And an audit entry should be created with severity "Critical" and the justification

  Scenario: Drug-drug interaction detected -- moderate alert
    Given "Whiskers" has an active prescription for "Amoxicillin" from 5 days ago
    And "Amoxicillin" has a moderate interaction with "Metronidazole" with description "Increased risk of GI side effects"
    When I create a prescription for patient "Whiskers" with drug "Metronidazole"
    Then I should see a moderate warning with message containing "GI side effects"
    And I should be able to proceed without providing justification

  Scenario: No interactions detected
    Given "Whiskers" has no active prescriptions
    When I create a prescription for patient "Whiskers" with drug "Amoxicillin"
    Then I should see no interaction alerts
    And the prescription should be saved successfully

  Scenario: Dosage out of range warning
    Given "Whiskers" has a recorded weight of 4.5 kg
    And "Amoxicillin" has a dosage guideline for "Cat" of 10 to 25 mg/kg
    When I create a prescription for patient "Whiskers" with drug "Amoxicillin" and dosage "200 mg"
    Then I should see an info alert indicating the dosage exceeds the recommended range of "45 mg to 112.5 mg"

  Scenario: Free-text medication -- no interaction check available
    When I create a prescription for patient "Whiskers" with free-text medication "Custom Compound XY-42"
    Then I should see an info message "No interaction data available for custom medications"
    And the prescription should be saved successfully

  Scenario: RECEPTIONIST cannot override critical alerts
    Given I am logged in as a RECEPTIONIST
    Then I should not have access to the prescription creation feature

  Scenario: ASSISTANT can view but not create prescriptions
    Given I am logged in as an ASSISTANT
    When I view the medical record for patient "Whiskers"
    Then I should see existing prescriptions in read-only mode
    And I should not see a "New Prescription" button
```

### 4.4 Active Prescription Window

A prescription is considered "active" if it was created within the last 90 days. This window is configurable per clinic (setting: `ActivePrescriptionWindowDays`, default: 90).

---

## 5. Feature 2: Stock-Prescription Integration

### 5.1 User Stories

**US-6**: As a vet, when I prescribe a medication, I want to see the current stock level so that I know if the medication is available in the clinic.

**US-7**: As a vet, when the prescribed medication is out of stock, I want to see equivalent medications that are in stock so that I can choose an available alternative.

**US-8**: As a vet, when I confirm a prescription, I want the stock to be decremented automatically so that inventory stays accurate without manual work.

**US-9**: As a vet, I want to skip stock decrement when the medication is not dispensed from the clinic so that the inventory is not incorrectly reduced.

### 5.2 Stock Check Flow

```
1. Vet selects drug from catalog
2. System finds StockItem(s) with matching DrugCatalogEntryId in the clinic
3. System displays:
   - Current quantity + unit
   - "Low stock" badge if below threshold
   - "Out of stock" badge if quantity = 0
   - "Expiring soon" badge if within 30 days
4. If out of stock:
   - System queries other StockItems in same DrugCategory with quantity > 0
   - Suggests alternatives that are also in the drug catalog (with interaction check applied)
5. Vet confirms prescription:
   a. "Dispense from clinic stock" -> creates StockMovement (Out, quantity based on prescription)
   b. "Do not dispense" -> no stock movement, prescription still saved
6. If insufficient stock for requested quantity -> warning, vet can:
   a. Reduce quantity to available stock
   b. Proceed anyway (stock goes to 0, triggers StockLowEvent)
   c. Choose alternative
```

### 5.3 Gherkin Scenarios

```gherkin
Feature: Stock-Prescription Integration
  As a veterinarian
  I want prescriptions to be linked to clinic stock
  So that inventory is automatically updated and I know what is available

  Background:
    Given I am logged in as a VET
    And a patient "Rex" of species "Dog" exists in my clinic
    And the drug catalog contains "Amoxicillin" as a Medication
    And the drug catalog contains "Cephalexin" as a Medication

  Scenario: Stock availability shown during prescription creation
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
    When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
    Then I should see stock information showing "100 tablets available"

  Scenario: Low stock warning during prescription
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 5 and unit "tablets" and threshold 20
    When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
    Then I should see stock information showing "5 tablets available"
    And I should see a "Low stock" warning

  Scenario: Out of stock with alternative suggestion
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 0
    And a stock item "Cephalexin 500mg capsules" linked to catalog entry "Cephalexin" with quantity 50 and unit "capsules"
    And "Cephalexin" has no contraindication for "Dog"
    When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
    Then I should see "Out of stock" for "Amoxicillin"
    And I should see "Cephalexin" suggested as an in-stock alternative with "50 capsules available"

  Scenario: Stock decremented on prescription confirmation
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
    When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
    And I confirm "Dispense from clinic stock"
    Then the stock quantity for "Amoxicillin 250mg tablets" should be 86
    And a stock movement of type "Out" with quantity 14 and reason containing "Prescription" should be recorded

  Scenario: Vet skips stock decrement
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
    When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
    And I select "Do not dispense from stock"
    Then the prescription should be saved successfully
    And the stock quantity for "Amoxicillin 250mg tablets" should remain 100

  Scenario: Insufficient stock -- partial dispense
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 5 and unit "tablets"
    When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
    And I confirm "Dispense from clinic stock"
    Then I should see a warning "Only 5 tablets available, 14 requested"
    And I should be able to dispense the available 5 tablets
    And the stock quantity for "Amoxicillin 250mg tablets" should be 0

  Scenario: Free-text prescription -- no automatic stock link
    When I create a prescription for patient "Rex" with free-text medication "Custom Compound"
    Then I should not see stock information
    And I should be able to manually select a stock item to decrement
    Or skip stock decrement entirely

  Scenario: Stock low event triggered after prescription
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 22 and unit "tablets" and threshold 20
    When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
    And I confirm "Dispense from clinic stock"
    Then the stock quantity should be 8
    And a stock low alert should be triggered for "Amoxicillin 250mg tablets"
```

---

## 6. Module Boundaries and Communication

### 6.1 Where Each Concern Lives

| Concern | Module | Rationale |
|---|---|---|
| DrugCatalogEntry entity + CRUD | MedicalRecords | Drugs are a medical concern, not stock |
| DrugCatalogEntryId on Prescription | MedicalRecords | Prescription owns the drug reference |
| DrugCatalogEntryId on StockItem | Stock | Stock links to catalog via Contracts |
| Interaction check logic | AI | AI module owns all inference/checking logic |
| Stock availability query | Stock | Stock owns inventory data |
| Stock decrement command | Stock | Stock owns mutations |
| Prescription creation orchestration | MedicalRecords | MedicalRecords is the orchestrator for the prescription flow |

### 6.2 Inter-Module Communication

All communication is via `.Contracts` assemblies and MediatR notifications (integration events).

```
MedicalRecords.Contracts (public):
  - DrugCatalogEntryDto
  - PrescriptionCreatedEvent : INotification
  - IInteractionCheckService (interface, implemented by AI)
  - CheckInteractionsQuery : IRequest<Result<InteractionCheckResult>>

AI.Contracts (public):
  - InteractionCheckResult
  - InteractionAlert { Severity, Type, Message, Alternatives }

Stock.Contracts (public):
  - CheckStockAvailabilityQuery : IRequest<Result<StockAvailabilityResult>>
  - DecrementStockForPrescriptionCommand : IRequest<Result>
  - StockAvailabilityResult { Available, Quantity, Unit, Alternatives }
```

**Flow**:
1. Vet submits prescription -> MedicalRecords handler
2. MedicalRecords sends `CheckInteractionsQuery` -> AI handler responds with alerts
3. MedicalRecords sends `CheckStockAvailabilityQuery` -> Stock handler responds with availability
4. MedicalRecords returns combined result to frontend (alerts + stock info)
5. Vet confirms (with optional override + stock dispense choice)
6. MedicalRecords saves prescription, publishes `PrescriptionCreatedEvent`
7. Stock consumes `PrescriptionCreatedEvent`, creates StockMovement if `StockDecrementConfirmed = true`

### 6.3 No Runtime Cross-References

- MedicalRecords references: `Vetolib.AI.Contracts`, `Vetolib.Stock.Contracts` (queries only)
- AI references: `Vetolib.MedicalRecords.Contracts` (for DrugCatalogEntryDto, Species)
- Stock references: `Vetolib.MedicalRecords.Contracts` (for DrugCatalogEntryId type, PrescriptionCreatedEvent)
- No module references another module's runtime assembly.

---

## 7. Drug Catalog Seed Strategy

### 7.1 Initial Seed (MVP)

The system ships with a curated seed of common veterinary drugs:

- **~200 medications**: antibiotics, NSAIDs, antiparasitics, cardiac drugs, anesthetics
- **~50 vaccines**: core + non-core for Dog, Cat, Horse, Camel
- **~100 interaction pairs**: well-documented drug-drug interactions
- **~30 species contraindications**: the critical ones (ibuprofen/cats, permethrin/cats, xylitol/dogs, etc.)
- **Dosage guidelines**: for the top 50 most prescribed drugs, per species

### 7.2 Data Source

Seed data compiled from:
- Plumb's Veterinary Drug Handbook (public knowledge summaries)
- BSAVA formulary (species contraindications)
- Clinician-reviewed interaction databases

Seed is applied via EF Core migrations (DbInitializer), not API calls. Global entries have `ClinicId = null`.

### 7.3 Clinic Customization

Clinics can:
- Add custom catalog entries (ClinicId = their clinic ID, isolated by tenant)
- Mark global entries as "hidden" in their clinic (soft filter, not deletion)
- They CANNOT modify global entries (read-only)
- Custom entries have no interaction data unless manually added by the clinic admin

---

## 8. UI/UX Guidelines

### 8.1 Prescription Form Changes

The current prescription form has two fields: `Medication` (text), `Dosage` (text).

New form:
1. **Drug selector**: autocomplete search against the drug catalog (INN name + display name). Fallback: toggle to free-text mode.
2. **Dosage**: text field, with calculated recommended range shown below (if patient weight + catalog dosage guidelines exist).
3. **Interaction alerts panel**: appears after drug selection. Color-coded by severity (red = critical, amber = moderate, blue = info).
4. **Stock availability panel**: appears after drug selection. Shows quantity, low/out-of-stock badges.
5. **Dispense toggle**: "Dispense from clinic stock" checkbox (default: checked if stock available).
6. **Override section**: appears only when critical alert exists. Requires justification text (min 10 characters).

### 8.2 Alert Display Rules

- **Critical**: red banner, blocks submission until override justification provided. Icon: warning triangle.
- **Moderate**: amber banner, does not block. Collapsible. Icon: exclamation circle.
- **Info**: blue text below the dosage field. No icon.
- Maximum 5 alerts shown at once. If more, show "N more alerts" expandable.
- Alerts are NOT shown to ASSISTANT or RECEPTIONIST (they cannot create prescriptions anyway).

### 8.3 Performance

- Interaction check must complete in < 500ms for catalog drugs (local DB query).
- Stock check must complete in < 200ms (simple query).
- Drug catalog autocomplete: debounced 300ms, show results in < 200ms.
- No loading spinners for checks -- use optimistic UI with skeleton states.

---

## 9. Phasing

### Phase 1: Drug Catalog + Prescription Linking (no AI, no stock link)
- Create DrugCatalogEntry entity in MedicalRecords
- Seed initial data
- Update Prescription to reference catalog
- Update prescription form with drug selector
- **No interaction checking, no stock integration yet**

### Phase 2: Interaction Checking
- Implement CheckInteractionsQuery handler in AI module
- Species contraindications
- Drug-drug interactions
- Dosage range checking
- Alert display in prescription form
- Override with audit trail

### Phase 3: Stock-Prescription Integration
- Add DrugCatalogEntryId to StockItem
- Implement CheckStockAvailabilityQuery in Stock
- Implement PrescriptionCreatedEvent consumer in Stock
- Stock display in prescription form
- Dispense/skip flow

---

## 10. Out of Scope (Explicit)

- Controlled substance tracking / DEA-equivalent reporting
- Prescription printing / PDF generation (separate feature)
- Owner-facing prescription history (owner portal not in MVP)
- Drug manufacturer / batch number tracking
- Prescription refill management
- Integration with external pharmacy systems
- Country-specific drug regulation compliance (beyond catalog structure)

---

## 11. Open Questions for Human Review

| # | Question | Impact |
|---|---|---|
| 1 | The global catalog (ClinicId = null) requires a **modification to the multi-tenant query filter** in Shared/. This is a frozen file. Needs MODIF_GELE authorization. | Blocks Phase 1 implementation |
| 2 | Should the WeightKg field on Patient be required for dosage checking, or should dosage warnings simply not appear when weight is absent? | PO recommends: optional, warnings skipped when absent |
| 3 | Seed data licensing: Plumb's and BSAVA content is copyrighted. The seed must use only INN names (public domain) and clinician-authored interaction descriptions (original content). Legal review recommended before production use. | No code impact, legal/compliance concern |
