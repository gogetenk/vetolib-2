# Task: Unit tests for Messaging uncovered handlers

**Module**: Messaging
**Type**: test
**Priority**: high (SonarCloud quality gate)

## Context
SonarCloud quality gate fails at 59.9% coverage on new code (needs 80%).
These Messaging handlers changed in PR #14 but have no unit tests.

## Files to cover
1. `Application/Queries/GetConversationById/GetConversationByIdHandler.cs` — test found/not-found
2. `Application/Queries/GetOwnerConversationById/GetOwnerConversationByIdHandler.cs` — test found/not-found
3. `Application/Queries/ListConversations/ListConversationsHandler.cs` — test list returns results

## Acceptance criteria
- [ ] Unit tests for each handler (happy path + not found)
- [ ] All tests GREEN locally before PR

## Skills
`ardalis-result`, `cqrs-mediatr`
