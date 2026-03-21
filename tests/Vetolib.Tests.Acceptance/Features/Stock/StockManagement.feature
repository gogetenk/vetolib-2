@wip
Feature: Stock Management
  As a veterinarian or admin
  I want to manage medication and vaccine inventory
  So that I never run out of essential supplies

  Background:
    Given I am authenticated as a user with role "Admin"

  Scenario: Create a medication stock item
    When I create a stock item with:
      | Name        | Category   | Quantity | Unit    | MinThreshold | ExpiryDate |
      | Amoxicillin | Medication | 100      | tablets | 20           | 2027-06-15 |
    Then the stock item is created successfully
    And the stock item is created with an identifier

  Scenario: Create a vaccine stock item
    When I create a stock item with:
      | Name           | Category | Quantity | Unit  | MinThreshold |
      | Rabies Vaccine | Vaccine  | 50       | doses | 10           |
    Then the stock item is created successfully
    And the stock item quantity is 50

  Scenario: List stock items with filters
    Given the following stock items exist:
      | Name           | Category   | Quantity | MinThreshold |
      | Amoxicillin    | Medication | 100      | 20           |
      | Rabies Vaccine | Vaccine    | 5        | 10           |
      | Syringes       | Supply     | 200      | 50           |
    When I request stock items filtered by category "Vaccine"
    Then I should receive 1 stock item
    And the item should be "Rabies Vaccine"

  Scenario: Record stock movement IN
    Given a stock item "Amoxicillin" exists with quantity 100
    When I record a stock movement:
      | MovementType | Quantity | Reason            |
      | IN           | 50       | New delivery       |
    Then the stock item quantity is 150

  Scenario: Record stock movement OUT
    Given a stock item "Amoxicillin" exists with quantity 100
    When I record a stock movement:
      | MovementType | Quantity | Reason                   |
      | OUT          | 10       | Used for patient Luna     |
    Then the stock item quantity is 90

  Scenario: Stock movement OUT cannot exceed current quantity
    Given a stock item "Amoxicillin" exists with quantity 5
    When I record a stock movement:
      | MovementType | Quantity | Reason      |
      | OUT          | 10       | Used for patient |
    Then the system indicates insufficient stock

  Scenario: Low stock alert
    Given a stock item "Rabies Vaccine" exists with quantity 5 and threshold 10
    When I request stock alerts
    Then the alerts should include "Rabies Vaccine" as low-stock

  Scenario: Expiring soon alert
    Given a stock item "Ketamine" exists with expiry date in 15 days
    When I request stock alerts
    Then the alerts should include "Ketamine" as expiring-soon

  Scenario: Update stock item threshold
    Given a stock item "Amoxicillin" exists with threshold 20
    When I update the stock item threshold to 30
    Then the stock item threshold is 30

  Scenario: Vet can view and record movements
    Given I am authenticated as a user with role "Vet"
    When I request stock items
    Then the stock list is returned successfully

  Scenario: Receptionist cannot create stock items
    Given I am authenticated as a user with role "Receptionist"
    When I create a stock item with:
      | Name        | Category   | Quantity | Unit    | MinThreshold |
      | Amoxicillin | Medication | 100      | tablets | 20           |
    Then the user is denied access

  Scenario: Multi-tenant isolation
    Given clinic A has stock item "Amoxicillin" with quantity 100
    And clinic B has stock item "Amoxicillin" with quantity 50
    When I am authenticated in clinic A
    And I request stock items
    Then I should only see clinic A's stock items

  Scenario: Empty name rejected
    When I attempt to create a stock item with an empty name
    Then the system rejects the input as invalid

  Scenario: Negative quantity rejected
    When I attempt to create a stock item with a quantity of -5
    Then the system rejects the input as invalid
