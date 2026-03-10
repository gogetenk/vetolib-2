# todo-front-prescriptions-weight-001 -- UI poids patient

**Module** : Frontend
**Phase** : 1 (preparatoire pour Phase 2 dosage checking)
**Dependances** : todo-back-prescriptions-weight-001 (contrats API)
**[MSW: oui]**
**Branchement ulterieur** : wire-prescriptions-weight-001

## Objectif

Ajouter le champ WeightKg dans les formulaires patient (creation + edition) et l'afficher dans le detail patient.

## Skills a lire

1. `skills/shadcn-nextjs/SKILL.md`
2. `skills/msw-mock-api/SKILL.md`

## Scope detaille

### MSW Handlers

- Modifier les handlers patients existants pour inclure WeightKg dans les responses
- Mock : poids realistes par espece (chat 3-7kg, chien 5-50kg, cheval 400-600kg, chameau 300-700kg)

### Types

- Modifier `PatientDto` dans types.ts : ajouter weightKg (number | null)
- Modifier `CreatePatientRequest` : ajouter weightKg (number | null)
- Modifier `UpdatePatientRequest` : ajouter weightKg (number | null)

### Composants

- Modifier `src/components/features/patients/PatientForm.tsx` :
  - Ajouter champ "Weight (kg)" : input number, step 0.1, min 0
  - Label : "Weight (kg)" avec hint "Optional -- used for dosage calculations"
  - data-testid :
    - `data-testid="patient-weight-input"`

- Modifier le detail patient (si composant existe) :
  - Afficher le poids si disponible
  - Afficher "Not recorded" si null
  - data-testid :
    - `data-testid="patient-weight-display"`

## Criteres de completion

- [ ] Champ weight dans PatientForm (creation + edition)
- [ ] Validation : > 0 si fourni
- [ ] MSW handlers patients enrichis avec weightKg
- [ ] Types TypeScript mis a jour
- [ ] Affichage du poids dans le detail patient
- [ ] data-testid sur les elements
- [ ] "Not recorded" affiche si poids null
