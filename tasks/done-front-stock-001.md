# todo-front-stock-001.md — Frontend page gestion de stock

**Module** : Frontend
**Dependances** : done-back-stock-management-001
**Priorite** : MOYENNE
[MSW: oui]
[Branchement ulterieur: wire-stock]
**Skills a lire** : `shadcn-nextjs`, `msw-mock-api`, `playwright-e2e`

---

## Objectif

Page de gestion de stock medicaments/vaccins avec tableau, alertes, et formulaires.

## Implementation

1. **Route : /[locale]/stock**
   - Tableau avec colonnes : nom, categorie, quantite, seuil, expiry, statut
   - Badge rouge si low-stock (qty < threshold), orange si expiring (< 30 jours)
   - Filtres : categorie (Medication/Vaccine/Supply), statut (low-stock, expiring)

2. **Formulaire ajout stock item**
   - Dialog avec champs : nom, categorie, quantite, unite, seuil min, date expiry
   - Validation inline

3. **Formulaire mouvement**
   - Dialog : type (IN/OUT/ADJUSTMENT), quantite, raison
   - Validation : OUT ne peut pas depasser la quantite actuelle

4. **Section alertes**
   - En haut de la page, banner si items low-stock ou expiring

5. **MSW handlers**
   - GET /api/v1/stock → liste mockee
   - POST /api/v1/stock → 201
   - POST /api/v1/stock/{id}/movements → 200
   - GET /api/v1/stock/alerts → liste alertes mockees
   - PATCH /api/v1/stock/{id} → 200

6. **i18n EN + AR**
7. **data-testid** sur tous les elements
8. **Playwright E2E** : CRUD stock, mouvement, alertes affichees

## Critere

```
[] Page /stock avec tableau + filtres
[] Formulaire ajout stock item
[] Formulaire mouvement (IN/OUT)
[] Alertes low-stock et expiring
[] MSW handlers
[] i18n EN + AR
[] Playwright tests
[] data-testid
[] Renommer en done
```
