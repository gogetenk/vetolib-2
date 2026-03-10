# todo-refacto-20260310-messaging-004 -- Conversation missing AssignedToUserId/Role in DTO
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging.Contracts/ConversationDto.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Domain/Conversation.cs` (ToDto method)
**Violation** : The `ConversationDto` is missing `AssignedToUserId`, `AssignedToRole`, `AiTriageConfidence`, and `IsTriageUncertain` fields. These are required for:
- Admin reassignment workflow (spec section 3, AdminMessaging feature)
- Triage uncertain flag display (spec section 2.2, ReceptionistInbox feature)
- Staff inbox filtering by assignment
**Correction attendue** :
1. Add `Guid? AssignedToUserId`, `string? AssignedToRole`, `decimal? AiTriageConfidence`, `bool IsTriageUncertain` to `ConversationDto`
2. Update `Conversation.ToDto()` to include these fields
**Critere** : ConversationDto contains all 4 missing fields
