# todo-back-messaging-gherkin-001.md — Features Gherkin + step definitions Reqnroll

**Module** : Messaging
**Dependances** : todo-back-messaging-staff-handlers-001, todo-back-messaging-owner-portal-001
**Priorite** : HAUTE
**Skills a lire** : `reqnroll-bindings`, `ardalis-result`

---

## Objectif

Creer les features Gherkin et step definitions Reqnroll pour valider les workflows messaging. Le fichier `MessageTriage.feature` existe deja, il faut le mettre a jour et ajouter les features manquantes.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 3 (User Workflows - tous les scenarios Gherkin)

## Implementation

### 1. Mettre a jour MessageTriage.feature

Le fichier existant utilise des categories incorrectes ("Emergency" au lieu de "MedicalUrgency", "AdminQuestion" au lieu de "Administrative"). Aligner avec les enums reels.

### 2. Creer les features manquantes

Creer dans `tests/Vetolib.Tests.Acceptance/Features/Messaging/` :

- **OwnerPortal.feature** : scenarios du portail owner (magic link, envoi message, consentement, limites, hors heures, export)
- **ReceptionistInbox.feature** : scenarios de l'inbox receptionist (filtrage role, reply AI, transfer, convert to appointment, spam, contexte patient)
- **VetInbox.feature** : scenarios de l'inbox vet (priority order, medical context, internal notes, AI suggestions, create urgent appointment, add to medical record, emergency notification, escalation)
- **AdminMessaging.feature** : scenarios admin (vue globale, reassignment, templates, heures messagerie, stats, outbound conversation, spam folder)
- **AssistantAccess.feature** : scenarios assistant (read-only, pas de conversations medicales)

### 3. Step Definitions

Creer dans `tests/Vetolib.Tests.Acceptance/StepDefinitions/Messaging/` :

- `OwnerPortalSteps.cs`
- `ReceptionistInboxSteps.cs`
- `VetInboxSteps.cs`
- `AdminMessagingSteps.cs`
- `AssistantAccessSteps.cs`
- `MessageTriageSteps.cs` (mettre a jour l'existant ou creer)

### 4. Helpers de test

- Helper pour creer un magic link token de test
- Helper pour creer des conversations avec messages (seed data)
- Helper pour simuler les heures de bureau (clock injecte)

## Regles

- Copier les scenarios Gherkin mot-a-mot depuis la spec
- Les step definitions appellent les endpoints HTTP via `HttpClient`
- Utiliser Testcontainers pour PostgreSQL
- Un scenario = une tranche verticale complete
- Les tests doivent etre RED tant que les handlers ne sont pas implementes

## Critere

```
[] MessageTriage.feature mis a jour (categories alignees)
[] OwnerPortal.feature cree (12 scenarios)
[] ReceptionistInbox.feature cree (7 scenarios)
[] VetInbox.feature cree (10 scenarios)
[] AdminMessaging.feature cree (6 scenarios)
[] AssistantAccess.feature cree (2 scenarios)
[] Step definitions creees pour chaque feature
[] dotnet build passe (tests RED attendus)
[] Renommer en done
```
