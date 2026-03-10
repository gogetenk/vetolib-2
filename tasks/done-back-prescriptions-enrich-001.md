# todo-back-prescriptions-enrich-001 -- Enrichir Prescription avec reference catalogue

**Module** : MedicalRecords
**Phase** : 1 (Drug Catalog + Prescription Linking)
**Dependances** : todo-back-prescriptions-catalog-001
**Branchement ulterieur** : todo-front-prescriptions-form-001

## Objectif

Modifier l'entite Prescription pour referencer le catalogue de medicaments (DrugCatalogEntryId) et ajouter les champs necessaires pour le flow d'override et de stock.

## Skills a lire

1. `skills/ardalis-result/SKILL.md`
2. `skills/cqrs-mediatr/SKILL.md`
3. `skills/ardalis-modular-monolith/SKILL.md`

## Scope detaille

### Contracts (Vetolib.MedicalRecords.Contracts)

- Modifier `AddPrescriptionRequest` : ajouter DrugCatalogEntryId (Guid?), OverrideJustification (string?), OverrideSeverity (string?), StockDecrementConfirmed (bool)
- Modifier `PrescriptionDto` : ajouter DrugCatalogEntryId, DrugName (resolved from catalog), StockDecrementConfirmed, OverrideJustification, OverrideSeverity
- Nouveau : `PrescriptionCreatedEvent : INotification` (Id, ClinicId, DrugCatalogEntryId?, Quantity?, StockDecrementConfirmed)

### Domain

- Modifier `Prescription` :
  - Ajouter : DrugCatalogEntryId (Guid?), StockDecrementConfirmed (bool), OverrideJustification (string?), OverrideSeverity (InteractionSeverity?)
  - Modifier factory `Create()` : accepter les nouveaux champs
  - Regle : si DrugCatalogEntryId est fourni, Medication peut etre vide (il sera resolu depuis le catalogue)
  - Regle : si DrugCatalogEntryId est null, Medication est obligatoire (mode free-text)
- Modifier `Prescription.ToDto()` pour mapper les nouveaux champs

### Infrastructure

- Modifier `PrescriptionConfiguration` pour les nouvelles colonnes
- Migration EF Core

### CQRS

- Modifier `AddPrescriptionHandler` :
  - Si DrugCatalogEntryId fourni : resoudre le nom du medicament depuis le catalogue
  - Publier `PrescriptionCreatedEvent` apres sauvegarde
  - Pas encore de check d'interactions (Phase 2) ni de stock (Phase 3)

## Criteres de completion

- [ ] Prescription entity enrichie avec les 4 nouveaux champs
- [ ] Factory Create() accepte DrugCatalogEntryId nullable
- [ ] Logique : DrugCatalogEntryId XOR Medication obligatoire
- [ ] PrescriptionCreatedEvent publie via MediatR
- [ ] PrescriptionDto enrichi dans Contracts
- [ ] AddPrescriptionRequest enrichi dans Contracts
- [ ] Migration EF Core generee
- [ ] Handler resout le nom du medicament depuis le catalogue si DrugCatalogEntryId fourni
- [ ] Backward compatible : les prescriptions free-text continuent de fonctionner
