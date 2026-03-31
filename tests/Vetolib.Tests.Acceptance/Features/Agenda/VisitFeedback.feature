@wip
Feature: Post-visit feedback collection
  As a clinic manager
  I want to collect feedback from pet owners after their visit
  In order to measure satisfaction and improve service quality

  Background:
    Given a clinic "Happy Paws" with identifier "clinic-happy-paws"
    And an animal "Max" breed "Labrador" belonging to "John Smith"
    And a veterinarian "Dr. Ahmed Al-Rashid" with license "UAE-VET-12345"

  # --- Submitting Feedback ---

  Scenario: Pet owner submits feedback after a completed visit
    Given a completed appointment for "Max" with "Dr. Ahmed"
    When the owner of "Max" submits feedback with a rating of 9 out of 10 and comment "Excellent care for Max"
    Then the feedback is recorded for that appointment
    And the feedback shows rating 9 and comment "Excellent care for Max"

  # --- Validation ---

  Scenario: Cannot submit feedback for a non-completed appointment
    Given a scheduled appointment for "Max" with "Dr. Ahmed"
    When the owner of "Max" attempts to submit feedback for that appointment
    Then the system rejects with code "APPOINTMENT_NOT_COMPLETED"
    And the message indicates feedback can only be submitted after a completed visit

  Scenario: Only one feedback per appointment is allowed
    Given a completed appointment for "Max" with "Dr. Ahmed"
    And the owner has already submitted feedback for that appointment
    When the owner of "Max" attempts to submit feedback again
    Then the system rejects with code "FEEDBACK_ALREADY_SUBMITTED"
    And the message indicates that feedback was already provided for this visit

  # --- Reporting ---

  Scenario: View aggregated feedback statistics including NPS
    Given 10 completed appointments with feedback ratings: 9, 10, 8, 10, 9, 7, 10, 6, 9, 10
    When I view the feedback statistics for "Happy Paws"
    Then the average rating is 8.8
    And the NPS score is calculated based on promoters, passives, and detractors
