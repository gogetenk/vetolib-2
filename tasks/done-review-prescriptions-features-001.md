# todo-review-prescriptions-features-001.md — Review PO + Archi des features Prescriptions

**Type** : Review (pas de code)
**Priorité** : HAUTE (doit être fait AVANT la vague 2 de dev)
**Dépendances** : done-back-prescriptions-catalog-001, done-back-prescriptions-weight-001, done-back-prescriptions-gherkin-001
**Assignés** : Agent PO + Agent Architecte

## Objectif

Valider que le catalogue médicamenteux, le champ poids, et les Gherkins sont conformes à la spec `docs/PRESCRIPTIONS-AI-SPEC.md`.

## Review PO
- Le catalogue couvre-t-il les médicaments vétérinaires courants ?
- Les Gherkins couvrent-ils les interactions critiques (espèce, drug-drug, dosage) ?
- Les scénarios stock↔prescription sont-ils réalistes ?
- Le workflow override est-il clair pour le vétérinaire ?

## Review Architecte
- DrugCatalogEntry dans MedicalRecords (pas Shared/) ?
- Query filter custom pour ClinicId IS NULL sur le catalogue ?
- WeightKg nullable decimal sur Patient ?
- Pas de modification de Shared/ ?
- Result<T> sur les nouvelles factories ?

## Critère
```
[] Review PO validée
[] Review Archi validée — 0 violation
[] Renommer en done
```
