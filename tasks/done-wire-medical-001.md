# todo-wire-medical-001.md — Brancher MedicalRecords sur l'API réelle
**Dépendances** : done-medical-002, done-front-medical-001
[MSW: non]

## Objectif
Supprimer les handlers MSW de `src/mocks/handlers/patients.ts`.
Lancer Playwright contre le vrai backend. Tous les tests doivent rester verts.

## Critère
- [ ] Aucun handler MSW pour le module MedicalRecords
- [ ] Tests Playwright verts contre API réelle
- [ ] Renommer en done-wire-medical-001.md
