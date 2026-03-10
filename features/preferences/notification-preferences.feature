# language: en
Feature: Notification Preferences
  As a clinic staff member
  I want to control which email notifications I receive
  So that I am not overwhelmed by irrelevant emails

  Background:
    Given the clinic "Desert Paws Clinic" exists with id "clinic-001"
    And an admin user "admin@desertpaws.ae" belongs to "clinic-001"
    And a vet user "dr.sarah@desertpaws.ae" belongs to "clinic-001"

  # --- System defaults ---

  Scenario: Default notification preferences are all ON for a new user
    Given I am logged in as "dr.sarah@desertpaws.ae"
    When I navigate to the preferences page
    Then I see the following notification preferences:
      | Preference              | Value |
      | Appointment reminders   | ON    |
      | Invoice notifications   | ON    |
      | Email notifications     | ON    |

  # --- Per-user overrides ---

  Scenario: A vet disables appointment reminder emails
    Given I am logged in as "dr.sarah@desertpaws.ae"
    And my notification preferences are at their defaults
    When I navigate to the preferences page
    And I toggle "Appointment reminders" to OFF
    And I save my preferences
    Then my "Appointment reminders" preference is OFF
    And when an appointment reminder is due for "dr.sarah@desertpaws.ae"
    Then no reminder email is sent to "dr.sarah@desertpaws.ae"

  Scenario: A vet enables invoice email notifications
    Given I am logged in as "dr.sarah@desertpaws.ae"
    And my "Invoice notifications" preference is OFF
    When I navigate to the preferences page
    And I toggle "Invoice notifications" to ON
    And I save my preferences
    Then my "Invoice notifications" preference is ON

  Scenario: Preferences are persisted across sessions
    Given I am logged in as "dr.sarah@desertpaws.ae"
    And I toggle "Appointment reminders" to OFF on the preferences page
    And I save my preferences
    When I log out
    And I log back in as "dr.sarah@desertpaws.ae"
    And I navigate to the preferences page
    Then my "Appointment reminders" preference is OFF

  # --- Clinic defaults (Admin) ---

  Scenario: An admin configures clinic default notification preferences
    Given I am logged in as "admin@desertpaws.ae"
    When I navigate to the clinic preferences page
    And I set the clinic default for "Appointment reminders" to OFF
    And I save the clinic defaults
    Then the clinic default for "Appointment reminders" is OFF

  Scenario: New users inherit clinic defaults instead of system defaults
    Given I am logged in as "admin@desertpaws.ae"
    And the clinic default for "Invoice notifications" is OFF
    When a new vet user "dr.ahmed@desertpaws.ae" is added to "clinic-001"
    And "dr.ahmed@desertpaws.ae" navigates to the preferences page
    Then the "Invoice notifications" preference shows OFF
    And the preference source is "clinic_default"

  # --- User override > Clinic default ---

  Scenario: User preference overrides clinic default
    Given I am logged in as "admin@desertpaws.ae"
    And the clinic default for "Appointment reminders" is OFF
    And I am logged in as "dr.sarah@desertpaws.ae"
    When I navigate to the preferences page
    And I toggle "Appointment reminders" to ON
    And I save my preferences
    Then my "Appointment reminders" preference is ON
    And the preference source is "user_override"
    And when an appointment reminder is due for "dr.sarah@desertpaws.ae"
    Then a reminder email is sent to "dr.sarah@desertpaws.ae"

  # --- RBAC ---

  Scenario: A non-admin user cannot access clinic default preferences
    Given I am logged in as "dr.sarah@desertpaws.ae"
    When I try to navigate to the clinic preferences page
    Then I receive a 403 Forbidden response
