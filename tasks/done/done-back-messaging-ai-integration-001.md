# todo-back-messaging-ai-integration-001.md — Integration AI pour triage et suggestions

**Module** : Messaging (consommateur de AI.Contracts)
**Dependances** : todo-back-messaging-staff-handlers-001, todo-back-messaging-owner-portal-001
**Priorite** : HAUTE
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`

---

## Objectif

Integrer le module AI pour le triage automatique des messages entrants et la generation de suggestions de reponse.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 2.2 (Triage), section 2.3 (AI Role), section 5.3 (Messaging --> AI)

## Implementation

### 1. Triage a la reception d'un message owner

Quand un owner envoie un message (CreateOwnerConversationCommand ou SendOwnerMessageCommand) :
- Appeler `IMessageTriageService.TriageAsync(messageContent, context)` depuis `Vetolib.AI.Contracts`
- Le resultat contient : `Category`, `Confidence`, `SuggestedReplies[]`
- Mettre a jour `Conversation.Category`, `AiTriageConfidence`, `IsTriageUncertain`
- Si confidence < 0.7 : `IsTriageUncertain = true`, router vers receptionist
- **Emergency bias** : si le triage hesite entre MedicalQuestion et MedicalUrgency, choisir MedicalUrgency

### 2. Routage base sur le triage

Creer `IMessageRouter` (interne) :
- `MedicalUrgency` -> tous les vets
- `PostOperativeFollowUp` -> vet referent (ou premier vet disponible)
- `MedicalQuestion` -> vet disponible (round-robin ou assigne)
- `AppointmentRequest` -> receptionist
- `Administrative` -> receptionist
- `Feedback` -> admin
- `Other` -> receptionist

Le routage met a jour `AssignedToRole` (et `AssignedToUserId` si un staff specifique).

### 3. Suggestions de reponse

- Quand un staff ouvre une conversation (GetConversationByIdQuery), generer les suggestions si elles n'existent pas encore
- Appeler `IMessageTriageService` ou une methode dediee de AI.Contracts
- Retourner 1-3 suggestions dans la reponse du query
- Les suggestions sont **transientes** (pas persistees dans le Messaging module)

### 4. Resume de conversation

- Quand une conversation a > 5 messages et qu'un staff l'ouvre
- Appeler `IConversationSummaryService.SummarizeAsync(messages)` depuis AI.Contracts
- Resume factuel de 3-5 phrases, sans diagnostic medical

### 5. Fallback si AI non disponible

- Si le service AI n'est pas enregistre ou leve une exception :
  - Triage : router vers receptionist avec `IsTriageUncertain = true`
  - Suggestions : retourner une liste vide
  - Resume : retourner NotFound
- Ne JAMAIS bloquer le flux principal a cause d'une erreur AI

### 6. ReplyAudit

Quand un staff envoie une reponse (SendReplyCommand) :
- Creer un `ReplyAudit` avec `AiSuggestedReply`, `WasSuggestedReplyUsed`, `ActualReply`
- Permet de mesurer l'utilite des suggestions AI

## Regles

- L'AI ne send JAMAIS de message directement au owner
- Le disclaimer AI est un string constant, jamais genere dynamiquement
- Le Messaging module ne reference PAS le runtime du module AI, uniquement AI.Contracts
- Le triage est une aide a la decision, le staff peut toujours recategoriser

## Critere

```
[] Triage automatique a chaque message owner entrant
[] Routage base sur la categorie (IMessageRouter)
[] Suggestions de reponse transientes
[] Resume de conversation pour threads > 5 messages
[] Emergency bias implemente (doute = MedicalUrgency)
[] Seuil de confiance 0.7 pour triage uncertain
[] ReplyAudit enregistre pour chaque reponse staff
[] Fallback gracieux si AI indisponible
[] dotnet build passe
[] Renommer en done
```
