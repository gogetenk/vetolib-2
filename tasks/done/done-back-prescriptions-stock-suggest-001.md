# todo-back-prescriptions-stock-suggest-001 -- Stock insuffisant -> suggestion alternative

**Module** : Stock + MedicalRecords (orchestration)
**Phase** : 3 (Stock-Prescription Integration)
**Dependances** : todo-back-prescriptions-stock-link-001, todo-back-prescriptions-interactions-001
**Branchement ulterieur** : todo-front-prescriptions-stock-001

## Objectif

Quand un medicament prescrit est en rupture de stock, suggerer des alternatives en stock qui passent aussi le check d'interactions.

## Skills a lire

1. `skills/ardalis-result/SKILL.md`
2. `skills/cqrs-mediatr/SKILL.md`

## Spec de reference

`docs/PRESCRIPTIONS-AI-SPEC.md` -- section 5.2 point 4

## Scope detaille

### Flow

1. Vet selectionne un medicament du catalogue
2. MedicalRecords handler envoie `CheckStockAvailabilityQuery` -> Stock repond
3. Si out of stock : Stock retourne des alternatives (meme DrugCategory, quantity > 0)
4. Pour chaque alternative, MedicalRecords envoie `CheckInteractionsQuery` -> AI verifie
5. Seules les alternatives sans alerte Critical sont presentees au vet

### MedicalRecords CQRS

- Nouveau : `GetPrescriptionPreflightQuery : IRequest<Result<PrescriptionPreflightResult>>`
  - PatientId, DrugCatalogEntryId, DosageAmount (decimal?), ClinicId
- `PrescriptionPreflightResult` : record
  - InteractionAlerts (List<InteractionAlert>)
  - StockAvailability (StockAvailabilityResult)
  - SafeAlternatives (List<SafeAlternativeDto>) -- alternatives en stock sans interaction critique

- `SafeAlternativeDto` : record (DrugCatalogEntryId, DrugName, StockQuantity, StockUnit, InteractionAlerts -- les warnings non-critiques)

- `GetPrescriptionPreflightHandler` :
  - Orchestre les appels a CheckInteractionsQuery et CheckStockAvailabilityQuery
  - Filtre les alternatives pour ne garder que les "safe"
  - Retourne le resultat combine

### Endpoint

- `POST /api/medical-records/prescriptions/preflight` -- appele par le frontend avant la creation
  - Body : { PatientId, DrugCatalogEntryId, DosageAmount }
  - Retour : PrescriptionPreflightResult

## Criteres de completion

- [ ] GetPrescriptionPreflightQuery dans MedicalRecords.Contracts
- [ ] PrescriptionPreflightResult dans MedicalRecords.Contracts
- [ ] Handler orchestre interactions + stock + filtrage alternatives
- [ ] Alternatives filtrees : pas d'alerte Critical
- [ ] Endpoint POST /prescriptions/preflight fonctionnel
- [ ] Unit tests : out of stock avec alternatives safe
- [ ] Unit tests : out of stock avec toutes alternatives unsafe (liste vide)
- [ ] Unit tests : en stock, pas d'alternatives suggerees
