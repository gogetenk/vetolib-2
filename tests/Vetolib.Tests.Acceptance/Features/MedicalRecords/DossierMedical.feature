Feature: Animal medical record
  Background:
    Given a clinic "Happy Paws"
    And an owner "John Smith" with email "john@example.com"
    And an animal "Max" breed "Labrador" belonging to "John Smith"
    And I am authenticated as VET

  Scenario: Create an animal record (new patient)
    When I create an animal "Luna" breed "Persian Cat" for owner "John Smith"
    Then the animal is created in the clinic
    And its medical record is empty
    And the owner "John Smith" is linked to "Luna"

  Scenario: RECEPTIONIST cannot write to a record
    Given I am authenticated as RECEPTIONIST
    When I attempt to add an examination for "Max"
    Then the system rejects with code "INSUFFICIENT_PERMISSIONS"

  Scenario: Tenant isolation — cannot see animals from another clinic
    Given an animal "Rocky" in clinic "Desert Vets"
    When I list the animals of "Happy Paws"
    Then "Rocky" does not appear in the list

  Scenario: Add an examination to the record
    When I add an examination for "Max" with diagnosis "Bacterial otitis" and treatment "Ear cleaning + antibiotics 7 days"
    Then the examination appears in the history of "Max"
    And it is timestamped with today's date
    And it bears the current veterinarian as author

  Scenario: View complete history
    Given 3 examinations in the record of "Max"
    When I view the record of "Max"
    Then I see 3 examinations in reverse chronological order

  Scenario: Create a prescription
    Given an existing examination for "Max"
    When I create a prescription with medication "Amoxicilline 250mg" dosage "2x/day for 7 days"
    Then the prescription is created with license number "TEST-VET-001"
    And it is linked to the examination

  Scenario: A record is never deleted
    Given an examination in the record of "Max"
    When I attempt to delete this examination
    Then the system rejects with code "MEDICAL_RECORD_IMMUTABLE"
    And the examination is still visible in the history
