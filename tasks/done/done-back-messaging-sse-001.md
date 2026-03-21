# todo-back-messaging-sse-001.md — SSE endpoint pour temps reel

**Module** : Messaging
**Dependances** : todo-back-messaging-staff-handlers-001
**Priorite** : MOYENNE
**Skills a lire** : `ardalis-result`, `aspnet-minimal-api`

---

## Objectif

Implementer un endpoint SSE (Server-Sent Events) pour les mises a jour temps reel de l'inbox staff (nouveaux messages, changements de status).

## Spec de reference

`docs/MESSAGING-SPEC.md` section 4.2 (Real-time updates), section 4.3 (Notifications), section 7 (Real-Time Endpoint)

## Implementation

### 1. SSE Endpoint

```
GET /api/v1/messaging/sse  → RequireAuthorization (staff)
```

- Content-Type: `text/event-stream`
- Keep-alive avec des commentaires SSE toutes les 30 secondes
- Deconnexion propre quand le client ferme la connexion

### 2. Types d'events SSE

- `new-message` : nouveau message dans une conversation visible par le role du staff
- `conversation-updated` : changement de status, transfert, recategorisation
- `unread-count` : mise a jour du compteur de messages non lus

### 3. Architecture interne

- Creer `IMessagingEventBroadcaster` (interface interne)
- Implementation avec `Channel<T>` (System.Threading.Channels) par connexion SSE
- Les handlers (SendReply, CreateOwnerConversation, etc.) publient via le broadcaster
- Le broadcaster filtre par clinicId et role avant d'envoyer

### 4. Event payload

```json
{
  "type": "new-message",
  "conversationId": "...",
  "category": "MedicalUrgency",
  "preview": "My dog ate chocolate..."
}
```

### 5. Unread count

- A la connexion SSE, envoyer immediatement le count actuel
- A chaque nouveau message, recalculer et envoyer le nouveau count

## Regles

- SSE est unidirectionnel (server -> client), pas de WebSocket
- Le filtrage par role est obligatoire : un receptionist ne recoit pas les events de conversations medicales
- Multi-tenant : seuls les events de la clinique courante
- Gestion propre de la deconnexion (CancellationToken)

## Critere

```
[] Endpoint SSE fonctionne avec Content-Type text/event-stream
[] Events new-message, conversation-updated, unread-count implementes
[] Filtrage par role et clinicId
[] Keep-alive toutes les 30 secondes
[] Deconnexion propre sans memory leak
[] dotnet build passe
[] Renommer en done
```
