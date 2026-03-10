# todo-front-scheduling-001.md — Frontend slot suggestion dans formulaire RDV

**Module** : Frontend
**Dependances** : done-back-scheduling-001
**Priorite** : HAUTE
[MSW: oui]
[Branchement ulterieur: wire-scheduling]
**Skills a lire** : `shadcn-nextjs`, `msw-mock-api`, `playwright-e2e`

---

## Objectif

Integrer la suggestion de creneaux dans le formulaire de prise de RDV existant.

## Implementation

1. **Formulaire RDV** : ajouter un bouton "Suggest best slot" qui appelle POST /api/agenda/suggest-slot
2. **Affichage** : panel avec les 3 creneaux suggeres, score, nom du vet, duree estimee, reasoning
3. **Selection** : cliquer sur un creneau pre-remplit les champs date/heure/vet du formulaire
4. **MSW handler** : POST /api/agenda/suggest-slot → 200 avec 3 suggestions mockees
5. **i18n EN + AR**
6. **data-testid** sur tous les elements interactifs
7. **Playwright E2E** : suggestion affichee, selection pre-remplit le formulaire

## Critere

```
[] Bouton "Suggest best slot" dans le formulaire RDV
[] Panel suggestions avec score, vet, duree, reasoning
[] Selection pre-remplit le formulaire
[] MSW handler
[] i18n EN + AR
[] Playwright tests
[] data-testid sur tous les champs
[] Renommer en done
```
