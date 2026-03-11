# todo-test-agenda-handlers-001 — Unit tests for Agenda command handlers

**Module** : Agenda
**Priorité** : HAUTE
**Skills à lire** : `skills/ardalis-result/SKILL.md`, `skills/cqrs-mediatr/SKILL.md`

---

## Contexte

Le module Agenda contient 3 command handlers critiques sans aucun test unitaire :

- `CreateAppointmentHandler`
- `EditAppointmentHandler`
- `UpdateAppointmentStatusHandler`

Ces handlers contiennent de la logique métier (détection de conflits, transitions de statut) qui doit être couverte indépendamment des tests BDD d'acceptance.

## Travail à faire

Créer `tests/Vetolib.Tests.Unit/Agenda/CreateAppointmentHandlerTests.cs` couvrant :
- Happy path : rendez-vous créé, `Result.Success` retourné
- Conflit de plage horaire : `Result.Invalid` avec erreur descriptive
- PatientId inexistant : `Result.NotFound`

Créer `tests/Vetolib.Tests.Unit/Agenda/UpdateAppointmentStatusHandlerTests.cs` couvrant :
- Transition valide (Scheduled → Confirmed)
- Transition invalide (Completed → Scheduled) : `Result.Invalid`
- Appointment introuvable : `Result.NotFound`

Créer `tests/Vetolib.Tests.Unit/Agenda/EditAppointmentHandlerTests.cs` couvrant :
- Happy path : champs modifiés en base
- Appointment introuvable : `Result.NotFound`
- Conflit après édition : `Result.Invalid`

## Contraintes techniques

- Utiliser InMemory EF Core avec ClinicId fixe `11111111-1111-1111-1111-111111111111` (cf. mémoire projet — EF bake Expression.Constant)
- NSubstitute pour toutes les dépendances externes (ISender, IPublisher)
- Zéro Testcontainers dans les tests unitaires
- Pattern : copier `tests/Vetolib.Tests.Unit/MedicalRecords/AddPrescriptionHandlerTests.cs` comme modèle

## Critères de complétion

```
□ CreateAppointmentHandlerTests.cs créé avec minimum 3 scénarios
□ UpdateAppointmentStatusHandlerTests.cs créé avec minimum 3 scénarios
□ EditAppointmentHandlerTests.cs créé avec minimum 2 scénarios
□ dotnet test tests/Vetolib.Tests.Unit/ → 0 erreur, 0 échec
```
