@wip
# language: en
Feature: Preferences Integration -- cross-module opt-in/opt-out
  As a clinic administrator or staff member
  I want preferences to control which AI features and notifications are active
  So that the system respects user and clinic-level opt-in/opt-out decisions

  Background:
    Given I am authenticated as a user with role "Vet"

  Scenario: Owner without User account always receives reminder
    When an appointment reminder event is published for an owner email without UserId
    Then an email is sent to the owner email

  Scenario: Owner always receives invoice email
    When an invoice sent event is published for an owner email without UserId
    Then an invoice email is sent to the owner email

  Scenario: AI triage is allowed by default
    When a vet requests AI triage for a dog with symptoms "Limping on front left paw"
    Then the triage response is successful or AI service unavailable

  Scenario: AI triage respects user preference disabled
    Given the current user has preference "AITriage" set to "false"
    When a vet requests AI triage for a dog with symptoms "Limping on front left paw"
    Then the response indicates AI triage is disabled with error "AI_TRIAGE_DISABLED"

  Scenario: AI triage enabled after preference is enabled
    Given the current user has preference "AITriage" set to "true"
    When a vet requests AI triage for a dog with symptoms "Limping on front left paw"
    Then the triage response is successful or AI service unavailable

  Scenario: Drug interaction check ignores preferences
    When the drug interaction check is performed
    Then the drug interaction check always executes

  Scenario: AI no-show prediction is disabled by preference
    Given the current user has preference "AINoShow" set to "false"
    When a vet requests no-show prediction for an appointment
    Then the response indicates AI no-show is disabled with error "AI_NOSHOW_DISABLED"
