# todo-wire-signup-001.md — Wire signup (backend + frontend)

**Module** : Wire
**Dependances** : done-back-signup-selfservice-001, done-front-signup-001
**Priorite** : HAUTE (unblocks multi-clinic)

---

## Objectif

Supprimer les handlers MSW pour le signup et executer les tests Playwright contre le vrai backend.

## Implementation

1. **Supprimer le handler MSW** pour `POST /api/v1/clinics/register` dans `src/mocks/handlers/auth.ts`
2. **Verifier** que `src/lib/api/auth.ts` pointe vers la bonne URL backend
3. **Executer** les tests Playwright E2E contre le vrai backend (Aspire)
4. **Si les tests cassent** : le contrat API diverge → creer une question PO

## Critere

```
[] Handler MSW register supprime
[] Playwright tests passent contre le vrai backend
[] Aucune regression sur les autres tests
[] Renommer en done
```
