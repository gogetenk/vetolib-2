@wip
Feature: Veterinarian Messaging Inbox
  As a veterinarian
  I want to see and respond to medical messages with full patient context
  So that I can provide informed responses to pet owners

  Background:
    Given I am authenticated as a user with role "Vet"

  Scenario: Vet sees medical messages in priority order
    Given there are messages: one "MedicalUrgency", one "PostOperativeFollowUp", one "MedicalQuestion"
    When I open the Messages inbox
    Then "MedicalUrgency" should appear first (red background)
    Then "PostOperativeFollowUp" should appear second
    Then "MedicalQuestion" should appear third

  Scenario: Emergency messages always appear at the top
    Given there is a "MedicalQuestion" from 2 hours ago
    And there is a "MedicalUrgency" from 5 minutes ago
    When I open the Messages inbox
    Then the "MedicalUrgency" should be first regardless of the older message

  Scenario: Vet sees full medical context for a message
    Given I open a message linked to patient "Luna" (cat, 3 years old)
    Then I should see alongside the message:
      | Context                      |
      | Last examination date        |
      | Current prescriptions        |
      | Known allergies              |
      | Vaccination history          |

  Scenario: Vet sees AI conversation summary for long threads
    Given a conversation has more than 5 messages
    When I open the conversation
    Then I should see an AI-generated summary at the top
    And the summary should be collapsible
    And the summary should be factual (3-5 sentences, no medical interpretation)

  Scenario: Vet adds an internal note
    Given I open a conversation with an owner
    When I click "Add internal note"
    And I type "Suspect potential kidney issue based on symptoms described. Schedule blood work."
    And I click "Save note"
    Then the note should appear in the conversation thread
    And the note should be visually distinct (marked as "Internal note")
    And the owner should NOT see this note

  Scenario: Vet replies with a modified AI suggestion
    Given I open a message with 3 AI-suggested replies
    When I click the second suggestion
    And I modify the text to add specific medical advice
    And I click "Send"
    Then the reply should be sent to the owner
    And the system should record WasSuggestedReplyUsed as false (modified)
    And the system should record the ActualReply

  Scenario: Vet creates an urgent appointment from a message
    Given I open an emergency message about patient "Buddy"
    When I click "Create urgent appointment"
    Then a new appointment form should open with:
      | Field    | Pre-filled value             |
      | Patient  | Buddy                        |
      | Type     | Emergency                    |
      | Reason   | Extracted from message text   |
    And the appointment should be created in the next available slot

  Scenario: Vet attaches message content to medical record
    Given I open a message where the owner describes symptoms and attached a photo
    When I click "Add to medical record"
    Then the message text and photos should be added as a note in the patient's medical record
    And a confirmation should appear "Added to Luna's medical record"

  Scenario: Vet receives push notification for emergency
    Given an owner sends a message classified as "MedicalUrgency"
    Then I should receive a browser push notification immediately
    And the notification should show the patient name and a preview of the message

  Scenario: Emergency escalation after 10 minutes without viewing
    Given an emergency message was received 10 minutes ago
    And no veterinarian has viewed the message
    Then an escalation notification should be sent to all veterinarians of the clinic
    And the notification should include "URGENT -- unread emergency message"
