# language: en
Feature: Login
  As a clinic staff member
  I want to sign in to Vetolib
  So that I can access clinic features for my clinic only

  Background:
    Given the clinic "Desert Paws Clinic" exists with id "clinic-001"
    And a vet user "dr.sarah@desertpaws.ae" with password "Secure123!" belongs to "clinic-001"

  Scenario: Successful login redirects to dashboard
    Given I am on the login page
    When I enter email "dr.sarah@desertpaws.ae" and password "Secure123!"
    And I click "Sign In"
    Then I am redirected to the appointments page
    And the page shows "Desert Paws Clinic" in the header
    And I do not see any other clinic's data

  Scenario: Invalid password shows error message
    Given I am on the login page
    When I enter email "dr.sarah@desertpaws.ae" and password "wrongpassword"
    And I click "Sign In"
    Then I see the error "Invalid email or password"
    And I remain on the login page

  Scenario: Unknown email shows same generic error (no user enumeration)
    Given I am on the login page
    When I enter email "unknown@example.com" and password "anything"
    And I click "Sign In"
    Then I see the error "Invalid email or password"

  Scenario: Account locked after 5 failed attempts
    Given I am on the login page
    When I fail to login 5 times with email "dr.sarah@desertpaws.ae"
    Then I see the error "Account locked. Try again in 15 minutes."
    And a 6th login attempt with correct password still shows the locked error

  Scenario: Empty fields show client-side validation (no API call)
    Given I am on the login page
    When I click "Sign In" without filling any fields
    Then I see validation errors without any network request being made

  Scenario: Expired access token is refreshed transparently
    Given I am logged in as "dr.sarah@desertpaws.ae"
    And my access token has expired
    When I navigate to the appointments page
    Then the page loads successfully without redirecting to login
    And a new access token was issued

  Scenario: Logging out clears session and redirects to login
    Given I am logged in as "dr.sarah@desertpaws.ae"
    When I click my name in the header
    And I click "Sign Out"
    Then I am redirected to the login page
    And navigating to "/appointments" redirects me back to login

  Scenario: Tenant isolation — vet cannot see another clinic's data
    Given clinic "Al Barsha Vets" exists with id "clinic-002"
    And a vet "dr.omar@albarsha.ae" belongs to "clinic-002"
    And I am logged in as "dr.sarah@desertpaws.ae" (clinic-001)
    When I try to access an appointment belonging to clinic-002
    Then I receive a 403 Forbidden response
