# todo-refacto-20260310-messaging-007 -- Conversation missing UpdateCategory method for re-categorization
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Domain/Conversation.cs`
**Violation** : Spec MESSAGING-SPEC.md section 7 defines PATCH `/api/v1/messaging/conversations/{id}/category` for re-categorization. The Conversation entity has no `UpdateCategory(MessageCategory newCategory)` method. The `Category` property has a private setter but no domain method to change it.
**Correction attendue** :
Add `Result UpdateCategory(MessageCategory newCategory)` method that:
1. Updates the Category
2. Resets `IsTriageUncertain` to false (since a human has now verified)
3. Potentially updates routing (AssignedToRole based on category routing rules)
**Critere** : Conversation has a public UpdateCategory method returning Result
