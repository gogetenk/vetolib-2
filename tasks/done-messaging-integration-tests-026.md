# todo-messaging-integration-tests-026.md -- Add integration tests for Messaging endpoints

**Module** : Messaging (Tests)
**Priority** : Haute
**Dependencies** : aucune

## Context
Test coverage audit: Messaging has 33 endpoints and 0 integration tests. Largest untested module.

## Scope
Add at least 1 integration test per critical Messaging endpoint:
- POST /api/v1/messaging/conversations
- GET /api/v1/messaging/conversations
- PATCH /api/v1/messaging/conversations/{id}/status
- POST /api/v1/messaging/conversations/{id}/messages
- GET /api/v1/messaging/templates
