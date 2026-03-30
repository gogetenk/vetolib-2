Feature: Patient lineage and pedigree
  As a breeder or veterinarian,
  I want to navigate a patient's family tree — parents, grandparents, and descendants —
  so that I can track genetic lines and make informed breeding decisions.

  Background:
    Given a clinic "Haras de la Reine"
    And an owner "Jean-Pierre Dupont" with email "jp@example.fr"
    And I am authenticated as VET

  Scenario: Set parents on a patient
    Given a patient "Etoile" species "Horse" breed "Selle Francais" sex "Female" belonging to "Jean-Pierre Dupont"
    And a patient "Tonnerre" species "Horse" breed "Selle Francais" sex "Male" belonging to "Jean-Pierre Dupont"
    And a patient "Aurore" species "Horse" breed "Selle Francais" sex "Female" belonging to "Jean-Pierre Dupont"
    When I set the mother of "Etoile" to "Aurore" and the father to "Tonnerre"
    Then the lineage of "Etoile" shows "Aurore" as mother and "Tonnerre" as father

  Scenario: Navigate upward — view grandparents
    Given a 3-generation lineage:
      | Patient  | Mother  | Father   |
      | Etoile   | Aurore  | Tonnerre |
      | Aurore   | Comete  | Eclat    |
      | Tonnerre | Brume   | Orage    |
    When I view the pedigree of "Etoile"
    Then I see 2 generations of ancestors
    And the maternal grandmother is "Comete"
    And the paternal grandfather is "Orage"

  Scenario: Navigate downward — view descendants
    Given "Aurore" is the mother of "Etoile" and "Soleil"
    When I view the descendants of "Aurore"
    Then I see "Etoile" and "Soleil" as offspring

  Scenario: Lineage respects species consistency
    Given a patient "Cleo" species "Cat" breed "Chartreux" sex "Female" belonging to "Jean-Pierre Dupont"
    And a patient "Sultan" species "Horse" breed "Arabian" sex "Male" belonging to "Jean-Pierre Dupont"
    When I attempt to set the father of "Cleo" to "Sultan"
    Then the system rejects with reason "Parent and offspring must be the same species"

  Scenario: LOF-registered dog lineage (France)
    Given a patient "Oscar" species "Dog" breed "Berger de Beauce" sex "Male" belonging to "Jean-Pierre Dupont"
    And "Oscar" has LOF number "123456"
    When I view the profile of "Oscar"
    Then I see the LOF registration number "123456"

  Scenario: Falcon pedigree (UAE)
    Given a clinic "Abu Dhabi Falcon Hospital"
    And an owner "Sultan Al Falasi" with email "sultan@example.ae"
    And a patient "Haboob" species "Falcon" breed "Peregrine" sex "Male" belonging to "Sultan Al Falasi"
    And a patient "Rimal" species "Falcon" breed "Peregrine" sex "Female" belonging to "Sultan Al Falasi"
    When I set the mother of "Haboob" to "Rimal"
    Then the lineage of "Haboob" shows "Rimal" as mother
