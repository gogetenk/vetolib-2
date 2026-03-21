@wip
# language: en
Feature: Onboarding State Management
  As a user
  I want my onboarding progress to be tracked server-side
  So that it persists across sessions and devices

  Background:
    Given a clinic "Desert Paws" exists
    And a user with role "Admin" exists in the clinic

  Scenario: New admin gets onboarding state initialized on first login
    Given the user has never logged in before
    When the user logs in for the first time
    Then the onboarding state is initialized with role "Admin"
    And the welcome banner is marked as "visible"
    And the checklist is marked as "visible"
    And all checklist steps are in "pending" status

  Scenario: Retrieve onboarding state
    Given the user has an onboarding state initialized
    When the user retrieves their onboarding state
    Then the response contains the welcome banner status
    And the response contains checklist steps with completion status
    And the response contains the progress count

  Scenario: Complete an onboarding step
    Given the user has an onboarding state initialized
    When the step "add_first_patient" is marked as completed
    Then the step "add_first_patient" has status "completed"
    And the progress count is updated

  Scenario: Auto-complete steps based on existing data
    Given the clinic already has 5 patients
    And the clinic already has 2 appointments
    When the user retrieves their onboarding state
    Then the step "add_first_patient" is auto-completed
    And the step "book_first_appointment" is auto-completed

  Scenario: Dismiss welcome banner
    Given the user has an onboarding state initialized
    When the user dismisses the welcome banner
    Then the welcome banner is marked as "dismissed"
    And the checklist remains "visible"

  Scenario: Dismiss checklist
    Given the user has an onboarding state initialized
    When the user dismisses the checklist
    Then the checklist is marked as "dismissed"
    And the welcome banner status is unchanged

  Scenario: All steps completed triggers onboarding completion
    Given the user has all checklist steps completed
    When the user retrieves their onboarding state
    Then the onboarding status is "completed"

  Scenario: Onboarding state persists across sessions
    Given the user has dismissed the welcome banner
    When the user logs out and logs back in
    Then the welcome banner is still marked as "dismissed"

  Scenario: Vet gets role-specific checklist
    Given a user with role "Vet" exists in the clinic
    When the vet user retrieves their onboarding state
    Then the checklist contains 4 steps specific to the Vet role

  Scenario: Receptionist gets role-specific checklist
    Given a user with role "Receptionist" exists in the clinic
    When the receptionist user retrieves their onboarding state
    Then the checklist contains 4 steps specific to the Receptionist role

  Scenario: Assistant gets role-specific checklist
    Given a user with role "Assistant" exists in the clinic
    When the assistant user retrieves their onboarding state
    Then the checklist contains 3 steps specific to the Assistant role

  Scenario: Non-authenticated user cannot access onboarding state
    When an unauthenticated request is made to the onboarding endpoint
    Then the user must sign in
