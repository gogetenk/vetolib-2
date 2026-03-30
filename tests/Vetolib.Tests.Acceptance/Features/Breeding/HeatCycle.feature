Feature: Heat cycle tracking
  As a veterinarian or breeder,
  I want to record heat cycles for female patients
  so that I can predict optimal breeding windows and monitor reproductive health.

  Background:
    Given a clinic "Palm Jumeirah Vet"
    And an owner "Sara Al Blooshi" with email "sara@example.ae"
    And a patient "Dalma" species "Dog" breed "Saluki" sex "Female" belonging to "Sara Al Blooshi"
    And I am authenticated as VET

  Scenario: Record a heat cycle
    When I record a heat cycle for "Dalma" starting on 2026-01-10 ending on 2026-01-25
    Then the heat cycle is recorded for "Dalma"

  Scenario: Record multiple heat cycles
    Given the following heat cycles for "Dalma":
      | StartDate  | EndDate    |
      | 2025-07-10 | 2025-07-25 |
      | 2026-01-10 | 2026-01-25 |
    When I view the heat cycle history of "Dalma"
    Then I see 2 cycles in chronological order

  Scenario: Predict next heat cycle
    Given the following heat cycles for "Dalma":
      | StartDate  | EndDate    |
      | 2025-01-15 | 2025-01-30 |
      | 2025-07-10 | 2025-07-25 |
      | 2026-01-10 | 2026-01-25 |
    When I request the predicted next heat for "Dalma"
    Then the system predicts the next heat around 2026-07-10
    And the average cycle interval is approximately 180 days

  Scenario: End date must be after start date
    When I attempt to record a heat cycle for "Dalma" starting on 2026-03-15 ending on 2026-03-10
    Then the system rejects with reason "End date must be after start date"

  Scenario: Only female patients can have heat cycles
    Given a patient "Zayed" species "Dog" breed "Saluki" sex "Male" belonging to "Sara Al Blooshi"
    When I attempt to record a heat cycle for "Zayed"
    Then the system rejects with reason "Only female patients can have heat cycles recorded"

  Scenario: Spayed patients cannot have heat cycles
    Given a patient "Noura" species "Dog" breed "Saluki" sex "SpayedFemale" belonging to "Sara Al Blooshi"
    When I attempt to record a heat cycle for "Noura"
    Then the system rejects with reason "Spayed patients do not have heat cycles"

  Scenario: Add notes to a heat cycle
    When I record a heat cycle for "Dalma" starting on 2026-01-10 ending on 2026-01-25 with note "Strong signs, good candidate for breeding"
    Then the heat cycle is recorded with the note

  Scenario: Heat cycles are visible to VET and ADMIN only
    Given I am authenticated as ASSISTANT
    When I attempt to view the heat cycle history of "Dalma"
    Then the system rejects with reason "Insufficient permissions"
