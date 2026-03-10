# todo-front-prescriptions-catalog-001 -- UI autocomplete catalogue medicamenteux

**Module** : Frontend
**Phase** : 1 (Drug Catalog + Prescription Linking)
**Dependances** : todo-back-prescriptions-catalog-001 (contrats API), todo-back-prescriptions-enrich-001
**[MSW: oui]**
**Branchement ulterieur** : wire-prescriptions-catalog-001

## Objectif

Remplacer le champ texte libre "Medication" du formulaire de prescription par un autocomplete qui cherche dans le catalogue de medicaments, avec fallback vers texte libre.

## Skills a lire

1. `skills/shadcn-nextjs/SKILL.md`
2. `skills/msw-mock-api/SKILL.md`

## Spec de reference

`docs/PRESCRIPTIONS-AI-SPEC.md` -- section 8.1 (Prescription Form Changes) point 1

## Scope detaille

### MSW Handlers

- `GET /api/medical-records/drugs?search={term}` -- retourne liste DrugCatalogEntryDto mockee
  - Mock : 10-15 medicaments realistes (INN names)
  - Simuler delai 100ms
- `GET /api/medical-records/drugs/{id}` -- retourne detail d'un medicament

### lib/api/

- `src/lib/api/drugs.ts` :
  - `searchDrugs(term: string): Promise<DrugCatalogEntryDto[]>`
  - `getDrugById(id: string): Promise<DrugCatalogEntryDto>`

### Types

- `src/lib/api/types.ts` : ajouter
  - `DrugCatalogEntryDto`
  - `DrugCategory`
  - `InteractionSeverity`
  - `SpeciesContraindicationDto`
  - `DosageGuidelineDto`

### Composants

- `src/components/features/patients/DrugSelector.tsx` :
  - Autocomplete avec debounce 300ms
  - Affiche INN name + display name
  - Toggle "Free text mode" pour saisie libre
  - data-testid sur tous les elements interactifs :
    - `data-testid="drug-selector-input"`
    - `data-testid="drug-selector-option-{id}"`
    - `data-testid="drug-selector-free-text-toggle"`
    - `data-testid="drug-selector-free-text-input"`

- Modifier `src/components/features/patients/MedicalRecordForm.tsx` (ou PrescriptionForm si existe) :
  - Remplacer input Medication par DrugSelector
  - Garder le champ Dosage (texte)

### Performance

- Debounce 300ms sur la recherche
- Skeleton states pendant le chargement (pas de spinner)
- Resultats en < 200ms (mock)

## Criteres de completion

- [ ] MSW handler GET /drugs?search= fonctionnel
- [ ] MSW handler GET /drugs/{id} fonctionnel
- [ ] lib/api/drugs.ts avec searchDrugs et getDrugById
- [ ] Types TypeScript alignes avec les Contracts backend
- [ ] DrugSelector composant avec autocomplete debounced
- [ ] Toggle free-text mode fonctionnel
- [ ] data-testid sur tous les elements interactifs
- [ ] Integration dans le formulaire de prescription existant
- [ ] Skeleton states (pas de spinner)
