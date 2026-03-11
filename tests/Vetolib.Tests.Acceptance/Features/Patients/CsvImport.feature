@wip @ignore
# language: en
Feature: CSV Import Patients
  As a vet or admin
  I want to import patients from a CSV file
  So that I can migrate existing data without manual entry

  Background:
    Given a clinic "Desert Paws"
    And I am authenticated as Vet

  Scenario: Successful CSV import returns report
    When I import a CSV with 3 valid patient rows
    Then the import report shows 3 imported, 0 skipped
    And the imported patients appear in the patient list

  Scenario: Rows with missing required fields are skipped
    When I import a CSV where 1 row is missing Species
    Then the import report shows 1 imported, 1 skipped
    And the errors list contains "Species is required"

  Scenario: Duplicate owner email reuses existing owner
    Given an owner with email "shared@test.ae" already exists
    When I import a CSV with 2 patients sharing owner email "shared@test.ae"
    Then the import report shows 2 imported, 0 skipped
    And only 1 owner exists with email "shared@test.ae"

  Scenario: Download CSV template
    When I request the CSV import template
    Then I receive a CSV file with header "PatientName,Species,Breed,DateOfBirth,OwnerName,OwnerEmail,OwnerPhone"

  Scenario: Receptionist cannot import patients
    Given I am authenticated as Receptionist
    When I import a CSV with 1 valid patient row
    Then the import is rejected with status 403
