# todo-wire-ai-triage-001.md — Brancher AI Triage frontend sur l'API réelle

**Dépendances** : done-back-ai-triage-001, done-front-ai-triage-001
**Skills** : shadcn-nextjs

## Objectif
Remplacer les handlers MSW de `src/mocks/handlers/ai-triage.ts`
par les appels réels vers `lib/api/ai-triage.ts`.

## Étapes
1. Supprimer les handlers MSW du module dans `src/mocks/handlers/ai-triage.ts`
2. Retirer l'import/spread de `aiTriageHandlers` dans `src/mocks/handlers/index.ts`
3. Vérifier que `lib/api/ai-triage.ts` pointe sur NEXT_PUBLIC_API_URL
4. Lancer les tests Playwright contre l'API réelle (backend doit tourner)
5. Tous les tests doivent rester verts

## Critère de complétion
```
[] Aucun handler MSW restant pour ai-triage
[] Tests Playwright verts contre API réelle
[] Renommer en done-wire-ai-triage-001.md
```
