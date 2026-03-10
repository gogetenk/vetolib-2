Feature: Analytics Consent Management
  As a clinic staff member
  I want to control whether my usage is tracked by analytics
  So that I have transparency over my data

  Background:
    Given I am logged in as a "VET" user
    And I have never set my analytics consent preference

  # UI element: data-testid="consent-banner"
  # Accept button: data-testid="consent-accept"
  # Decline button: data-testid="consent-decline"
  # Settings toggle: data-testid="analytics-consent-toggle"

  Scenario: User sees a consent banner on first login
    When I land on the dashboard for the first time
    Then I should see the analytics consent banner
    And the banner should explain that usage data is collected to improve the product
    And the banner should offer an "Accept" and a "Decline" option

  Scenario: User accepts analytics tracking
    Given I see the analytics consent banner
    When I click "Accept"
    Then the consent banner should disappear
    And my analytics preference should be saved as "accepted"
    And I should not see the consent banner on my next visit

  Scenario: User declines analytics tracking
    Given I see the analytics consent banner
    When I click "Decline"
    Then the consent banner should disappear
    And my analytics preference should be saved as "declined"
    And I should not see the consent banner on my next visit

  Scenario: User changes consent preference in settings
    Given I have previously accepted analytics tracking
    When I navigate to the settings page
    And I toggle the analytics tracking option to "off"
    Then my analytics preference should be updated to "declined"
    And a confirmation message should be displayed

  Scenario: Analytics tracking does not start when user has declined
    Given I have declined analytics tracking
    When I navigate through the application
    Then no analytics events should be sent to the tracking service

  Scenario: Analytics tracking starts when user has accepted
    Given I have accepted analytics tracking
    When I navigate through the application
    Then analytics events should be sent to the tracking service

  Scenario: Consent banner does not appear on the public landing page
    Given I am not logged in
    When I visit the landing page
    Then I should not see the analytics consent banner

  Scenario: Admin can opt the entire clinic out of analytics
    Given I am logged in as an "ADMIN" user
    When I navigate to the clinic settings page
    And I toggle the clinic-level analytics tracking to "off"
    Then analytics tracking should be disabled for all users in the clinic
    And individual users should not see the consent banner
