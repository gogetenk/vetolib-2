# todo-back-prescriptions-stock-link-001 -- Lien Stock-Prescription via DrugCatalogEntryId

**Module** : Stock (entity modification) + Stock.Contracts (queries/events)
**Phase** : 3 (Stock-Prescription Integration)
**Dependances** : todo-back-prescriptions-catalog-001, todo-back-prescriptions-enrich-001
**Branchement ulterieur** : todo-front-prescriptions-stock-001

## Objectif

Ajouter `DrugCatalogEntryId` sur StockItem pour lier stock et catalogue, et implementer les queries de disponibilite stock.

## Skills a lire

1. `skills/ardalis-result/SKILL.md`
2. `skills/cqrs-mediatr/SKILL.md`
3. `skills/ardalis-modular-monolith/SKILL.md`

## Spec de reference

`docs/PRESCRIPTIONS-AI-SPEC.md` -- sections 5.1, 5.2, 2.6 (Matching)

## Scope detaille

### Stock.Contracts

- Modifier `StockItemDto` : ajouter DrugCatalogEntryId (Guid?)
- Modifier `CreateStockItemRequest` : ajouter DrugCatalogEntryId (Guid?)
- Nouveau : `CheckStockAvailabilityQuery : IRequest<Result<StockAvailabilityResult>>`
  - DrugCatalogEntryId (Guid), ClinicId (Guid)
- Nouveau : `StockAvailabilityResult` : record
  - Available (bool), Quantity (int), Unit (string), IsLowStock (bool), IsExpiringSoon (bool)
  - Alternatives (List<StockAlternativeDto>)
- Nouveau : `StockAlternativeDto` : record (StockItemId, Name, DrugCatalogEntryId, Quantity, Unit)
- Nouveau : `DecrementStockForPrescriptionCommand : IRequest<Result>`
  - StockItemId (Guid), Quantity (int), PrescriptionId (Guid), ClinicId (Guid)

### Stock Domain

- Modifier `StockItem` :
  - Ajouter `DrugCatalogEntryId` (Guid?) -- nullable car les supplies n'ont pas de lien catalogue
  - Modifier factory `Create()` pour accepter DrugCatalogEntryId
- Modifier `StockItem.ToDto()` pour mapper le nouveau champ

### Stock Infrastructure

- Modifier `StockItemConfiguration` pour la colonne + index sur DrugCatalogEntryId
- Migration EF Core

### Stock CQRS

- `CheckStockAvailabilityHandler` :
  - Chercher StockItem(s) avec le DrugCatalogEntryId dans la clinique courante
  - Si out of stock : chercher alternatives (meme categorie, quantity > 0)
  - Retourner StockAvailabilityResult
- `DecrementStockForPrescriptionHandler` :
  - Appeler StockItem.ApplyMovement(Out, quantity)
  - Creer StockMovement avec Reason = "Prescription #{prescriptionId}"
  - Publier StockLowEvent si seuil atteint

### Consumer

- `PrescriptionCreatedConsumer` (dans Stock) :
  - Consomme `PrescriptionCreatedEvent` (publie par MedicalRecords)
  - Si StockDecrementConfirmed = true ET DrugCatalogEntryId != null :
    - Envoyer `DecrementStockForPrescriptionCommand`
  - Si stock insuffisant : publier `StockInsufficientForPrescriptionEvent`

### Stock.Contracts (events)

- Nouveau : `StockInsufficientForPrescriptionEvent : INotification`
  - PrescriptionId, DrugCatalogEntryId, RequestedQuantity, AvailableQuantity

## Criteres de completion

- [ ] StockItem enrichi avec DrugCatalogEntryId
- [ ] CreateStockItemRequest enrichi
- [ ] CheckStockAvailabilityQuery + handler fonctionnel
- [ ] DecrementStockForPrescriptionCommand + handler fonctionnel
- [ ] PrescriptionCreatedConsumer dans Stock
- [ ] StockMovement cree avec reason "Prescription #..."
- [ ] Alternatives suggerees quand out of stock
- [ ] StockInsufficientForPrescriptionEvent publie si stock insuffisant
- [ ] StockLowEvent publie si seuil atteint apres decrementation
- [ ] Migration EF Core generee
- [ ] Index sur DrugCatalogEntryId
- [ ] Aucune reference runtime cross-module
