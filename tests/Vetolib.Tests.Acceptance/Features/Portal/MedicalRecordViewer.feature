Feature: Medical Record Viewer
  As a pet owner
  I want to view my animals' medical records on the portal
  So that I can stay informed about their health history

  Background:
    Given a clinic "Desert Paws" exists with animals registered
    And an owner "Fatima Al Rashid" has a portal account linked to "Desert Paws"

  Scenario: Owner sees their animals list
    Given "Fatima Al Rashid" has a cat "Luna" and a dog "Rex" at "Desert Paws"
    When she views her animals on the portal
    Then she should see 2 animals listed
    And the list should include "Luna" and "Rex"

  Scenario: Owner views medical records for an animal
    Given "Fatima Al Rashid" has a cat "Luna" at "Desert Paws"
    And "Luna" has 3 medical records
    When she views the medical records for "Luna"
    Then she should see 3 medical records

  Scenario: Owner views prescriptions for an animal
    Given "Fatima Al Rashid" has a cat "Luna" at "Desert Paws"
    And "Luna" has an active prescription for "Amoxicillin"
    When she views the prescriptions for "Luna"
    Then she should see a prescription for "Amoxicillin"

  Scenario: Owner views vaccination history
    Given "Fatima Al Rashid" has a cat "Luna" at "Desert Paws"
    And "Luna" has a vaccination record for "Rabies"
    When she views the vaccinations for "Luna"
    Then she should see a vaccination for "Rabies"

  Scenario: Owner views weight curve
    Given "Fatima Al Rashid" has a cat "Luna" at "Desert Paws"
    And "Luna" has weight entries recorded
    When she views the weight history for "Luna"
    Then she should see the weight entries

  Scenario: Owner cannot see another owner's animals
    Given "Fatima Al Rashid" has a cat "Luna" at "Desert Paws"
    And another owner "Ahmed Hassan" has a dog "Rex" at "Desert Paws"
    When "Fatima Al Rashid" views her animals on the portal
    Then she should not see "Rex" in her animal list
