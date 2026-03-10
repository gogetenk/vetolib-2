# todo-back-prescriptions-gherkin-001 -- Features Gherkin pour prescriptions enrichies

**Module** : Tests (cross-module)
**Phase** : Toutes phases (a ecrire avant l'implementation -- BDD-first)
**Dependances** : aucune (les features sont ecrites AVANT le code)

## Objectif

Creer les fichiers .feature Gherkin pour les deux features (Drug Interaction Checking + Stock-Prescription Integration) telles que definies dans la spec.

## Skills a lire

1. `skills/reqnroll-bindings/SKILL.md`

## Spec de reference

`docs/PRESCRIPTIONS-AI-SPEC.md` -- sections 4.3 et 5.3 (scenarios Gherkin complets)

## Scope detaille

### Feature files a creer

1. `tests/Vetolib.Tests.Acceptance/Features/Prescriptions/DrugInteractionChecking.feature`
   - Copier les 7 scenarios de la section 4.3 de la spec
   - Adapter le format si necessaire pour Reqnroll

2. `tests/Vetolib.Tests.Acceptance/Features/Prescriptions/StockPrescriptionIntegration.feature`
   - Copier les 8 scenarios de la section 5.3 de la spec
   - Adapter le format si necessaire pour Reqnroll

### Step Definitions (squelettes)

3. `tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/DrugInteractionSteps.cs`
   - Steps Given/When/Then pour les scenarios d'interaction
   - Implementation initiale : throw PendingStepException() (RED state)

4. `tests/Vetolib.Tests.Acceptance/StepDefinitions/Prescriptions/StockPrescriptionSteps.cs`
   - Steps Given/When/Then pour les scenarios stock-prescription
   - Implementation initiale : throw PendingStepException() (RED state)

### Contraintes

- Les feature files reprennent EXACTEMENT les scenarios de la spec PO
- Les step definitions sont des squelettes (PendingStepException) -- elles seront implementees par les taches de dev
- Ne PAS implementer les steps -- c'est le role des taches de dev (BDD-first : RED d'abord)

## Criteres de completion

- [ ] DrugInteractionChecking.feature cree avec 7 scenarios
- [ ] StockPrescriptionIntegration.feature cree avec 8 scenarios
- [ ] DrugInteractionSteps.cs squelette cree (PendingStepException)
- [ ] StockPrescriptionSteps.cs squelette cree (PendingStepException)
- [ ] Tous les fichiers compilent
- [ ] Tous les tests sont en etat RED/Pending (pas d'erreur de compilation)
