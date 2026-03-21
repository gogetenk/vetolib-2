@wip
Feature: Multi-clinic management
  As a clinic group administrator
  I want to manage multiple clinics under a single account
  So that I can oversee all my veterinary locations in the UAE

  Background:
    Given I am authenticated as an admin "khalid@vetgroup.ae"

  # --- Clinic group creation ---

  Scenario: Admin creates a clinic group
    When I create a clinic group named "Al Barsha Vet Group"
    Then the clinic group "Al Barsha Vet Group" is created
    And my current clinic is automatically part of the group

  Scenario: Admin adds an existing clinic to the group
    Given I have a clinic group "Al Barsha Vet Group"
    And there is a clinic "Desert Paws JBR" not yet in any group
    When I add "Desert Paws JBR" to the group "Al Barsha Vet Group"
    Then "Desert Paws JBR" appears in the group's clinic list

  Scenario: Admin adds a new clinic to the group
    Given I have a clinic group "Al Barsha Vet Group"
    When I create a new clinic "Desert Paws Marina" within the group
    Then "Desert Paws Marina" is created and appears in the group's clinic list

  # --- Switching clinics ---

  Scenario: Admin switches between clinics in the same group
    Given I have a clinic group with clinics "Desert Paws JBR" and "Desert Paws Marina"
    And I am currently working in "Desert Paws JBR"
    When I switch to "Desert Paws Marina"
    Then my active clinic becomes "Desert Paws Marina"
    And I see the data for "Desert Paws Marina" only

  Scenario: Switching clinic refreshes the session context
    Given I have a clinic group with clinics "Desert Paws JBR" and "Desert Paws Marina"
    And I am currently working in "Desert Paws JBR"
    When I switch to "Desert Paws Marina"
    Then the patients I see belong to "Desert Paws Marina"
    And the appointments I see belong to "Desert Paws Marina"
    And no data from "Desert Paws JBR" is visible

  # --- Access restrictions ---

  Scenario: Non-admin user cannot switch clinics
    Given I am authenticated as a vet "dr.omar@desertpaws.ae" assigned to "Desert Paws JBR"
    When I try to switch to "Desert Paws Marina"
    Then the switch is denied with the message "Only administrators can switch between clinics"

  Scenario: Vet assigned to multiple clinics can switch
    Given I am authenticated as a vet "dr.sara@desertpaws.ae" assigned to both "Desert Paws JBR" and "Desert Paws Marina"
    When I switch to "Desert Paws Marina"
    Then my active clinic becomes "Desert Paws Marina"

  # --- Data isolation ---

  Scenario: Clinics in the same group have separate data
    Given I have a clinic group with clinics "Desert Paws JBR" and "Desert Paws Marina"
    And "Desert Paws JBR" has patient "Luna" (cat)
    And "Desert Paws Marina" has patient "Rocky" (dog)
    When I am working in "Desert Paws JBR"
    Then I can see "Luna" but not "Rocky"
    When I switch to "Desert Paws Marina"
    Then I can see "Rocky" but not "Luna"
