# todo-front-stock-history-001.md — Historique des mouvements de stock

**Module** : Frontend (Stock)
**Dépendances** : aucune
**Skills à lire** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**

---

## Objectif

Ajouter une vue "Historique des mouvements" à la page Stock existante (`/stock`). Actuellement on peut enregistrer des mouvements IN/OUT/ADJUSTMENT mais il n'y a pas de vue journal pour les consulter.

## Backend existant

Les mouvements sont enregistrés via `POST /api/v1/stock/{id}/movements`. Le backend stocke les `StockMovement` entities mais il n'y a pas encore d'endpoint GET pour les lister. Il faudra en ajouter un côté MSW en attendant le vrai endpoint.

## Implémentation

### 1. MSW handler pour lister les mouvements
- Ajouter dans `src/frontend/src/mocks/handlers/stock.ts` :
  - `GET /api/v1/stock/:id/movements` → retourne la liste des mouvements pour un item
- Mock data : 5-10 mouvements réalistes par item (IN achats, OUT prescriptions, ADJUSTMENT inventaire)

### 2. API client
- Ajouter dans `src/frontend/src/lib/api/stock.ts` :
  - `getStockMovements(stockItemId: string): Promise<StockMovementDto[]>`

### 3. Composant StockMovementHistory
- Créer `src/frontend/src/components/features/stock/StockMovementHistory.tsx`
- Table avec colonnes : Date, Type (IN/OUT/ADJUSTMENT avec badges colorés), Quantity (+/-), Reason, Created By
- Filtrage par type de mouvement
- Tri par date (plus récent en haut)

### 4. Intégration dans la page Stock
- Ajouter un bouton "History" sur chaque ligne de la StockTable
- Clic → ouvre un drawer/dialog avec StockMovementHistory pour cet item
- Alternative : onglet "Movements" dans un panneau détail

### 5. i18n
- Ajouter les clés dans messages/en.json et messages/ar.json sous `stock.movements.*`

## Critère de complétion

```
□ MSW handler GET /api/v1/stock/:id/movements
□ API client getStockMovements()
□ Composant StockMovementHistory avec table
□ Bouton History accessible depuis StockTable
□ Filtrage par type de mouvement
□ data-testid sur tous éléments interactifs
□ i18n EN + AR
□ npm run lint → 0 errors
□ npm run build → 0 errors
□ PR vers develop
□ Renommer en done-front-stock-history-001.md
```
