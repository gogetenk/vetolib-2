# todo-back-prescriptions-weight-001 -- Ajouter WeightKg sur Patient

**Module** : MedicalRecords
**Phase** : 1 (preparatoire pour Phase 2 dosage checking)
**Dependances** : aucune (peut etre fait en parallele de catalog-001)
**Branchement ulterieur** : todo-front-prescriptions-weight-001

## Objectif

Ajouter le champ `WeightKg` (decimal?) sur l'entite Patient pour permettre le calcul de dosage par poids.

## Skills a lire

1. `skills/ardalis-result/SKILL.md`
2. `skills/cqrs-mediatr/SKILL.md`

## Scope detaille

### Contracts

- Modifier `PatientDto` : ajouter WeightKg (decimal?)
- Modifier `PatientDetailDto` : ajouter WeightKg (decimal?)
- Modifier `CreatePatientRequest` : ajouter WeightKg (decimal?)
- Modifier `UpdatePatientRequest` : ajouter WeightKg (decimal?)

### Domain

- Modifier `Patient` :
  - Ajouter `WeightKg` (decimal?) -- nullable car pas tous les patients sont peses a chaque visite
  - Modifier factory `Create()` : accepter WeightKg optionnel
  - Modifier `UpdateInfo()` : accepter WeightKg optionnel, valider > 0 si fourni
- Modifier `Patient.ToDto()` pour mapper WeightKg

### Infrastructure

- Modifier `PatientConfiguration` : colonne decimal(8,2) nullable
- Migration EF Core

### CQRS

- Modifier `CreatePatientHandler` : passer WeightKg a la factory
- Modifier `UpdatePatientHandler` : passer WeightKg a UpdateInfo()

### Endpoint

- Pas de nouvel endpoint -- les endpoints existants (POST/PUT patients) acceptent deja les request enrichis

## Criteres de completion

- [ ] Patient entity avec WeightKg decimal? nullable
- [ ] Validation : si fourni, WeightKg > 0
- [ ] CreatePatientRequest et UpdatePatientRequest enrichis
- [ ] PatientDto et PatientDetailDto enrichis
- [ ] Handlers Create/Update modifies
- [ ] Migration EF Core generee
- [ ] Backward compatible : WeightKg null par defaut pour patients existants
