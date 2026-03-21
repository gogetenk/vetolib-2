# todo-wire-users-001.md — Brancher Users sur l'API réelle
**Dépendances** : done-back-users-001, done-front-users-001
[MSW: non]
## Objectif
Supprimer les handlers MSW de `src/mocks/handlers/users.ts`.
Lancer Playwright contre le vrai backend. Tous les tests doivent rester verts.
## Critère
- [ ] Aucun handler MSW pour ce module
- [ ] Tests Playwright verts contre API réelle
- [ ] Renommer en done-wire-users-001.md
