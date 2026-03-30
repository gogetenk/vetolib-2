# todo-fix-messaging-enums-020.md -- Fix missing Messaging enum values (Spam, Escalate)

**Module** : Messaging
**Priority** : Haute (breaks CI)
**Dependencies** : aucune

## Context
2 BDD tests fail because enum values are missing. Pre-existing issue, not caused by recent PRs.

## Scope

### 1. Add Spam to ConversationStatus enum
- File: `src/backend/Modules/Messaging/Vetolib.Messaging.Contracts/ConversationStatus.cs`
- Add `Spam` value
- Update `ListConversationsHandler` to filter by Spam status

### 2. Add Escalate to ConversationStatusAction enum
- File: `src/backend/Modules/Messaging/Vetolib.Messaging.Contracts/ConversationStatusAction.cs`
- Add `Escalate` value
- Update `ChangeConversationStatusHandler` to handle escalation

## Completion criteria
- [ ] Spam enum value added + handler filters by it
- [ ] Escalate enum value added + handler processes it
- [ ] `dotnet build` + `dotnet test` GREEN
