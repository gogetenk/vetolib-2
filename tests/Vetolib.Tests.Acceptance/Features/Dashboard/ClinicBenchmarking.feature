@wip
Feature: Clinic Benchmarking
  As a clinic administrator
  I want to compare my clinic's performance against anonymized market averages
  So that I can identify areas for improvement and track my competitive position

  Background:
    Given I am authenticated as an administrator at clinic "Desert Paws Veterinary"
    And my clinic has opted into the benchmarking program

  # --- Viewing Metrics vs Market ---

  Scenario: Admin sees clinic metrics compared to market average
    Given the benchmarking pool contains data from 15 clinics in the UAE
    When I open the benchmarking dashboard
    Then I should see my clinic's average transaction value compared to the UAE median
    And I should see my clinic's no-show rate compared to the UAE median
    And I should see my clinic's consultations per week compared to the UAE median
    And each metric should show my clinic's percentile ranking

  Scenario: Admin sees percentile position for each metric
    Given my clinic's average transaction value is 480 AED
    And the UAE median average transaction value is 420 AED
    When I open the benchmarking dashboard
    Then I should see that my clinic is in the 68th percentile for average transaction value
    And the display should show the median and top 25% values for reference

  # --- Anonymization ---

  Scenario: Benchmarking data is fully anonymized
    When I view the benchmarking dashboard
    Then I should not see any other clinic's name
    And I should not see any other clinic's individual data
    And I should only see aggregated statistics such as median, top 25%, and top 10%

  Scenario: Benchmarking never reveals individual clinic identity
    Given the benchmarking pool contains data from 8 clinics
    When I view the revenue metrics
    Then I should see my own value and the market median
    But I should not see minimum or maximum values
    And no individual clinic should be identifiable from the displayed data

  # --- Recommendations ---

  Scenario: Admin sees a recommendation to improve the no-show rate
    Given my clinic's no-show rate is 18%
    And the UAE median no-show rate is 10%
    When I open the benchmarking dashboard
    Then I should see a recommendation suggesting ways to reduce my no-show rate
    And the recommendation language should be encouraging and actionable

  Scenario: No recommendation shown when metric is above median
    Given my clinic's slot utilization rate is 85%
    And the UAE median slot utilization rate is 72%
    When I open the benchmarking dashboard
    Then I should not see a recommendation for slot utilization
    And the metric should be highlighted as a strength

  # --- Minimum Pool Size ---

  Scenario: Benchmarking requires a minimum of 5 clinics in the pool
    Given the benchmarking pool for my emirate contains only 3 clinics
    When I attempt to view emirate-level benchmarks
    Then the benchmarks should not be displayed
    And I should see a message "Not enough clinics in this segment yet"

  Scenario: National benchmarks available when pool is large enough
    Given the benchmarking pool contains 12 clinics across the UAE
    When I view national-level benchmarks
    Then the benchmarks should be displayed with aggregated data from all 12 clinics

  # --- Opt-In / Opt-Out ---

  Scenario: Clinic can opt out of the benchmarking program
    When I disable benchmarking participation in my clinic settings
    Then my clinic's data should be excluded from future benchmark calculations
    And I should no longer see the benchmarking dashboard

  Scenario: Newly opted-in clinic sees benchmarks after next data refresh
    Given my clinic has just opted into benchmarking
    When the nightly benchmark refresh job runs
    Then my clinic's data should be included in the aggregated benchmarks
    And I should be able to view my metrics compared to the market

  # --- Multi-Tenancy ---

  Scenario: Admin only sees their own clinic's data on the benchmarking dashboard
    Given clinic "Desert Paws Veterinary" has an average transaction value of 480 AED
    And clinic "Marina Pet Clinic" has an average transaction value of 520 AED
    When I open the benchmarking dashboard
    Then I should see 480 AED as my clinic's value
    And I should not see 520 AED or any data attributed to "Marina Pet Clinic"
