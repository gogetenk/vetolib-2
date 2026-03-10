# Question — drug-catalog-placement-001

**Module** : MedicalRecords / nouveau module Pharmacy ?
**Bloquant** : Non (étude archi en cours)

## Problème
La spec PRESCRIPTIONS-AI-SPEC.md propose un DrugCatalogEntry global (sans ClinicId).
La première implémentation suggérait de le mettre dans Shared/ — REFUSÉ par le product owner.
Le catalogue doit vivre dans un module, pas dans Shared/ (gelé).

## Options
**Option A** : DrugCatalogEntry dans MedicalRecords.Contracts (c'est lié aux prescriptions)
**Option B** : Nouveau module Vetolib.Pharmacy (catalogue + interactions + dispensing)
**Option C** : DrugCatalogEntry dans Stock.Contracts (c'est lié aux items en stock)

## Décision attendue
L'architecte tranche dans l'étude multi-tenant ou dans les tasks prescriptions.

## Contrainte
- Le catalogue est global (pas de ClinicId) — nécessite une exception au query filter
- Pas dans Shared/ — décision product owner explicite
