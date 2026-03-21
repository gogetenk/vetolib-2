# todo-wire-scheduling-001.md — Wire scheduling (backend + frontend)

**Module** : Wire
**Dependances** : done-back-scheduling-001, done-front-scheduling-001
**Priorite** : MOYENNE

---

## Objectif

Supprimer les handlers MSW pour le scheduling et executer les tests Playwright contre le vrai backend.

## Implementation

1. **Supprimer le handler MSW** pour `POST /api/agenda/suggest-slot` dans `src/mocks/handlers/scheduling.ts`
2. **Verifier** que `src/lib/api/appointments.ts` pointe vers la bonne URL backend
3. **Executer** les tests Playwright E2E contre le vrai backend (Aspire)
4. **Si les tests cassent** : le contrat API diverge → creer une question PO

## Critere

```
[] Handler MSW suggest-slot supprime
[] Playwright tests passent contre le vrai backend
[] Aucune regression sur les autres tests
[] Renommer en done
```
