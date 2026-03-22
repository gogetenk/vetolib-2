
Feature: Receptionist Messaging Inbox
  As a receptionist
  I want to see and respond to appointment requests and administrative questions
  So that I can handle owner inquiries efficiently

  Background:
    Given I am authenticated as a user with role "Receptionist"

  Scenario: Receptionist sees only relevant messages
    Given there are messages categorized as "AppointmentRequest", "Administrative", and "MedicalQuestion"
    When I open the Messages inbox
    Then I should see messages categorized as "AppointmentRequest" and "Administrative"
    And I should NOT see messages categorized as "MedicalQuestion"
    And I should NOT see messages categorized as "MedicalUrgency"

  Scenario: Messages are sorted by priority then by date
    Given there are 3 messages: one "Administrative" from yesterday, one "AppointmentRequest" from today, one "Administrative" from today
    When I open the Messages inbox
    Then the "AppointmentRequest" message should appear first (higher priority)
    And the two "Administrative" messages should be sorted oldest first

  Scenario: Receptionist replies using AI suggestion
    Given I open a message from an owner asking about appointment availability
    And the AI has generated 2 suggested replies
    When I click on the first suggestion
    Then the reply field should be pre-filled with the suggestion text
    When I modify the text and click "Send"
    Then the reply should be sent to the owner
    And the message status should change to "InProgress"

  Scenario: Receptionist uses a quick response template
    Given the clinic has configured a template "Appointment confirmation"
    When I open a message and click "Templates"
    And I select the "Appointment confirmation" template
    Then the reply field should be pre-filled with the template text
    And I can modify it before sending

  Scenario: Receptionist transfers a medical message to vet
    Given I receive a message flagged as "Triage uncertain -- please verify category"
    And the message describes medical symptoms
    When I click "Transfer to veterinarian"
    Then the message should disappear from my inbox
    And it should appear in the vet inbox with a note "Transferred by [Receptionist Name]"

  Scenario: Receptionist converts a message to an appointment
    Given I open a message requesting an appointment for pet "Buddy"
    When I click "Convert to appointment"
    Then a new appointment form should open
    And the patient field should be pre-filled with "Buddy"
    And the owner field should be pre-filled
    And the reason should contain the message content

  Scenario: Receptionist marks a message as spam
    Given I open a message that is clearly spam
    When I click "Mark as spam"
    Then the message should disappear from my inbox
    And the admin should be able to view it in the spam folder

  Scenario: Receptionist sees patient context alongside message
    Given I open a message linked to patient "Buddy"
    Then I should see alongside the message: pet name, species, last appointment date, and outstanding invoices
    And I should NOT see medical records (consistent with receptionist RBAC)
