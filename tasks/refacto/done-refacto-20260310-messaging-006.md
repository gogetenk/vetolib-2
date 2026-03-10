# todo-refacto-20260310-messaging-006 -- MessageAttachment missing domain validation for max 3 per message and 5MB limit
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Domain/MessageAttachment.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Domain/Message.cs`
**Violation** : Spec MESSAGING-SPEC.md section 2.7 requires: "max 3 per message, max 5 MB each, JPG/PNG only." The `MessageAttachment.Create()` factory validates basic fields but does NOT enforce:
1. 5 MB max size (`fileSizeBytes <= 5 * 1024 * 1024`)
2. ContentType must be `image/jpeg` or `image/png`
Additionally, `Message` has no collection of attachments and no way to enforce the 3-attachment limit at the domain level.
**Correction attendue** :
1. Add size limit validation in `MessageAttachment.Create()`: return error if `fileSizeBytes > 5_242_880`
2. Add content type validation: return error if not `image/jpeg` or `image/png`
3. Add `List<MessageAttachment> _attachments` to `Message` entity
4. Add `Result<MessageAttachment> AddAttachment(...)` method on `Message` that enforces max 3 count
**Critere** : Unit tests confirm all 3 constraints are enforced at domain level
