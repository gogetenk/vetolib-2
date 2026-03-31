@wip
Feature: Pricing page for veterinary clinics
  As a veterinarian evaluating Vetolib
  I want to see the available plans and their features
  So that I can choose the right plan for my clinic

  # --- Plan display --- #

  Scenario: Veterinarian sees the three available plans
    When I visit the Vetolib pricing page
    Then I see 3 plans displayed side by side
    And I see a "Starter" plan
    And I see a "Pro" plan
    And I see an "Enterprise" plan

  Scenario: Each plan shows its monthly price in AED
    When I visit the Vetolib pricing page
    Then the "Starter" plan displays a price in AED per month
    And the "Pro" plan displays a price in AED per month
    And the "Enterprise" plan displays "Contact us" instead of a price

  # --- Feature comparison --- #

  Scenario: Plans show a feature comparison
    When I visit the Vetolib pricing page
    Then I see a feature comparison table
    And the table lists features with checkmarks for each plan
    And the "Pro" plan includes more features than "Starter"
    And the "Enterprise" plan includes all features

  Scenario: Starter plan shows its included features
    When I visit the Vetolib pricing page
    And I review the "Starter" plan
    Then I see it includes appointment management
    And I see it includes basic medical records
    And I see the maximum number of users allowed

  Scenario: Pro plan highlights additional features over Starter
    When I visit the Vetolib pricing page
    And I review the "Pro" plan
    Then I see it includes everything in "Starter"
    And I see it includes advanced analytics
    And I see it includes the owner portal
    And I see it includes WhatsApp notifications

  Scenario: Enterprise plan shows premium features
    When I visit the Vetolib pricing page
    And I review the "Enterprise" plan
    Then I see it includes everything in "Pro"
    And I see it includes multi-clinic management
    And I see it includes dedicated support
    And I see it includes custom integrations

  # --- Call to action --- #

  Scenario: Veterinarian clicks to sign up for the Starter plan
    When I visit the Vetolib pricing page
    And I click "Get started" on the "Starter" plan
    Then I am directed to the clinic registration page
    And the "Starter" plan is pre-selected

  Scenario: Veterinarian clicks to sign up for the Pro plan
    When I visit the Vetolib pricing page
    And I click "Get started" on the "Pro" plan
    Then I am directed to the clinic registration page
    And the "Pro" plan is pre-selected

  Scenario: Veterinarian clicks to contact sales for Enterprise
    When I visit the Vetolib pricing page
    And I click "Contact us" on the "Enterprise" plan
    Then I see a contact form to reach the Vetolib sales team

  # --- Pro plan highlight --- #

  Scenario: Pro plan is visually highlighted as the recommended choice
    When I visit the Vetolib pricing page
    Then the "Pro" plan is visually highlighted
    And the "Pro" plan shows a "Most popular" badge

  # --- Annual discount --- #

  Scenario: Pricing page shows a toggle for annual billing
    When I visit the Vetolib pricing page
    And I switch to annual billing
    Then the prices are lower than monthly billing
    And I see the discount percentage displayed
