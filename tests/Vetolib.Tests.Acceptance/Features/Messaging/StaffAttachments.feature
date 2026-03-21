@wip
Feature: Staff file attachments in messaging
  As a veterinary staff member
  I want to attach files to my replies in conversations
  So that I can share photos, documents and results with pet owners

  Background:
    Given I am authenticated as a user with role "Vet"
    And there is a conversation with owner "Fatima Al Mansoori" about pet "Luna"

  Scenario: Vet attaches a photo to a reply
    When I reply to the conversation with the text "Here is the X-ray result" and a photo "xray-luna.jpg"
    Then the owner can see my reply with the photo in the conversation

  Scenario: Vet attaches multiple files to a reply
    When I reply with the text "Lab results attached" and 3 files:
      | Filename              | Type |
      | blood-test.pdf        | PDF  |
      | urine-analysis.pdf    | PDF  |
      | xray-abdomen.jpg      | JPEG |
    Then all 3 files are visible in the conversation thread
    And the owner can download each file

  Scenario: Receptionist attaches a document to a reply
    Given I am authenticated as a user with role "Receptionist"
    And there is a conversation with owner "Ahmed Al Rashid" about a billing question
    When I reply with the text "Please find your invoice attached" and a file "invoice-2026-001.pdf"
    Then the owner can see the reply with the attached document

  Scenario Outline: Unsupported file type is rejected
    When I try to attach a file "<filename>" to my reply
    Then the attachment is rejected with the message "File type not allowed"
    And the reply is not sent

    Examples:
      | filename          |
      | malware.exe       |
      | script.bat        |
      | archive.zip       |
      | macro-doc.docm    |

  Scenario: File exceeding the size limit is rejected
    When I try to attach a file larger than 10 MB
    Then the attachment is rejected with the message "File exceeds the maximum size of 10 MB"

  Scenario: Vet attaches an allowed file type
    When I reply with a file "vaccination-record.pdf"
    Then the file is accepted and visible in the conversation

  Scenario: Owner sees staff attachments in chronological order
    Given I have previously sent a reply with a photo "checkup-photo.jpg"
    And I send another reply with a document "lab-results.pdf"
    When the owner opens the conversation
    Then the attachments appear in the order they were sent
