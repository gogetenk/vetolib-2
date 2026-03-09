# language: en
Feature: Appointments
  As a clinic receptionist or vet
  I want to manage appointments
  So that the clinic schedule runs smoothly

  Background:
    Given I am logged in as receptionist "reception@desertpaws.ae" of clinic "clinic-001"
    And the clinic has vets: "Dr. Sarah Johnson" and "Dr. Ahmed Khalil"

  Scenario: Receptionist sees today's appointments list
    Given there are 3 appointments scheduled for today
    When I navigate to the Appointments page
    Then I see a table with 3 rows
    And each row shows: patient name, species icon, vet name, time, and status badge

  Scenario: Create a new appointment
    Given I am on the Appointments page
    When I click "New Appointment"
    And I fill in patient name "Simba", species "Cat", owner "Khalid Al-Mansoori"
    And I select vet "Dr. Sarah Johnson" and time slot "10:30"
    And I click "Save"
    Then the new appointment appears in the list with status "SCHEDULED"
    And I see a success notification

  Scenario: Check in a patient on arrival
    Given there is a SCHEDULED appointment for "Max" at 09:00
    When I open that appointment
    And I click "Check In"
    And I confirm the action
    Then the status badge changes to "CHECKED_IN"

  Scenario: Vet starts and completes a consultation
    Given I am logged in as vet "dr.sarah@desertpaws.ae"
    And there is a CHECKED_IN appointment for "Max"
    When I click "Start Consultation"
    Then the status becomes "IN_PROGRESS"
    When I click "Complete"
    Then the status becomes "COMPLETED"
    And the appointment appears in the completed section

  Scenario: Cancel an appointment with reason
    Given there is a SCHEDULED appointment for "Coco"
    When I click "Cancel"
    And I enter reason "Owner called to cancel"
    And I confirm
    Then the status becomes "CANCELLED"
    And the cancellation reason is displayed

  Scenario: Cannot schedule on a time slot already taken by same vet
    Given "Dr. Sarah Johnson" has an appointment at 10:30
    When I try to book "Dr. Sarah Johnson" at 10:30 for a new patient
    Then I see "This time slot is not available for Dr. Sarah Johnson"
    And the form stays open for correction

  Scenario: Receptionist cannot access medical record from appointment
    Given there is a COMPLETED appointment for "Luna"
    When I open that appointment as a receptionist
    Then I do not see a "View Medical Record" button

  Scenario: Filter appointments by status
    Given there are appointments with statuses: SCHEDULED, CHECKED_IN, COMPLETED
    When I select "Scheduled" from the status filter
    Then I only see appointments with status "SCHEDULED"

  Scenario: Vet sees only their own appointments by default
    Given I am logged in as vet "dr.sarah@desertpaws.ae"
    When I navigate to Appointments
    Then the vet filter is pre-set to "Dr. Sarah Johnson"
    And I see a "Show all" option to remove the filter
