# todo-refacto-20260310-messaging-005 -- Conversation missing Reopen on owner message to Resolved conversation
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Domain/Conversation.cs`
**Violation** : Spec MESSAGING-SPEC.md section 2.9 states: "Resolved: staff marked the conversation as resolved. The owner can still send a new message, which reopens the conversation." Currently `AddMessage()` only transitions Open -> InProgress when a staff member replies. It does NOT reopen a Resolved conversation when an owner sends a new message.
**Correction attendue** :
In `AddMessage()`, add logic: if `Status == ConversationStatus.Resolved && sender == MessageSender.Owner`, automatically call `Reopen()` (set Status back to Open).
**Critere** : Unit test confirms that adding an Owner message to a Resolved conversation reopens it
