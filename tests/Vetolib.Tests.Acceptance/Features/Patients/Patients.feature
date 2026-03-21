@wip
# language: en
Feature: Patient standalone CRUD
  As a vet or admin
  I want to create and manage patients independently of appointments
  So that I can maintain a complete patient registry with UAE species

  Background:
    Given a clinic "Desert Paws"
    And I am authenticated as Vet

  Scenario: Vet creates a patient with owner inline
    When I create a patient with name "Rocky", species "Dog", breed "Labrador", birth date "2021-05-10", owner name "Faisal Al-Kuwari", owner phone "+971 50 111 2222"
    Then the patient is created successfully
    And the patient name is "Rocky"
    And the patient owner name is "Faisal Al-Kuwari"

  Scenario: Camel is a valid species for UAE market
    When I create a patient with name "Layla", species "Camel", breed "Dromedary", birth date "2018-03-15", owner name "Ahmed Al-Mansouri", owner phone "+971 55 222 3333"
    Then the patient is created successfully
    And the patient species is "Camel"

  Scenario: Patient list can be filtered by name
    Given 3 patients exist including one named "Max"
    When I list patients with name filter "max"
    Then I see 1 patient in the results
    And the patient name is "Max"

  Scenario: Vet can update patient phone number
    Given a patient named "Rocky" with owner phone "+971 50 111 2222"
    When I update the patient owner phone to "+971 50 999 8888"
    Then the patient owner phone is "+971 50 999 8888"

  Scenario: Receptionist cannot create patients
    Given I am authenticated as Receptionist
    When I create a patient with name "Buddy", species "Dog", breed "Poodle", birth date "2020-01-01", owner name "Owner Name", owner phone "+971 50 000 0000"
    Then the user is denied access

  Scenario: Tenant isolation on patients
    Given a clinic "Al Barsha Vets"
    And a patient named "Buddy" exists in clinic "Al Barsha Vets"
    When I list patients as vet of clinic "Desert Paws"
    Then I cannot see "Buddy" in the patient list

  Scenario: Patient detail includes medical records
    Given a patient named "Rocky" with owner phone "+971 50 111 2222"
    And 2 medical records exist for "Rocky"
    When I get the patient detail
    Then the detail contains 2 medical records
