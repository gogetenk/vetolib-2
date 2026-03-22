@wip
Feature: AI Message Classification
  As a clinic staff member
  I want incoming owner messages to be automatically classified by urgency and category
  So that urgent messages are handled immediately and routine ones are routed efficiently

  Background:
    Given I am a staff member at clinic "Gulf Veterinary Center"

  # --- Automatic Classification ---

  Scenario: Incoming message is automatically classified with urgency and category
    When pet owner "Amina Al-Rashid" sends a message "My cat has not eaten for 2 days and seems lethargic"
    Then the message should be automatically classified
    And the urgency should be "High"
    And the category should be "Medical concern"

  Scenario: Routine message classified as low urgency
    When pet owner "John Smith" sends a message "Can I get a copy of my dog's vaccination certificate?"
    Then the message should be automatically classified
    And the urgency should be "Low"
    And the category should be "Administrative request"

  Scenario: Appointment-related message classified correctly
    When pet owner "Khalid Al-Mansoori" sends a message "I need to reschedule my appointment from Thursday to Saturday"
    Then the message should be automatically classified
    And the urgency should be "Normal"
    And the category should be "Appointment request"

  # --- Urgent Message Flagging ---

  Scenario: Urgent message is flagged immediately for veterinarian attention
    When pet owner "Sara Al-Blooshi" sends a message "My dog is bleeding heavily after a fall"
    Then the message should be classified with urgency "Critical"
    And the message should be flagged for immediate veterinarian attention
    And all on-duty veterinarians should receive a notification within 1 minute

  Scenario: Potentially life-threatening keywords trigger critical urgency
    When pet owner "David Chen" sends a message "I think my puppy swallowed rat poison"
    Then the message should be classified with urgency "Critical"
    And the message should be flagged for immediate veterinarian attention

  # --- Vet Override ---

  Scenario: Veterinarian overrides the AI classification
    Given a message from "Amina Al-Rashid" was classified as urgency "High" and category "Medical concern"
    When the veterinarian changes the classification to urgency "Normal" and category "Post-operative follow-up"
    Then the message should display the updated classification
    And the original AI classification should be preserved for training purposes

  Scenario: Overridden classification is used for routing going forward
    Given a message was classified as "Administrative request" by the AI
    When the veterinarian overrides the category to "Medical concern"
    Then the message should be re-routed to the veterinarian queue
    And the routing change should be logged

  # --- Classification Accuracy Feedback ---

  Scenario: Staff confirms AI classification as correct
    Given a message was classified as urgency "Normal" and category "Appointment request"
    When the staff member confirms the classification is correct
    Then the confirmation should be recorded as positive feedback for the AI model

  Scenario: Staff corrections contribute to classification improvement
    Given 20 messages have been classified this month
    And staff corrected 4 of them
    When I view the classification accuracy report
    Then the accuracy rate should show 80%
    And the most common correction categories should be listed

  # --- Edge Cases ---

  Scenario: Message in Arabic is classified correctly
    When pet owner "Mohammed Al-Ketbi" sends a message in Arabic about his cat being sick
    Then the message should be automatically classified regardless of language
    And the urgency and category should be appropriate to the content

  Scenario: Very short message receives a classification
    When pet owner "Lisa Park" sends a message "Help"
    Then the message should be classified with urgency "High"
    And the category should be "Unspecified"
    And the message should be flagged for staff review due to insufficient context

  Scenario: Message with image attachment is classified based on text
    Given pet owner "Reem Al-Shamsi" sends a message "Look at this rash on my dog's belly" with a photo
    When the message is classified
    Then the classification should be based on the text content
    And the urgency should be "Normal"
    And the category should be "Medical concern"
