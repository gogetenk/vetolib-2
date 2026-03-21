# todo-back-messaging-staff-handlers-001.md — Handlers CQRS staff (conversations + messages)

**Module** : Messaging
**Dependances** : todo-back-messaging-domain-001
**Priorite** : HAUTE
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`

---

## Objectif

Implementer les handlers CQRS et endpoints Minimal API pour les operations staff sur les conversations et messages.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 7 (Staff Endpoints)

## Implementation

### Queries

1. **ListConversationsQuery** + Handler
   - Filtre par role (Receptionist voit AppointmentRequest/Administrative/Other, Vet voit MedicalUrgency/PostOperativeFollowUp/MedicalQuestion, Admin voit tout, Assistant voit non-medical read-only)
   - Filtre optionnel par status, category, date range
   - Tri par priorite (Critical > High > Normal > Low) puis par date
   - Pagination

2. **GetConversationByIdQuery** + Handler
   - Retourne la conversation avec tous ses messages
   - Verifie que le role a acces a cette categorie
   - Ne retourne PAS les internal notes si le role est Assistant

3. **GetConversationSummaryQuery** + Handler
   - Retourne le resume AI si > 5 messages
   - Appelle `IConversationSummaryService` via AI.Contracts (si disponible, sinon retourne NotFound)
   - Policy : VetOrAdmin

### Commands

4. **SendReplyCommand** + Handler + Validator
   - Ajoute un message (sender = Vet ou Staff selon le role)
   - Met a jour le status en InProgress si Open
   - Publie `OwnerMessageReplyEvent` via MassTransit
   - Policy : ClinicStaff (pas Assistant)

5. **AddInternalNoteCommand** + Handler + Validator
   - Ajoute un message avec `IsInternalNote = true`
   - Policy : VetOrAdmin

6. **ChangeConversationStatusCommand** + Handler + Validator
   - Actions : Resolve, Close, Reopen
   - Policy : ClinicStaff

7. **TransferConversationCommand** + Handler + Validator
   - Change `AssignedToUserId` et/ou `AssignedToRole`
   - Policy : ClinicStaff

8. **RecategorizeConversationCommand** + Handler + Validator
   - Change la categorie (override du triage AI)
   - Met a jour le routage
   - Policy : ClinicStaff

9. **MarkAsSpamCommand** + Handler
   - Cache la conversation de l'inbox (ajoute un flag IsSpam sur Conversation)
   - Policy : ClinicStaff

10. **CreateOutboundConversationCommand** + Handler + Validator
    - Admin cree une conversation proactive vers un owner
    - Publie un event pour notification email
    - Policy : AdminOnly

### Endpoints

Tous sous `/api/v1/messaging` :

```
GET  /conversations           → ListConversationsQuery
GET  /conversations/{id}      → GetConversationByIdQuery
POST /conversations/{id}/reply → SendReplyCommand
POST /conversations/{id}/notes → AddInternalNoteCommand
PATCH /conversations/{id}/status → ChangeConversationStatusCommand
PATCH /conversations/{id}/transfer → TransferConversationCommand
PATCH /conversations/{id}/category → RecategorizeConversationCommand
POST /conversations/{id}/spam → MarkAsSpamCommand
POST /conversations/outbound  → CreateOutboundConversationCommand
GET  /conversations/{id}/summary → GetConversationSummaryQuery
```

## Regles

- Tous les handlers retournent `Result<T>` ou `Result`
- Tous les endpoints utilisent `.ToMinimalApiResult()`
- Le filtrage RBAC est dans le handler, pas dans un middleware custom
- Le multi-tenant est automatique via le global query filter
- Ne PAS ajouter de `Where(c => c.ClinicId == ...)` manuellement

## Critere

```
[] 3 queries implementees avec handlers
[] 7 commands implementees avec handlers + validators
[] 10 endpoints Minimal API mappes
[] Filtrage par role fonctionne (Receptionist, Vet, Admin, Assistant)
[] Tous les handlers retournent Result<T>
[] dotnet build passe
[] Renommer en done
```
