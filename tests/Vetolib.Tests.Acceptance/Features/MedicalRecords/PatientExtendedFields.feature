@wip
Feature: Patient extended fields — Sex, Microchip, and expanded Species
  As a veterinarian working in a UAE or French clinic,
  I want to record the sex, microchip number, and precise species of each patient
  so that I have complete identification data for medical and regulatory purposes.

  Background:
    Given a clinic "Desert Paws Veterinary"
    And an owner "Fatima Al Maktoum" with email "fatima@example.ae"
    And I am authenticated as VET

  # --- F1: Sex field ---

  Scenario: Register a new patient with sex
    When I create a patient "Simba" species "Cat" breed "Arabian Mau" sex "Male"
    Then the patient "Simba" is created with sex "Male"

  Scenario: Register a neutered patient
    When I create a patient "Bella" species "Dog" breed "Saluki" sex "SpayedFemale"
    Then the patient "Bella" is created with sex "SpayedFemale"

  Scenario: Sex defaults to Unknown when not provided
    When I create a patient "Shadow" species "Cat" breed "Persian" without specifying sex
    Then the patient "Shadow" is created with sex "Unknown"

  Scenario: Update sex after neutering
    Given a patient "Rex" species "Dog" breed "German Shepherd" sex "Male"
    When I update the sex of "Rex" to "NeuteredMale"
    Then the patient "Rex" has sex "NeuteredMale"

  # --- F2: Microchip number ---

  Scenario: Register a patient with a microchip number
    When I create a patient "Nala" species "Cat" breed "Siamese" with microchip "900118000123456"
    Then the patient "Nala" is created with microchip number "900118000123456"

  Scenario: Microchip number is optional
    When I create a patient "Rocky" species "Dog" breed "Saluki" without a microchip
    Then the patient "Rocky" is created without a microchip number

  Scenario: Reject invalid microchip format
    When I attempt to create a patient with microchip "12345"
    Then the system rejects with reason "Microchip number must be 15 digits (ISO 11784/11785)"

  Scenario: Search patient by microchip number
    Given a patient "Luna" with microchip "900118000654321"
    When I search for a patient by microchip "900118000654321"
    Then the result contains "Luna"

  Scenario: Microchip number must be unique within a clinic
    Given a patient "Buddy" with microchip "900118000111111"
    When I attempt to create a patient "Max" with microchip "900118000111111"
    Then the system rejects with reason "A patient with this microchip number already exists"

  # --- F3: Expanded Species enum ---

  Scenario: Register a falcon patient
    When I create a patient "Shaheen" species "Falcon" breed "Peregrine" sex "Male"
    Then the patient "Shaheen" is created with species "Falcon"

  Scenario: Register a reptile patient
    When I create a patient "Scales" species "Reptile" breed "Ball Python" sex "Male"
    Then the patient "Scales" is created with species "Reptile"

  Scenario: All supported species can be used
    Then the following species are available:
      | Species |
      | Dog     |
      | Cat     |
      | Bird    |
      | Rabbit  |
      | Horse   |
      | Exotic  |
      | Camel   |
      | Falcon  |
      | Reptile |
