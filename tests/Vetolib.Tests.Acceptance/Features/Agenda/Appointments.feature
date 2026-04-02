# features/agenda/appointments.feature

Feature: Veterinary appointment management
  As a veterinary clinic receptionist
  I want to manage appointments
  In order to organize the veterinarians' schedule efficiently

  Background:
    Given a clinic "Happy Paws" with hours 9am-6pm
    And a veterinarian "Dr. Ahmed Al-Rashid" with license "UAE-VET-12345"
    And an animal "Max" breed "Labrador" belonging to "John Smith"
    And I am authenticated as RECEPTIONIST

  Scenario: Create an appointment in a free slot
    When I create an appointment for "Max" with "Dr. Ahmed" on "2030-06-01" at "10:00" for 30 minutes
    Then the appointment is created with status "SCHEDULED"
    And the appointment appears in "Dr. Ahmed" agenda at "10:00"

  Scenario: List appointments for the day
    Given an existing appointment for "Max" at "10:00"
    And an existing appointment for "Luna" at "14:00"
    When I view the agenda for "2030-06-01"
    Then I see 2 appointments in the list

  Scenario: Reject if slot already taken
    Given an existing appointment for "Max" with "Dr. Ahmed" at "10:00" for 30 minutes
    When I try to create an appointment for "Luna" with "Dr. Ahmed" at "10:00"
    Then the system rejects with code "APPOINTMENT_CONFLICT"
    And the message is "This time slot is already taken for this veterinarian"
    And the next available slots are suggested

  Scenario: Reject if partial overlap
    Given an existing appointment from "10:00" to "10:30" with "Dr. Ahmed"
    When I try to create an appointment from "10:15" to "10:45" with "Dr. Ahmed"
    Then the system rejects with code "APPOINTMENT_CONFLICT"

  Scenario: Reject outside business hours
    When I try to create an appointment at "19:00"
    Then the system rejects with code "OUTSIDE_BUSINESS_HOURS"
    And the message indicates the hours "9h00 - 18h00"

  Scenario: Reject in the past
    When I try to create an appointment on date "2020-01-01" at "10:00"
    Then the system rejects with code "PAST_DATE_NOT_ALLOWED"

  Scenario: Record patient arrival (check-in)
    Given an existing appointment for "Max" with "Dr. Ahmed" at "10:00" for 30 minutes
    When I update the last appointment status to "CheckedIn"
    Then the appointment status is "CheckedIn"

  Scenario: Start the consultation
    Given an existing appointment for "Max" with "Dr. Ahmed" at "10:00" for 30 minutes
    And the last appointment status has been updated to "CheckedIn"
    When I update the last appointment status to "InProgress"
    Then the appointment status is "InProgress"

  Scenario: Complete the consultation
    Given an existing appointment for "Max" with "Dr. Ahmed" at "10:00" for 30 minutes
    And the last appointment status has been updated to "CheckedIn"
    And the last appointment status has been updated to "InProgress"
    When I update the last appointment status to "Completed"
    Then the appointment status is "Completed"

  Scenario: Cancel an appointment with reason
    Given an existing appointment for "Max" with "Dr. Ahmed" at "10:00" for 30 minutes
    When I cancel the last appointment with reason "Owner called to cancel"
    Then the appointment status is "Cancelled"

  Scenario: Invalid transition rejected
    Given an existing appointment for "Max" with "Dr. Ahmed" at "10:00" for 30 minutes
    When I update the last appointment status to "Completed"
    Then the system rejects the transition with code "INVALID_TRANSITION"

  Scenario: View veterinarian availability
    Given an existing appointment for "Max" with "Dr. Ahmed" at "10:00" for 30 minutes
    When I check availability for "Dr. Ahmed" on "2030-06-01" for 30 minutes
    Then I see available and unavailable slots
    And the slot "10:00" is marked unavailable

  Scenario: Get appointment by ID
    Given an existing appointment for patient "Max" on "2030-06-01" at "10:00"
    When I request the appointment by its ID
    Then the operation succeeds
    And the appointment details include patient "Max" and time "10:00"

  Scenario: Looking up a non-existent appointment fails
    When I request appointment with a random non-existent ID
    Then the record is not found

  Scenario: Edit appointment date and time
    Given an existing appointment for patient "Max" on "2030-06-01" at "10:00"
    When I update the appointment to "2030-06-02" at "14:00"
    Then the operation succeeds
    And the appointment is now scheduled for "2030-06-02" at "14:00"

  Scenario: Edit appointment refused if slot conflict
    Given an existing appointment on "2030-06-02" at "14:00"
    And another appointment for "Max" on "2030-06-01" at "10:00"
    When I update the second appointment to "2030-06-02" at "14:00"
    Then a conflict is detected

  Scenario: Admin updates appointment status directly
    Given a clinic "Happy Paws"
    And I am authenticated as ADMIN
    And an existing appointment with status "SCHEDULED"
    When I update the appointment status to "NO_SHOW"
    Then the operation succeeds
    And the appointment status is "NoShow"

  Scenario: Updating an appointment with an invalid status is rejected
    Given a clinic "Happy Paws"
    And I am authenticated as ADMIN
    And an existing appointment with status "SCHEDULED"
    When I update the appointment status to "INVALID_STATUS"
    Then the request is rejected

  Scenario: Updating a non-existent appointment fails
    Given a clinic "Happy Paws"
    And I am authenticated as ADMIN
    When I update a non-existent appointment status to "NO_SHOW"
    Then the record is not found
