@wip
Feature: WhatsApp integration for clinic messaging
  As a clinic administrator
  I want to integrate WhatsApp with my clinic
  So that appointment reminders and messages reach pet owners on their preferred channel

  Background:
    Given a clinic "Dubai Pet Care" with an active subscription

  # --- Configuration ---

  Scenario: Admin configures WhatsApp credentials
    Given I am authenticated as an admin of "Dubai Pet Care"
    When I enter the WhatsApp Business API credentials
    And I save the configuration
    Then the WhatsApp integration is marked as configured

  Scenario: Admin tests WhatsApp connection
    Given I am authenticated as an admin of "Dubai Pet Care"
    And WhatsApp credentials have been configured
    When I click "Test connection"
    Then the system sends a test message to the admin's phone number
    And a success confirmation is displayed "WhatsApp connection is working"

  Scenario: Test connection fails with invalid credentials
    Given I am authenticated as an admin of "Dubai Pet Care"
    And WhatsApp credentials have been configured with an invalid API key
    When I click "Test connection"
    Then an error is displayed "WhatsApp connection failed. Please verify your credentials."

  # --- Appointment reminders ---

  Scenario: Appointment reminder is sent via WhatsApp
    Given WhatsApp is configured for "Dubai Pet Care"
    And owner "Fatima Al Mansoori" has opted in to WhatsApp notifications
    And "Fatima Al Mansoori" has an appointment tomorrow at 10:00 for pet "Luna"
    When the reminder schedule runs 24 hours before the appointment
    Then a WhatsApp message is sent to "Fatima Al Mansoori" with the appointment details

  Scenario: Reminder includes appointment details
    Given WhatsApp is configured for "Dubai Pet Care"
    And owner "Ahmed Al Rashid" has opted in to WhatsApp notifications
    And "Ahmed Al Rashid" has an appointment on Sunday at 14:30 for pet "Rocky" with Dr. Omar
    When the appointment reminder is sent
    Then the WhatsApp message contains:
      | Detail          | Value                     |
      | Clinic name     | Dubai Pet Care            |
      | Date and time   | Sunday at 14:30           |
      | Pet name        | Rocky                     |
      | Veterinarian    | Dr. Omar                  |

  # --- Opt-in enforcement ---

  Scenario: Owner without opt-in does not receive WhatsApp messages
    Given WhatsApp is configured for "Dubai Pet Care"
    And owner "Sara Al Dhaheri" has NOT opted in to WhatsApp notifications
    And "Sara Al Dhaheri" has an appointment tomorrow at 09:00
    When the reminder schedule runs
    Then no WhatsApp message is sent to "Sara Al Dhaheri"

  Scenario: Owner opts out of WhatsApp notifications
    Given owner "Fatima Al Mansoori" had previously opted in to WhatsApp notifications
    When "Fatima Al Mansoori" opts out of WhatsApp notifications
    Then future reminders are not sent via WhatsApp to "Fatima Al Mansoori"
    And the opt-out is recorded with a timestamp

  # --- Non-admin restrictions ---

  Scenario: Non-admin cannot configure WhatsApp
    Given I am authenticated as a vet of "Dubai Pet Care"
    When I try to access the WhatsApp configuration
    Then access is denied with the message "Only administrators can configure integrations"
