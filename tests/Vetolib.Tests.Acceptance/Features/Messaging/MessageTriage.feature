Feature: Message Triage
  As the messaging system
  I want to automatically classify incoming owner messages
  So that they are routed to the right staff member with the right priority

  Scenario: Emergency message classified correctly
    When an owner sends a message "My dog ate chocolate 1 hour ago and is trembling"
    Then the message should be classified as "MedicalUrgency"
    And the confidence should be above 0.8
    And the message should be routed to all veterinarians
    And a push notification should be sent immediately

  Scenario: Appointment request classified correctly
    When an owner sends a message "I would like to book an appointment for next week"
    Then the message should be classified as "AppointmentRequest"
    And the message should be routed to the receptionist

  Scenario: Administrative question classified correctly
    When an owner sends a message "What are your opening hours on Friday?"
    Then the message should be classified as "Administrative"
    And the message should be routed to the receptionist

  Scenario: Post-operative follow-up classified correctly
    Given the owner's pet had surgery 5 days ago
    When the owner sends a message "The stitches look red and swollen"
    Then the message should be classified as "PostOperativeFollowUp"
    And the message should be routed to the referring veterinarian

  Scenario: Feedback classified correctly
    When an owner sends a message "Thank you for the excellent care for my cat"
    Then the message should be classified as "Feedback"
    And the message should be routed to the admin

  Scenario: Low confidence triggers uncertain triage
    When an owner sends an ambiguous message "I have a question about my cat"
    And the AI confidence is below 0.7
    Then the message should be routed to the receptionist
    And the message should be flagged as "Triage uncertain -- please verify category"

  Scenario: AI biases toward emergency for safety
    When an owner sends a message "My dog has not moved for a while"
    And the AI is uncertain between "MedicalQuestion" and "MedicalUrgency"
    Then the message should be classified as "MedicalUrgency"

  Scenario: AI suggests replies for incoming message
    When an owner sends a message "My cat has been sneezing for 3 days"
    Then the system should generate 1 to 3 suggested replies
    And each suggestion should be professional and empathetic
    And no suggestion should prescribe medication or diagnose
    And the suggestions should be in the same language as the original message

  Scenario: AI generates conversation summary
    Given a conversation has 7 messages
    When a staff member opens the conversation
    Then an AI summary should be displayed at the top
    And the summary should be 3 to 5 factual sentences
    And the summary should not contain medical diagnoses

  Scenario: Emergency escalation after 10 minutes
    Given an emergency message was received 10 minutes ago
    And no veterinarian has viewed the message
    Then an escalation notification should be sent to all veterinarians

  Scenario: Veterinarian validates suggested reply before sending
    Given an owner message has an AI-suggested reply
    When the veterinarian modifies and sends the reply
    Then the sent reply should be recorded as ActualReply
    And WasSuggestedReplyUsed should be false

  Scenario: Message linked to patient record
    Given the owner selects their pet "Luna" when sending a message
    Then the message should be linked to patient "Luna"
    And the veterinarian should see Luna's medical context alongside the message

  Scenario: Multi-tenant message isolation
    Given clinic A has a conversation with owner "Al-Rashid"
    And clinic B has a conversation with owner "Smith"
    When I am authenticated in clinic A
    Then I should only see clinic A's conversations

  Scenario: Arabic message detected and replied in Arabic
    When an owner sends a message in Arabic "قطتي لا تأكل منذ يومين"
    Then the AI should detect the language as Arabic
    And the suggested replies should be in Arabic
