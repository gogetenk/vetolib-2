# todo-wire-billing-001.md — Brancher Billing sur l'API réelle
**Dépendances** : done-billing-001, done-front-billing-001
[MSW: non]

## Objectif
Supprimer les handlers MSW de `src/mocks/handlers/billing.ts`.
Lancer Playwright contre le vrai backend. Tous les tests doivent rester verts.

## Critère
- [ ] Aucun handler MSW pour le module Billing
- [ ] Tests Playwright verts contre API réelle
- [ ] Renommer en done-wire-billing-001.md
