# todo-back-messaging-owner-portal-001.md — Handlers portail owner (magic link auth + conversations)

**Module** : Messaging
**Dependances** : todo-back-messaging-domain-001
**Priorite** : HAUTE
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`

---

## Objectif

Implementer les endpoints du portail owner avec authentification par magic link, gestion du consentement et operations sur les conversations.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 7 (Owner Portal Endpoints), section 2.10 (Owner Portal Authentication), section 2.11 (PDPL Compliance)

## Implementation

### 1. Magic Link Authentication

- Creer `MagicLinkAuthMiddleware` ou un `IEndpointFilter` qui :
  - Extrait le token du query string ou header `X-Portal-Token`
  - Valide le token contre `OwnerPortalToken` (non expire, clinicId match)
  - Injecte `OwnerId` et `ClinicId` dans le HttpContext
- Les endpoints portal ne sont PAS proteges par JWT mais par ce token
- Creer `IPortalContext` pour exposer `OwnerId`, `ClinicId`

### 2. Queries

1. **ListOwnerConversationsQuery** + Handler
   - Retourne les conversations de l'owner (filtrees par OwnerId du token)
   - Ne retourne JAMAIS les internal notes
   - Tri par date desc

2. **GetOwnerConversationByIdQuery** + Handler
   - Retourne la conversation avec ses messages (sans internal notes)
   - Verifie que la conversation appartient a l'owner

3. **ListOwnerPetsQuery** + Handler
   - Retourne les pets enregistres de l'owner
   - Lecture via `IPatientReader` de MedicalRecords.Contracts (ou query directe si l'interface n'existe pas encore -- creer une question)

4. **ExportOwnerConversationsQuery** + Handler
   - Genere un fichier texte avec toutes les conversations
   - Droit d'acces PDPL UAE

### 3. Commands

5. **CreateOwnerConversationCommand** + Handler + Validator
   - L'owner cree une nouvelle conversation
   - Selectionne un pet (sauf categorie "Other")
   - Body max 2000 chars
   - Verifie le consentement (ConsentAcceptedAt non null)
   - Verifie la limite de 5 messages/jour
   - Si hors heures de bureau : envoie auto-acknowledgment systeme
   - Publie un event pour le triage AI

6. **SendOwnerMessageCommand** + Handler + Validator
   - L'owner envoie un message dans une conversation existante
   - Conversation doit etre non-Closed
   - Verifie la limite de 5 messages/jour
   - Si la conversation etait Resolved, la reouvre
   - Body max 2000 chars

7. **AcceptConsentCommand** + Handler
   - Enregistre le consentement avec timestamp et version
   - Prerequis avant le premier message

### 4. Endpoints

Tous sous `/api/v1/portal` :

```
GET  /conversations           → ListOwnerConversationsQuery
GET  /conversations/{id}      → GetOwnerConversationByIdQuery
POST /conversations           → CreateOwnerConversationCommand
POST /conversations/{id}/messages → SendOwnerMessageCommand
POST /consent                 → AcceptConsentCommand
GET  /export                  → ExportOwnerConversationsQuery
GET  /pets                    → ListOwnerPetsQuery
```

### 5. Anti-spam

- Rate limit : 5 messages/jour/owner/clinique
- Compter les messages du jour dans le handler (pas de middleware global)

## Regles

- L'authentification portal est completement separee du JWT staff
- Les internal notes ne sont JAMAIS retournees aux endpoints portal
- Le consentement est obligatoire avant le premier message
- La limite de 5 messages/jour est par clinique

## Critere

```
[] Magic link auth (IEndpointFilter ou middleware)
[] IPortalContext implemente
[] 4 queries implementees
[] 3 commands implementees avec validators
[] 7 endpoints Minimal API mappes sous /api/v1/portal
[] Consentement PDPL fonctionne
[] Limite 5 messages/jour fonctionne
[] Auto-acknowledgment hors heures fonctionne
[] dotnet build passe
[] Renommer en done
```
