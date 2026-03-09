# language: en
Feature: Patient Management
  As a vet or receptionist
  I want to manage patient records independently of appointments
  So that I can maintain a complete patient database

  Background:
    Given I am logged in as vet "dr.sarah@desertpaws.ae" of clinic "clinic-001"

  Scenario: Vet sees the patient list with search
    Given the clinic has 5 patients including "Max" (Golden Retriever) and "Layla" (Camel)
    When I navigate to the Patients page
    Then I see all 5 patients
    When I type "max" in the search bar
    Then I see only "Max" in the results

  Scenario: Vet creates a new patient
    When I click "Add Patient"
    And I fill in name "Rocky", species "Dog", breed "Labrador", birth date "2021-05-10"
    And I fill in owner name "Faisal Al-Kuwari", phone "+971 50 111 2222"
    And I click "Save Patient"
    Then Rocky appears in the patient list
    And Rocky's age is shown as "4 years"

  Scenario: Camel is available as a species (UAE market)
    When I open the "Add Patient" form
    Then I see "Camel" in the species dropdown
    And I can create a patient with species "Camel"

  Scenario: Patient detail shows complete history
    Given patient "Max" has 2 past appointments and 1 medical record
    When I open Max's patient file
    Then I see Max's profile with species icon, age, and owner contact
    And I see 2 appointments in the history
    And I see 1 medical record in the Medical Records tab

  Scenario: Vet can edit patient information
    Given patient "Max" exists
    When I open Max's file and click "Edit"
    And I change the phone number to "+971 50 999 8888"
    And I save
    Then Max's owner phone shows "+971 50 999 8888"

  Scenario: Receptionist has read-only access to patients
    Given I am logged in as receptionist "reception@desertpaws.ae"
    When I navigate to the Patients page
    Then I see the patient list
    But I do not see an "Add Patient" button
    When I open any patient's file
    Then I do not see an "Edit" button

  Scenario: Tenant isolation on patients
    Given clinic "Al Barsha Vets" has patient "Buddy"
    When I am logged in as vet of "Desert Paws Clinic"
    Then I cannot see or access "Buddy" in any patient list or API call
