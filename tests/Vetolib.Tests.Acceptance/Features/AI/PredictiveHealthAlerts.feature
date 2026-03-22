@wip
Feature: Predictive Health Alerts
  As a veterinarian
  I want to see proactive health alerts on the patient dashboard
  So that I can recommend preventive screenings and catch health issues early

  Background:
    Given I am authenticated as a veterinarian at clinic "Al-Noor Veterinary Center"

  # --- Alerts on Dashboard ---

  Scenario: Vet sees health alerts on the patient dashboard
    Given patient "Luna" is a Golden Retriever, female, 8 years old
    And "Luna" has not had a renal panel in the last 14 months
    When I open the patient dashboard for "Luna"
    Then I should see a health alert "Renal screening overdue"
    And the alert should indicate a high priority
    And the alert should mention the breed-specific risk for Golden Retrievers

  Scenario: No alerts displayed for a patient with up-to-date care
    Given patient "Buddy" is a Labrador, male, 3 years old
    And "Buddy" has all vaccinations up to date
    And "Buddy" had a wellness exam last month
    When I open the patient dashboard for "Buddy"
    Then I should see no health alerts

  # --- Breed and Age Based Alerts ---

  Scenario: Alert for overdue blood work based on breed and age
    Given patient "Simba" is a Domestic Shorthair cat, male, 9 years old
    And "Simba" has not had a renal panel in the last 13 months
    When I open the patient dashboard for "Simba"
    Then I should see a health alert "Annual renal screening overdue"
    And the alert description should mention that CKD risk increases after age 7 in cats

  Scenario: Alert for cardiac screening based on breed predisposition
    Given patient "Charlie" is a Cavalier King Charles Spaniel, male, 6 years old
    And "Charlie" has not had a cardiac exam in the last 13 months
    When I open the patient dashboard for "Charlie"
    Then I should see a health alert "Annual cardiac screening recommended"
    And the alert should mention MVD breed predisposition

  Scenario: Alert for senior wellness exam
    Given patient "Rocky" is a German Shepherd, male, 9 years old
    And "Rocky" has not had a wellness exam in the last 7 months
    When I open the patient dashboard for "Rocky"
    Then I should see a health alert "Senior wellness exam recommended"

  # --- Weight Trend Anomaly ---

  Scenario: Alert for weight gain trend anomaly
    Given patient "Max" is a Domestic Shorthair cat, male, 6 years old
    And "Max" weighed 5.0 kg three visits ago
    And "Max" weighed 5.5 kg two visits ago
    And "Max" now weighs 6.1 kg
    When I open the patient dashboard for "Max"
    Then I should see a health alert "Weight gain trend detected"
    And the alert should mention a weight increase of more than 15% over recent visits
    And the alert should recommend an obesity screening

  Scenario: No weight alert when weight is stable
    Given patient "Nala" is a Persian cat, female, 4 years old
    And "Nala" has weighed between 3.8 kg and 4.0 kg across her last 3 visits
    When I open the patient dashboard for "Nala"
    Then I should not see a weight-related health alert

  # --- Vet Actions on Alerts ---

  Scenario: Vet dismisses a health alert
    Given patient "Luna" has a health alert "Renal screening overdue"
    When I dismiss the alert with reason "Owner declined screening"
    Then the alert should no longer appear on the dashboard
    And the dismissal should be recorded with the reason and my name

  Scenario: Vet converts a health alert into an appointment
    Given patient "Charlie" has a health alert "Annual cardiac screening recommended"
    When I choose to schedule an appointment from the alert
    Then a new appointment should be pre-filled for patient "Charlie"
    And the appointment notes should reference the health alert
    And the alert status should change to "Scheduled"

  Scenario: Vet acknowledges an alert without immediate action
    Given patient "Rocky" has a health alert "Senior wellness exam recommended"
    When I acknowledge the alert
    Then the alert should be marked as "Acknowledged"
    And it should remain visible but deprioritized on the dashboard

  # --- Alert Generation Rules ---

  Scenario: Overdue vaccination generates a health alert
    Given patient "Bella" is a Labrador, female, 2 years old
    And "Bella" has a core vaccination overdue by 45 days
    When I open the patient dashboard for "Bella"
    Then I should see a health alert "Core vaccination overdue"
    And the alert should indicate it is 45 days past due

  Scenario: Brachycephalic breed with respiratory history receives airway alert
    Given patient "Mochi" is a French Bulldog, male, 4 years old
    And "Mochi" has a previous diagnosis of respiratory distress
    When I open the patient dashboard for "Mochi"
    Then I should see a health alert "Annual airway assessment recommended"
    And the alert should mention brachycephalic breed risk

  # --- Multi-Tenancy ---

  Scenario: Health alerts are scoped to the current clinic
    Given patient "Luna" exists at clinic "Al-Noor Veterinary Center" with 2 alerts
    And patient "Luna" also exists at clinic "Dubai Pet Hospital" with 1 alert
    When I view the patient dashboard at "Al-Noor Veterinary Center"
    Then I should only see the 2 alerts from my clinic
