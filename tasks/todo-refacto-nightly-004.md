# todo-refacto-nightly-004 — MedicalRecords: AddPrescriptionHandler > 80 lignes

**Module** : MedicalRecords
**Priorité** : MOYENNE — maintenabilité
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

`AddPrescriptionHandler` fait 112 lignes. Il mélange plusieurs responsabilités dans `Handle()` : validation métier du patient, vérification du stock, construction de la prescription, publication d'événements.

Fichier : `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Commands/AddPrescription/AddPrescriptionHandler.cs`

## Fix attendu

Extraire des méthodes privées `internal` dans le handler pour réduire la méthode `Handle()` à < 40 lignes :

- `private async Task<Result<Patient>> GetValidatedPatientAsync(Guid patientId, CancellationToken ct)` — chargement + vérification existence
- `private static Result<Prescription> BuildPrescription(AddPrescriptionCommand cmd, Patient patient)` — construction de l'entité domain
- Éventuellement une méthode pour la publication d'événements

Les méthodes extraites restent `private` dans la même classe — pas de nouveaux services.

## Critères de complétion

```
□ Méthode Handle() <= 40 lignes
□ Méthodes privées extraites avec noms explicites
□ Aucun changement de comportement (même logique métier)
□ dotnet build → 0 erreur
□ Tests unitaires AddPrescription toujours verts
```
