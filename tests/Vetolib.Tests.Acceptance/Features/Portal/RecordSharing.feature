Feature: Record Sharing
  As a pet owner
  I want to share my animal's medical records with another vet
  So that they can provide informed care during a visit

  Background:
    Given a clinic "Desert Paws" exists with animals registered
    And an owner "Fatima Al Rashid" has a portal account linked to "Desert Paws"
    And "Fatima Al Rashid" has a cat "Luna" at "Desert Paws"

  Scenario: Owner creates a share link for an animal
    When she creates a share link for "Luna" valid for 48 hours
    Then a share link is generated successfully
    And the link expires in 48 hours

  Scenario: Shared link gives read-only access to records
    Given she has created a share link for "Luna"
    When someone accesses the share link
    Then they can see "Luna"'s medical records
    And they cannot modify any records

  Scenario: Owner revokes a share link
    Given she has created a share link for "Luna"
    When she revokes the share link
    Then the link is no longer accessible

  Scenario: Expired share link is rejected
    Given she has created a share link for "Luna" that has expired
    When someone accesses the expired share link
    Then access is denied because the link has expired

  Scenario: Owner sees list of active share links
    Given she has created 2 share links for "Luna"
    When she views her active share links
    Then she should see 2 share links listed
