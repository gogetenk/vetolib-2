# todo-back-prescriptions-interactions-001 -- Drug interaction checking service

**Module** : AI (handler) + MedicalRecords.Contracts (query interface) + AI.Contracts (result types)
**Phase** : 2 (Interaction Checking)
**Dependances** : todo-back-prescriptions-catalog-001, todo-back-prescriptions-enrich-001, todo-back-prescriptions-weight-001
**Branchement ulterieur** : todo-front-prescriptions-alerts-001

## Objectif

Implementer le service de verification des interactions medicamenteuses. Le handler vit dans le module AI, les types de requete/resultat sont dans les Contracts respectifs.

## Skills a lire

1. `skills/ardalis-result/SKILL.md`
2. `skills/cqrs-mediatr/SKILL.md`
3. `skills/ardalis-modular-monolith/SKILL.md`

## Spec de reference

`docs/PRESCRIPTIONS-AI-SPEC.md` -- sections 4.2 (Interaction Check Flow) et 4.4 (Active Prescription Window)

## Scope detaille

### Contracts

**MedicalRecords.Contracts** :
- `CheckInteractionsQuery : IRequest<Result<InteractionCheckResult>>`
  - PatientId, DrugCatalogEntryId, DosageAmount (decimal?), ClinicId
- `InteractionCheckResult` : record
  - Alerts (List<InteractionAlert>), Alternatives (List<DrugCatalogEntryDto>)
- `InteractionAlert` : record
  - Severity (InteractionSeverity), Type (InteractionAlertType), Message (string), AlternativeDrugIds (List<Guid>)

**AI.Contracts** :
- Re-exporter les types si necessaire pour eviter que AI reference MedicalRecords.Contracts circulairement
- Note : AI reference MedicalRecords.Contracts (pour DrugCatalogEntryDto, Species). MedicalRecords.Contracts reference AI.Contracts n'est PAS necessaire -- les types de resultat vivent dans MedicalRecords.Contracts.

### AI Module (handler interne)

- `CheckInteractionsHandler : IRequestHandler<CheckInteractionsQuery, Result<InteractionCheckResult>>`
- Logique :
  1. Recuperer le DrugCatalogEntry pour le medicament prescrit (via MedicalRecords DbContext ? Non -- via query MediatR)
  2. Recuperer les prescriptions actives du patient (derniers 90 jours, configurable)
  3. Checker :
     a. **Species contraindication** : drug vs patient.Species
     b. **Drug-drug interaction** : drug vs chaque prescription active ayant un DrugCatalogEntryId
     c. **Dosage out of range** : si patient.WeightKg est set et dosage guidelines existent
  4. Retourner les alertes triees par severite (Critical > Moderate > Info)
  5. Pour chaque alerte, suggerer des alternatives (medicaments du catalogue sans la meme contraindication)

### Probleme d'acces aux donnees inter-modules

Le handler AI a besoin de :
- DrugCatalogEntry (dans MedicalRecords)
- Patient (dans MedicalRecords)
- Prescriptions actives (dans MedicalRecords)

**Solution** : Creer des queries MediatR dans MedicalRecords.Contracts :
- `GetDrugCatalogEntryQuery : IRequest<Result<DrugCatalogEntryDto>>` (deja fait dans catalog-001)
- `GetActivePrescriptionsForPatientQuery : IRequest<Result<List<PrescriptionDto>>>` -- nouveau
- `GetPatientSpeciesAndWeightQuery : IRequest<Result<PatientSpeciesWeightDto>>` -- nouveau

Le handler AI envoie ces queries via ISender et recoit les DTOs sans jamais acceder au runtime MedicalRecords.

### Configuration

- Setting clinic-configurable : `ActivePrescriptionWindowDays` (default: 90)
- Stocke dans la configuration du module AI ou dans un setting clinic (a decider -- si ambiguite, creer une question)

## Criteres de completion

- [ ] CheckInteractionsQuery dans MedicalRecords.Contracts
- [ ] InteractionCheckResult, InteractionAlert dans MedicalRecords.Contracts
- [ ] GetActivePrescriptionsForPatientQuery handler dans MedicalRecords
- [ ] GetPatientSpeciesAndWeightQuery handler dans MedicalRecords
- [ ] CheckInteractionsHandler dans AI module
- [ ] Check species contraindication fonctionnel
- [ ] Check drug-drug interaction fonctionnel
- [ ] Check dosage out of range fonctionnel (si WeightKg present)
- [ ] Alternatives suggerees pour chaque alerte
- [ ] Alertes triees par severite
- [ ] Aucune reference runtime cross-module
- [ ] Unit tests pour chaque type d'alerte
