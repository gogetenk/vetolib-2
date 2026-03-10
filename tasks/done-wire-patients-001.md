# todo-wire-patients-001.md — Brancher Patients sur l'API réelle
**Dépendances** : done-back-patients-001, done-front-patients-001
[MSW: non]
## Objectif
Supprimer les handlers MSW de `src/mocks/handlers/patients.ts`.
Lancer Playwright contre le vrai backend. Tous les tests doivent rester verts.
## Critère
- [ ] Aucun handler MSW pour ce module
- [ ] Tests Playwright verts contre API réelle
- [ ] Renommer en done-wire-patients-001.md
