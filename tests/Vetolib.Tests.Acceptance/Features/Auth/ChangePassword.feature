Feature: Change password
  As a logged-in user
  I want to change my password
  So that I can maintain account security

  Background:
    Given une clinique "Happy Paws"

  Scenario: Successful password change
    Given I am authenticated as VET with password "Secure@1234567!"
    When I change my password from "Secure@1234567!" to "NewSecure@7654321!"
    Then the response status is 200
    And I can log in with the new password "NewSecure@7654321!"

  Scenario: Wrong current password is rejected
    Given I am authenticated as VET with password "Secure@1234567!"
    When I change my password from "WrongPassword@!" to "NewSecure@7654321!"
    Then the response status is 422
    And the error code is "INVALID_CURRENT_PASSWORD"

  Scenario: Weak new password is rejected
    Given I am authenticated as VET with password "Secure@1234567!"
    When I change my password from "Secure@1234567!" to "weak"
    Then the response status is 400

  Scenario: Unauthenticated request is rejected
    When I send a change password request without authentication
    Then the response status is 401
