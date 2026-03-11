@wip @ignore
Feature: Admin Messaging Management
  As a clinic admin
  I want to manage all messaging configuration and monitor triage quality
  So that the messaging system runs effectively

  Background:
    Given I am authenticated as a user with role "Admin"

  Scenario: Admin sees all conversations
    Given there are conversations across all categories
    When I open the Messages section
    Then I should see all conversations regardless of category
    And I should be able to filter by: status, category, assigned staff, date range

  Scenario: Admin reassigns a conversation
    Given a conversation is currently assigned to "Dr. Ahmad"
    When I click "Reassign" and select "Dr. Fatima"
    Then the conversation should appear in Dr. Fatima's inbox
    And Dr. Ahmad should no longer see it in his inbox

  Scenario: Admin configures quick response templates
    When I go to Messaging Settings > Templates
    And I create a new template with:
      | Field      | Value                                                    |
      | Name       | Vaccination reminder                                     |
      | English    | Your pet is due for vaccination. Please book an appointment. |
      | Arabic     | حيوانك الأليف بحاجة إلى التطعيم. يرجى حجز موعد.              |
    Then the template should be available to all staff when replying to messages

  Scenario: Admin configures messaging hours
    When I go to Messaging Settings > Business Hours
    And I set hours to Sunday-Thursday 08:00-20:00, Friday 08:00-12:00
    Then messages sent outside these hours should trigger the auto-acknowledgment
    And emergency messages should still notify the on-call vet at any hour

  Scenario: Admin views triage statistics dashboard
    When I go to Messaging Settings > Statistics
    Then I should see:
      | Metric                              |
      | Average first response time         |
      | Messages by category (pie chart)    |
      | AI triage accuracy (% re-categorized)|
      | Volume per day (trend)              |
      | Conversion rate: message to appointment |

  Scenario: Admin proactively messages an owner
    When I click "New outbound message"
    And I select owner "Mrs. Al-Rashid" and pet "Luna"
    And I type "Luna is due for her annual vaccination next month"
    And I click "Send"
    Then the owner should receive an email notification
    And a new conversation should be created

  Scenario: Admin views spam folder
    When I go to Messages > Spam
    Then I should see all messages marked as spam
    And I should be able to restore a message to the inbox
