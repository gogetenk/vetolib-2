# todo-notifications-impl-025.md -- Implement missing notification types

**Module** : Notifications
**Priority** : Haute
**Dependencies** : aucune

## Context
Notifications audit found 4 stub consumers and 5 missing notification types.

## Scope
1. Implement AppointmentCancellationConsumer (stub → real email)
2. Implement booking confirmation notification
3. Fix inconsistent from-address (noreply@vetolib.ae vs noreply@desertpaws.ae)
4. Wire PreferredLanguage field so emails are sent in the user's language
