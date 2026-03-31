@wip
Feature: QR code-based appointment check-in
  As a pet owner arriving at the clinic
  I want to check in by scanning a QR code
  In order to notify the clinic of my arrival without waiting at the desk

  Background:
    Given a clinic "Happy Paws" with hours 9am-6pm
    And a veterinarian "Dr. Ahmed Al-Rashid" with license "UAE-VET-12345"
    And an animal "Max" breed "Labrador" belonging to "John Smith"

  # --- QR Generation ---

  Scenario: Generate a QR code for a scheduled appointment
    Given a scheduled appointment for "Max" with "Dr. Ahmed" on "2026-04-10" at "10:00"
    When I request a QR code for this appointment
    Then a unique QR code is generated
    And the QR code is linked to the appointment

  # --- Check-In ---

  Scenario: Check in successfully with a valid QR code
    Given a scheduled appointment for "Max" with "Dr. Ahmed" on "2026-04-10" at "10:00"
    And a QR code has been generated for this appointment
    When the owner scans the QR code within the allowed time window
    Then the appointment status changes to "CheckedIn"
    And the clinic is notified that "Max" has arrived

  # --- Time Window ---

  Scenario: Check-in is rejected outside the allowed time window
    Given a scheduled appointment for "Max" with "Dr. Ahmed" on "2026-04-10" at "10:00"
    And a QR code has been generated for this appointment
    When the owner scans the QR code more than 2 hours before the appointment
    Then the system rejects with code "CHECKIN_WINDOW_CLOSED"
    And the message indicates the check-in window is not yet open

  # --- Security ---

  Scenario: Check-in is rejected when the QR code signature is tampered with
    Given a scheduled appointment for "Max" with "Dr. Ahmed" on "2026-04-10" at "10:00"
    When someone scans a QR code with a tampered signature
    Then the system rejects with code "INVALID_QR_SIGNATURE"
    And the message indicates the QR code is not valid
