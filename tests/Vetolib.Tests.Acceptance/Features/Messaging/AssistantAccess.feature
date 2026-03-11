@wip
Feature: Assistant Messaging Access
  As an assistant
  I want to view messaging conversations in read-only mode
  So that I can stay informed without modifying anything

  Background:
    Given I am authenticated as a user with role "Assistant"

  Scenario: Assistant can view non-medical conversations
    Given there are conversations categorized as "AppointmentRequest" and "Administrative"
    When I open the Messages inbox
    Then I should see these conversations
    And I should NOT see a reply button
    And I should NOT see action buttons (transfer, convert to appointment)

  Scenario: Assistant cannot view medical conversations
    Given there are conversations categorized as "MedicalUrgency" and "MedicalQuestion"
    When I open the Messages inbox
    Then I should NOT see these conversations
