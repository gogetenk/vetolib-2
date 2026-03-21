# features/auth/login.feature

Feature: Authentication and JWT token management
  As a Vetolib user
  I want to authenticate with my email and password
  In order to access my clinic's features securely

  Background:
    Given a clinic "Happy Paws" with identifier "clinic-happy-paws"
    And an existing user with the following information:
      | Email              | Password     | Role         | ClinicId          | VetLicenseNumber |
      | vet@happypaws.ae   | SecurePass1  | Vet          | clinic-happy-paws | UAE-VET-12345    |
    And an existing admin user:
      | Email                | Password     | Role  | ClinicId          |
      | admin@happypaws.ae   | AdminPass1   | Admin | clinic-happy-paws |

  # ─── Login — Happy Path ──────────────────────────────────

  Scenario: Successful login returns a JWT and a refresh token
    When I log in with email "vet@happypaws.ae" and password "SecurePass1"
    Then I receive a valid JWT access token
    And I receive a refresh token
    And the response contains the user information:
      | Email            | Role | ClinicId          | VetLicenseNumber |
      | vet@happypaws.ae | Vet  | clinic-happy-paws | UAE-VET-12345    |
    And the JWT contains the claim "clinic_id" with value "clinic-happy-paws"

  Scenario: The access token expires after 15 minutes
    When I log in with email "vet@happypaws.ae" and password "SecurePass1"
    Then the access token has a validity duration of 15 minutes

  # ─── Refresh Token — Happy Path ──────────────────────────

  Scenario: Refreshing the token returns a new pair and invalidates the old refresh token
    Given I am logged in as "vet@happypaws.ae"
    And I have a valid refresh token
    When I call POST /api/v1/auth/refresh with my refresh token
    Then I receive a new valid JWT access token
    And I receive a new refresh token different from the old one
    And the old refresh token is invalid

  Scenario: The new refresh token expires after 7 days
    Given I am logged in as "vet@happypaws.ae"
    And I have a valid refresh token
    When I call POST /api/v1/auth/refresh with my refresh token
    Then the new refresh token has a validity duration of 7 days

  # ─── Logout — Happy Path ─────────────────────────────────

  Scenario: Logout invalidates the refresh token
    Given I am logged in as "vet@happypaws.ae"
    And I have a valid refresh token
    When I call POST /api/v1/auth/logout
    Then the logout is confirmed
    And the refresh token is invalid
    And an attempt to refresh with the old token fails

  # ─── GET /me — Happy Path ────────────────────────────────

  Scenario: Retrieve the connected user profile
    Given I am logged in as "vet@happypaws.ae"
    When I call GET /api/v1/auth/me
    Then I receive my profile information:
      | Email            | Role | ClinicId          | VetLicenseNumber |
      | vet@happypaws.ae | Vet  | clinic-happy-paws | UAE-VET-12345    |

  # ─── Login — Errors ─────────────────────────────────────

  Scenario: Incorrect password returns an error
    When I log in with email "vet@happypaws.ae" and password "MauvaisPass1"
    Then the system rejects with code "INVALID_CREDENTIALS"
    And the error message is "Invalid email or password"

  Scenario: Non-existent email returns an error
    When I log in with email "inconnu@happypaws.ae" and password "SecurePass1"
    Then the system rejects with code "INVALID_CREDENTIALS"
    And the error message is "Invalid email or password"

  # ─── Account lockout ────────────────────────────────────

  Scenario: 5 failed attempts lock the account for 15 minutes
    When I log in 5 times with email "vet@happypaws.ae" and an incorrect password
    Then the system rejects with code "ACCOUNT_LOCKED"
    And the message indicates the account is locked for 15 minutes

  Scenario: Login refused during lockout period even with the correct password
    Given the account "vet@happypaws.ae" is locked after 5 failed attempts
    When I log in with email "vet@happypaws.ae" and password "SecurePass1"
    Then the system rejects with code "ACCOUNT_LOCKED"
    And the message indicates the account is locked

  Scenario: Successful login after lockout period expires
    Given the account "vet@happypaws.ae" was locked 16 minutes ago
    When I log in with email "vet@happypaws.ae" and password "SecurePass1"
    Then I receive a valid JWT access token
    And the failed attempts counter is reset

  # ─── Refresh Token — Errors ─────────────────────────────

  Scenario: Refresh with a revoked token fails
    Given I am logged in as "vet@happypaws.ae"
    And my refresh token has been revoked by a previous refresh
    When I call POST /api/v1/auth/refresh with the revoked refresh token
    Then the system rejects with code "INVALID_REFRESH_TOKEN"

  Scenario: Refresh with an expired token fails
    Given I am logged in as "vet@happypaws.ae"
    And my refresh token has expired for more than 7 days
    When I call POST /api/v1/auth/refresh with the expired refresh token
    Then the system rejects with code "INVALID_REFRESH_TOKEN"

  # ─── GET /me — Errors ───────────────────────────────────

  Scenario: Access to /me without token returns 401
    When I call GET /api/v1/auth/me without authentication token
    Then the user must sign in

  # ─── Multi-tenancy ───────────────────────────────────────

  Scenario: A user from clinic A cannot see clinic B data
    Given a clinic "Desert Vet" with identifier "clinic-desert-vet"
    And an existing user with the following information:
      | Email                | Password     | Role | ClinicId           |
      | recep@desertvet.ae   | SecurePass1  | Receptionist | clinic-desert-vet |
    When I log in with email "recep@desertvet.ae" and password "SecurePass1"
    Then the JWT contains the claim "clinic_id" with value "clinic-desert-vet"
    And the requests from this user only return data from "clinic-desert-vet"

  # ─── User creation (minimal support) ────────────────────

  Scenario: An admin can create a user
    Given I am logged in as "admin@happypaws.ae"
    When I create a user with the following information:
      | Email                  | Password     | Role         | VetLicenseNumber |
      | newvet@happypaws.ae    | NewVetPass1  | Vet          | UAE-VET-99999    |
    Then the user is created successfully
    And the created user belongs to clinic "clinic-happy-paws"

  Scenario: A non-admin cannot create a user
    Given I am logged in as "vet@happypaws.ae"
    When I attempt to create a user with the following information:
      | Email                  | Password     | Role         |
      | autre@happypaws.ae     | OtherPass1   | Receptionist |
    Then the system rejects with code "FORBIDDEN"

  Scenario: The Vet role requires a veterinary license number
    Given I am logged in as "admin@happypaws.ae"
    When I attempt to create a user with the following information:
      | Email                  | Password     | Role | VetLicenseNumber |
      | novet@happypaws.ae     | NoVetPass1   | Vet  |                  |
    Then the system rejects with code "VET_LICENSE_REQUIRED"
    And the error message is "A veterinary license number is required for the Vet role"

  # ─── Password validation (user creation) ────────────────

  Scenario Outline: Invalid password during user creation
    Given I am logged in as "admin@happypaws.ae"
    When I attempt to create a user with email "test@happypaws.ae" and password "<password>"
    Then the system rejects with code "VALIDATION_ERROR"
    And the message contains "<reason>"

    Examples:
      | password | reason                                      |
      | Short1   | Password must contain at least 8 characters |
      | alllowercase1 | Password must contain at least one uppercase letter |
      | AllUpperCase  | Password must contain at least one digit    |
