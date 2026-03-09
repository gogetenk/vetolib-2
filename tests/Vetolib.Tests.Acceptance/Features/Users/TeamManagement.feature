# language: en
Feature: Team Management
  As a clinic admin
  I want to manage my clinic team members
  So that staff can access Vetolib with the right permissions

  Background:
    Given I am a clinic admin "admin@desertpaws.ae" in clinic "desert-paws-001"

  Scenario: Admin lists team members
    Given the team has a vet "dr.omar@desertpaws.ae" and a receptionist "amira@desertpaws.ae"
    When I call GET /api/users
    Then I receive a list with 3 members

  Scenario: Admin invites a new user
    When I invite a new user with email "dr.new@desertpaws.ae", name "Dr. New Vet", role "Vet"
    Then the invitation succeeds
    And the response contains a temporary password
    And the new user appears in the team list

  Scenario: Admin changes a user role
    Given there is a user "amira@desertpaws.ae" with role "Receptionist"
    When I change "amira@desertpaws.ae" role to "Assistant"
    Then the role change succeeds
    And "amira@desertpaws.ae" has role "Assistant"

  Scenario: Admin deactivates a user
    Given there is an active user "dr.sarah@desertpaws.ae" with role "Vet"
    When I deactivate "dr.sarah@desertpaws.ae"
    Then the deactivation succeeds
    And "dr.sarah@desertpaws.ae" is inactive

  Scenario: Admin cannot deactivate themselves
    When I try to deactivate myself
    Then I receive a 400 error with message "Cannot deactivate your own account"

  Scenario: Non-admin cannot access user management
    Given I am a vet "dr.sarah@desertpaws.ae" in clinic "desert-paws-001"
    When I call GET /api/users as a vet
    Then I receive a 403 error

  Scenario: Deactivated user cannot log in
    Given there is an active user "dr.inactive@desertpaws.ae" with role "Vet"
    And "dr.inactive@desertpaws.ae" has been deactivated by admin
    When "dr.inactive@desertpaws.ae" tries to log in
    Then login fails with message "Your account has been deactivated"
