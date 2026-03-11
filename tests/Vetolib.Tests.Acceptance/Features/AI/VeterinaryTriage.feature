@wip
Feature: AI Veterinary Triage
  As a veterinarian or receptionist
  I want AI to analyze pet symptoms and suggest severity
  So that I can prioritize appointments and estimate consultation duration

  Background:
    Given I am authenticated as a user with role "Vet"

  Scenario: Triage returns a valid suggestion
    When I submit a triage request with:
      | Symptoms                                        | Species | Breed   | AgeMonths | WeightKg |
      | Vomiting for 2 days and refusing to eat          | cat     | persian | 36        | 4.2      |
    Then I should receive a triage suggestion with status 200
    And the suggestion should contain a severity of "Normal", "Emergency", or "Routine"
    And the suggestion should contain an estimated duration in minutes
    And the suggestion should contain a recommended specialty
    And the suggestion should contain a confidence score between 0 and 1
    And the suggestion should contain a non-empty disclaimer
    And the suggestion should contain a triage ID

  Scenario: Triage accepted by veterinarian
    Given I submitted a triage request and received a suggestion with triageId
    When the veterinarian accepts the triage suggestion
    Then the triage result should be persisted with WasAccepted true
    And OverriddenSeverity should be null

  Scenario: Triage overridden by veterinarian
    Given I submitted a triage request and received a suggestion with severity "Normal"
    When the veterinarian overrides the severity to "Emergency"
    Then the triage result should be persisted with WasAccepted false
    And OverriddenSeverity should be "Emergency"

  Scenario: Emergency symptoms detected
    When I submit a triage request with:
      | Symptoms                                              | Species |
      | Dog ate chocolate 1 hour ago, trembling and vomiting   | dog     |
    Then the suggestion severity should be "Emergency"

  Scenario: Triage with missing symptoms returns validation error
    When I submit a triage request with:
      | Symptoms | Species |
      |          | cat     |
    Then I should receive a validation error for "Symptoms"

  Scenario: Triage with missing species returns validation error
    When I submit a triage request with:
      | Symptoms               | Species |
      | Cat is scratching a lot |         |
    Then I should receive a validation error for "Species"

  Scenario: Disclaimer is always present and constant
    When I submit a triage request with:
      | Symptoms             | Species |
      | Routine annual checkup | dog     |
    Then the disclaimer should contain "AI" and "informational purposes only"
    And the disclaimer should not change between requests

  Scenario: AI service unavailable returns graceful error
    Given the AI service is unavailable
    When I submit a triage request with:
      | Symptoms         | Species |
      | General checkup   | cat     |
    Then I should receive an error "AI_SERVICE_UNAVAILABLE"
    And the HTTP status should be 503

  Scenario: Triage result is persisted for audit
    When I submit a triage request with:
      | Symptoms                  | Species | Breed    | AgeMonths | WeightKg |
      | Limping on front left paw  | dog     | labrador | 60        | 30.0     |
    Then a triage result should be persisted in the database
    And the persisted result should include the model used
    And the persisted result should include prompt and completion token counts
    And the persisted result should include latency in milliseconds

  Scenario: Receptionist can also request triage
    Given I am authenticated as a user with role "Receptionist"
    When I submit a triage request with:
      | Symptoms       | Species |
      | Ear infection   | dog     |
    Then I should receive a triage suggestion with status 200

  Scenario: Unauthorized user cannot request triage
    Given I am authenticated as a user with role "Owner"
    When I submit a triage request with:
      | Symptoms       | Species |
      | Ear infection   | dog     |
    Then I should receive a 403 Forbidden response
