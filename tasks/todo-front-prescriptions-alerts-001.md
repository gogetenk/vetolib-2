# todo-front-prescriptions-alerts-001 -- UI alertes interactions + override

**Module** : Frontend
**Phase** : 2 (Interaction Checking)
**Dependances** : todo-front-prescriptions-catalog-001, todo-back-prescriptions-interactions-001 (contrats API)
**[MSW: oui]**
**Branchement ulterieur** : wire-prescriptions-interactions-001

## Objectif

Afficher les alertes d'interactions medicamenteuses dans le formulaire de prescription, avec le mecanisme d'override pour les alertes critiques.

## Skills a lire

1. `skills/shadcn-nextjs/SKILL.md`
2. `skills/msw-mock-api/SKILL.md`

## Spec de reference

`docs/PRESCRIPTIONS-AI-SPEC.md` -- sections 8.1 (points 3, 6) et 8.2 (Alert Display Rules)

## Scope detaille

### MSW Handlers

- `POST /api/medical-records/prescriptions/preflight` :
  - Simule differents scenarios selon le DrugCatalogEntryId envoye :
    - Drug "Ibuprofen" + patient Cat -> alerte Critical (species contraindication)
    - Drug "Metronidazole" + patient avec prescription active Amoxicillin -> alerte Moderate
    - Drug "Amoxicillin" + dosage hors range -> alerte Info
    - Drug sans probleme -> pas d'alerte
  - Retourne PrescriptionPreflightResult

### lib/api/

- `src/lib/api/prescriptions.ts` : ajouter
  - `checkPrescriptionPreflight(data: PreflightRequest): Promise<PrescriptionPreflightResult>`

### Types

- Ajouter dans `src/lib/api/types.ts` :
  - `InteractionAlert` { severity, type, message, alternativeDrugIds }
  - `PrescriptionPreflightResult` { interactionAlerts, stockAvailability, safeAlternatives }
  - `PreflightRequest` { patientId, drugCatalogEntryId, dosageAmount }

### Composants

- `src/components/features/patients/InteractionAlertsPanel.tsx` :
  - Affiche les alertes color-coded :
    - Critical : banniere rouge, icone warning triangle
    - Moderate : banniere amber, collapsible, icone exclamation circle
    - Info : texte bleu sous le champ dosage
  - Maximum 5 alertes affichees, "N more alerts" expandable
  - data-testid :
    - `data-testid="interaction-alert-{severity}-{index}"`
    - `data-testid="interaction-alerts-expand"`

- `src/components/features/patients/OverrideSection.tsx` :
  - Apparait UNIQUEMENT quand alerte Critical presente
  - Textarea pour justification (min 10 caracteres)
  - Compteur de caracteres
  - Bouton submit desactive tant que < 10 caracteres
  - data-testid :
    - `data-testid="override-justification-input"`
    - `data-testid="override-submit-button"`
    - `data-testid="override-char-count"`

- `src/components/features/patients/AlternativeSuggestions.tsx` :
  - Liste des medicaments alternatifs suggeres
  - Bouton "Use this instead" pour chaque alternative
  - data-testid :
    - `data-testid="alternative-drug-{id}"`
    - `data-testid="alternative-use-button-{id}"`

- `src/components/features/patients/DosageRangeIndicator.tsx` :
  - Affiche le range recommande sous le champ dosage (si patient.WeightKg + guidelines existent)
  - Indicateur visuel : vert si dans le range, orange si hors range
  - data-testid :
    - `data-testid="dosage-range-indicator"`
    - `data-testid="dosage-range-min"`
    - `data-testid="dosage-range-max"`

### Integration dans le formulaire

- Modifier le formulaire de prescription :
  - Apres selection du medicament, appeler preflight
  - Afficher InteractionAlertsPanel
  - Afficher DosageRangeIndicator
  - Si alerte Critical : afficher OverrideSection, bloquer submit tant que justification non fournie
  - Afficher AlternativeSuggestions si disponibles

### RBAC frontend

- Cacher le bouton "New Prescription" pour ASSISTANT et RECEPTIONIST
- Utiliser le hook `use-role.ts` existant

## Criteres de completion

- [ ] MSW handler POST /prescriptions/preflight avec scenarios varies
- [ ] InteractionAlertsPanel avec color-coding correct
- [ ] OverrideSection avec validation 10 caracteres
- [ ] AlternativeSuggestions avec bouton "Use this instead"
- [ ] DosageRangeIndicator fonctionnel
- [ ] Appel preflight automatique apres selection du medicament
- [ ] Submit bloque si alerte Critical sans justification
- [ ] Maximum 5 alertes affichees + expand
- [ ] RBAC : bouton "New Prescription" cache pour ASSISTANT/RECEPTIONIST
- [ ] data-testid sur TOUS les elements interactifs
- [ ] Skeleton states (pas de spinner)
