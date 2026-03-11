@wip @ignore
# language: en
Feature: Stock-Prescription Integration
  As a veterinarian
  I want prescriptions to be linked to clinic stock
  So that inventory is automatically updated and I know what is available

  Background:
    Given I am logged in as a VET
    And a patient "Rex" of species "Dog" exists in my clinic
    And the drug catalog contains "Amoxicillin" as a Medication
    And the drug catalog contains "Cephalexin" as a Medication

  Scenario: Stock availability shown during prescription creation
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
    When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
    Then I should see stock information showing "100 tablets available"

  Scenario: Low stock warning during prescription
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 5 and unit "tablets" and threshold 20
    When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
    Then I should see stock information showing "5 tablets available"
    And I should see a "Low stock" warning

  Scenario: Out of stock with alternative suggestion
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 0
    And a stock item "Cephalexin 500mg capsules" linked to catalog entry "Cephalexin" with quantity 50 and unit "capsules"
    And "Cephalexin" has no contraindication for "Dog"
    When I start creating a prescription for patient "Rex" with drug "Amoxicillin"
    Then I should see "Out of stock" for "Amoxicillin"
    And I should see "Cephalexin" suggested as an in-stock alternative with "50 capsules available"

  Scenario: Stock decremented on prescription confirmation
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
    When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
    And I confirm "Dispense from clinic stock"
    Then the stock quantity for "Amoxicillin 250mg tablets" should be 86
    And a stock movement of type "Out" with quantity 14 and reason containing "Prescription" should be recorded

  Scenario: Vet skips stock decrement
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 100 and unit "tablets"
    When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
    And I select "Do not dispense from stock"
    Then the prescription should be saved successfully
    And the stock quantity for "Amoxicillin 250mg tablets" should remain 100

  Scenario: Insufficient stock -- partial dispense
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 5 and unit "tablets"
    When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
    And I confirm "Dispense from clinic stock"
    Then I should see a warning "Only 5 tablets available, 14 requested"
    And I should be able to dispense the available 5 tablets
    And the stock quantity for "Amoxicillin 250mg tablets" should be 0

  Scenario: Free-text prescription -- no automatic stock link
    When I create a prescription for patient "Rex" with free-text medication "Custom Compound"
    Then I should not see stock information
    And I should be able to manually select a stock item to decrement
    And I should be able to skip stock decrement entirely

  Scenario: Stock low event triggered after prescription
    Given a stock item "Amoxicillin 250mg tablets" linked to catalog entry "Amoxicillin" with quantity 22 and unit "tablets" and threshold 20
    When I create a prescription for patient "Rex" with drug "Amoxicillin" and quantity 14
    And I confirm "Dispense from clinic stock"
    Then the stock quantity should be 8
    And a stock low alert should be triggered for "Amoxicillin 250mg tablets"
