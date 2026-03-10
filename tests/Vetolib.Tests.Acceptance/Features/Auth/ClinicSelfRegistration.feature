@wip
# language: en
Feature: Clinic self-service registration
  As a new veterinary practice owner
  I want to register my clinic online without manual intervention
  So that I can start using Vetolib immediately with a 14-day Pro trial

  Scenario: Successful clinic registration returns JWT and creates tenant
    When I POST /api/v1/clinics/register with:
      | ClinicName    | Email                     | Password        | Phone         | Country |
      | Desert Paws   | owner@desertpaws.ae       | Secure@1234567! | +971501234567 | AE      |
    Then the response status is 201
    And I receive a JWT access token
    And the JWT contains claim "clinic_id"
    And a new clinic "Desert Paws" exists in the database
    And the admin user "owner@desertpaws.ae" belongs to the new clinic
    And the clinic trial ends in 14 days from now
    And the clinic subscription plan is "Pro"

  Scenario: Duplicate email across tenants is rejected
    Given a clinic "Al Barsha Vets" already registered with email "owner@albarsha.ae"
    When I POST /api/v1/clinics/register with:
      | ClinicName    | Email             | Password        | Phone         | Country |
      | Jumeirah Pets | owner@albarsha.ae | Secure@1234567! | +971509876543 | AE      |
    Then the response status is 422
    And the response contains "EMAIL_EXISTS"

  Scenario: Blank clinic name is rejected
    When I POST /api/v1/clinics/register with:
      | ClinicName | Email                    | Password        | Phone         | Country |
      |            | owner2@desertpaws.ae     | Secure@1234567! | +971501234567 | AE      |
    Then the response status is 400
    And the response contains validation error for "ClinicName"

  Scenario: Weak password is rejected
    When I POST /api/v1/clinics/register with:
      | ClinicName  | Email                  | Password | Phone         | Country |
      | Happy Paws  | newowner@happypaws.ae  | short    | +971501234567 | AE      |
    Then the response status is 400
    And the response contains validation error for "Password"

  Scenario: Password must have at least 10 characters and a special character
    When I POST /api/v1/clinics/register with:
      | ClinicName  | Email                    | Password          | Phone         | Country |
      | Happy Paws  | newowner2@happypaws.ae   | NoSpecialChar1234 | +971501234567 | AE      |
    Then the response status is 400
    And the response contains validation error for "Password"
