Feature: Dashboard statistics
  As a clinic user
  I want to see dashboard statistics and today's schedule
  So that I can manage the clinic efficiently

  Background:
    Given a clinic "Happy Paws"
    And I am authenticated as ADMIN

  Scenario: Admin sees all dashboard stats
    When I request dashboard stats
    Then I see appointments today count
    And I see pending checkin count
    And I see unpaid invoices total in AED
    And I see total patients count

  Scenario: Today appointments list
    Given there are 3 appointments today
    When I request today's appointments
    Then I see 3 appointments sorted by time

  Scenario: Recent activity feed
    When I request recent activity
    Then I receive a recent activity list

  Scenario: Admin sees analytics data
    When I request dashboard analytics
    Then the operation succeeds
    And the analytics include a revenue by month list
    And the analytics include an appointments by status list

  Scenario: Analytics endpoint requires authentication
    When I request dashboard analytics without authentication
    Then the user is denied access

  Scenario: Non-admin cannot access analytics
    Given I am authenticated as RECEPTIONIST
    When I request dashboard analytics
    Then the user is denied access
