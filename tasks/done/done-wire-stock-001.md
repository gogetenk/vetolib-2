# todo-wire-stock-001.md — Brancher Stock frontend sur l'API réelle

**Dépendances** : done-back-stock-management-001, done-front-stock-001
**Skills** : shadcn-nextjs

## Objectif
Remplacer les handlers MSW de `src/mocks/handlers/stock.ts`
par les appels réels vers `lib/api/stock.ts`.

## Étapes
1. Supprimer les handlers MSW du module dans `src/mocks/handlers/stock.ts`
2. Retirer l'import/spread de `stockHandlers` dans `src/mocks/handlers/index.ts`
3. Vérifier que `lib/api/stock.ts` pointe sur NEXT_PUBLIC_API_URL
4. Lancer les tests Playwright contre l'API réelle (backend doit tourner)
5. Tous les tests doivent rester verts

## Critère de complétion
```
[] Aucun handler MSW restant pour stock
[] Tests Playwright verts contre API réelle
[] Renommer en done-wire-stock-001.md
```
