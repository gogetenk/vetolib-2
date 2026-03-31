Feature: Owner Registration
  As a pet owner
  I want to register on the portal with my email and phone
  So that I can access all my animals across clinics

  Background:
    Given a clinic "Desert Paws" exists with animals registered

  Scenario: Successful registration with auto-link by email
    Given an owner "Fatima Al Rashid" with email "fatima@example.com" exists at clinic "Desert Paws" with a cat named "Luna"
    When "Fatima Al Rashid" registers on the portal with email "fatima@example.com" and phone "+971501234567"
    Then her account is created successfully
    And her account is automatically linked to the owner record at "Desert Paws"

  Scenario: Successful registration with auto-link by phone
    Given an owner "Ahmed Hassan" with phone "+971509876543" exists at clinic "Desert Paws" with a dog named "Rex"
    When "Ahmed Hassan" registers on the portal with email "ahmed@example.com" and phone "+971509876543"
    Then his account is created successfully
    And his account is automatically linked to the owner record at "Desert Paws"

  Scenario: Registration with duplicate email is rejected
    Given a portal account already exists with email "fatima@example.com"
    When someone tries to register with email "fatima@example.com"
    Then the registration is rejected because the email is already taken

  Scenario: Owner logs in and sees linked clinics
    Given "Fatima Al Rashid" has a portal account with email "fatima@example.com"
    And her account is linked to clinics "Desert Paws" and "Palm Vet"
    When she logs in with her credentials
    Then she receives a token containing her linked clinic identifiers

  Scenario: Auto-link by microchip after registration
    Given "Fatima Al Rashid" has a portal account with email "fatima@example.com"
    And a patient with microchip "123456789012345" exists at clinic "Desert Paws" owned by an unlinked owner
    When she provides microchip number "123456789012345"
    Then the patient's owner is linked to her account
