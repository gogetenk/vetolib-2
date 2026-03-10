# todo-back-messaging-agenda-integration-001.md — Integration Agenda (conversion message -> RDV)

**Module** : Messaging (consommateur de Agenda.Contracts)
**Dependances** : todo-back-messaging-staff-handlers-001
**Priorite** : MOYENNE
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`

---

## Objectif

Permettre au staff de convertir un message en rendez-vous et consulter le vet de garde pour les urgences hors heures.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 5.1 (Messaging --> Agenda)

## Implementation

### 1. Conversion message -> rendez-vous

**ConvertToAppointmentCommand** + Handler + Validator :
- Recoit : `ConversationId`, `PreferredDate?`, `Notes?`
- Lit la conversation pour extraire PatientId, OwnerId, le contenu du dernier message owner
- Publie `CreateAppointmentFromMessageCommand` via MediatR (ou un integration event MassTransit)
- Le handler Agenda cree le RDV avec les champs pre-remplis
- Policy : ClinicStaff (Receptionist, Vet, Admin)

Endpoint :
```
POST /api/v1/messaging/conversations/{id}/convert-to-appointment → ConvertToAppointmentCommand
```

### 2. Vet de garde pour urgences hors heures

- Dans le handler d'urgence hors heures, appeler `IOnCallVetReader.GetCurrentOnCallVetAsync(clinicId)` via Agenda.Contracts
- Si l'interface n'existe pas encore dans Agenda.Contracts, creer une question dans `questions/`
- Fallback : si pas de vet de garde configure, notifier tous les vets

### 3. Contrats necessaires dans Agenda.Contracts

Verifier que ces types existent, sinon les creer :
- `CreateAppointmentFromMessageRequest` (record public)
- `IOnCallVetReader` (interface publique)

## Regles

- Le module Messaging ne reference JAMAIS le runtime de Agenda
- Communication via Contracts uniquement
- Si Agenda.Contracts ne fournit pas encore les interfaces necessaires, creer une question

## Critere

```
[] ConvertToAppointmentCommand implemente
[] Endpoint POST .../convert-to-appointment mappe
[] Integration avec IOnCallVetReader (ou fallback)
[] Les champs du RDV sont pre-remplis depuis la conversation
[] dotnet build passe
[] Renommer en done
```
