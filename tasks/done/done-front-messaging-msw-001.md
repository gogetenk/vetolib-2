# todo-front-messaging-msw-001.md — MSW handlers pour dev messaging

**Module** : Frontend (Messaging)
**Dependances** : aucune (premiere tache frontend messaging)
**Priorite** : HAUTE
**Skills a lire** : `msw-mock-api`, `shadcn-nextjs`
**[MSW: oui]**

---

## Objectif

Creer les handlers MSW qui simulent l'API messaging backend. Ces handlers permettent le dev frontend en autonomie totale.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 7 (API Contract Overview)

## Implementation

### 1. Donnees de test

Creer dans `src/mocks/data/messaging.ts` :
- 5-10 conversations avec des categories variees (MedicalUrgency, AppointmentRequest, Administrative, etc.)
- Messages associes avec des timestamps realistes
- Internal notes (visibles uniquement par staff)
- Templates de reponse (EN + AR)
- Heures de messagerie par defaut UAE

Noms realistes UAE : Al-Rashid, Al-Maktoum, etc. Devise AED. Timezone Asia/Dubai.

### 2. Handlers MSW staff

Creer dans `src/mocks/handlers/messaging.ts` :

```
GET  /api/v1/messaging/conversations        → liste filtree par role
GET  /api/v1/messaging/conversations/:id    → conversation avec messages
POST /api/v1/messaging/conversations/:id/reply → ajouter une reponse
POST /api/v1/messaging/conversations/:id/notes → ajouter note interne
PATCH /api/v1/messaging/conversations/:id/status → changer status
PATCH /api/v1/messaging/conversations/:id/transfer → transferer
PATCH /api/v1/messaging/conversations/:id/category → recategoriser
POST /api/v1/messaging/conversations/:id/spam → marquer spam
POST /api/v1/messaging/conversations/outbound → conversation proactive
GET  /api/v1/messaging/conversations/:id/summary → resume AI (mock)
GET  /api/v1/messaging/stats                → stats triage (mock)
GET  /api/v1/messaging/templates             → liste templates
POST /api/v1/messaging/templates             → creer template
PUT  /api/v1/messaging/templates/:id        → modifier template
DELETE /api/v1/messaging/templates/:id       → supprimer template
```

### 3. Handlers MSW portal owner

Creer dans `src/mocks/handlers/portal.ts` :

```
GET  /api/v1/portal/conversations           → liste owner
GET  /api/v1/portal/conversations/:id       → conversation sans notes internes
POST /api/v1/portal/conversations           → nouvelle conversation
POST /api/v1/portal/conversations/:id/messages → envoyer message
POST /api/v1/portal/consent                 → accepter consentement
GET  /api/v1/portal/export                  → export texte
GET  /api/v1/portal/pets                    → liste pets
```

### 4. API client functions

Creer dans `src/lib/api/messaging.ts` :
- Toutes les fonctions fetch correspondantes aux endpoints staff
- Types TypeScript pour les DTOs

Creer dans `src/lib/api/portal.ts` :
- Toutes les fonctions fetch correspondantes aux endpoints owner portal
- Types TypeScript pour les DTOs portal

### 5. Types TypeScript

Creer dans `src/lib/api/types.ts` ou `src/lib/api/messaging-types.ts` :
- `Conversation`, `Message`, `MessageCategory`, `ConversationStatus`, `MessageSender`
- `ResponseTemplate`, `TriageStats`, `MessagingHours`
- `PatientContext` (pour le panneau lateral)

## Regles

- Zero code conditionnel `if (process.env.NODE_ENV === 'development')` dans les composants
- Les handlers MSW simulent le filtrage par role
- Les donnees mockees sont coherentes entre elles
- L'API client utilise le meme `fetch` que le reste de l'app (via `lib/api/client.ts`)

## Critere

```
[] Donnees de test messaging creees (conversations, messages, templates)
[] 16 handlers MSW staff implementes
[] 7 handlers MSW portal implementes
[] API client functions dans lib/api/messaging.ts
[] API client functions dans lib/api/portal.ts
[] Types TypeScript complets
[] Handlers enregistres dans src/mocks/handlers/index.ts
[] npm run dev fonctionne sans erreur
[] Renommer en done
```
