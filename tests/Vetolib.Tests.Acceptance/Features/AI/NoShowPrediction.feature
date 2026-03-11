@wip
Feature: No-Show Prediction
  As a veterinarian or receptionist
  I want to see the probability of a patient not showing up
  So that I can send extra reminders or plan overbooking

  Background:
    Given I am authenticated as a user with role "Vet"

  Scenario: Predict no-show for appointment with sufficient history
    Given the clinic has more than 50 completed appointments with no-show data
    And an appointment exists for owner "Al-Rashid" on "2026-03-15" at "10:00"
    When I request a no-show prediction for that appointment
    Then I should receive a prediction with status 200
    And the prediction should contain a probability between 0 and 1
    And the prediction should contain a risk level of "Low", "Medium", or "High"
    And the prediction should contain top contributing factors
    And the prediction should contain actionable suggestions

  Scenario: Cold start returns insufficient data error
    Given the clinic has fewer than 50 completed appointments
    And an appointment exists for owner "New Client" on "2026-03-15" at "10:00"
    When I request a no-show prediction for that appointment
    Then I should receive an error "INSUFFICIENT_DATA"

  Scenario: Batch prediction for a given date
    Given the clinic has sufficient appointment history
    And there are 5 appointments on "2026-03-15"
    When I request batch no-show predictions for "2026-03-15"
    Then I should receive 5 predictions
    And each prediction should contain a probability and risk level

  Scenario: High risk triggers reminder suggestion
    Given owner "Habitual Skipper" has a 40% historical no-show rate
    And an appointment exists for "Habitual Skipper" on "2026-03-15"
    When I request a no-show prediction for that appointment
    Then the risk level should be "High"
    And the suggestions should include sending an extra reminder

  Scenario: No-show score is never visible to owners
    Given I am authenticated as a user with role "Owner"
    When I request a no-show prediction for any appointment
    Then I should receive a 403 Forbidden response

  Scenario: Receptionist can view predictions
    Given I am authenticated as a user with role "Receptionist"
    And the clinic has sufficient appointment history
    And an appointment exists on "2026-03-15"
    When I request a no-show prediction for that appointment
    Then I should receive a prediction with status 200

  Scenario: Prediction factors are non-discriminatory
    When I request a no-show prediction for an appointment
    Then the top factors should not include owner name or address
    And the factors should reference behavioral data only

  Scenario: Weekend appointments have adjusted prediction
    Given the clinic operates Sunday through Thursday
    And an appointment exists on a Friday
    When I request a no-show prediction for that appointment
    Then the day-of-week factor should reflect UAE weekend patterns
