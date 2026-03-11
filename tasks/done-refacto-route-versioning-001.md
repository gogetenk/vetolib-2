# todo-refacto-route-versioning-001.md — Uniformiser le préfixe /api/v1/ sur toutes les routes

**Module** : Cross-module (Auth, AI)
**Priorité** : BASSE
**Dépendance** : Aucune bloquante, mais nécessite coordination front+back+tests

---

## Contexte

Audit `todo-refacto-consistency-001` a identifié 4 groupes de routes qui utilisent `/api/` sans `/v1/` :

| Endpoint file | Route actuelle | Route cible |
|---|---|---|
| `Auth/Api/AuthEndpoints.cs` | `/api/auth` | `/api/v1/auth` |
| `Auth/Api/UserEndpoints.cs` | `/api/users` | `/api/v1/users` |
| `Auth/Api/OnboardingEndpoints.cs` | `/api/onboarding` | `/api/v1/onboarding` |
| `AI/Api/AIEndpoints.cs` | `/api/ai` | `/api/v1/ai` |

Ces routes sont les seules sans `/v1/` — toutes les autres (Agenda, Billing, MedicalRecords, Stock, Messaging, Clinics) utilisent déjà `/api/v1/`.

## Impact

Le changement de route est cassant pour :
- `src/frontend/src/lib/api/auth.ts` (auth, users)
- `src/frontend/src/lib/api/onboarding.ts`
- `src/frontend/src/lib/api/ai-triage.ts`
- `src/frontend/src/app/api/auth/login/route.ts`
- `src/frontend/src/app/api/auth/refresh/route.ts`
- `src/frontend/src/mocks/handlers/auth.ts`, `users.ts`, `onboarding.ts`
- `tests/Vetolib.Tests.Acceptance/StepDefinitions/` — nombreux fichiers (Auth, AI, Agenda, Billing, Messaging, Dashboard, Stock)
- `tests/Vetolib.Tests.Integration/`

## Procédure recommandée

1. Ajouter les nouvelles routes (`/api/v1/...`) en **alias** (MapGroup supplémentaire) → double routage temporaire
2. Migrer le frontend + les tests vers les nouvelles routes
3. Supprimer les anciens groupes de routes

## Critères de complétion

```
□ AuthEndpoints : /api/v1/auth
□ UserEndpoints : /api/v1/users
□ OnboardingEndpoints : /api/v1/onboarding
□ AIEndpoints : /api/v1/ai
□ Frontend migré (lib/api/, mocks/handlers/)
□ Tests Acceptance + Integration migrés
□ dotnet build 0 erreurs
□ Tous les tests verts
```
