# todo-refacto-20260309-back-004 — throw new dans les consumers Notifications (control flow infra)
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/AppointmentReminderConsumer.cs` (ligne 41)
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/InvoiceSentConsumer.cs` (ligne 40)
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/UserInvitedConsumer.cs` (ligne 40)
**Violation** : `throw new InvalidOperationException(...)` utilise pour declencher le retry MassTransit. Bien que ce soit dans une couche infrastructure (pas business flow), cela contourne le pattern Result<T>. Le throw est utilise comme mecanisme de signalisation vers MassTransit (qui ne comprend que les exceptions pour le retry).
**Correction attendue** : Documenter l'exception dans un commentaire standardise : "INFRA: MassTransit retry mechanism requires exception propagation — not business control flow". Alternativement, utiliser `ConsumeContext.Defer()` ou un fault consumer pour eviter le throw.
**Critere** : Chaque throw est soit remplace par un mecanisme MassTransit natif, soit annote avec un commentaire justificatif.
