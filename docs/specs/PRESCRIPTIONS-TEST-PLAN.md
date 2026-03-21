# Plan de Tests -- Prescriptions AI & Stock Integration

> Base sur : 
> Date : 2026-03-10
> Modules : MedicalRecords, AI, Stock
> Phases : Phase 1 (Catalog), Phase 2 (Interactions), Phase 3 (Stock)

---

## Conventions du document

- **P0** : Bloquant -- doit passer avant toute PR. Securite patient et regles metier critiques.
- **P1** : Requis -- doit passer avant merge main. Couvre tous les scenarios Gherkin de la spec.
- **P2** : Souhaitable -- edge cases et performances.

Identifiants de test :
- `BDD` = Reqnroll acceptance test
- `E2E` = Playwright UI test
- `UNIT` = xUnit unit test
- `INTG` = Test integration cross-module

---

## 1. Tests BDD (Reqnroll)

Fichiers feature a creer :
- `tests/Vetolib.Tests.Acceptance/Features/MedicalRecords/DrugInteractionChecking.feature`
- `tests/Vetolib.Tests.Acceptance/Features/MedicalRecords/StockPrescriptionIntegration.feature`
- `tests/Vetolib.Tests.Acceptance/Features/MedicalRecords/DrugCatalog.feature`

Step definitions cibles :
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/MedicalRecords/DrugCatalogSteps.cs`
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/MedicalRecords/DrugInteractionSteps.cs`
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/MedicalRecords/StockPrescriptionSteps.cs`

---

### 1.1 Feature : Drug Catalog (Phase 1)

#### BDD-CAT-001 -- Autocomplete retourne des resultats filtres [P0]

Bindings requis : `GivenDrugCatalogContains`, `WhenISearchCatalogWith`, `ThenResultsContain`

    Feature: Drug Catalog
      Background:
        Given I am logged in as a VET

      Scenario: Drug catalog autocomplete returns filtered results
        Given the drug catalog contains "Amoxicillin", "Amoxicillin/Clavulanate", and "Metronidazole"
        When I search the drug catalog with query "Amox"
        Then I should receive 2 results
        And the results should contain "Amoxicillin" and "Amoxicillin/Clavulanate"
        And "Metronidazole" should not be in the results

#### BDD-CAT-002 -- Recherche par INN name et display name [P1]

    Scenario: Search matches INN name and display name
      Given the drug catalog contains an entry with InnName "Acetylsalicylic acid" and DisplayName "Aspirin"
      When I search the drug catalog with query "Aspirin"
      Then I should receive 1 result
      And the result should have InnName "Acetylsalicylic acid"

#### BDD-CAT-003 -- Entree globale visible par toutes les cliniques [P0]

Note : Ce scenario valide l exception au filtre multi-tenant (WHERE ClinicId = @current OR ClinicId IS NULL).
Necessite MODIF_GELE sur `Shared/` -- voir section 8 ambiguite n.2.

    Scenario: Global catalog entries are visible to all clinics
      Given a global drug catalog entry "Amoxicillin" with ClinicId null
      And I am authenticated in clinic "Desert Paws"
      When I search the drug catalog with query "Amoxicillin"
      Then I should see "Amoxicillin" in the results

#### BDD-CAT-004 -- Entree clinique isolee des autres cliniques [P0]

    Scenario: Clinic-specific drug entries are isolated between tenants
      Given clinic "Desert Paws" has a custom drug entry "Custom Compound X"
      And clinic "Happy Paws" has no custom drugs
      When I am authenticated in clinic "Happy Paws"
      And I search the drug catalog with query "Custom Compound X"
      Then I should receive 0 results

#### BDD-CAT-005 -- Clinic ne peut pas modifier une entree globale [P0]

    Scenario: Clinic cannot modify a global catalog entry
      Given a global drug catalog entry "Amoxicillin" with ClinicId null
      When I am authenticated as ADMIN in clinic "Desert Paws"
      And I attempt to update the InnName of "Amoxicillin" to "Modified Name"
      Then the system should return a 403 Forbidden response

#### BDD-CAT-006 -- Admin peut ajouter une entree personnalisee [P1]

    Scenario: Clinic admin adds a custom drug to the catalog
      Given I am authenticated as ADMIN
      When I create a custom drug catalog entry with InnName "Custom Herbal Mix" and Category "Supplement"
      Then the entry should be created with the current clinic ClinicId
      And it should be visible only to my clinic when searching

#### BDD-CAT-007 -- Prescription avec entree catalogue (catalog-linked) [P0]

    Scenario: Create prescription linked to catalog entry
      Given a patient "Rex" of species "Dog" exists
      And a drug catalog entry "Amoxicillin" with Id "drug-0001"
      And an existing medical record for "Rex"
      When I create a prescription with DrugCatalogEntryId "drug-0001" and Dosage "250 mg twice daily"
      Then the prescription should be saved with DrugCatalogEntryId "drug-0001"
      And the Medication field should reflect "Amoxicillin"

#### BDD-CAT-008 -- Prescription en texte libre (sans catalogue) [P1]

    Scenario: Create free-text prescription without catalog entry
      Given a patient "Rex" of species "Dog" exists
      And an existing medical record for "Rex"
      When I create a prescription with free-text Medication "Custom Compound XY-42" and Dosage "1 tablet daily"
      Then the prescription should be saved successfully
      And DrugCatalogEntryId should be null

---

### 1.2 Feature : Drug Interaction Checking (Phase 2)

Scenarios issus directement de la section 4.3 de la spec.

#### BDD-INT-001 -- Contre-indication espece critique (ibuprofen -> chat) [P0]

Bindings requis : `GivenDrugHasContraindication`, `GivenAlternativeExists`,
`ThenShouldSeeCriticalAlert`, `ThenShouldSeeAlternative`, `ThenPrescriptionRequiresOverride`

    Feature: Drug Interaction Checking
      Background:
        Given I am logged in as a VET
        And a patient "Whiskers" of species "Cat" exists in my clinic
        And the drug catalog contains entries for "Amoxicillin", "Metronidazole", "Ibuprofen", "Meloxicam"

      Scenario: Species contraindication detected -- critical alert
        Given "Ibuprofen" has a critical species contraindication for "Cat" with reason "Nephrotoxic and GI ulceration in cats"
        And "Meloxicam" is listed as an alternative to "Ibuprofen" for "Cat"
        When I create a prescription for patient "Whiskers" with drug "Ibuprofen"
        Then I should see a critical alert with message containing "Nephrotoxic"
        And I should see "Meloxicam" suggested as an alternative
        And the prescription should not be saved until I provide an override justification

#### BDD-INT-002 -- Override alerte critique avec justification et audit [P0]

Bindings requis : `WhenIProvideOverrideJustification`, `ThenPrescriptionSavedWithJustification`, `ThenAuditEntryCreated`

      Scenario: Vet overrides a critical alert with justification
        Given "Ibuprofen" has a critical species contraindication for "Cat"
        When I create a prescription for patient "Whiskers" with drug "Ibuprofen"
        And I provide override justification "Only available NSAID, owner informed of risks, low dose protocol"
        Then the prescription should be saved with the override justification recorded
        And an audit entry should be created with severity "Critical" and the justification

#### BDD-INT-003 -- Interaction moderee drug-drug (sans blocage) [P0]

      Scenario: Drug-drug interaction detected -- moderate alert
        Given "Whiskers" has an active prescription for "Amoxicillin" from 5 days ago
        And "Amoxicillin" has a moderate interaction with "Metronidazole" described as "Increased risk of GI side effects"
        When I create a prescription for patient "Whiskers" with drug "Metronidazole"
        Then I should see a moderate warning with message containing "GI side effects"
        And I should be able to proceed without providing justification

#### BDD-INT-004 -- Aucune interaction detectee [P1]

      Scenario: No interactions detected
        Given "Whiskers" has no active prescriptions
        When I create a prescription for patient "Whiskers" with drug "Amoxicillin"
        Then I should see no interaction alerts
        And the prescription should be saved successfully

#### BDD-INT-005 -- Alerte info : dosage hors range par poids [P1]

Bindings requis : `GivenPatientHasWeight`, `GivenDrugHasDosageGuideline`, `ThenDosageOutOfRangeAlert`

      Scenario: Dosage out of range warning
        Given "Whiskers" has a recorded weight of 4.5 kg
        And "Amoxicillin" has a dosage guideline for "Cat" of 10 to 25 mg/kg
        When I create a prescription for patient "Whiskers" with drug "Amoxicillin" and dosage "200 mg"
        Then I should see an info alert indicating the dosage exceeds the recommended range of "45 mg to 112.5 mg"

#### BDD-INT-006 -- Dosage dans range : aucun avertissement [P2]

      Scenario: Dosage within range produces no dosage warning
        Given "Whiskers" has a recorded weight of 4.5 kg
        And "Amoxicillin" has a dosage guideline for "Cat" of 10 to 25 mg/kg
        When I create a prescription for patient "Whiskers" with drug "Amoxicillin" and dosage "80 mg"
        Then I should not see a dosage warning

#### BDD-INT-007 -- Prescription texte libre : aucune verification d interaction [P1]

      Scenario: Free-text medication -- no interaction check available
        When I create a prescription for patient "Whiskers" with free-text medication "Custom Compound XY-42"
        Then I should see an info message "No interaction data available for custom medications"
        And the prescription should be saved successfully

#### BDD-INT-008 -- Fenetre active 90 jours : prescription expiree ignoree [P1]

      Scenario: Prescription older than 90 days is not considered active
        Given "Whiskers" has a prescription for "Amoxicillin" created 95 days ago
        And "Amoxicillin" has a critical interaction with "Metronidazole"
        When I create a prescription for patient "Whiskers" with drug "Metronidazole"
        Then I should see no interaction alert for "Amoxicillin"

#### BDD-INT-009 -- Fenetre active configurable par clinique [P2]

      Scenario: Clinic can configure the active prescription window
        Given clinic "Desert Paws" has ActivePrescriptionWindowDays set to 30
        And "Whiskers" has a prescription for "Amoxicillin" created 45 days ago
        And "Amoxicillin" has a moderate interaction with "Metronidazole"
        When I create a prescription for patient "Whiskers" with drug "Metronidazole"
        Then I should see no interaction alert for "Amoxicillin"

#### BDD-INT-010 -- Maximum 5 alertes retournees (overflow) [P2]

      Scenario: Only the 5 most severe alerts are returned when there are more
        Given "Whiskers" has 7 active prescriptions each interacting with "Ibuprofen" at different severities
        When I create a prescription for patient "Whiskers" with drug "Ibuprofen"
        Then I should receive at most 5 alerts in the response

---

### 1.3 Feature : RBAC sur Prescriptions

#### BDD-RBAC-001 -- RECEPTIONIST ne peut pas creer de prescription [P0]

    Feature: RBAC Prescriptions
      Scenario: RECEPTIONIST cannot create prescriptions
        Given I am logged in as a RECEPTIONIST
        When I attempt to add a prescription to a medical record
        Then the system should return 403 Forbidden

#### BDD-RBAC-002 -- ASSISTANT ne peut pas creer de prescription [P0]

      Scenario: ASSISTANT cannot create prescriptions
        Given I am logged in as an ASSISTANT
        When I attempt to add a prescription to a medical record
        Then the system should return 403 Forbidden

#### BDD-RBAC-003 -- Override critique : ambiguite ADMIN vs VetOnly [P0]

AMBIGUITE : La spec section 2.2 dit "VET or ADMIN" pour l override. Le RBAC.feature
existant ligne 35 indique VetOnly pour les prescriptions. Creer
`questions/prescription-rbac-admin-override-001.md` avant d implementer.

      Scenario: Only authorized roles can override a critical alert
        Given I am logged in as a RECEPTIONIST
        And a drug "Ibuprofen" has a critical contraindication for "Cat"
        When I attempt to create a prescription with override justification "Emergency use"
        Then the system should return 403 Forbidden

---

### 1.4 Feature : Stock-Prescription Integration (Phase 3)

Scenarios issus directement de la section 5.3 de la spec.

    Feature: Stock-Prescription Integration
      Background:
        Given I am logged in as a VET
        And a patient "Rex" of species "Dog" exists in my clinic
        And the drug catalog contains "Amoxicillin" as a Medication
        And the drug catalog contains "Cephalexin" as a Medication

#### BDD-STK-001 -- Disponibilite stock affichee a la creation [P1]

      Scenario: Stock availability shown during prescription creation
        Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
        When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
        Then I should see stock information showing "100 tablets available"

#### BDD-STK-002 -- Badge low stock lors de la prescription [P1]

      Scenario: Low stock warning during prescription
        Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 5 and unit "tablets" and threshold 20
        When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
        Then I should see stock information showing "5 tablets available"
        And I should see a "Low stock" warning

#### BDD-STK-003 -- Out of stock avec suggestion alternative [P1]

      Scenario: Out of stock with alternative suggestion
        Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 0
        And a stock item "Cephalexin 500mg capsules" linked to catalog entry "Cephalexin" with quantity 50 and unit "capsules"
        And "Cephalexin" has no contraindication for "Dog"
        When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
        Then I should see "Out of stock" for "Amoxicillin"
        And I should see "Cephalexin" suggested as an in-stock alternative with "50 capsules available"

#### BDD-STK-004 -- Decrementation stock a la confirmation [P0]

      Scenario: Stock decremented on prescription confirmation
        Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
        When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
        And I confirm "Dispense from clinic stock"
        Then the stock quantity for "Amoxicillin 250mg tablets" should be 86
        And a stock movement of type "Out" with quantity 14 and reason containing "Prescription" should be recorded

#### BDD-STK-005 -- Skip decrementation (no dispense) [P1]

      Scenario: Vet skips stock decrement
        Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
        When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
        And I select "Do not dispense from stock"
        Then the prescription should be saved successfully
        And the stock quantity for "Amoxicillin 250mg tablets" should remain 100

#### BDD-STK-006 -- Stock insuffisant : dispense partielle [P1]

      Scenario: Insufficient stock -- partial dispense
        Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 5 and unit "tablets"
        When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
        And I confirm "Dispense from clinic stock"
        Then I should see a warning "Only 5 tablets available, 14 requested"
        And I should be able to dispense the available 5 tablets
        And the stock quantity for "Amoxicillin 250mg tablets" should be 0

#### BDD-STK-007 -- Prescription texte libre : pas de lien stock automatique [P1]

      Scenario: Free-text prescription -- no automatic stock link
        When I create a prescription for patient "Rex" with free-text medication "Custom Compound"
        Then I should not see automatic stock information
        And I should be able to manually select a stock item to decrement

#### BDD-STK-008 -- Stock low event declenche apres dispense [P1]

      Scenario: Stock low event triggered after prescription
        Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 22 and unit "tablets" and threshold 20
        When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
        And I confirm "Dispense from clinic stock"
        Then the stock quantity should be 8
        And a stock low alert should be triggered for "Amoxicillin 250mg tablets"

#### BDD-STK-009 -- Isolation multi-tenant sur stock lie aux prescriptions [P0]

      Scenario: Stock decrement only affects the prescribing clinic stock
        Given clinic "Desert Paws" has stock item "Amoxicillin" with quantity 100
        And clinic "Happy Paws" has stock item "Amoxicillin" with quantity 50
        When I am authenticated in clinic "Desert Paws"
        And I create a prescription for "Rex" and confirm dispense of 14 tablets
        Then clinic "Desert Paws" stock for "Amoxicillin" should be 86
        And clinic "Happy Paws" stock for "Amoxicillin" should remain 50

---

## 2. Tests E2E (Playwright)

Fichiers cibles :
- `src/frontend/e2e/patients/prescription-catalog.spec.ts`
- `src/frontend/e2e/patients/prescription-interactions.spec.ts`
- `src/frontend/e2e/patients/prescription-stock.spec.ts`

Handlers MSW a creer :
- `src/frontend/src/mocks/handlers/drug-catalog.ts`
- `src/frontend/src/mocks/handlers/drug-interactions.ts`
- `src/frontend/src/mocks/handlers/prescription-stock.ts`

Donnees MSW UAE obligatoires : noms arabes/anglais (ex: "Dr. Fatima Al-Hashimi"),
AED si applicable, timezone Asia/Dubai.

---

### 2.1 UI Prescription enrichie -- Catalogue

#### E2E-CAT-001 -- Formulaire affiche le drug selector [P0]

```
Given I am logged in as VET at /en/patients/{id}/records/new
Then data-testid="drug-selector-input" should be visible
And data-testid="free-text-toggle" should be visible
```

#### E2E-CAT-002 -- Autocomplete apparait apres 300ms de debounce [P1]

```
Given I am on the prescription form
When I type "Amox" in data-testid="drug-selector-input" and wait 300ms
Then data-testid="drug-autocomplete-list" should be visible
And items with data-testid matching "drug-option-{id}" should be present

MSW GET /api/v1/drug-catalog/search?q=Amox :
  - Amoxicillin (InnName), DisplayName "Amoxicilline"
  - Amoxicillin/Clavulanate, DisplayName "Augmentin"
```

#### E2E-CAT-003 -- Selection d un medicament du catalogue [P1]

```
Given the autocomplete dropdown is visible with "Amoxicillin"
When I click "Amoxicillin" in the dropdown
Then data-testid="drug-selector-input" should display "Amoxicillin"
And data-testid="input-dosage" should become active
And data-testid="interactions-panel" should appear or show skeleton
```

#### E2E-CAT-004 -- Bascule vers mode texte libre [P1]

```
Given I am on the prescription form in catalog mode
When I click data-testid="free-text-toggle"
Then data-testid="input-medication-freetext" should replace the autocomplete
And data-testid="freetext-info-banner" should be visible
And data-testid="interactions-panel" should not be visible
```

---

### 2.2 Alertes visuelles -- Interactions

#### E2E-INT-001 -- Alerte critique rouge affichee [P0]

```
Given the MSW handler returns a critical alert for "Ibuprofen" for species "Cat"
When I select "Ibuprofen" in the drug selector for patient "Whiskers" (Cat)
Then data-testid="alert-panel" should be visible
And data-testid="alert-critical-0" should be visible with red styling
And data-testid="alert-message-0" should contain "Nephrotoxic"
And data-testid="submit-prescription-btn" should be disabled
```

#### E2E-INT-002 -- Alternatives suggerees dans l alerte critique [P1]

```
Given a critical alert is displayed with alternative "Meloxicam"
Then data-testid="alternatives-list" should be visible
And data-testid="alternative-Meloxicam" should be visible
When I click data-testid="alternative-Meloxicam"
Then data-testid="drug-selector-input" should display "Meloxicam"
```

#### E2E-INT-003 -- Alerte moderee ambree non-bloquante [P1]

```
Given the MSW handler returns a moderate alert for "Metronidazole"
When I select "Metronidazole" in the drug selector
Then data-testid="alert-moderate-0" should be visible with amber styling
And data-testid="submit-prescription-btn" should be enabled
```

#### E2E-INT-004 -- Alerte info bleue pour dosage hors range [P1]

```
Given patient weight 4.5 kg and MSW returns info alert with range "45 mg to 112.5 mg"
When I fill data-testid="input-dosage" with "200 mg"
Then data-testid="alert-info-dosage" should be visible containing "45 mg to 112.5 mg"
And data-testid="submit-prescription-btn" should be enabled (not blocking)
```

#### E2E-INT-005 -- Maximum 5 alertes affichees avec overflow [P2]

```
Given the MSW handler returns 7 interaction alerts
When the alert panel renders
Then exactly 5 alert items should be visible
And data-testid="alerts-overflow-count" should contain "2 more alerts"
When I click data-testid="alerts-overflow-count"
Then all 7 alerts should be visible
```

---

### 2.3 Modale override avec justification

#### E2E-OVR-001 -- Section override presente sur alerte critique [P0]

```
Given a critical alert is displayed
Then data-testid="override-section" should be visible
And data-testid="override-justification-input" should be visible
And data-testid="submit-prescription-btn" should be disabled
```

#### E2E-OVR-002 -- Justification trop courte bloque la soumission [P0]

```
Given a critical alert is displayed
When I type "too short" in data-testid="override-justification-input"
And I click data-testid="submit-prescription-btn"
Then data-testid="error-override-justification" should be visible
And no POST to /api/v1/.../prescriptions should be made
```

#### E2E-OVR-003 -- Override soumis avec justification valide [P0]

```
Given a critical alert is displayed
When I type "Only available NSAID, owner informed of risks, low dose protocol" in override input
And I click data-testid="submit-prescription-btn"
Then the form should submit successfully (MSW returns 201)
And I should be redirected to the patient detail page
```

#### E2E-OVR-004 -- Section override absente pour alerte moderee uniquement [P1]

```
Given only a moderate alert is displayed (no critical)
Then data-testid="override-section" should not be visible
And data-testid="submit-prescription-btn" should be enabled
```

---

### 2.4 Stock -- Disponibilite et dispense

#### E2E-STK-001 -- Panel stock affiche apres selection catalogue [P1]

```
Given MSW stock handler returns 100 tablets for "Amoxicillin"
When I select "Amoxicillin" from the drug catalog
Then data-testid="stock-info-panel" should become visible
And data-testid="stock-quantity" should contain "100 tablets"
```

#### E2E-STK-002 -- Badge "Low stock" affiche [P1]

```
Given MSW returns quantity 5 with threshold 20 for "Amoxicillin"
When I select "Amoxicillin"
Then data-testid="stock-low-badge" should be visible
And data-testid="stock-quantity" should contain "5 tablets"
```

#### E2E-STK-003 -- Badge "Out of stock" avec alternative [P1]

```
Given MSW returns quantity 0 for "Amoxicillin" and "Cephalexin" with 50 capsules
When I select "Amoxicillin"
Then data-testid="stock-out-badge" should be visible
And data-testid="stock-alternatives-list" should be visible
And data-testid="alternative-stock-Cephalexin" should contain "50 capsules"
When I click data-testid="alternative-stock-Cephalexin"
Then data-testid="drug-selector-input" should display "Cephalexin"
```

#### E2E-STK-004 -- Toggle "Dispense from clinic stock" active par defaut [P1]

```
Given stock is available for the selected drug
Then data-testid="dispense-toggle" should be visible and checked by default
```

#### E2E-STK-005 -- Decocher dispense : soumission sans decrementation [P1]

```
Given the dispense toggle is checked
When I uncheck data-testid="dispense-toggle" and submit the form
Then the POST body should contain StockDecrementConfirmed=false
And no StockMovement endpoint should be called via MSW
```

#### E2E-STK-006 -- Avertissement stock insuffisant [P1]

```
Given MSW returns quantity 5 for "Amoxicillin"
When I set prescription quantity to 14 and confirm dispense
Then data-testid="stock-insufficient-warning" should be visible
And it should contain "Only 5 tablets available, 14 requested"
And data-testid="dispense-available-btn" should be visible
```


---

## 3. Tests unitaires

Projet cible : tests/Vetolib.Tests.Unit/

Fichiers a creer :
- tests/Vetolib.Tests.Unit/MedicalRecords/PrescriptionDomainTests.cs
- tests/Vetolib.Tests.Unit/AI/DrugInteractionServiceTests.cs
- tests/Vetolib.Tests.Unit/AI/DosageValidatorTests.cs
- tests/Vetolib.Tests.Unit/MedicalRecords/DrugCatalogEntryDomainTests.cs

---

### 3.1 Prescription domain -- Create

#### UNIT-PRESC-001 -- Create avec DrugCatalogEntryId retourne succes [P0]

Signature attendue :
  public static Result<Prescription> Create(Guid clinicId, Guid medicalRecordId,
    Guid? drugCatalogEntryId, string? medication, string dosage, string vetLicenseNumber,
    InteractionSeverity? overrideSeverity = null, string? overrideJustification = null,
    bool stockDecrementConfirmed = false)

Cas : clinicId valid, medicalRecordId valid, drugCatalogEntryId valid, dosage set.
Assert : result.IsSuccess, result.Value.DrugCatalogEntryId == drugCatalogEntryId.

#### UNIT-PRESC-002 -- Create en texte libre (sans catalogue) retourne succes [P1]

Cas : drugCatalogEntryId null, medication = Custom Compound XY-42.
Assert : result.IsSuccess, DrugCatalogEntryId null, Medication set.

#### UNIT-PRESC-003 -- Create sans medicament ni catalogue echoue [P0]

Cas : drugCatalogEntryId null, medication null.
Assert : result.IsSuccess false, ValidationErrors contain Identifier medication.

#### UNIT-PRESC-004 -- Create avec alerte critique et pas de justification echoue [P0]

Cas : overrideSeverity Critical, overrideJustification null.
Assert : result.IsSuccess false, error contains Override justification required.

#### UNIT-PRESC-005 -- Justification trop courte (moins de 10 chars) echoue [P0]

Cas : overrideSeverity Critical, overrideJustification = too short.
Assert : result.IsSuccess false.

#### UNIT-PRESC-006 -- Justification valide sur alerte critique reussit [P0]

Cas : overrideSeverity Critical, overrideJustification = Only available NSAID owner informed of risks.
Assert : result.IsSuccess, Value.OverrideJustification set, Value.OverrideSeverity Critical.

#### UNIT-PRESC-007 -- Alerte moderee sans justification reussit [P1]

Cas : overrideSeverity Moderate, overrideJustification null.
Assert : result.IsSuccess (moderate does not block per spec).

---

### 3.2 DrugInteractionService

Service : Vetolib.AI/Application/Services/DrugInteractionService.cs
Dependencies mockes avec NSubstitute : IDrugCatalogRepository (MedicalRecords.Contracts).

#### UNIT-DIS-001 -- Contre-indication critique detectee [P0]

Setup : entry Ibuprofen, contraindication { Species.Cat, Severity.Critical, Nephrotoxic }.
Input : drugId ibuprofen.Id, patientSpecies Cat, activePrescriptions empty.
Assert : result.IsSuccess, alerts[0].Severity Critical, alerts[0].Type SpeciesContraindication,
alerts[0].Message contains Nephrotoxic.

#### UNIT-DIS-002 -- Interaction moderee drug-drug detectee [P0]

Setup : Amoxicillin x Metronidazole interaction Moderate.
Input : drug Metronidazole, activePrescriptions [Amoxicillin 30 days ago].
Assert : alert.Severity Moderate, alert.Type DrugInteraction.

#### UNIT-DIS-003 -- Prescription expiree ignoree [P1]

Setup : Amoxicillin critical interaction with Metronidazole.
Input : activePrescriptions [Amoxicillin 95 days ago], windowDays 90.
Assert : alerts.Count == 0.

#### UNIT-DIS-004 -- Alertes triees par severite decroissante [P1]

Setup : drug triggers 1 Info + 1 Moderate + 1 Critical.
Assert : alerts[0].Severity Critical, [1] Moderate, [2] Info.

#### UNIT-DIS-005 -- Alternatives dans l alerte critique [P1]

Setup : Ibuprofen contraindicated Cat, Meloxicam as alternative.
Assert : alerts[0].Alternatives contains Meloxicam.

#### UNIT-DIS-006 -- Texte libre : alerte info uniquement [P1]

Input : drugCatalogEntryId null, freeTextMedication Custom Compound.
Assert : alerts.Count 1, alerts[0].Severity Info,
alerts[0].Message No interaction data available for custom medications.

---

### 3.3 DosageValidator

Service : Vetolib.AI/Application/Services/DosageValidator.cs

#### UNIT-DOS-001 -- Dosage dans range : pas d alerte [P0]

Input : weightKg 4.5, dosageMg 80, guideline Cat 10-25 mg/kg.
RecommendedMin = 45mg, RecommendedMax = 112.5mg. 80 in range.
Assert : result null.

#### UNIT-DOS-002 -- Dosage superieur au max : alerte info [P1]

Input : weightKg 4.5, dosageMg 200, guideline Cat 10-25.
Assert : alert.Severity Info, alert.Message contains 45 mg to 112.5 mg.

#### UNIT-DOS-003 -- Dosage inferieur au min : alerte info [P1]

Input : weightKg 4.5, dosageMg 20, guideline Cat 10-25. 20 < 45.
Assert : alert.Severity Info (never blocking per spec).

#### UNIT-DOS-004 -- Patient sans poids : pas de validation [P1]

Input : weightKg null. Assert : result null.

#### UNIT-DOS-005 -- Aucune guideline pour l espece : pas de validation [P1]

Setup : guideline for Dog only. Input : patientSpecies Cat. Assert : result null.

---

### 3.4 DrugCatalogEntry domain

#### UNIT-CAT-001 -- Create entree globale (ClinicId null) valide [P1]

Input : clinicId null, innName Amoxicillin, category Medication.
Assert : result.IsSuccess, result.Value.ClinicId null.

#### UNIT-CAT-002 -- Create entree clinique valide [P1]

Input : clinicId validGuid.
Assert : result.IsSuccess, result.Value.ClinicId validGuid.

#### UNIT-CAT-003 -- InnName vide echoue [P1]

Assert : ValidationErrors contain Identifier innName.

#### UNIT-CAT-004 -- AddContraindication critique pour Cat [P1]

Input : species Cat, severity Critical, reason Nephrotoxic.
Assert : entry.Contraindications.Count 1, Severity Critical.


---

## 4. Tests d integration cross-module

### 4.1 Prescription -> Stock decrement (via events MassTransit)

#### INTG-001 -- PrescriptionCreatedEvent declenche StockMovement [P0]

Flow complet (Testcontainers PostgreSQL + RabbitMQ) :
1. VET POST /api/v1/patients/{id}/records/{recordId}/prescriptions
   body = { DrugCatalogEntryId: drug-amox, StockDecrementConfirmed: true, Quantity: 14 }
2. MedicalRecords handler publie PrescriptionCreatedEvent via MassTransit
3. Stock Consumer (StockPrescriptionConsumer) recoit l event
4. StockMovement OUT cree : Reason = Prescription # + prescriptionId
5. StockItem.Quantity decremente de 14

Assertions (polling 100ms, timeout 5s) :
- GET /api/v1/stock/{itemId} retourne Quantity = initialQty - 14
- StockMovement en DB avec MovementType Out et Reason contenant prescriptionId

Implementation recommandee : assertion en boucle avec exponential backoff jusqu a 5s.

#### INTG-002 -- Sans confirmation : pas de StockMovement [P1]

Flow : Prescription avec StockDecrementConfirmed false.
Consumer verifie le flag, ne cree aucun mouvement.

Assertions (apres 3s) :
- Stock quantity inchangee
- Aucun StockMovement lie a ce prescriptionId en DB

#### INTG-003 -- StockInsufficientForPrescriptionEvent publie [P1]

Flow :
1. StockItem.Quantity = 5, prescription quantity = 14, dispense confirme
2. Stock decremente a 0, deficit = 9
3. Stock publie StockInsufficientForPrescriptionEvent

Assertions :
- Event publie avec PrescriptionId et Deficit = 9
- Consumer downstream reçoit l event (via test consumer spy)

---

### 4.2 Prescription -> Interaction check (cross-module MediatR)

#### INTG-004 -- CheckInteractionsQuery resolu par AI module en moins de 500ms [P1]

Flow :
1. AddPrescriptionHandler envoie CheckInteractionsQuery via ISender
2. AI module handler requete la DB DrugCatalogEntries + Interactions
3. Retourne InteractionCheckResult

Assertions :
- result.IsSuccess
- Alerts correspondent au catalog seed
- Temps de reponse < 500ms (stopwatch autour de ISender.Send)

SLA : spec section 8.3 impose < 500ms pour les medicaments du catalogue.

#### INTG-005 -- CheckStockAvailabilityQuery resolu par Stock module en moins de 200ms [P1]

Flow :
1. AddPrescriptionHandler envoie CheckStockAvailabilityQuery
2. Stock handler repond avec StockAvailabilityResult

Assertions :
- Available true si quantity > 0
- Alternatives non-vides si quantity == 0
- Temps de reponse < 200ms

SLA : spec section 8.3 impose < 200ms pour le stock check.

---

## 5. Tests de securite

#### SEC-001 -- RECEPTIONIST : POST prescription -> 403 [P0]

Extension du fichier tests/Vetolib.Tests.Acceptance/Features/Auth/RBAC.feature :

    Scenario: Receptionist cannot add prescription
      Given je suis authentifie en tant que Receptionist
      When je tente d ajouter une prescription a un dossier medical
      Then le systeme retourne 403

#### SEC-002 -- ASSISTANT : POST prescription -> 403 [P0]

    Scenario: Assistant cannot add prescription
      Given je suis authentifie en tant que Assistant
      When je tente d ajouter une prescription a un dossier medical
      Then le systeme retourne 403

#### SEC-003 -- ADMIN sans droit prescription directe -> 403 (a confirmer) [P0]

Voir ambiguite n.1 section 8. Bloquer avant d implementer si la matrice RBAC
n est pas reconciliee avec la spec section 2.2.

#### SEC-004 -- Audit trail non modifiable [P0]

Cas 1 : DELETE /api/v1/audit/{entryId} -> 405 Method Not Allowed ou 404.
Cas 2 : PATCH /api/v1/audit/{entryId} -> 405 ou 403.
Dans les deux cas : la justification en DB reste inchangee.

#### SEC-005 -- Catalogue global : lecture seule pour toutes les cliniques [P0]

PUT /api/v1/drug-catalog/{globalEntryId} par ADMIN de n importe quelle clinique -> 403.
DELETE /api/v1/drug-catalog/{globalEntryId} -> 403.

#### SEC-006 -- Isolation cross-tenant sur catalogue clinique [P0]

Entry id = drug-clinic-001 appartenant a clinic Desert Paws.
GET /api/v1/drug-catalog/drug-clinic-001 par ADMIN de Happy Paws -> 404 (filtre tenant).
PUT /api/v1/drug-catalog/drug-clinic-001 par ADMIN de Happy Paws -> 404.

#### SEC-007 -- Audit override contient toutes les donnees tracables [P0]

Given VET avec license UAE-DVM-2024-001 override alerte critique.
When la prescription est sauvegardee.
Then l audit entry contient :
  - VetId == authenticated user ID
  - VetLicenseNumber == UAE-DVM-2024-001
  - Timestamp within 1 second of the request (UTC)
  - OverrideSeverity == Critical
  - OverrideJustification == the submitted text (non-empty, unmodified)

---

### 5.2 Performance (SLA spec section 8.3)

#### SEC-PERF-001 -- Interaction check < 500ms avec catalogue seed [P2]

Given drug catalog seeded with 500 entries and 100 interaction pairs.
And patient has 5 active prescriptions.
When CheckInteractionsQuery is sent.
Then response time < 500ms measured at handler level.

#### SEC-PERF-002 -- Stock check < 200ms [P2]

Given clinic has 1000 stock items.
When CheckStockAvailabilityQuery sent for a specific DrugCatalogEntryId.
Then response time < 200ms.

#### SEC-PERF-003 -- Autocomplete catalog < 200ms [P2]

Given drug catalog has 500+ entries.
When GET /api/v1/drug-catalog/search?q=Amox.
Then response time < 200ms.
And subsequent identical queries served from Output Cache.

