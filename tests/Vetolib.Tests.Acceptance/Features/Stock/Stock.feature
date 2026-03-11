@wip @ignore
Feature: Medication and vaccine stock management
  Background:
    Given a clinic "Happy Paws"
    And I am authenticated as VET

  Scenario: Create a medication stock item
    When I create a stock item "Amoxicillin" category "Medication" quantity 100 unit "ml" threshold 20
    Then the stock item is created with active status
    And the quantity is 100

  Scenario: Create a vaccine stock item
    When I create a stock item "Rabies Vaccine" category "Vaccine" quantity 50 unit "doses" threshold 10
    Then the stock item is created with active status
    And the quantity is 50

  Scenario: List stock items
    Given a stock item "Saline Solution" category "Supply" quantity 200 unit "ml" threshold 30
    When I list stock items
    Then the list contains at least 1 item

  Scenario: Record stock movement IN
    Given a stock item "Bandages" category "Supply" quantity 50 unit "pieces" threshold 10
    When I record a stock movement "IN" quantity 25 reason "Restock"
    Then the new quantity is 75

  Scenario: Record stock movement OUT
    Given a stock item "Syringes" category "Supply" quantity 100 unit "pieces" threshold 20
    When I record a stock movement "OUT" quantity 10 reason "Used during consultation"
    Then the new quantity is 90

  Scenario: Low stock alerts
    Given a stock item "Parvovirus Vaccine" category "Vaccine" quantity 5 unit "doses" threshold 10
    When I check stock alerts
    Then the alert includes "Parvovirus Vaccine" for low stock

  Scenario: Update alert threshold
    Given a stock item "Sterile Gloves" category "Supply" quantity 200 unit "pieces" threshold 50
    When I update the item threshold to 100
    Then the threshold is updated to 100

  Scenario: Empty name rejected
    When I attempt to create a stock item with an empty name
    Then the system rejects with code "VALIDATION_ERROR"

  Scenario: Negative quantity rejected
    When I attempt to create a stock item with a quantity of -5
    Then the system rejects with code "VALIDATION_ERROR"
