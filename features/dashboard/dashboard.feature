Feature: Dashboard statistics
  As a clinic user
  I want to see dashboard statistics and today's schedule
  So that I can manage the clinic efficiently

  Background:
    Given une clinique "Happy Paws"
    And je suis authentifié en tant que ADMIN

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
