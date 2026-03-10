# todo-front-prescriptions-stock-001 -- UI dispense stock + disponibilite

**Module** : Frontend
**Phase** : 3 (Stock-Prescription Integration)
**Dependances** : todo-front-prescriptions-catalog-001, todo-front-prescriptions-alerts-001, todo-back-prescriptions-stock-link-001 (contrats API)
**[MSW: oui]**
**Branchement ulterieur** : wire-prescriptions-stock-001

## Objectif

Afficher la disponibilite stock dans le formulaire de prescription et implementer le flow de dispense/skip.

## Skills a lire

1. `skills/shadcn-nextjs/SKILL.md`
2. `skills/msw-mock-api/SKILL.md`

## Spec de reference

`docs/PRESCRIPTIONS-AI-SPEC.md` -- sections 5.2 (Stock Check Flow) et 8.1 (points 4, 5)

## Scope detaille

### MSW Handlers

- Enrichir le handler `POST /prescriptions/preflight` pour inclure stockAvailability :
  - Scenarios :
    - Drug en stock (quantity 100, unit "tablets")
    - Drug low stock (quantity 5, threshold 20)
    - Drug out of stock (quantity 0) avec alternatives en stock
    - Drug hors catalogue (free-text) -> pas de stock info

### Types

- Ajouter dans `src/lib/api/types.ts` :
  - `StockAvailabilityResult` { available, quantity, unit, isLowStock, isExpiringSoon, alternatives }
  - `StockAlternativeDto` { stockItemId, name, drugCatalogEntryId, quantity, unit }

### Composants

- `src/components/features/patients/StockAvailabilityPanel.tsx` :
  - Affiche quantite disponible + unite
  - Badges : "Low stock" (amber), "Out of stock" (red), "Expiring soon" (orange)
  - Si out of stock : affiche alternatives en stock
  - data-testid :
    - `data-testid="stock-quantity"`
    - `data-testid="stock-badge-low"`
    - `data-testid="stock-badge-out"`
    - `data-testid="stock-badge-expiring"`
    - `data-testid="stock-alternative-{id}"`

- `src/components/features/patients/DispenseToggle.tsx` :
  - Checkbox "Dispense from clinic stock" (default: checked si stock disponible)
  - Si coche : champ quantite a dispenser (pre-rempli avec la quantite prescrite)
  - Si stock insuffisant pour la quantite demandee : warning + option partial dispense
  - Si decoche : prescription sauvee sans mouvement stock
  - data-testid :
    - `data-testid="dispense-toggle"`
    - `data-testid="dispense-quantity-input"`
    - `data-testid="dispense-partial-warning"`
    - `data-testid="dispense-partial-confirm"`

### Integration dans le formulaire

- Afficher StockAvailabilityPanel apres selection du medicament (dans le preflight result)
- Afficher DispenseToggle en bas du formulaire
- Free-text prescription : cacher StockAvailabilityPanel, proposer selection manuelle de stock item

## Criteres de completion

- [ ] StockAvailabilityPanel affiche quantite + badges
- [ ] Alternatives en stock affichees quand out of stock
- [ ] DispenseToggle avec checkbox + champ quantite
- [ ] Warning stock insuffisant avec option partial dispense
- [ ] Free-text : pas de stock info automatique
- [ ] MSW handlers enrichis avec scenarios stock
- [ ] data-testid sur TOUS les elements interactifs
- [ ] Integration dans le formulaire de prescription
