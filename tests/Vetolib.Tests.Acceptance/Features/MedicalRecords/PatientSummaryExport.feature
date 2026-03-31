@wip
Feature: Patient medical summary export
  As a veterinarian
  I want to export a complete medical summary for a patient
  In order to share it with the pet owner or another clinic

  Background:
    Given a clinic "Happy Paws" with identifier "clinic-happy-paws"
    And an animal "Max" breed "Labrador" belonging to "John Smith"
    And I am authenticated as VET

  # --- Export ---

  Scenario: Export a patient medical summary
    Given "Max" has medical records in the system
    When I export the medical summary for "Max"
    Then a summary document is generated
    And the summary contains the patient name "Max" and owner "John Smith"

  # --- Content Inclusion ---

  Scenario: Summary includes vaccination history and active prescriptions
    Given "Max" has the following vaccination records:
      | Vaccine  | Date       |
      | Rabies   | 2025-06-15 |
      | DHPP     | 2025-08-20 |
    And "Max" has an active prescription for "Amoxicillin 250mg"
    When I export the medical summary for "Max"
    Then the summary includes the vaccination for "Rabies" on "2025-06-15"
    And the summary includes the vaccination for "DHPP" on "2025-08-20"
    And the summary includes the prescription for "Amoxicillin 250mg"

  # --- Content Exclusion ---

  Scenario: Summary excludes internal clinical notes
    Given "Max" has a medical record with an internal note "Owner seems non-compliant with treatment plan"
    When I export the medical summary for "Max"
    Then the summary does not contain internal notes
    And the summary does not include the text "Owner seems non-compliant"
