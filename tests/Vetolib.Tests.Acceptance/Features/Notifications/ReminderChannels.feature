@wip
Feature: Appointment reminder channel preferences
  As a clinic administrator
  I want to configure which channels are used for appointment reminders
  In order to reach pet owners through their preferred communication method

  Background:
    Given a clinic "Happy Paws" with identifier "clinic-happy-paws"
    And I am authenticated as ADMIN

  # --- Channel Configuration ---

  Scenario: Configure email-only reminders
    When I set the reminder channel to "email" only
    Then the clinic reminder settings show "email" as the active channel
    And "WhatsApp" is not an active channel

  Scenario: Configure WhatsApp-only reminders
    When I set the reminder channel to "WhatsApp" only
    Then the clinic reminder settings show "WhatsApp" as the active channel
    And "email" is not an active channel

  Scenario: Configure both email and WhatsApp reminders
    When I set the reminder channels to "email" and "WhatsApp"
    Then the clinic reminder settings show both "email" and "WhatsApp" as active channels

  # --- Default Behavior ---

  Scenario: Default reminder channel is email for a new clinic
    Given a newly created clinic "Desert Vet"
    When I view the reminder channel settings for "Desert Vet"
    Then the default active channel is "email"
