Feature: Analytics Event Tracking
  As the Vetolib product team
  I want user actions to be tracked as analytics events
  So that I can measure feature adoption, identify churn signals, and prioritize the roadmap

  Background:
    Given I am logged in as a "VET" user
    And I have accepted analytics tracking

  # All events must include clinicId (anonymized) and user role as properties.
  # No PII (patient names, owner names, emails, medical data) may be sent in event properties.

  Scenario: Page views are tracked automatically in the dashboard
    When I navigate to the "Appointments" page
    Then a page view event should be recorded with the route "/appointments"
    When I navigate to the "Patients" page
    Then a page view event should be recorded with the route "/patients"

  Scenario: Creating an appointment generates an analytics event
    When I create a new appointment with species "Dog"
    Then an "appointment_created" event should be recorded
    And the event should include the property "species" with value "Dog"

  Scenario: Creating an invoice generates an analytics event
    When I create a new invoice with 2 items
    Then an "invoice_created" event should be recorded
    And the event should include the property "item_count" with value "2"

  Scenario: Adding a patient generates an analytics event
    When I add a new patient with species "Cat"
    Then a "patient_created" event should be recorded
    And the event should include the property "species" with value "Cat"

  Scenario: Using AI triage generates an analytics event
    When I use the AI triage feature for a patient
    And I accept the triage suggestion
    Then an "ai_triage_accepted" event should be recorded
    When I override the triage suggestion
    Then an "ai_triage_overridden" event should be recorded

  Scenario: No analytics events are sent when user has opted out
    Given I have declined analytics tracking
    When I create a new appointment with species "Dog"
    And I add a new patient with species "Cat"
    And I create a new invoice with 1 items
    Then no analytics events should be sent to the tracking service

  Scenario: Sending an invoice generates an analytics event
    When I change an invoice status from "DRAFT" to "SENT"
    Then an "invoice_sent" event should be recorded

  Scenario: Downloading an invoice PDF generates an analytics event
    Given an invoice exists with status "SENT"
    When I download the invoice as PDF
    Then an "invoice_pdf_downloaded" event should be recorded

  Scenario: CSV import of patients generates an analytics event
    When I import a CSV file with 10 patient records
    And 8 records are imported successfully and 2 have errors
    Then a "csv_import_completed" event should be recorded
    And the event should include the property "row_count" with value "10"
    And the event should include the property "success_count" with value "8"
    And the event should include the property "error_count" with value "2"

  Scenario: Time from login to first action is tracked
    When I log in to the application
    And I create my first appointment within the session
    Then a "first_action_after_login" event should be recorded
    And the event should include the elapsed time in seconds

  Scenario: No personally identifiable information is sent in events
    When I create a new appointment for a patient named "Buddy" owned by "Ahmed Al-Rashid"
    Then the analytics event should not contain the patient name "Buddy"
    And the analytics event should not contain the owner name "Ahmed Al-Rashid"
    And the analytics event should only contain anonymized identifiers
