@wip
Feature: Vaccination reminders for pet owners
  As a pet owner
  I want to receive reminders before my animal's vaccinations are due
  So that I never miss a booster and keep my pet protected

  Background:
    Given a clinic "Happy Paws" in Dubai
    And I am a registered owner "Fatima Hassan" with phone "+971501234567" and email "fatima@example.com"
    And my animal "Luna" is registered at "Happy Paws"
    And "Luna" has a Rabies vaccination expiring on 2026-05-15

  # --- Reminder delivery --- #

  Scenario: Owner receives a vaccination reminder 30 days before due date
    When the date reaches 2026-04-15
    Then I receive a reminder that Luna's Rabies vaccination is due in 30 days
    And the reminder includes the vaccination name and due date

  Scenario: Reminder is sent via WhatsApp
    Given my preferred contact method is WhatsApp
    When the date reaches 2026-04-15
    Then I receive a WhatsApp message about Luna's upcoming Rabies vaccination
    And the message includes a link to book an appointment

  Scenario: Reminder is sent via email
    Given my preferred contact method is email
    When the date reaches 2026-04-15
    Then I receive an email about Luna's upcoming Rabies vaccination at "fatima@example.com"
    And the email includes a link to book an appointment

  Scenario: Owner with both WhatsApp and email receives reminder on both channels
    Given my preferred contact methods are WhatsApp and email
    When the date reaches 2026-04-15
    Then I receive both a WhatsApp message and an email about the vaccination

  # --- Opt-out --- #

  Scenario: Reminders are enabled by default
    When I check my notification preferences
    Then vaccination reminders are turned on

  Scenario: Owner disables vaccination reminders
    Given vaccination reminders are enabled
    When I turn off vaccination reminders in my settings
    Then vaccination reminders are disabled
    And when the date reaches 2026-04-15
    Then I do not receive any vaccination reminder

  Scenario: Owner re-enables vaccination reminders
    Given I have previously disabled vaccination reminders
    When I turn on vaccination reminders in my settings
    Then vaccination reminders are enabled again

  # --- Booking from reminder --- #

  Scenario: Owner books an appointment directly from the reminder
    Given I received a WhatsApp reminder about Luna's Rabies vaccination
    When I tap the booking link in the reminder
    Then I am taken to the appointment booking page
    And the clinic is pre-selected as "Happy Paws"
    And the animal is pre-selected as "Luna"
    And the reason is pre-filled with "Rabies vaccination"

  # --- Multiple animals --- #

  Scenario: Owner with multiple animals receives separate reminders
    Given I also have an animal "Rocky" registered at "Happy Paws"
    And "Rocky" has a Distemper vaccination expiring on 2026-05-20
    When the date reaches 2026-04-20
    Then I receive a reminder for Rocky's Distemper vaccination
    And this reminder is separate from Luna's Rabies reminder

  # --- Edge cases --- #

  Scenario: No reminder is sent if the vaccination has no expiry date
    Given "Luna" has a vaccination with no expiry date recorded
    Then no reminder is scheduled for that vaccination

  Scenario: Reminder is not sent if the animal is no longer linked to the owner
    Given "Luna" has been unlinked from my account
    When the date reaches 2026-04-15
    Then I do not receive a vaccination reminder for "Luna"
