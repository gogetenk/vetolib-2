@wip
Feature: Loyalty Program
  As a clinic administrator
  I want to offer a points-based loyalty program to pet owners
  So that I can improve client retention and reward preventive care compliance

  Background:
    Given a clinic "Happy Paws Veterinary" in Dubai has the loyalty program enabled
    And the loyalty program is configured with 1 point per AED spent
    And points expire after 12 months of inactivity

  # --- Earning Points ---

  Scenario: Owner earns points when paying an invoice
    Given pet owner "Fatima Al-Hashimi" has a loyalty account with 0 points
    When "Fatima Al-Hashimi" pays an invoice of 450 AED for a consultation
    Then her loyalty balance should be 450 points
    And she should see a transaction "Earned 450 points from invoice"

  Scenario: Owner earns points from a vaccination visit
    Given pet owner "Rashid Al-Maktoum" has a loyalty account with 200 points
    When "Rashid Al-Maktoum" pays an invoice of 300 AED for a vaccination
    Then his loyalty balance should be 500 points

  # --- Compliance Bonus ---

  Scenario: Owner receives bonus points for on-time vaccination
    Given pet owner "Sarah Johnson" has a pet "Bella" with a rabies vaccine due on 15 March 2026
    And the clinic awards 50 bonus points for on-time vaccinations
    When "Bella" receives her rabies vaccination on 10 March 2026
    Then "Sarah Johnson" should earn 50 bonus compliance points
    And she should see a transaction "Compliance bonus: on-time vaccination for Bella"

  Scenario: Owner does not receive bonus for late vaccination
    Given pet owner "Ahmed Al-Suwaidi" has a pet "Rex" with a vaccine due on 1 February 2026
    And the clinic awards 50 bonus points for on-time vaccinations
    When "Rex" receives the vaccination on 15 March 2026
    Then "Ahmed Al-Suwaidi" should not receive compliance bonus points

  # --- Redeeming Points ---

  Scenario: Owner redeems points for a discount on the next invoice
    Given pet owner "Fatima Al-Hashimi" has a loyalty account with 500 points
    And the clinic offers a reward "25 AED discount" costing 500 points
    When "Fatima Al-Hashimi" redeems the "25 AED discount" reward
    Then her loyalty balance should be 0 points
    And she should receive a 25 AED discount on her next invoice

  Scenario: Owner cannot redeem a reward with insufficient points
    Given pet owner "Omar Khalil" has a loyalty account with 100 points
    And the clinic offers a reward "Free nail trim" costing 300 points
    When "Omar Khalil" attempts to redeem the "Free nail trim" reward
    Then the redemption should be refused because of insufficient points
    And his loyalty balance should remain 100 points

  # --- Point Expiration ---

  Scenario: Points expire after 12 months of inactivity
    Given pet owner "Layla Al-Farsi" earned 200 points on 1 January 2025
    And she has had no activity since then
    When the nightly expiration job runs on 2 January 2026
    Then her 200 points from January 2025 should expire
    And she should see a transaction "200 points expired (12-month inactivity)"

  Scenario: Active owner's points do not expire
    Given pet owner "Noor Hassan" earned 300 points on 1 March 2025
    And she earned 100 points on 15 December 2025
    When the nightly expiration job runs on 2 March 2026
    Then her total balance should still include the 300 points from March 2025
    Because her account had activity within the last 12 months

  # --- Admin Configuration ---

  Scenario: Clinic admin configures the points-per-AED rate
    Given I am authenticated as a clinic administrator
    When I set the loyalty earning rate to 2 points per AED spent
    Then all future invoices should earn points at 2 points per AED

  Scenario: Clinic admin creates a new reward in the catalog
    Given I am authenticated as a clinic administrator
    When I create a reward "Free dental checkup" costing 1000 points
    Then the reward should appear in the clinic's reward catalog
    And pet owners should be able to see and redeem it

  Scenario: Clinic admin sets the minimum redemption threshold
    Given I am authenticated as a clinic administrator
    When I set the minimum redemption threshold to 200 points
    Then owners with fewer than 200 points should not be able to redeem any reward

  # --- Multi-Tenancy ---

  Scenario: Loyalty points are isolated between clinics
    Given pet owner "Fatima Al-Hashimi" has 500 points at clinic "Happy Paws Veterinary"
    And pet owner "Fatima Al-Hashimi" has 200 points at clinic "Dubai Pet Care"
    When "Fatima Al-Hashimi" views her loyalty account at "Happy Paws Veterinary"
    Then she should see 500 points
    And she should not see the 200 points from "Dubai Pet Care"
