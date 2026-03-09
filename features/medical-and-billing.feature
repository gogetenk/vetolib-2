# language: en
Feature: Medical Records
  As a vet
  I want to create and view medical records for animals
  So that I maintain accurate health history

  Background:
    Given I am logged in as vet "dr.sarah@desertpaws.ae" of clinic "clinic-001"

  Scenario: View a patient's complete medical history
    Given patient "Max" (Golden Retriever) has 3 past medical records
    When I navigate to Patients and open Max's record
    Then I see a profile card with species, breed, age, and owner contact
    And I see 3 records in the "Medical Records" tab
    And each record shows: date, vet name, reason, and a summary

  Scenario: Vet creates a new medical record after consultation
    Given there is a COMPLETED appointment for "Luna"
    When I open Luna's patient file
    And I click "New Medical Record"
    And I fill in weight "3.2 kg", temperature "38.5°C", heart rate "140 bpm"
    And I enter diagnosis "Mild dehydration" and treatment "IV fluids, recheck in 3 days"
    And I click "Save Record"
    Then the new record appears at the top of the Medical Records tab
    And the appointment status remains "COMPLETED"

  Scenario: Assistant can view but not create medical records
    Given I am logged in as assistant "assistant@desertpaws.ae"
    When I open any patient's file
    Then I see the Medical Records tab with all records
    But I do not see the "New Medical Record" button

  Scenario: Receptionist cannot access medical records section at all
    Given I am logged in as receptionist "reception@desertpaws.ae"
    When I look at the sidebar navigation
    Then I do not see "Medical Records" in the menu
    And navigating directly to "/patients" redirects me to the appointments page

---

Feature: Billing
  As a receptionist or vet
  I want to create and manage invoices
  So that the clinic gets paid for its services

  Background:
    Given I am logged in as receptionist "reception@desertpaws.ae" of clinic "clinic-001"

  Scenario: Create an invoice for a completed appointment
    Given there is a COMPLETED appointment for "Max" (owner: Ahmed)
    When I navigate to Billing and click "New Invoice"
    And I select patient "Max" and link to that appointment
    And I add line item "Annual vaccination" at AED 150.00
    And I add line item "Consultation fee" at AED 100.00
    Then I see:
      | Subtotal | AED 250.00 |
      | VAT (5%) | AED 12.50  |
      | Total    | AED 262.50 |
    When I click "Save as Draft"
    Then the invoice appears in the list with status "DRAFT"

  Scenario: Send invoice to client
    Given there is a DRAFT invoice for "Max"
    When I open it and click "Send"
    Then the status changes to "SENT"
    And the send date is recorded

  Scenario: Mark invoice as paid
    Given there is a SENT invoice for "Max" totalling AED 262.50
    When I click "Mark as Paid"
    Then the status becomes "PAID"
    And the paid date is shown on the invoice

  Scenario: VAT is always displayed in AED with 5% rate
    Given any invoice with line items
    Then I always see three lines: Subtotal, VAT (5%), and Total in AED
    And amounts use the format "AED X,XXX.XX"
    And the currency is never shown as $ or €

  Scenario: Cancel a draft invoice
    Given there is a DRAFT invoice
    When I click "Delete"
    And I confirm
    Then the invoice is removed from the list

  Scenario: Cannot edit a sent or paid invoice
    Given there is a SENT invoice
    When I open it
    Then I do not see an "Edit" button
    And the line items are read-only
