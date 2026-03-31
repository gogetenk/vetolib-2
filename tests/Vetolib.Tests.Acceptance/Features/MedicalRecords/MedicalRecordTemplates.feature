@wip
Feature: Medical record templates
  As a veterinarian
  I want to use templates for common medical record entries
  In order to save time and ensure consistency across consultations

  Background:
    Given a clinic "Happy Paws" with identifier "clinic-happy-paws"
    And I am authenticated as VET

  # --- Listing ---

  Scenario: List available system templates
    When I list the available medical record templates
    Then I see system templates including "General Consultation" and "Vaccination Visit"
    And each template has a name, category, and content structure

  # --- Custom Templates ---

  Scenario: Create a custom template for the clinic
    When I create a custom template named "Post-Surgery Follow-Up" with the following sections:
      | Section          |
      | Wound assessment |
      | Pain level       |
      | Medication check |
    Then the template "Post-Surgery Follow-Up" is created
    And it is marked as a custom template for "Happy Paws"

  # --- Protection ---

  Scenario: System templates cannot be modified
    Given a system template "General Consultation"
    When I attempt to modify the system template "General Consultation"
    Then the system rejects with code "TEMPLATE_READ_ONLY"
    And the message indicates that system templates cannot be modified

  # --- Deletion ---

  Scenario: Delete a custom template
    Given a custom template "Post-Surgery Follow-Up" belonging to "Happy Paws"
    When I delete the template "Post-Surgery Follow-Up"
    Then the template is removed from the clinic's template list
    And existing records using this template are not affected
