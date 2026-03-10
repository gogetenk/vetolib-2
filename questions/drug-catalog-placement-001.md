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

## Réponse PO

**Option A retenue** : DrugCatalogEntry dans MedicalRecords (entité dans le runtime, DTO dans MedicalRecords.Contracts).

Justification :
1. Le catalogue de médicaments est un outil au service de la prescription. La prescription vit dans MedicalRecords. Le catalogue doit vivre au même endroit pour éviter une dépendance circulaire inutile.
2. L'implémentation actuelle (`DrugCatalogEntry.cs` dans `MedicalRecords/Application/Domain/`) confirme que le choix a déjà été fait dans ce sens -- c'est cohérent.
3. Un module Pharmacy serait prématuré pour le MVP. Si un jour le dispensing en pharmacie devient un domaine à part entière (gestion de stock pharmaceutique réglementée, bon de commande fournisseur, etc.), on pourra extraire.
4. Stock.Contracts n'est pas le bon endroit car le catalogue est un référentiel médical, pas un référentiel d'inventaire. Stock référence le catalogue via DrugCatalogEntryId, pas l'inverse.

La contrainte du query filter global (ClinicId nullable) est un problème technique, pas fonctionnel. Le custom query filter `WHERE ClinicId = @current OR ClinicId IS NULL` dans MedicalRecordsDbContext est la bonne approche, déjà documentée dans la spec.

-> Escalade humain requise : non
