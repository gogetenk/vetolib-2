Feature: Preference Management
  As a clinic user or admin
  I want to manage my preferences and consent settings
  So that I can control my experience and data usage

  Scenario: User retrieves effective preferences
    Given a clinic "VetClinic Preferences"
    And I am authenticated as Vet
    And a clinic default "AITriage" is set to "false"
    When I retrieve my preferences
    Then the preference "AITriage" has value "false" with source "Clinic"
    And the preference "NotificationEmail" has value "true" with source "System"

  Scenario: User updates a preference
    Given a clinic "VetClinic Preferences"
    And I am authenticated as Vet
    When I set preference "NotificationEmail" to "false"
    Then the response is successful
    And the preference "NotificationEmail" has value "false" with source "User"
    And a consent audit entry exists for preference "NotificationEmail"

  Scenario: User cannot disable drug interaction alerts
    Given a clinic "VetClinic Preferences"
    And I am authenticated as Vet
    When I set preference "AIDrugInteractions" to "false"
    Then the response status is 400
    And the preference "AIDrugInteractions" remains "true"

  Scenario: User revokes analytics consent
    Given a clinic "VetClinic Preferences"
    And I am authenticated as Vet
    And the user has opted in to analytics preferences
    When I revoke consent for category "Analytics"
    Then all analytics preferences are set to "false"
    And consent audit entries exist for the revoked preferences

  Scenario: Admin updates clinic defaults
    Given a clinic "VetClinic Preferences"
    And I am authenticated as Admin
    When the admin sets clinic default "AITriage" to "false"
    Then the clinic default "AITriage" is "false"

  Scenario: Non-admin cannot update clinic defaults
    Given a clinic "VetClinic Preferences"
    And I am authenticated as Vet
    When the user tries to update clinic defaults with "AITriage" set to "true"
    Then the response status is 403

  Scenario: Admin views consent audit trail
    Given a clinic "VetClinic Preferences"
    And I am authenticated as Admin
    And several preference changes have been recorded
    When I retrieve the consent audit trail
    Then the audit entries are returned
