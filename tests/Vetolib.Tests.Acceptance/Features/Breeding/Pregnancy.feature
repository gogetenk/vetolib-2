Feature: Pregnancy and gestation tracking
  As a veterinarian managing breeding animals,
  I want to track pregnancies from mating to delivery
  so that I can schedule follow-up exams and anticipate complications.

  Background:
    Given a clinic "Royal Stud Farm"
    And an owner "Mohammed Al Maktoum" with email "mohammed@example.ae"
    And a patient "Yasmin" species "Horse" breed "Arabian" sex "Female" belonging to "Mohammed Al Maktoum"
    And I am authenticated as VET

  Scenario: Record a natural mating
    When I record a pregnancy for "Yasmin" with method "Natural" mated on 2026-02-15
    Then the pregnancy is created for "Yasmin"
    And the expected due date is calculated based on species gestation period

  Scenario: Record an artificial insemination
    When I record a pregnancy for "Yasmin" with method "ArtificialInsemination" mated on 2026-02-15
    Then the pregnancy is created with method "ArtificialInsemination"

  Scenario: Record an embryo transfer
    When I record a pregnancy for "Yasmin" with method "EmbryoTransfer" mated on 2026-02-15
    Then the pregnancy is created with method "EmbryoTransfer"

  Scenario: Expected due date is species-specific
    Given a patient "Bella" species "Dog" breed "Saluki" sex "Female" belonging to "Mohammed Al Maktoum"
    When I record a pregnancy for "Bella" with method "Natural" mated on 2026-01-01
    Then the expected due date for "Bella" is approximately 63 days after mating

  Scenario: Schedule an ultrasound check
    Given a pregnancy recorded for "Yasmin"
    When I schedule an ultrasound for "Yasmin" on 2026-03-15 with note "28-day confirmation scan"
    Then the ultrasound appears in the pregnancy timeline

  Scenario: Record delivery outcome — live birth
    Given a pregnancy recorded for "Yasmin"
    When I record the delivery on 2026-12-20 with outcome "LiveBirth" and 1 offspring
    Then the pregnancy is marked as completed
    And the actual delivery date is 2026-12-20

  Scenario: Record delivery outcome — stillbirth
    Given a pregnancy recorded for "Yasmin"
    When I record the delivery on 2026-12-20 with outcome "Stillbirth" and 0 live offspring
    Then the pregnancy is marked as completed with outcome "Stillbirth"

  Scenario: Record delivery outcome — miscarriage
    Given a pregnancy recorded for "Yasmin"
    When I record the pregnancy ended with outcome "Miscarriage" on 2026-08-01
    Then the pregnancy is marked as completed with outcome "Miscarriage"

  Scenario: Only female patients can have pregnancies
    Given a patient "Majid" species "Horse" breed "Arabian" sex "Male" belonging to "Mohammed Al Maktoum"
    When I attempt to record a pregnancy for "Majid"
    Then the system rejects with reason "Only female patients can have pregnancies"

  Scenario: Cannot record pregnancy for spayed patient
    Given a patient "Noor" species "Dog" breed "Saluki" sex "SpayedFemale" belonging to "Mohammed Al Maktoum"
    When I attempt to record a pregnancy for "Noor"
    Then the system rejects with reason "Spayed patients cannot be pregnant"

  Scenario: View active pregnancies for a clinic
    Given 2 active pregnancies in the clinic
    When I view active pregnancies
    Then I see 2 pregnancies sorted by expected due date
