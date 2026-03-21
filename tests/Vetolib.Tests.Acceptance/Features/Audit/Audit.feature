@wip
Feature: Audit trail
  As a clinic administrator
  I want to query the audit log
  So that I can review changes made across all modules

  Background:
    Given a clinic "Happy Paws"

  Scenario: Admin can query audit log
    Given I am authenticated as ADMIN
    And a patient was created
    When I query audit for entityType "Patient"
    Then I see an audit entry with action "Created"
    And the entry contains a changedBy value

  Scenario: Non-admin cannot access audit
    Given I am authenticated as VET
    When I query audit
    Then the user is denied access
