Feature: Veterinary invoicing
  Background:
    Given a clinic "Happy Paws"
    And an animal "Max" in the clinic
    And I am authenticated as VET

  Scenario: Create a draft invoice
    When I create an invoice for "Max" with item "Consultation" at 200 AED
    Then the invoice is created with status "DRAFT"
    And the number matches format "INV-2026-001"
    And the 5% VAT is calculated automatically (10 AED)
    And the total is 210 AED

  Scenario: Add multiple items to an invoice
    Given a "DRAFT" invoice for "Max"
    When I add item "Vaccin" at 150 AED
    And I add item "Medication" at 80 AED
    Then the invoice contains 3 items
    And the subtotal is 430 AED
    And the total VAT is 21.5 AED
    And the total is 451.5 AED

  Scenario: Send an invoice
    Given a "DRAFT" invoice for "Max" with at least one item
    When I change the invoice status to "SENT"
    Then the status is "SENT"
    And the due date is set to 30 days

  Scenario: Mark an invoice as paid
    Given a "SENT" invoice for "Max"
    When I mark the invoice as "PAID"
    Then the status is "PAID"

  Scenario: Cannot modify a paid invoice
    Given a "PAID" invoice for "Max"
    When I attempt to add an item to the invoice
    Then the system rejects with code "INVOICE_IMMUTABLE"
    And the error message is "Une facture payée ne peut plus être modifiée"

  Scenario: Sequential numbering per clinic
    Given 3 existing invoices for "Happy Paws"
    When I create a new invoice
    Then the number is "INV-2026-004"

  Scenario: Download PDF of a sent invoice
    Given a "SENT" invoice for "Max" with at least one item
    When I download the PDF of this invoice
    Then the response has status 200
    And the Content-Type is "application/pdf"
    And the content is not empty

  Scenario: Cannot download PDF of a draft invoice
    Given a "DRAFT" invoice for "Max"
    When I download the PDF of this invoice
    Then the response has status 422

  Scenario: Non-existent PDF returns 404
    When I download the PDF of an invoice with a random non-existent ID
    Then the response has status 404
