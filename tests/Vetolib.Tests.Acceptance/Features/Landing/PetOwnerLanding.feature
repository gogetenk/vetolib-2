@wip
Feature: Pet owner landing page
  As a pet owner visiting Vetolib for the first time
  I want to understand what the platform offers and find my clinic
  So that I can register and manage my pet's health online

  # --- Value proposition --- #

  Scenario: Owner sees the value proposition on the landing page
    When I visit the Vetolib pet owner landing page
    Then I see a headline about managing my pet's health
    And I see benefits including medical records, vaccination reminders, and appointment booking
    And I see a search bar to find my clinic

  Scenario: Landing page displays trust signals
    When I visit the Vetolib pet owner landing page
    Then I see the number of clinics registered on Vetolib
    And I see testimonials from pet owners

  # --- Clinic search from landing --- #

  Scenario: Owner searches for their clinic from the landing page
    Given the clinic "Happy Paws" in Dubai is registered on Vetolib
    When I visit the Vetolib pet owner landing page
    And I search for "Happy Paws" in the clinic search bar
    Then I see "Happy Paws" in the results
    And I can select it to start registration

  Scenario: Owner selects a clinic and is directed to registration
    Given the clinic "Happy Paws" in Dubai is registered on Vetolib
    When I search for "Happy Paws" in the clinic search bar
    And I select "Happy Paws"
    Then I am directed to the owner registration page
    And "Happy Paws" is pre-selected as my clinic

  # --- Clinic not found flow --- #

  Scenario: Owner's clinic is not on Vetolib
    When I search for "My Local Vet" in the clinic search bar
    And no results are found
    Then I see a message "Your clinic is not yet on Vetolib"
    And I see an option to notify my veterinarian

  Scenario: Owner requests their vet to join Vetolib
    Given I searched for a clinic and it was not found
    When I click "Ask your vet to join"
    And I enter my veterinarian's email "drvet@example.com"
    And I submit the request
    Then I see a confirmation that an invitation has been sent
    And my veterinarian receives an email about Vetolib

  # --- Registration entry points --- #

  Scenario: Owner clicks the main registration button
    When I visit the Vetolib pet owner landing page
    And I click "Create my account"
    Then I am directed to the owner registration page

  Scenario: Landing page is accessible on mobile
    When I visit the Vetolib pet owner landing page on a mobile device
    Then the page is responsive and readable
    And the clinic search bar is prominently displayed
    And the registration button is easily tappable
