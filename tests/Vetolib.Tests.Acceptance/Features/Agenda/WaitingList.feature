@wip
Feature: Appointment waiting list
  As a clinic receptionist
  I want to manage a waiting list for fully booked time slots
  In order to fill cancellations quickly and reduce lost revenue

  Background:
    Given a clinic "Happy Paws" with hours 9am-6pm
    And a veterinarian "Dr. Ahmed Al-Rashid" with license "UAE-VET-12345"
    And an animal "Max" breed "Labrador" belonging to "John Smith"
    And I am authenticated as RECEPTIONIST

  # --- Adding to Waitlist ---

  Scenario: Add a patient to the waiting list for a fully booked day
    Given all slots for "Dr. Ahmed" on "2026-04-10" are booked
    When I add "Max" to the waiting list for "Dr. Ahmed" on "2026-04-10"
    Then "Max" appears on the waiting list for "Dr. Ahmed" on "2026-04-10"
    And the waitlist entry has status "WAITING"

  # --- Notification on Cancellation ---

  Scenario: Pet owner is notified when a slot opens up
    Given "Max" is on the waiting list for "Dr. Ahmed" on "2026-04-10"
    When an appointment for "Dr. Ahmed" on "2026-04-10" is cancelled
    Then a notification is sent to the owner of "Max" about the available slot

  # --- Removal ---

  Scenario: Remove a patient from the waiting list
    Given "Max" is on the waiting list for "Dr. Ahmed" on "2026-04-10"
    When I remove "Max" from the waiting list
    Then "Max" no longer appears on the waiting list for "Dr. Ahmed" on "2026-04-10"

  # --- Expiry ---

  Scenario: Waiting list entry expires after 48 hours without response
    Given "Max" was added to the waiting list for "Dr. Ahmed" on "2026-04-10" 49 hours ago
    When the system processes expired waitlist entries
    Then the waitlist entry for "Max" is marked as "EXPIRED"
    And "Max" no longer appears on the active waiting list
