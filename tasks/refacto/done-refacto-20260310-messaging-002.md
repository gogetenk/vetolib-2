# todo-refacto-20260310-messaging-002 -- Conversation missing IsSpam property
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Domain/Conversation.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Infrastructure/ConversationConfiguration.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging.Contracts/ConversationDto.cs`
**Violation** : Spec MESSAGING-SPEC.md section 2.7 requires a "Mark as spam" action. The `Conversation` entity has no `IsSpam` (bool) property, no `MarkAsSpam()` method returning `Result`, and no way to filter spam in the inbox or display spam in the admin folder.
**Correction attendue** :
1. Add `bool IsSpam` property to `Conversation` (default false)
2. Add `Result MarkAsSpam()` method
3. Add `Result RestoreFromSpam()` method
4. Add `IsSpam` to `ConversationDto`
5. Add EF configuration for the property
**Critere** : The Conversation entity has IsSpam with MarkAsSpam/RestoreFromSpam returning Result
