# todo-wire-auth-001.md — Brancher Auth sur l'API réelle
**Dépendances** : done-auth-002, done-front-auth-001
[MSW: non]

## Objectif
Supprimer les handlers MSW de `src/mocks/handlers/auth.ts`.
Lancer Playwright contre le vrai backend. Tous les tests doivent rester verts.

## Critère
- [ ] Aucun handler MSW pour le module Auth
- [ ] Tests Playwright verts contre API réelle
- [ ] Renommer en done-wire-auth-001.md
