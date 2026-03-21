# todo-front-messaging-realtime-001.md — Composants temps reel (SSE + push notifications)

**Module** : Frontend (Messaging)
**Dependances** : todo-front-messaging-inbox-001
**Priorite** : MOYENNE
**Skills a lire** : `shadcn-nextjs`
**[MSW: oui]**
**[Branchement ulterieur]** : wire-messaging-realtime

---

## Objectif

Implementer les composants temps reel : SSE pour les mises a jour de l'inbox et push notifications navigateur pour les urgences.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 4.2 (Real-time updates), section 4.3 (Notifications)

## Implementation

### 1. Hook SSE

Creer `src/hooks/use-messaging-sse.ts` :
- Connexion EventSource a `/api/v1/messaging/sse`
- Reconnexion automatique apres deconnexion (exponential backoff)
- Parse des events : `new-message`, `conversation-updated`, `unread-count`
- Expose un state React : `unreadCount`, `latestEvent`
- Cleanup propre a l'unmount

### 2. Badge temps reel dans le sidebar

Modifier le composant Sidebar existant :
- Utiliser le hook `useMessagingSse` pour mettre a jour le badge "Messages (N)"
- Animation subtile quand le count augmente (pulse)

### 3. Toast notifications

Quand un event `new-message` arrive :
- Afficher un toast (sonner) avec la preview du message
- Clic sur le toast navigue vers la conversation

### 4. Push notifications navigateur

Creer `src/hooks/use-push-notifications.ts` :
- Demander la permission Notification API au premier login
- Quand un event `new-message` de categorie `MedicalUrgency` arrive :
  - Afficher une notification navigateur (meme si l'onglet est inactif)
  - Titre : "URGENT - Emergency message"
  - Body : patient name + preview
  - Clic : ouvre la conversation

### 5. MSW mock pour SSE

- MSW ne supporte pas nativement SSE
- Creer un mock SSE simple avec un `ReadableStream` ou un mock EventSource
- Emettre des events mock toutes les 10 secondes pour le dev

### 6. Composants

- `MessagingSseProvider.tsx` : context provider pour le SSE
- Modifier `Sidebar.tsx` : badge dynamique
- `NewMessageToast.tsx` : toast notification

## Regles

- Le SSE est unidirectionnel (server -> client)
- La reconnexion est automatique et transparente
- Les push notifications ne sont envoyees que pour MedicalUrgency
- Le mock SSE est suffisant pour le dev frontend

## Critere

```
[] Hook useMessagingSse cree et fonctionnel
[] Badge unread count mis a jour en temps reel dans le sidebar
[] Toast notification pour nouveaux messages
[] Push notification navigateur pour urgences
[] Reconnexion SSE automatique
[] Mock SSE pour MSW
[] data-testid sur les elements interactifs
[] npm run dev fonctionne
[] Renommer en done
```
