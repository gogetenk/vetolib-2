Feature: Clinic Search
  As a pet owner
  I want to search for veterinary clinics
  So that I can find a clinic near me that treats my pet

  Background:
    Given the following clinics exist in the directory
      | Name                   | City      | Supported Species       |
      | Desert Paws Vet Clinic | Dubai     | Dog, Cat, Rabbit        |
      | Al Barsha Pet Hospital | Dubai     | Dog, Cat, Horse, Falcon |
      | Abu Dhabi Animal Care  | Abu Dhabi | Dog, Cat                |
      | Sharjah Exotic Clinic  | Sharjah   | Falcon, Reptile, Bird   |

  Scenario: Search clinics by name with partial match
    When I search for clinics with name "Desert"
    Then I should see 1 clinic in the results
    And the results should include "Desert Paws Vet Clinic"

  Scenario: Search clinics by city
    When I search for clinics in city "Dubai"
    Then I should see 2 clinics in the results
    And the results should include "Desert Paws Vet Clinic"
    And the results should include "Al Barsha Pet Hospital"

  Scenario: Search clinics by supported species
    When I search for clinics that treat "Falcon"
    Then I should see 2 clinics in the results
    And the results should include "Al Barsha Pet Hospital"
    And the results should include "Sharjah Exotic Clinic"

  Scenario: Search clinics with combined filters
    When I search for clinics in city "Dubai" that treat "Dog"
    Then I should see 2 clinics in the results

  Scenario: Search clinics with no matching results
    When I search for clinics with name "NonExistent"
    Then I should see 0 clinics in the results

  Scenario: Search is case insensitive
    When I search for clinics with name "desert paws"
    Then I should see 1 clinic in the results
    And the results should include "Desert Paws Vet Clinic"

  Scenario: Search results are paginated
    When I search for all clinics with page size 2
    Then I should see 2 clinics in the results
    And the total count should be 4

  Scenario: No authentication required for clinic search
    When I search for clinics without being logged in
    Then the search should succeed
