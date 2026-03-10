# language: en
Feature: AI Feature Preferences
  As a clinic admin
  I want to control which AI features are active in my clinic
  So that I can manage our technology adoption pace

  Background:
    Given the clinic "Desert Paws Clinic" exists with id "clinic-001"
    And an admin user "admin@desertpaws.ae" belongs to "clinic-001"
    And a vet user "dr.sarah@desertpaws.ae" belongs to "clinic-001"

  # --- Default state ---

  Scenario: AI features are enabled by default for a new clinic
    Given I am logged in as "admin@desertpaws.ae"
    When I navigate to the clinic preferences page
    Then I see the following AI preferences:
      | Preference              | Value |
      | AI Triage               | ON    |
      | No-show predictions     | ON    |
      | Drug interaction alerts | ON    |

  # --- Admin disables AI features ---

  Scenario: An admin disables AI triage for the entire clinic
    Given I am logged in as "admin@desertpaws.ae"
    When I navigate to the clinic preferences page
    And I toggle "AI Triage" to OFF
    And I save the clinic defaults
    Then the clinic AI Triage preference is OFF
    And when "dr.sarah@desertpaws.ae" requests an AI triage suggestion
    Then the system returns an error indicating AI triage is disabled

  Scenario: An admin disables no-show predictions for the clinic
    Given I am logged in as "admin@desertpaws.ae"
    When I navigate to the clinic preferences page
    And I toggle "No-show predictions" to OFF
    And I save the clinic defaults
    Then the clinic no-show prediction preference is OFF
    And no-show scores are not computed for appointments in "clinic-001"

  # --- Drug interaction alerts: SAFETY RULE ---

  Scenario: Drug interaction alerts cannot be disabled
    Given I am logged in as "admin@desertpaws.ae"
    When I navigate to the clinic preferences page
    Then the "Drug interaction alerts" toggle is not editable
    And I see a safety notice "Drug interaction alerts cannot be disabled for patient safety"

  Scenario: Drug interaction alerts fire even if other AI features are disabled
    Given I am logged in as "admin@desertpaws.ae"
    And the clinic AI Triage preference is OFF
    And the clinic no-show prediction preference is OFF
    When "dr.sarah@desertpaws.ae" creates a prescription with a known drug interaction
    Then a drug interaction alert is displayed
    And the alert cannot be dismissed without providing an override reason

  # --- Per-clinic, not per-user in V1 ---

  Scenario: AI preferences apply to all users in the clinic
    Given I am logged in as "admin@desertpaws.ae"
    And the clinic AI Triage preference is OFF
    When "dr.sarah@desertpaws.ae" navigates to the appointment form
    Then the AI triage suggestion section is not visible

  Scenario: A vet cannot modify AI preferences
    Given I am logged in as "dr.sarah@desertpaws.ae"
    When I navigate to the clinic preferences page
    Then I receive a 403 Forbidden response

  # --- Tenant isolation ---

  Scenario: AI preferences are isolated per clinic
    Given the clinic "Al Barsha Vets" exists with id "clinic-002"
    And an admin user "admin@albarsha.ae" belongs to "clinic-002"
    And the AI Triage preference is OFF for "clinic-001"
    And the AI Triage preference is ON for "clinic-002"
    When a vet in "clinic-002" requests an AI triage suggestion
    Then the AI triage suggestion is returned successfully
