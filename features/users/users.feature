# language: en
Feature: Team Management
  As a clinic admin
  I want to manage my clinic's team members
  So that staff can access Vetolib with the right permissions

  Background:
    Given I am logged in as admin "admin@desertpaws.ae" of clinic "clinic-001"
    And the team has 3 members: 1 admin, 1 vet, 1 receptionist

  Scenario: Admin sees the full team list
    When I navigate to Settings > Team
    Then I see a table with 3 rows
    And each row shows name, email, role badge, and status

  Scenario: Admin invites a new vet
    When I click "Invite Member"
    And I fill in email "dr.omar@desertpaws.ae", name "Dr. Omar Khalil", role "VET"
    And I click "Send Invite"
    Then I see a temporary password displayed once
    And the message "Share this password securely — it won't be shown again."
    And the new user appears in the team list with status "Active"

  Scenario: Admin changes a user's role
    Given there is a RECEPTIONIST "amira@desertpaws.ae" in the team
    When I click "Change Role" on Amira's row
    And I select "ASSISTANT" and confirm
    Then Amira's role badge shows "ASSISTANT"

  Scenario: Admin deactivates a team member
    Given there is an active VET "dr.sarah@desertpaws.ae" in the team
    When I click "Deactivate" on Dr. Sarah's row and confirm
    Then Dr. Sarah's status shows "Inactive"
    And Dr. Sarah can no longer log in

  Scenario: Admin cannot deactivate themselves
    When I look at my own row in the team table
    Then the "Deactivate" button is disabled or absent

  Scenario: Non-admin cannot access team management
    Given I am logged in as vet "dr.sarah@desertpaws.ae"
    When I navigate to "/settings/team"
    Then I am redirected to the appointments page
    And I do not see "Team" in the sidebar navigation

  Scenario: Deactivated user cannot log in
    Given user "dr.sarah@desertpaws.ae" has been deactivated
    When they try to login with their credentials
    Then they see "Your account has been deactivated. Contact your clinic admin."
