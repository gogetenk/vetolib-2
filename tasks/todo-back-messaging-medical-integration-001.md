# todo-back-messaging-medical-integration-001.md — Integration MedicalRecords (attacher message au dossier)

**Module** : Messaging (consommateur de MedicalRecords.Contracts)
**Dependances** : todo-back-messaging-staff-handlers-001
**Priorite** : MOYENNE
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`

---

## Objectif

Permettre au vet d'attacher le contenu d'un message au dossier medical du patient, et afficher le contexte patient dans l'inbox.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 5.2 (Messaging --> MedicalRecords)

## Implementation

### 1. Attacher un message au dossier medical

**AddMessageToRecordCommand** + Handler :
- Recoit : `ConversationId`, `MessageId`
- Lit le message et ses attachments
- Publie `AddMessageToRecordCommand` via MedicalRecords.Contracts
- Le texte du message + URLs des photos sont ajoutes comme note dans le dossier medical
- Policy : VetOrAdmin

Endpoint :
```
POST /api/v1/messaging/conversations/{conversationId}/messages/{messageId}/add-to-record
```

### 2. Contexte patient dans la conversation

Quand un staff ouvre une conversation liee a un patient :
- Appeler `IPatientReader.GetPatientContextAsync(patientId)` via MedicalRecords.Contracts
- Retourner un objet `PatientContext` avec :
  - Nom du pet, espece, age
  - Date du dernier examen
  - Prescriptions en cours
  - Allergies connues
  - Historique de vaccination
- Le contexte varie selon le role :
  - Receptionist : nom, espece, dernier RDV, factures impayees (via Billing.Contracts)
  - Vet/Admin : contexte medical complet

### 3. Contrats necessaires dans MedicalRecords.Contracts

Verifier que ces types existent :
- `IPatientReader` (interface publique)
- `PatientContextDto` (record public)
- `AddMessageToRecordRequest` (record public)

## Regles

- Le Messaging module ne reference JAMAIS le runtime de MedicalRecords
- Le contexte patient est un read-only display, aucune ecriture depuis Messaging
- Le receptionist ne voit PAS les informations medicales (RBAC coherent)

## Critere

```
[] AddMessageToRecordCommand implemente
[] Endpoint POST .../add-to-record mappe
[] Contexte patient retourne avec GetConversationByIdQuery
[] Contexte filtre par role (receptionist vs vet)
[] dotnet build passe
[] Renommer en done
```
