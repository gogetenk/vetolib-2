# language: en
Feature: Analytics Consent
  As a clinic user
  I want to control whether my usage data is collected
  So that I have transparency and control over my data

  Background:
    Given the clinic "Desert Paws Clinic" exists with id "clinic-001"
    And an admin user "admin@desertpaws.ae" belongs to "clinic-001"
    And a vet user "dr.sarah@desertpaws.ae" belongs to "clinic-001"

  # --- Opt-OUT by default ---

  Scenario: Analytics tracking is opt-out by default
    Given "dr.sarah@desertpaws.ae" has never configured analytics preferences
    When I query the effective preferences for "dr.sarah@desertpaws.ae"
    Then the "Analytics tracking" preference is OFF
    And the "Usage data collection" preference is OFF
    And the preference source is "system_default"

  Scenario: No analytics data is collected before explicit consent
    Given "dr.sarah@desertpaws.ae" has not given analytics consent
    When "dr.sarah@desertpaws.ae" uses the application
    Then no analytics events are sent to the tracking service

  # --- Explicit consent ---

  Scenario: A user gives explicit consent to analytics tracking
    Given I am logged in as "dr.sarah@desertpaws.ae"
    When I navigate to the preferences page
    And I toggle "Analytics tracking" to ON
    And I save my preferences
    Then the "Analytics tracking" preference is ON
    And analytics events are collected for "dr.sarah@desertpaws.ae"

  Scenario: Cookie banner requires explicit action
    Given "dr.sarah@desertpaws.ae" has never configured analytics preferences
    When "dr.sarah@desertpaws.ae" logs in for the first time
    Then a cookie consent banner is displayed
    And the banner offers "Accept all", "Reject all", and "Manage settings" options
    And no analytics tracking is active until a choice is made

  Scenario: Accepting all via cookie banner enables analytics
    Given "dr.sarah@desertpaws.ae" sees the cookie consent banner
    When "dr.sarah@desertpaws.ae" clicks "Accept all"
    Then the "Analytics tracking" preference is set to ON
    And the "Usage data collection" preference is set to ON
    And the cookie consent banner disappears

  Scenario: Rejecting all via cookie banner keeps analytics off
    Given "dr.sarah@desertpaws.ae" sees the cookie consent banner
    When "dr.sarah@desertpaws.ae" clicks "Reject all"
    Then the "Analytics tracking" preference remains OFF
    And the "Usage data collection" preference remains OFF
    And the cookie consent banner disappears

  # --- Consent changes are audited ---

  Scenario: Enabling analytics consent creates an audit trail entry
    Given I am logged in as "dr.sarah@desertpaws.ae"
    And the "Analytics tracking" preference is OFF
    When I toggle "Analytics tracking" to ON
    And I save my preferences
    Then a consent audit entry is created with:
      | Field          | Value              |
      | User           | dr.sarah@desertpaws.ae |
      | Key            | Analytics tracking |
      | Previous value | OFF                |
      | New value      | ON                 |
      | Source         | user_action        |

  Scenario: Revoking analytics consent creates an audit trail entry
    Given I am logged in as "dr.sarah@desertpaws.ae"
    And the "Analytics tracking" preference is ON
    When I toggle "Analytics tracking" to OFF
    And I save my preferences
    Then a consent audit entry is created with:
      | Field          | Value              |
      | User           | dr.sarah@desertpaws.ae |
      | Key            | Analytics tracking |
      | Previous value | ON                 |
      | New value      | OFF                |
      | Source         | user_action        |

  Scenario: Every consent change is recorded even for repeated toggles
    Given I am logged in as "dr.sarah@desertpaws.ae"
    When I toggle "Analytics tracking" to ON and save
    And I toggle "Analytics tracking" to OFF and save
    And I toggle "Analytics tracking" to ON and save
    Then 3 consent audit entries exist for "dr.sarah@desertpaws.ae" and "Analytics tracking"

  # --- Admin consent summary ---

  Scenario: An admin can view the consent summary for the clinic
    Given I am logged in as "admin@desertpaws.ae"
    And "dr.sarah@desertpaws.ae" has analytics consent ON
    And a receptionist "reception@desertpaws.ae" has analytics consent OFF
    When I navigate to the consent audit page
    Then I see a summary showing:
      | Metric                    | Value |
      | Total users               | 3     |
      | Analytics consent given    | 1     |
      | Analytics consent declined | 1     |
      | Not yet decided            | 1     |

  Scenario: An admin can view the detailed consent audit trail
    Given I am logged in as "admin@desertpaws.ae"
    When I navigate to the consent audit page
    Then I see a chronological list of all consent changes in the clinic
    And each entry shows the user, preference key, old value, new value, and timestamp

  Scenario: A non-admin cannot access the consent audit page
    Given I am logged in as "dr.sarah@desertpaws.ae"
    When I try to navigate to the consent audit page
    Then I receive a 403 Forbidden response
