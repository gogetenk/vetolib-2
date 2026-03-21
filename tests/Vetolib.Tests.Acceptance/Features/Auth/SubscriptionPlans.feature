@wip
Feature: Subscription plan limits
  As a clinic administrator
  I want the system to enforce limits based on my subscription plan
  So that I can upgrade when my clinic grows

  Background:
    Given a clinic "Desert Paws Veterinary" in Dubai

  # --- Vet limits per plan ---

  Scenario: Free plan clinic cannot add more than 1 vet
    Given the clinic is on the "Free" plan
    And the clinic already has 1 active vet
    When the admin tries to invite a new vet "dr.omar@desertpaws.ae"
    Then the invitation is rejected with the message "Your Free plan allows a maximum of 1 veterinarian. Please upgrade to add more."

  Scenario: Starter plan clinic can add up to 3 vets
    Given the clinic is on the "Starter" plan
    And the clinic already has 2 active vets
    When the admin invites a new vet "dr.layla@desertpaws.ae"
    Then the invitation succeeds

  Scenario: Starter plan clinic cannot exceed 3 vets
    Given the clinic is on the "Starter" plan
    And the clinic already has 3 active vets
    When the admin tries to invite a new vet "dr.sara@desertpaws.ae"
    Then the invitation is rejected with the message "Your Starter plan allows a maximum of 3 veterinarians. Please upgrade to add more."

  Scenario: Pro plan clinic has unlimited vets
    Given the clinic is on the "Pro" plan
    And the clinic already has 10 active vets
    When the admin invites a new vet "dr.new@desertpaws.ae"
    Then the invitation succeeds

  # --- Usage warnings ---

  Scenario Outline: Warning is shown when approaching the plan limit
    Given the clinic is on the "<plan>" plan with a limit of <limit> vets
    And the clinic has <current> active vets
    When the admin opens the team management page
    Then a warning is displayed: "You are using <current> of <limit> veterinarian slots. Consider upgrading your plan."

    Examples:
      | plan    | limit | current |
      | Starter | 3     | 3       |
      | Free    | 1     | 1       |

  Scenario: No warning when usage is below 80%
    Given the clinic is on the "Starter" plan with a limit of 3 vets
    And the clinic has 1 active vet
    When the admin opens the team management page
    Then no plan limit warning is displayed

  # --- Trial expiration ---

  Scenario: Expired trial automatically downgrades to Free
    Given the clinic is on a "Trial" plan that started 15 days ago
    And the trial period is 14 days
    When the trial expires
    Then the clinic is downgraded to the "Free" plan
    And the admin receives a notification "Your trial has expired. Your clinic is now on the Free plan."

  Scenario: Expired trial with more vets than Free allows
    Given the clinic is on a "Trial" plan with 3 active vets
    And the trial period has expired
    When the clinic is downgraded to the "Free" plan
    Then existing vets remain active but the admin cannot add new ones
    And the admin sees a message "You have more veterinarians than your Free plan allows. Please upgrade or deactivate extra accounts."
