# todo-wire-agenda-001.md — Brancher Agenda sur l'API réelle
**Dépendances** : done-agenda-002, done-front-agenda-001
[MSW: non]

## Objectif
Supprimer les handlers MSW de `src/mocks/handlers/appointments.ts`.
Lancer Playwright contre le vrai backend. Tous les tests doivent rester verts.

## Critère
- [ ] Aucun handler MSW pour le module Agenda
- [ ] Tests Playwright verts contre API réelle
- [ ] Renommer en done-wire-agenda-001.md
