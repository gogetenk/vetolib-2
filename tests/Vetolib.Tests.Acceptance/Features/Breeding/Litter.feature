@wip
Feature: Litter management
  As a breeder or veterinarian,
  I want to record litters with their parents and offspring
  so that I can track breeding outcomes and link puppies/kittens/foals to their parents.

  Background:
    Given a clinic "Emirates Equine Centre"
    And an owner "Khalid Al Nahyan" with email "khalid@example.ae"
    And a patient "Shams" species "Horse" breed "Arabian" sex "Female" belonging to "Khalid Al Nahyan"
    And a patient "Buraq" species "Horse" breed "Arabian" sex "Male" belonging to "Khalid Al Nahyan"
    And I am authenticated as VET

  Scenario: Register a new litter
    When I register a litter for mother "Shams" with father "Buraq" on 2026-03-10 with 1 born and 1 alive
    Then the litter is created and linked to "Shams"
    And the litter shows "Buraq" as the father

  Scenario: Register a litter with external father
    When I register a litter for mother "Shams" with external father "Desert Wind" on 2026-03-10 with 1 born and 1 alive
    Then the litter is created with external father name "Desert Wind"

  Scenario: Add offspring to a litter
    Given a litter for mother "Shams" born on 2026-03-10
    When I add offspring "Najm" sex "Male" breed "Arabian" to the litter
    Then "Najm" appears as a patient in the clinic
    And "Najm" is linked to the litter as offspring

  Scenario: Offspring count must be consistent
    When I attempt to register a litter with 3 born and 5 alive
    Then the system rejects with reason "Alive count cannot exceed born count"

  Scenario: View litter details
    Given a litter for mother "Shams" with 3 offspring registered
    When I view the litter details
    Then I see the mother "Shams", the father, birth date, and all 3 offspring

  Scenario: Mother must be female
    When I attempt to register a litter for mother "Buraq"
    Then the system rejects with reason "Only female patients can be registered as mothers"

  Scenario: Father and mother must be same species
    Given a patient "Felix" species "Cat" breed "Persian" sex "Male" belonging to "Khalid Al Nahyan"
    When I attempt to register a litter for mother "Shams" with father "Felix"
    Then the system rejects with reason "Father and mother must be the same species"

  Scenario: View all litters for a mother
    Given 2 litters registered for mother "Shams"
    When I view the breeding history of "Shams"
    Then I see 2 litters in reverse chronological order

  Scenario: Tenant isolation on litters
    Given a litter for mother "Shams" in clinic "Emirates Equine Centre"
    And a clinic "Dubai Pet Care"
    And I am authenticated as VET in "Dubai Pet Care"
    When I list litters
    Then I do not see litters from "Emirates Equine Centre"
