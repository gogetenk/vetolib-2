@wip
Feature: Recurring appointment series
  As a clinic receptionist
  I want to schedule recurring appointments
  In order to plan follow-up visits for ongoing treatments

  Background:
    Given a clinic "Happy Paws" with hours 9am-6pm
    And a veterinarian "Dr. Ahmed Al-Rashid" with license "UAE-VET-12345"
    And an animal "Bella" breed "Persian Cat" belonging to "Sara Al-Mansoori"
    And I am authenticated as RECEPTIONIST

  # --- Series Creation ---

  Scenario: Create a weekly recurring series of 4 appointments
    When I create a weekly recurring appointment for "Bella" with "Dr. Ahmed" starting "2026-04-06" at "10:00" for 30 minutes repeating 4 times
    Then 4 appointments are created
    And the appointments are scheduled on "2026-04-06", "2026-04-13", "2026-04-20", and "2026-04-27"
    And all appointments have status "SCHEDULED"
    And all appointments belong to the same series

  Scenario: Create a monthly recurring series
    When I create a monthly recurring appointment for "Bella" with "Dr. Ahmed" starting "2026-04-06" at "14:00" for 30 minutes repeating 3 times
    Then 3 appointments are created
    And the appointments are scheduled on "2026-04-06", "2026-05-06", and "2026-06-06"

  # --- Series Cancellation ---

  Scenario: Cancel all future appointments in a series
    Given a weekly recurring series for "Bella" starting "2026-04-06" with 4 appointments
    And the appointment on "2026-04-06" has been completed
    When I cancel all future appointments in the series
    Then the appointments on "2026-04-13", "2026-04-20", and "2026-04-27" are cancelled
    And the completed appointment on "2026-04-06" remains unchanged

  Scenario: Past completed appointments are preserved when cancelling a series
    Given a weekly recurring series for "Bella" starting "2026-04-06" with 4 appointments
    And the appointment on "2026-04-06" has been completed
    And the appointment on "2026-04-13" has been completed
    When I cancel all future appointments in the series
    Then 2 appointments remain with status "Completed"
    And 2 appointments are cancelled
