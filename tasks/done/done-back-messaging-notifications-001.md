# todo-back-messaging-notifications-001.md — Integration Notifications (emails + escalation)

**Module** : Messaging (publieur) + Notifications (consommateur)
**Dependances** : todo-back-messaging-staff-handlers-001, todo-back-messaging-owner-portal-001
**Priorite** : HAUTE
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`

---

## Objectif

Implementer les events MassTransit pour notifier les owners et staff, et le mecanisme d'escalation d'urgence.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 2.4 (Escalation), section 2.5 (Out-of-Hours), section 4.3 (Notifications), section 5.4 (Messaging --> Notifications)

## Implementation

### 1. Integration Events (dans Messaging.Contracts)

Creer les events publics :

```csharp
public record OwnerMessageReplyEvent(Guid ConversationId, Guid OwnerId, Guid ClinicId, string ReplyPreview);
public record EmergencyMessageReceivedEvent(Guid ConversationId, Guid ClinicId, Guid? PatientId, string MessagePreview);
public record EmergencyEscalationEvent(Guid ConversationId, Guid ClinicId, string MessagePreview);
public record SendMagicLinkEvent(Guid OwnerId, Guid ClinicId, string OwnerEmail, string PortalUrl);
public record OutboundConversationCreatedEvent(Guid ConversationId, Guid OwnerId, Guid ClinicId, string MessagePreview);
```

### 2. Publication dans les handlers Messaging

- **SendReplyCommand** : publie `OwnerMessageReplyEvent`
- **CreateOwnerConversationCommand** (si MedicalUrgency) : publie `EmergencyMessageReceivedEvent`
- **CreateOutboundConversationCommand** : publie `OutboundConversationCreatedEvent`

### 3. Escalation d'urgence (10 minutes)

- Creer `EmergencyEscalationBackgroundService` (IHostedService)
- Toutes les minutes, scanner les conversations `MedicalUrgency` avec `Status = Open` et `CreatedAt < now - 10min` et pas encore vues par un vet
- Ajouter un champ `EscalationSentAt?` sur Conversation pour eviter les doublons
- Publier `EmergencyEscalationEvent` une seule fois par conversation
- Pas de deuxieme escalation automatique (spec section 2.4)

### 4. Consumers dans le module Notifications

Creer les consumers MassTransit :

- `OwnerMessageReplyConsumer` : envoie un email a l'owner avec un lien vers le portail
- `EmergencyMessageReceivedConsumer` : envoie une notification push + email a tous les vets de la clinique
- `EmergencyEscalationConsumer` : envoie push + email "URGENT -- unread emergency message" a tous les vets
- `SendMagicLinkConsumer` : envoie l'email avec le magic link
- `OutboundConversationCreatedConsumer` : envoie un email a l'owner

### 5. Out-of-Hours auto-acknowledgment

- Dans `CreateOwnerConversationCommand` : verifier les `MessagingHours` de la clinique
- Si hors heures et categorie != MedicalUrgency : ajouter un message systeme auto "Your message has been received. It will be processed when the clinic reopens."
- Ce message a `Sender = MessageSender.System`
- Si MedicalUrgency hors heures : publier `EmergencyMessageReceivedEvent` normalement (pas d'auto-acknowledgment)

## Regles

- L'auto-acknowledgment est le SEUL message envoye sans validation humaine
- Les events sont dans Messaging.Contracts (publics)
- Les consumers sont dans le module Notifications
- L'escalation ne se declenche qu'une seule fois par conversation

## Critere

```
[] 5 integration events crees dans Messaging.Contracts
[] Events publies dans les handlers correspondants
[] EmergencyEscalationBackgroundService fonctionne
[] 5 consumers crees dans le module Notifications
[] Out-of-hours auto-acknowledgment fonctionne
[] Escalation unique (pas de doublons)
[] dotnet build passe
[] Renommer en done
```
