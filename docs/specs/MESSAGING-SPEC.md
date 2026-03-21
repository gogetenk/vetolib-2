# Messaging Module -- Complete Business Specification

**Author**: Product Owner Vetolib
**Date**: 2026-03-10
**Status**: Validated
**Module**: `Vetolib.Messaging` (separate from `Vetolib.AI`)
**Market**: UAE veterinary clinics, EN+AR, AED

---

## Table of Contents

1. [Personas and Use Cases](#1-personas-and-use-cases)
2. [Business Rules](#2-business-rules)
3. [User Workflows (Gherkin)](#3-user-workflows-gherkin)
4. [Frontend Requirements](#4-frontend-requirements)
5. [Cross-Module Integrations](#5-cross-module-integrations)
6. [Out of Scope (MVP)](#6-out-of-scope-mvp)
7. [API Contract Overview](#7-api-contract-overview)
8. [Data Model Summary](#8-data-model-summary)

---

## 1. Personas and Use Cases

### 1.1 Personas

| Persona | System Role | Access to Messaging |
|---------|-------------|---------------------|
| **Owner** (pet proprietor) | No Vetolib account -- uses magic link portal | Send messages, view own conversations, attach photos. Never sees triage/priority. |
| **Receptionist** | RECEPTIONIST | Inbox: appointment requests + administrative questions only. Can reply, transfer to vet, convert message to appointment. Cannot see medical messages. |
| **Assistant** | ASSISTANT | Read-only view on all conversations visible to their role (non-medical). Cannot reply or take action. |
| **Vet** | VET | Inbox: medical urgencies, post-op follow-ups, medical questions, transferred messages. Can reply, add internal notes, create urgent appointments, attach message to medical record. |
| **Admin** | ADMIN | Global view of all conversations. Can reassign, configure templates, configure messaging hours, view triage statistics. All actions available. |

### 1.2 Message Flows

**Owner to Clinic (inbound):**
1. Owner opens the clinic portal via magic link (received by email after a visit)
2. Owner selects a pet, picks a category, writes a message (max 2000 chars), optionally attaches up to 3 photos (JPG/PNG, 5 MB each)
3. AI triages the message (category + priority + confidence score)
4. Message lands in the appropriate staff inbox based on triage routing

**Clinic to Owner (outbound):**
1. Staff member sees message in inbox with AI-suggested replies (1-3 suggestions)
2. Staff member validates, modifies, or writes a free-text reply
3. Reply is sent to the owner
4. Owner receives an email notification with a link back to the portal

**AI role -- assist only, never autonomous:**
- AI classifies the message category and assigns priority
- AI generates 1-3 reply suggestions per inbound message
- AI generates a conversation summary when the thread exceeds 5 messages
- AI never sends a message directly to the owner (the only exception is the out-of-hours auto-acknowledgment, which is a system message, not AI-generated)

### 1.3 Channel

**MVP: in-app web portal only.** No SMS, no WhatsApp, no native app. The owner accesses a responsive web portal branded with the clinic's name. The architecture uses an abstract channel model so WhatsApp Business API can be added in V2 without restructuring.

---

## 2. Business Rules

### 2.1 Conversation Initiation

| Who | Can initiate? | Conditions |
|-----|---------------|------------|
| Owner | Yes | Must have a valid magic link token (90-day validity, renewable). Must select a registered pet, except for category "Other" (routed to receptionist). |
| Receptionist | No | Can only reply to existing conversations or transfer them. |
| Vet | No | Can only reply to existing conversations. |
| Admin | Yes | Can create a proactive conversation to an owner (e.g., vaccination reminder). Requires selecting the owner and patient. |

An owner without any registered pet can only use the "Other" category. The message goes to the receptionist.

### 2.2 Message Categories and Triage Routing

| Category (enum) | Priority | Routed To | SLA (business hours) | Notification |
|-----------------|----------|-----------|---------------------|--------------|
| `MedicalUrgency` | CRITICAL | All vets (escalation if no view in 10 min) | 15 min | Browser push -- immediate |
| `PostOperativeFollowUp` | HIGH | Referring vet for the patient's last surgery | 2 hours | Browser push |
| `MedicalQuestion` | NORMAL | Available vet (round-robin or assigned) | 8 hours | Badge in sidebar |
| `AppointmentRequest` | NORMAL | Receptionist | 4 hours | Badge in sidebar |
| `Administrative` | LOW | Receptionist | 24 hours | Badge in sidebar |
| `Feedback` | LOW | Admin | 48 hours | Badge in sidebar |
| `Other` | LOW | Receptionist | 24 hours | Badge in sidebar |

**Triage confidence threshold**: if AI confidence < 70%, the message is routed to the receptionist with the flag "Triage uncertain -- please verify category." The receptionist can confirm or re-categorize.

**Emergency bias**: for safety, the AI is instructed to classify as `MedicalUrgency` when in doubt. False positives are acceptable; false negatives are not.

### 2.3 AI Role Boundaries

| AI does | AI does NOT |
|---------|-------------|
| Classify message category + confidence | Send a reply to the owner without human validation |
| Suggest 1-3 replies (professional, empathetic) | Prescribe medication |
| Summarize conversations > 5 messages | Diagnose a condition |
| Detect message language (EN/AR) | Make routing decisions autonomously (routing is rule-based on the category) |

**AI disclaimer** (constant, not LLM-generated, displayed on every suggestion):
> "This response was pre-drafted by AI. It will be reviewed and validated by a veterinarian before sending."

The disclaimer is stored as a constant string in the codebase, never generated dynamically.

### 2.4 Escalation

1. An inbound message categorized as `MedicalUrgency` triggers an immediate browser push notification to all vets of the clinic.
2. If no vet has viewed the message within 10 minutes, an escalation is triggered: a second notification (push + email) is sent to all vets.
3. There is no further automatic escalation beyond step 2. The admin is responsible for manual intervention if needed.

### 2.5 Out-of-Hours Behavior

- Each clinic configures its messaging hours (e.g., Sunday--Thursday 08:00--20:00, Friday 08:00--12:00, Saturday closed). This follows UAE work week conventions (Sunday--Thursday default, configurable per clinic).
- **Non-urgent messages outside business hours**: the system sends an automatic acknowledgment to the owner: "Your message has been received. It will be processed when the clinic reopens." This is the only automated message sent without human validation -- it is a system acknowledgment, not a medical reply.
- **Emergency messages outside business hours**: notification sent to the on-call vet (if configured in the Agenda module). If no on-call vet is configured, the notification goes to all vets.
- UAE public holidays (Eid Al Fitr, Eid Al Adha, National Day, etc.) are treated as closed days unless the clinic overrides.
- SLA timers count only business hours.

### 2.6 Multi-Tenancy

- Every Conversation and Message is scoped by `ClinicId` via the standard global query filter.
- An owner who visits two clinics has two separate sets of conversations, accessed via two different magic link portals.
- No cross-clinic visibility, ever.

### 2.7 Message Limits and Anti-Spam

- An owner can send a maximum of 5 messages per day per clinic.
- Message body max: 2000 characters.
- Photo attachments: max 3 per message, max 5 MB each, JPG/PNG only.
- A "Mark as spam" action is available to staff. Spam messages are hidden from the inbox but remain accessible to the admin.

### 2.8 Internal Notes

- Any staff member (Vet, Admin) can add an internal note to a conversation.
- Internal notes are visible only to staff, never to the owner.
- Internal notes are visually distinct in the conversation thread (different background, "Internal note" label).
- RECEPTIONIST cannot add internal notes to medical conversations (they cannot see those conversations at all).

### 2.9 Conversation Lifecycle

```
Open  -->  InProgress  -->  Resolved  -->  Closed
  ^            |                |              |
  |            v                v              |
  +--- Reopen (from any state except Open) ----+
```

- **Open**: new conversation, no staff reply yet.
- **InProgress**: at least one staff reply has been sent.
- **Resolved**: staff marked the conversation as resolved. The owner can still send a new message, which reopens the conversation.
- **Closed**: conversation permanently closed by admin. No new messages can be added. The owner sees a notice that the conversation is closed.

### 2.10 Owner Portal Authentication

- **Magic link via email**, not JWT. Token validity: 90 days, renewable by the clinic.
- The magic link is sent automatically after a consultation (via Notifications module) or manually by the receptionist.
- The portal is branded with the clinic's name and logo.
- No account creation, no password. The owner clicks the link and is authenticated.
- If the token expires, the owner sees a message: "This link has expired. Please contact your clinic to receive a new one."

### 2.11 PDPL UAE Compliance

- **Explicit consent**: the owner must accept messaging terms before sending their first message. Consent is recorded with timestamp and terms version.
- **Right of access**: the owner can download all their conversations as a text file from the portal.
- **Right of deletion**: the owner can request deletion. The clinic has 30 days to process. Internal notes are NOT deleted (they belong to the medical record).
- **Encryption**: messages encrypted at rest (AES-256) and in transit (TLS 1.3).
- **Data residency**: data stays in UAE region (hosting constraint).

---

## 3. User Workflows (Gherkin)

### Feature: Owner Portal

```gherkin
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
```

### Feature: Receptionist Inbox

```gherkin
Feature: Receptionist Messaging Inbox
  As a receptionist
  I want to see and respond to appointment requests and administrative questions
  So that I can handle owner inquiries efficiently

  Background:
    Given I am authenticated as a user with role "Receptionist"

  Scenario: Receptionist sees only relevant messages
    Given there are messages categorized as "AppointmentRequest", "Administrative", and "MedicalQuestion"
    When I open the Messages inbox
    Then I should see messages categorized as "AppointmentRequest" and "Administrative"
    And I should NOT see messages categorized as "MedicalQuestion"
    And I should NOT see messages categorized as "MedicalUrgency"

  Scenario: Messages are sorted by priority then by date
    Given there are 3 messages: one "Administrative" from yesterday, one "AppointmentRequest" from today, one "Administrative" from today
    When I open the Messages inbox
    Then the "AppointmentRequest" message should appear first (higher priority)
    And the two "Administrative" messages should be sorted oldest first

  Scenario: Receptionist replies using AI suggestion
    Given I open a message from an owner asking about appointment availability
    And the AI has generated 2 suggested replies
    When I click on the first suggestion
    Then the reply field should be pre-filled with the suggestion text
    When I modify the text and click "Send"
    Then the reply should be sent to the owner
    And the message status should change to "InProgress"

  Scenario: Receptionist uses a quick response template
    Given the clinic has configured a template "Appointment confirmation"
    When I open a message and click "Templates"
    And I select the "Appointment confirmation" template
    Then the reply field should be pre-filled with the template text
    And I can modify it before sending

  Scenario: Receptionist transfers a medical message to vet
    Given I receive a message flagged as "Triage uncertain -- please verify category"
    And the message describes medical symptoms
    When I click "Transfer to veterinarian"
    Then the message should disappear from my inbox
    And it should appear in the vet inbox with a note "Transferred by [Receptionist Name]"

  Scenario: Receptionist converts a message to an appointment
    Given I open a message requesting an appointment for pet "Buddy"
    When I click "Convert to appointment"
    Then a new appointment form should open
    And the patient field should be pre-filled with "Buddy"
    And the owner field should be pre-filled
    And the reason should contain the message content

  Scenario: Receptionist marks a message as spam
    Given I open a message that is clearly spam
    When I click "Mark as spam"
    Then the message should disappear from my inbox
    And the admin should be able to view it in the spam folder

  Scenario: Receptionist sees patient context alongside message
    Given I open a message linked to patient "Buddy"
    Then I should see alongside the message: pet name, species, last appointment date, and outstanding invoices
    And I should NOT see medical records (consistent with receptionist RBAC)
```

### Feature: Vet Inbox

```gherkin
Feature: Veterinarian Messaging Inbox
  As a veterinarian
  I want to see and respond to medical messages with full patient context
  So that I can provide informed responses to pet owners

  Background:
    Given I am authenticated as a user with role "Vet"

  Scenario: Vet sees medical messages in priority order
    Given there are messages: one "MedicalUrgency", one "PostOperativeFollowUp", one "MedicalQuestion"
    When I open the Messages inbox
    Then "MedicalUrgency" should appear first (red background)
    Then "PostOperativeFollowUp" should appear second
    Then "MedicalQuestion" should appear third

  Scenario: Emergency messages always appear at the top
    Given there is a "MedicalQuestion" from 2 hours ago
    And there is a "MedicalUrgency" from 5 minutes ago
    When I open the Messages inbox
    Then the "MedicalUrgency" should be first regardless of the older message

  Scenario: Vet sees full medical context for a message
    Given I open a message linked to patient "Luna" (cat, 3 years old)
    Then I should see alongside the message:
      | Context                      |
      | Last examination date        |
      | Current prescriptions        |
      | Known allergies              |
      | Vaccination history          |

  Scenario: Vet sees AI conversation summary for long threads
    Given a conversation has more than 5 messages
    When I open the conversation
    Then I should see an AI-generated summary at the top
    And the summary should be collapsible
    And the summary should be factual (3-5 sentences, no medical interpretation)

  Scenario: Vet adds an internal note
    Given I open a conversation with an owner
    When I click "Add internal note"
    And I type "Suspect potential kidney issue based on symptoms described. Schedule blood work."
    And I click "Save note"
    Then the note should appear in the conversation thread
    And the note should be visually distinct (marked as "Internal note")
    And the owner should NOT see this note

  Scenario: Vet replies with a modified AI suggestion
    Given I open a message with 3 AI-suggested replies
    When I click the second suggestion
    And I modify the text to add specific medical advice
    And I click "Send"
    Then the reply should be sent to the owner
    And the system should record WasSuggestedReplyUsed as false (modified)
    And the system should record the ActualReply

  Scenario: Vet creates an urgent appointment from a message
    Given I open an emergency message about patient "Buddy"
    When I click "Create urgent appointment"
    Then a new appointment form should open with:
      | Field    | Pre-filled value             |
      | Patient  | Buddy                        |
      | Type     | Emergency                    |
      | Reason   | Extracted from message text   |
    And the appointment should be created in the next available slot

  Scenario: Vet attaches message content to medical record
    Given I open a message where the owner describes symptoms and attached a photo
    When I click "Add to medical record"
    Then the message text and photos should be added as a note in the patient's medical record
    And a confirmation should appear "Added to Luna's medical record"

  Scenario: Vet receives push notification for emergency
    Given an owner sends a message classified as "MedicalUrgency"
    Then I should receive a browser push notification immediately
    And the notification should show the patient name and a preview of the message

  Scenario: Emergency escalation after 10 minutes without viewing
    Given an emergency message was received 10 minutes ago
    And no veterinarian has viewed the message
    Then an escalation notification should be sent to all veterinarians of the clinic
    And the notification should include "URGENT -- unread emergency message"
```

### Feature: Admin Messaging Management

```gherkin
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
```

### Feature: AI Triage

```gherkin
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
```

### Feature: Assistant Access

```gherkin
Feature: Assistant Messaging Access
  As an assistant
  I want to view messaging conversations in read-only mode
  So that I can stay informed without modifying anything

  Background:
    Given I am authenticated as a user with role "Assistant"

  Scenario: Assistant can view non-medical conversations
    Given there are conversations categorized as "AppointmentRequest" and "Administrative"
    When I open the Messages inbox
    Then I should see these conversations
    And I should NOT see a reply button
    And I should NOT see action buttons (transfer, convert to appointment)

  Scenario: Assistant cannot view medical conversations
    Given there are conversations categorized as "MedicalUrgency" and "MedicalQuestion"
    When I open the Messages inbox
    Then I should NOT see these conversations
```

---

## 4. Frontend Requirements

### 4.1 Owner Portal (separate from main Vetolib app)

| Page | Route | Description |
|------|-------|-------------|
| Portal landing | `/portal/{clinicSlug}` | Clinic-branded page. Shows existing conversations + "New Message" button. |
| Consent screen | `/portal/{clinicSlug}/consent` | First-time only. Terms and conditions acceptance. |
| New message | `/portal/{clinicSlug}/new` | Pet selector, category selector, text area, photo upload, send button. |
| Conversation view | `/portal/{clinicSlug}/conversations/{id}` | Chronological message thread. Reply input at the bottom. No internal notes visible. |
| Download export | `/portal/{clinicSlug}/export` | Button to download all conversations as text. |

**Key behaviors:**
- RTL layout when Arabic is selected
- Responsive (mobile-first -- most owners will use their phone)
- Branded with clinic name/logo (fetched from clinic config)
- No navigation to the main Vetolib app
- Character counter on message input (shows remaining out of 2000)
- Photo upload with preview and remove button

### 4.2 Staff Messaging (within Vetolib app)

| Component | Location | Description |
|-----------|----------|-------------|
| Sidebar badge | Sidebar | "Messages" menu item with unread count badge, updated in real time. |
| Inbox page | `/[locale]/messages` | List of conversations filtered by role-based routing. Filters: status, category, date. Search by full text. |
| Conversation detail | `/[locale]/messages/{id}` | Right panel or full page. Shows: message thread, AI suggestions panel, patient context panel, action buttons. |
| Templates management | `/[locale]/settings/messaging/templates` | Admin only. CRUD for quick response templates (EN + AR). |
| Messaging hours config | `/[locale]/settings/messaging/hours` | Admin only. Business hours configuration per day of week. |
| Triage statistics | `/[locale]/settings/messaging/stats` | Admin only. Dashboard with metrics. |

**Key behaviors:**
- **Real-time updates**: Server-Sent Events (SSE) for inbox badge count and new message notifications. SSE chosen over WebSocket because the data flow is unidirectional (server to client) and SSE is simpler to implement with ASP.NET Core Minimal APIs. The client already uses `fetch` for all other API calls.
- **Push notifications**: Browser Notification API for emergency messages. The user must grant permission on first login.
- **AI suggestions panel**: displayed alongside the message, 1-3 clickable suggestions. Clicking pre-fills the reply textarea. A "Loading suggestions..." indicator is shown while the AI generates.
- **Patient context panel**: displayed alongside the message. Content varies by role (receptionist sees basic info; vet sees full medical context).
- **Conversation summary**: collapsible card at the top of conversations with > 5 messages.
- `data-testid` on all interactive elements (required for Playwright tests).

### 4.3 Notifications

| Recipient | Channel | Trigger |
|-----------|---------|---------|
| Owner | Email | Staff replies to their message |
| Owner | In-portal | System acknowledgment for out-of-hours messages |
| Receptionist | Badge (SSE) | New message in their routing categories |
| Vet | Badge (SSE) | New message in their routing categories |
| Vet | Browser push | Emergency message received |
| Vet (all) | Browser push + email | Emergency escalation (10 min no view) |
| Admin | Badge (SSE) | New feedback/complaint message |

---

## 5. Cross-Module Integrations

### 5.1 Messaging --> Agenda

| Action | Trigger | Integration |
|--------|---------|-------------|
| Convert message to appointment | Receptionist/Vet clicks "Convert to appointment" | Publishes `CreateAppointmentFromMessageCommand` with pre-filled patient, owner, reason. Uses `Vetolib.Agenda.Contracts` types. |
| On-call vet lookup | Emergency message outside business hours | Reads on-call schedule via interface in `Vetolib.Agenda.Contracts` (e.g., `IOnCallVetReader.GetCurrentOnCallVetAsync(clinicId)`). |

### 5.2 Messaging --> MedicalRecords

| Action | Trigger | Integration |
|--------|---------|-------------|
| Add message to medical record | Vet clicks "Add to medical record" | Publishes `AddMessageToRecordCommand` via `Vetolib.MedicalRecords.Contracts`. Message text + photo URLs are added as a note. |
| Patient context display | Opening a conversation linked to a patient | Reads patient info via interface in `Vetolib.MedicalRecords.Contracts` (e.g., `IPatientReader.GetPatientContextAsync(patientId)`). |

### 5.3 Messaging --> AI

| Action | Trigger | Integration |
|--------|---------|-------------|
| Triage message | Every inbound owner message | Calls `IMessageTriageService.TriageAsync(messageContent, context)` from `Vetolib.AI.Contracts`. Returns category, confidence, suggested replies. |
| Summarize conversation | Conversation opened with > 5 messages | Calls `IConversationSummaryService.SummarizeAsync(messages)` from `Vetolib.AI.Contracts`. Returns summary text. |

### 5.4 Messaging --> Notifications

| Action | Trigger | Integration |
|--------|---------|-------------|
| Notify owner of reply | Staff sends a reply | Publishes `OwnerMessageReplyEvent` via MassTransit. Notifications module sends email. |
| Emergency alert | Message classified as `MedicalUrgency` | Publishes `EmergencyMessageReceivedEvent` via MassTransit. Notifications module sends push + email. |
| Escalation alert | Emergency unread for 10 min | Publishes `EmergencyEscalationEvent` via MassTransit. Notifications module sends push + email to all vets. |
| Magic link email | Admin/receptionist sends a portal link | Publishes `SendMagicLinkEvent` via MassTransit. Notifications module sends the email with the portal URL. |

### 5.5 Messaging --> Auth

| Action | Trigger | Integration |
|--------|---------|-------------|
| Owner authentication | Owner clicks magic link | The magic link token is validated by the Messaging module itself (not the Auth module). The token is a signed, time-limited token specific to the owner portal. This is separate from the JWT-based auth used by staff. |
| Role-based inbox filtering | Staff opens inbox | Uses the standard `IUserContext` to get the current user's role and filters conversations accordingly. |

### 5.6 Messaging --> Billing

| Action | Trigger | Integration |
|--------|---------|-------------|
| Invoice context display | Receptionist opens a message categorized as "Administrative" | Reads outstanding invoices via interface in `Vetolib.Billing.Contracts` (e.g., `IInvoiceReader.GetOutstandingForOwnerAsync(ownerId)`). Display only, no write action. |

---

## 6. Out of Scope (MVP)

| Feature | Reason | Target |
|---------|--------|--------|
| WhatsApp Business API | Meta certification cost + API integration complexity | V2 (6 months post-MVP) |
| SMS channel | Email is sufficient for MVP; SMS adds cost per message | V2 |
| Native mobile app for owners | Responsive web portal is sufficient | V3 |
| AI image analysis (photos) | Medical liability risk; text triage only at MVP | V3+ after clinical validation |
| Autonomous chatbot (AI replies without human) | Medical liability; human-in-the-loop is mandatory | Never for medical; V3 for administrative only |
| In-message payment | Payment gateway complexity + UAE regulations | V2 |
| Automatic message translation | Vet must read in original language at MVP | V2 |
| Multi-clinic owner portal | One portal per clinic at MVP. Owner with 2 clinics uses 2 separate portals | V2 |
| Video calls / telemedicine | UAE veterinary telemedicine regulations unclear | V3+ |
| NPS survey at end of conversation | Nice to have, not critical for MVP | V2 |
| Conversation retention/archival policy configuration | All conversations kept indefinitely at MVP | V2 |
| On-call vet configuration in Agenda | If not yet implemented in Agenda, emergency notifications go to all vets as fallback | V1.1 (fast follow) |
| Full-text search across messages | Basic list filtering is sufficient for MVP | V2 |

---

## 7. API Contract Overview

All endpoints under `/api/v1/messaging`. Multi-tenant via `ClinicId` from JWT (staff) or magic link token (owner).

### Staff Endpoints (JWT auth)

| Method | Path | Policy | Description |
|--------|------|--------|-------------|
| GET | `/api/v1/messaging/conversations` | RequireAuthorization | List conversations (filtered by role routing) |
| GET | `/api/v1/messaging/conversations/{id}` | RequireAuthorization | Get conversation with messages |
| POST | `/api/v1/messaging/conversations/{id}/reply` | ClinicStaff | Send reply to owner |
| POST | `/api/v1/messaging/conversations/{id}/notes` | VetOrAdmin | Add internal note |
| PATCH | `/api/v1/messaging/conversations/{id}/status` | ClinicStaff | Change status (resolve, close, reopen) |
| PATCH | `/api/v1/messaging/conversations/{id}/transfer` | ClinicStaff | Transfer to another role/user |
| PATCH | `/api/v1/messaging/conversations/{id}/category` | ClinicStaff | Re-categorize (overrides AI triage) |
| POST | `/api/v1/messaging/conversations/{id}/spam` | ClinicStaff | Mark as spam |
| POST | `/api/v1/messaging/conversations/outbound` | AdminOnly | Create proactive conversation to owner |
| GET | `/api/v1/messaging/conversations/{id}/summary` | VetOrAdmin | Get AI conversation summary |
| GET | `/api/v1/messaging/stats` | AdminOnly | Triage statistics dashboard data |
| GET | `/api/v1/messaging/templates` | ClinicStaff | List quick response templates |
| POST | `/api/v1/messaging/templates` | AdminOnly | Create template |
| PUT | `/api/v1/messaging/templates/{id}` | AdminOnly | Update template |
| DELETE | `/api/v1/messaging/templates/{id}` | AdminOnly | Delete template |

### Owner Portal Endpoints (magic link auth)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/v1/portal/conversations` | Magic link token | List owner's conversations |
| GET | `/api/v1/portal/conversations/{id}` | Magic link token | Get conversation (no internal notes) |
| POST | `/api/v1/portal/conversations` | Magic link token | Create new conversation |
| POST | `/api/v1/portal/conversations/{id}/messages` | Magic link token | Send a message in existing conversation |
| POST | `/api/v1/portal/consent` | Magic link token | Record consent acceptance |
| GET | `/api/v1/portal/export` | Magic link token | Download all conversations as text file |
| GET | `/api/v1/portal/pets` | Magic link token | List owner's registered pets |

### Real-Time Endpoint

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/v1/messaging/sse` | RequireAuthorization (staff) | SSE stream for real-time inbox updates |

---

## 8. Data Model Summary

### Entities (in `Vetolib.Messaging`)

**Conversation** (aggregate root, multi-tenant)
- `Id`, `ClinicId`, `OwnerId`, `PatientId?`, `Subject`, `Category` (MessageCategory enum), `Status` (ConversationStatus enum), `AssignedToUserId?`, `AssignedToRole?`, `AiTriageConfidence?`, `IsTriageUncertain` (bool), `LastMessageAt`, `CreatedAt`

**Message**
- `Id`, `ConversationId`, `Sender` (MessageSender enum), `SenderUserId?`, `Body`, `IsInternalNote`, `SentAt`
- No AI suggestion fields on Message itself -- AI suggestions are transient (returned by the AI module, not persisted on the message entity). The actual reply tracking is handled via a separate `ReplyAudit` entity.

**ReplyAudit** (tracks AI suggestion usage)
- `Id`, `MessageId` (the reply message), `OriginalOwnerMessageId`, `AiSuggestedReply?`, `WasSuggestedReplyUsed`, `ActualReply`

**MessageAttachment**
- `Id`, `MessageId`, `FileName`, `ContentType`, `FileSizeBytes`, `StoragePath`

**ResponseTemplate** (multi-tenant)
- `Id`, `ClinicId`, `Name`, `ContentEn`, `ContentAr`, `Category?` (optional -- template can be general or category-specific), `CreatedAt`, `UpdatedAt`

**OwnerPortalToken** (multi-tenant)
- `Id`, `ClinicId`, `OwnerId`, `Token` (unique, signed), `ExpiresAt`, `ConsentAcceptedAt?`, `ConsentVersion?`, `CreatedAt`

**MessagingHours** (multi-tenant)
- `Id`, `ClinicId`, `DayOfWeek` (0-6), `OpenTime`, `CloseTime`, `IsClosed` (bool)

### Enums (in `Vetolib.Messaging.Contracts`)

Already scaffolded:
- `MessageCategory`: MedicalUrgency, PostOperativeFollowUp, MedicalQuestion, AppointmentRequest, Administrative, Feedback, Other
- `ConversationStatus`: Open, InProgress, Resolved, Closed
- `MessageSender`: Owner, AI, Vet

To add:
- `MessageSender.Staff` -- for receptionist/admin replies (rename `Vet` to `Staff` since receptionists also reply). Decision: keep `Vet` as-is but add `Staff` value. When a receptionist replies, the sender is `Staff`. When a vet replies, the sender is `Vet`. This distinction allows filtering.
- `ConversationPriority`: Critical, High, Normal, Low (derived from category, not stored -- computed at query time)

### Updated `MessageSender` enum

```csharp
public enum MessageSender
{
    Owner,      // Pet owner via portal
    Vet,        // Veterinarian reply
    Staff,      // Receptionist or Admin reply
    System      // Auto-acknowledgment (out-of-hours)
}
```

The `AI` value is removed from `MessageSender`. AI does not send messages -- it generates suggestions that staff members validate. The AI triage result is stored in the `Vetolib.AI` module's `MessageTriage` entity, not in the Messaging module.

---

## Appendix A: RBAC Matrix for Messaging

| Action | Admin | Vet | Receptionist | Assistant |
|--------|-------|-----|--------------|-----------|
| View all conversations | Yes | No (filtered) | No (filtered) | No (filtered, read-only) |
| View medical conversations | Yes | Yes | No | No |
| View non-medical conversations | Yes | Yes | Yes | Yes (read-only) |
| Reply to conversation | Yes | Yes | Yes | No |
| Add internal note | Yes | Yes | No | No |
| Transfer conversation | Yes | Yes | Yes | No |
| Re-categorize | Yes | Yes | Yes | No |
| Convert to appointment | Yes | Yes | Yes | No |
| Add to medical record | Yes | Yes | No | No |
| Mark as spam | Yes | Yes | Yes | No |
| Create outbound conversation | Yes | No | No | No |
| Manage templates | Yes | No | No | No |
| Configure messaging hours | Yes | No | No | No |
| View triage statistics | Yes | No | No | No |
| Reassign conversation | Yes | No | No | No |

## Appendix B: Glossary

| Term | Definition |
|------|------------|
| Owner | Pet proprietor, client of the clinic. Accesses the portal via magic link. |
| Patient | The animal registered in Vetolib. |
| Conversation | A message thread on a given topic between an owner and the clinic. |
| Triage | Automatic classification of an inbound message by AI. |
| Routing | Assignment of a message to the appropriate role/staff based on triage category. |
| SLA | Target response time for a message category (counted in business hours). |
| Template | Pre-written response template, available in EN and AR, configurable by admin. |
| Internal note | Message visible only to clinic staff, never to the owner. |
| Escalation | Broadened notification when an emergency message is unread for 10 minutes. |
| Magic link | Time-limited, signed URL sent by email to authenticate an owner on the portal. |
| SSE | Server-Sent Events -- unidirectional real-time data stream from server to client. |
