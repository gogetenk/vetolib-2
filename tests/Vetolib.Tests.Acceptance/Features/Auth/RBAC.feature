@wip @ignore
@rbac
Feature: RBAC matrix -- role-based access control
  As a system
  I want to enforce the UAE veterinary clinic RBAC matrix
  So that each role can only perform authorized actions

  Background:
    Given a clinic "Desert Paws"

  Scenario: Assistant cannot create an appointment (403)
    Given I am authenticated as Assistant
    When I attempt to create an appointment
    Then the system returns 403

  Scenario: Receptionist cannot add a medical record (403)
    Given I am authenticated as Receptionist
    When I attempt to add a medical record
    Then the system returns 403

  Scenario: Vet can create an appointment
    Given I am authenticated as Vet
    When I attempt to create an appointment
    Then the system accepts the request

  Scenario: Admin can create an invoice
    Given I am authenticated as Admin
    When I attempt to create an invoice
    Then the system accepts the request

  Scenario: Assistant cannot create an invoice (403)
    Given I am authenticated as Assistant
    When I attempt to create an invoice
    Then the system returns 403

  Scenario: Only Vet can add a prescription (VetOnly)
    Given I am authenticated as Admin
    When I attempt to add a prescription to a medical record
    Then the system returns 403

  Scenario: Vet can add a prescription
    Given I am authenticated as Vet
    When I attempt to add a prescription to a medical record
    Then the system accepts the request
