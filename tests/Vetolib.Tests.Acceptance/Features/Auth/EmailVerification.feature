@wip
Feature: Email verification after registration
  As a newly registered clinic user
  I want to verify my email address
  In order to confirm my identity and unlock full access

  Background:
    Given a clinic "Happy Paws" with identifier "clinic-happy-paws"
    And a user registered with email "dr.fatima@happypaws.ae" and password "SecurePass1!"

  # --- Happy Path ---

  Scenario: Verify email with a valid token
    Given the user "dr.fatima@happypaws.ae" has a pending verification token
    When the user verifies their email with the valid token
    Then the email is marked as verified
    And the user can access all clinic features

  # --- Error Cases ---

  Scenario: Expired verification token is rejected
    Given the user "dr.fatima@happypaws.ae" has a verification token that expired 25 hours ago
    When the user attempts to verify their email with the expired token
    Then the system rejects with code "TOKEN_EXPIRED"
    And the message indicates the verification link has expired

  Scenario: Already verified email returns a clear message
    Given the user "dr.fatima@happypaws.ae" has already verified their email
    When the user attempts to verify their email again
    Then the system rejects with code "ALREADY_VERIFIED"
    And the message indicates the email is already verified

  # --- Login Warning ---

  Scenario: Login shows a warning when email is not yet verified
    Given the user "dr.fatima@happypaws.ae" has not verified their email
    When I log in with email "dr.fatima@happypaws.ae" and password "SecurePass1!"
    Then I am successfully authenticated
    And the response includes a warning that the email is unverified
