# language: en
Feature: Drug Interaction Checking
  As a veterinarian
  I want the system to check drug interactions when I prescribe medication
  So that I avoid harmful drug combinations and species contraindications

  Background:
    Given I am logged in as a VET
    And a patient "Whiskers" of species "Cat" exists in my clinic
    And the drug catalog contains the following entries:
      | InnName       | Category   |
      | Amoxicillin   | Medication |
      | Metronidazole | Medication |
      | Ibuprofen     | Medication |
      | Meloxicam     | Medication |

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
