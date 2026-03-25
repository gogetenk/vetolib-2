@wip
Feature: Patient weight history
  As a veterinarian,
  I want to record weight measurements over time for each patient
  so that I can track growth curves and detect health issues early.

  Background:
    Given a clinic "Al Barsha Vet Clinic"
    And an owner "Ahmed Al Rashid" with email "ahmed@example.ae"
    And a patient "Layla" species "Horse" breed "Arabian" belonging to "Ahmed Al Rashid"
    And I am authenticated as VET

  Scenario: Record a weight measurement
    When I record a weight of 450.5 kg for "Layla" with note "Post-competition weigh-in"
    Then the weight history of "Layla" contains 1 entry
    And the latest weight of "Layla" is 450.5 kg

  Scenario: Record multiple weights over time
    Given the following weight entries for "Layla":
      | Date       | WeightKg | Note                |
      | 2026-01-15 | 420.0    | Initial assessment  |
      | 2026-02-15 | 435.0    | Monthly check       |
      | 2026-03-15 | 450.5    | Gaining well        |
    When I view the weight history of "Layla"
    Then I see 3 entries in chronological order
    And the latest weight of "Layla" is 450.5 kg

  Scenario: Weight must be positive
    When I attempt to record a weight of 0 kg for "Layla"
    Then the system rejects with reason "Weight must be greater than zero"

  Scenario: Weight must be reasonable
    When I attempt to record a weight of 50000 kg for "Layla"
    Then the system rejects with reason "Weight exceeds maximum allowed value"

  Scenario: Weight entry updates current weight on patient
    Given the current weight of "Layla" is 420.0 kg
    When I record a weight of 450.5 kg for "Layla"
    Then the current weight displayed on "Layla" patient card is 450.5 kg

  Scenario: View weight curve data
    Given the following weight entries for "Layla":
      | Date       | WeightKg |
      | 2026-01-15 | 420.0    |
      | 2026-02-15 | 435.0    |
      | 2026-03-15 | 450.5    |
    When I request the weight curve for "Layla"
    Then I receive data points suitable for chart display
    And the data spans from 2026-01-15 to 2026-03-15

  Scenario: Weight history is per-patient
    Given a patient "Storm" species "Horse" breed "Arabian" belonging to "Ahmed Al Rashid"
    And I record a weight of 380.0 kg for "Storm"
    When I view the weight history of "Layla"
    Then the history does not contain entries from "Storm"

  Scenario: ASSISTANT can view weight history but not record
    Given I am authenticated as ASSISTANT
    When I view the weight history of "Layla"
    Then I see the weight history
    When I attempt to record a weight of 460.0 kg for "Layla"
    Then the system rejects with reason "Insufficient permissions"
