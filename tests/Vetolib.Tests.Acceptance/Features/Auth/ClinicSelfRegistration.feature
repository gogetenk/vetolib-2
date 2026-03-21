@wip
# language: en
Feature: Clinic self-service registration
  As a new veterinary practice owner
  I want to register my clinic online without manual intervention
  So that I can start using Vetolib immediately with a 14-day Pro trial

  Scenario: Successful clinic registration authenticates and creates tenant
    When I register a new clinic with:
      | ClinicName    | Email                     | Password        | Phone         | Country |
      | Desert Paws   | owner@desertpaws.ae       | Secure@1234567! | +971501234567 | AE      |
    Then the record is created successfully
    And I am successfully authenticated
    And the session is linked to the clinic
    And the clinic "Desert Paws" is created
    And the admin user "owner@desertpaws.ae" belongs to the new clinic
    And the clinic trial ends in 14 days from now
    And the clinic subscription plan is "Pro"

  Scenario: Duplicate email across tenants is rejected
    Given a clinic "Al Barsha Vets" already registered with email "owner@albarsha.ae"
    When I register a new clinic with:
      | ClinicName    | Email             | Password        | Phone         | Country |
      | Jumeirah Pets | owner@albarsha.ae | Secure@1234567! | +971509876543 | AE      |
    Then the operation is rejected with validation errors
    And the response contains "EMAIL_EXISTS"

  Scenario: Blank clinic name is rejected
    When I register a new clinic with:
      | ClinicName | Email                    | Password        | Phone         | Country |
      |            | owner2@desertpaws.ae     | Secure@1234567! | +971501234567 | AE      |
    Then the request is rejected
    And the operation is rejected because ClinicName is invalid

  Scenario: Weak password is rejected
    When I register a new clinic with:
      | ClinicName  | Email                  | Password | Phone         | Country |
      | Happy Paws  | newowner@happypaws.ae  | short    | +971501234567 | AE      |
    Then the request is rejected
    And the operation is rejected because Password is invalid

  Scenario: Password must have at least 10 characters and a special character
    When I register a new clinic with:
      | ClinicName  | Email                    | Password          | Phone         | Country |
      | Happy Paws  | newowner2@happypaws.ae   | NoSpecialChar1234 | +971501234567 | AE      |
    Then the request is rejected
    And the operation is rejected because Password is invalid
