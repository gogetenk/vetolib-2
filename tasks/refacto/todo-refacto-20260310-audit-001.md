# todo-refacto-20260310-audit-001 -- throw new dans Notifications Consumers
**Priorite** : critique
**Fichiers concernes** :
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/UserInvitedConsumer.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/SendMagicLinkConsumer.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/InvoiceSentConsumer.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/AppointmentReminderConsumer.cs`

**Violation** : Regle 1 CLAUDE.md -- "Zero exception pour le control flow business". Les consumers MassTransit utilisent `throw new InvalidOperationException(...)` au lieu de logger + retourner une erreur ou utiliser un retry policy MassTransit.
**Correction attendue** : Remplacer les `throw new InvalidOperationException` par un `_logger.LogError(...)` suivi d'un `throw` uniquement si c'est voulu pour que MassTransit retry (auquel cas documenter que c'est intentionnel pour le retry pipeline, pas pour le control flow). Alternative : retourner sans throw et enqueue dans une dead-letter queue.
**Critere** : [] Les consumers Notifications ne contiennent plus de `throw new InvalidOperationException` OU chaque throw est documente comme intentionnel pour le retry MassTransit
