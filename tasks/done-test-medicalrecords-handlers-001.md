# todo-test-medicalrecords-handlers-001 — Unit tests for MedicalRecords command handlers

**Module** : MedicalRecords
**Priorité** : HAUTE
**Skills à lire** : `skills/ardalis-result/SKILL.md`, `skills/cqrs-mediatr/SKILL.md`

---

## Contexte

Le module MedicalRecords ne couvre que `AddPrescriptionHandlerTests.cs` et `PatientDomainTests.cs`. Les handlers suivants sont sans tests unitaires :

- `CreatePatientHandler`
- `UpdatePatientHandler`
- `CreateOwnerHandler`
- `AddMedicalRecordHandler`
- `ImportPatientsHandler` (logique CSV complexe)

Les validators `CreatePatientValidator`, `UpdatePatientValidator`, `CreateOwnerValidator`, `AddMedicalRecordValidator` sont également sans tests.

## Travail à faire

Créer `tests/Vetolib.Tests.Unit/MedicalRecords/CreatePatientHandlerTests.cs` couvrant :
- Happy path : patient créé avec OwnerPatient lié, `Result.Success`
- OwnerId inexistant : `Result.NotFound`
- Nom de patient vide (validé côté handler ou validator) : `Result.Invalid`

Créer `tests/Vetolib.Tests.Unit/MedicalRecords/UpdatePatientHandlerTests.cs` couvrant :
- Happy path : champs mis à jour
- Patient introuvable : `Result.NotFound`
- Poids négatif (si règle métier) : `Result.Invalid`

Créer `tests/Vetolib.Tests.Unit/MedicalRecords/AddMedicalRecordHandlerTests.cs` couvrant :
- Happy path : entrée médicale créée avec date et auteur
- Patient introuvable : `Result.NotFound`
- Notes vides : `Result.Invalid`

Créer `tests/Vetolib.Tests.Unit/MedicalRecords/CreatePatientValidatorTests.cs` couvrant :
- Commande valide passe la validation
- Nom vide : erreur de validation avec message attendu
- Espèce invalide (enum hors range si applicable) : erreur de validation

## Contraintes techniques

- InMemory EF Core avec ClinicId fixe `11111111-1111-1111-1111-111111111111`
- Copier le pattern de `AddPrescriptionHandlerTests.cs` pour le setup EF InMemory
- NSubstitute pour `IPublisher`
- Zéro Testcontainers

## Critères de complétion

```
□ CreatePatientHandlerTests.cs créé avec minimum 3 scénarios
□ UpdatePatientHandlerTests.cs créé avec minimum 3 scénarios
□ AddMedicalRecordHandlerTests.cs créé avec minimum 3 scénarios
□ CreatePatientValidatorTests.cs créé avec minimum 3 scénarios
□ dotnet test tests/Vetolib.Tests.Unit/ → 0 erreur, 0 échec
```
