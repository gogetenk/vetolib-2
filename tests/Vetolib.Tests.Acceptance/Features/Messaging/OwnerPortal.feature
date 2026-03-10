Feature: Owner Messaging Portal
  As a pet owner
  I want to send messages to my veterinary clinic
  So that I can get help without calling or visiting

  Background:
    Given I am an owner with a valid magic link for "Dubai Pet Care Clinic"
    And I have a registered pet "Luna" (cat, 3 years old)

  Scenario: Owner sends a new message about a health concern
    Given I open the clinic portal via my magic link
    When I click "New Message"
    And I select my pet "Luna"
    And I select the category "My pet has a health problem"
    And I type "Luna has been vomiting since yesterday and refuses to eat"
    And I click "Send"
    Then I should see a confirmation "Your message has been sent"
    And I should see an estimated response time based on the category

  Scenario: Owner sends a message with photo attachments
    Given I am composing a new message
    When I attach 2 photos (JPG, under 5 MB each)
    And I click "Send"
    Then the message should be sent with the 2 attachments
    And the attachments should be visible in the conversation thread

  Scenario: Owner cannot attach more than 3 photos
    Given I am composing a new message
    When I try to attach a 4th photo
    Then I should see an error "Maximum 3 photos per message"

  Scenario: Owner cannot send a message exceeding 2000 characters
    Given I am composing a new message
    When I type a message longer than 2000 characters
    Then I should see a character counter warning
    And the "Send" button should be disabled

  Scenario: Owner receives a reply notification
    Given I have sent a message to the clinic
    When a veterinarian replies to my message
    Then I should receive an email "Dubai Pet Care Clinic has replied to your message"
    And the email should contain a link back to the portal

  Scenario: Owner views conversation history
    Given I have an existing conversation about "Luna's vaccination"
    When I open the portal
    Then I should see the conversation in my list
    And I should see all messages in chronological order
    And I should NOT see any internal notes from the staff

  Scenario: Owner with expired magic link
    Given my magic link has expired
    When I try to access the portal
    Then I should see "This link has expired. Please contact your clinic to receive a new one."
    And I should NOT be able to send any message

  Scenario: Owner without registered pets uses "Other" category
    Given I am an owner with no registered pets
    When I click "New Message"
    Then only the "Other" category should be available
    And the message should be routed to the receptionist

  Scenario: Owner must accept consent before first message
    Given I have never used the messaging portal before
    When I click "New Message"
    Then I should see the messaging terms and conditions
    And I must accept them before I can compose a message

  Scenario: Owner downloads conversation history
    Given I have conversations with the clinic
    When I click "Download my messages"
    Then I should receive a text file containing all my conversations

  Scenario: Owner cannot send more than 5 messages per day
    Given I have already sent 5 messages today
    When I try to send a 6th message
    Then I should see "You have reached the daily message limit. Please try again tomorrow."

  Scenario: Owner sends message outside business hours
    Given the clinic's business hours are Sunday-Thursday 08:00-20:00
    And the current time is Friday 22:00 Asia/Dubai
    When I send a non-urgent message
    Then I should receive an automatic acknowledgment "Your message has been received. It will be processed when the clinic reopens."

  Scenario: Owner sends emergency message outside business hours
    Given the clinic's business hours are Sunday-Thursday 08:00-20:00
    And the current time is Friday 22:00 Asia/Dubai
    When I send a message "My dog is bleeding heavily and cannot stand"
    Then the on-call veterinarian should be notified immediately
    And I should NOT receive the "will be processed when the clinic reopens" message
