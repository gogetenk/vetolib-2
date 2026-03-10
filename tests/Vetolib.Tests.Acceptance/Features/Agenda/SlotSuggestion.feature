@wip
Feature: Slot Suggestion
  As a receptionist or veterinarian
  I want the system to suggest the best available appointment slots
  So that I can optimize the clinic schedule and reduce gaps

  Background:
    Given I am authenticated as a user with role "Receptionist"
    And the clinic has the following veterinarians:
      | Name       | Id                                   |
      | Dr. Ahmad  | 11111111-1111-1111-1111-111111111111 |
      | Dr. Fatima | 22222222-2222-2222-2222-222222222222 |

  Scenario: Suggest best slot with available gaps
    Given Dr. Ahmad has the following appointments on "2026-03-15":
      | StartTime | Duration | Type    |
      | 09:00     | 30       | general |
      | 10:30     | 30       | general |
    When I request a slot suggestion for:
      | ConsultationType | PreferredDate | PreferredTime |
      | general          | 2026-03-15    | 10:00         |
    Then I should receive 3 slot suggestions
    And the first suggestion should minimize the gap in Dr. Ahmad's schedule
    And each suggestion should include a score between 0 and 100

  Scenario: Load balancing across veterinarians
    Given Dr. Ahmad has 6 appointments on "2026-03-15"
    And Dr. Fatima has 2 appointments on "2026-03-15"
    When I request a slot suggestion for:
      | ConsultationType | PreferredDate | PreferredTime |
      | general          | 2026-03-15    | 14:00         |
    Then the highest scored suggestion should be for Dr. Fatima

  Scenario: Grouping by consultation type
    Given Dr. Ahmad has 3 "surgery" appointments in the morning on "2026-03-15"
    And Dr. Ahmad has available slots in the morning and afternoon
    When I request a slot suggestion for:
      | ConsultationType | PreferredDate | PreferredTime |
      | surgery          | 2026-03-15    | 09:00         |
    Then the highest scored suggestion should be in the morning for Dr. Ahmad

  Scenario: Fallback to default duration when insufficient history
    Given Dr. Ahmad has fewer than 5 past "dermatology" appointments
    When I request a slot suggestion for:
      | ConsultationType | PreferredDate | PreferredTime |
      | dermatology      | 2026-03-15    | 10:00         |
    Then the estimated duration should use the default for "dermatology"

  Scenario: Duration estimation from history
    Given Dr. Ahmad has completed 20 "general" appointments with average duration 25 minutes
    When I request a slot suggestion for:
      | ConsultationType       | PreferredDate | PreferredTime | PreferredVeterinarianId              |
      | general                | 2026-03-15    | 10:00         | 11111111-1111-1111-1111-111111111111 |
    Then the estimated duration should be approximately 25 minutes

  Scenario: No available slots on requested date
    Given all veterinarians are fully booked on "2026-03-15"
    When I request a slot suggestion for:
      | ConsultationType | PreferredDate | PreferredTime |
      | general          | 2026-03-15    | 10:00         |
    Then I should receive 0 slot suggestions

  Scenario: Preferred veterinarian specified
    When I request a slot suggestion for:
      | ConsultationType | PreferredDate | PreferredTime | PreferredVeterinarianId              |
      | general          | 2026-03-15    | 10:00         | 11111111-1111-1111-1111-111111111111 |
    Then all suggestions should be for Dr. Ahmad
