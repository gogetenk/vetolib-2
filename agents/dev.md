# agents/dev.md — Agent Développeur

## Rôle

Tu implémentes une tâche atomique. Tu peux être sur une tâche **backend**, **frontend (MSW)**, ou **wire** (branchement).
Tu ne prends aucune décision métier. Tu respectes `archi-spec.md` et `CLAUDE.md`.

---

## Étape 0 — Lire avant de toucher au code

```
1. CLAUDE.md                              ← règles absolues
2. archi-spec.md                          ← structure de solution, patterns
3. openspec/{module}/spec.md              ← règles métier du module
4. tasks/{task-id}.md                     ← scope exact de ta tâche
5. features/{module}/*.feature            ← comportements à couvrir
6. skills/ listés dans ta tâche           ← patterns attendus
```

---

## Si ta tâche est BACKEND

### Étape 1 — Structure module (si première tâche du module)

```
Modules/{Module}/Vetolib.{Module}/
├── Api/{Entity}Endpoints.cs               ← internal static class
├── Application/
│   ├── Commands/{Action}{Entity}/
│   │   ├── {Action}{Entity}Command.cs     ← internal record
│   │   ├── {Action}{Entity}Handler.cs     ← internal class, retourne Result<T>
│   │   └── {Action}{Entity}Validator.cs   ← internal class FluentValidation
│   └── Queries/{Action}{Entity}/
│       ├── {Action}{Entity}Query.cs
│       └── {Action}{Entity}Handler.cs
├── Domain/
│   ├── {Entity}.cs                        ← internal, BaseEntity, IMultiTenant
│   └── {Entity}Status.cs                  ← internal enum
├── Infrastructure/
│   ├── {Module}DbContext.cs               ← internal : MultiTenantDbContext
│   └── Migrations/
└── {Module}ModuleServiceRegistrar.cs      ← public static class (seule exception)

Modules/{Module}/Vetolib.{Module}.Contracts/
├── DTOs/
├── Events/
└── Enums/

Tests/Vetolib.{Module}.Tests.Unit/        ← UN PROJET PAR MODULE
├── Domain/{Entity}Tests.cs
├── Application/Commands/{Action}{Entity}HandlerTests.cs
└── Application/Queries/...Tests.cs
```

### Étape 2 — Bindings Reqnroll EN PREMIER (BDD-first obligatoire)

```csharp
// Tests/Vetolib.Tests.Acceptance/StepDefinitions/{Module}/{Entity}Steps.cs
[Binding]
internal sealed class LoginSteps
{
    private readonly HttpClient _client;
    private HttpResponseMessage? _response;

    public LoginSteps(ApiFactory factory) => _client = factory.CreateClient();

    [Given(@"a vet clinic with a registered user ""(.*)"" and password ""(.*)""")]
    public async Task GivenARegisteredUser(string email, string password)
    { /* setup DB via TestWebApplicationFactory */ }

    [When(@"the user submits login with email ""(.*)"" and password ""(.*)""")]
    public async Task WhenSubmitsLogin(string email, string password)
    {
        _response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password });
    }

    [Then(@"the response contains a valid JWT access token")]
    public void ThenResponseContainsJwt()
    {
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
        // assertions → RED ici, pas encore implémenté
    }
}
```

```bash
dotnet test Tests/Vetolib.Tests.Acceptance/ --filter "Feature[Auth]"
# DOIT être ROUGE. Si vert → le binding est mal écrit.
```

### Étape 3 — Implémenter jusqu'au GREEN

Ordre : Domain → Command/Handler → Validator → DbContext → Endpoint → Migration

Rappels critiques :
- `Result<T>` partout, zéro exception pour le business flow
- `internal` sur tout sauf `ModuleServiceRegistrar`
- `IMultiTenant` sur toutes les entités (Global Query Filter automatique)
- Zéro Controller, uniquement Minimal APIs avec `.ToMinimalApiResult()`

### Étape 4 — Tests unitaires

```bash
dotnet test Tests/Vetolib.{Module}.Tests.Unit/
# Doit couvrir : Domain factories, Handler happy path, Handler error cases
```

---

## Si ta tâche est FRONTEND (avec [MSW: oui])

**Principe : tu développes TOUTE l'UI avec des données mockées. Le backend n'a pas besoin d'exister.**

### Étape 1 — MSW handlers (fausses données réalistes)

```typescript
// vetolib-frontend/src/mocks/handlers/{module}.ts
import { http, HttpResponse } from 'msw'

export const appointmentHandlers = [
  http.get('/api/appointments', () => {
    return HttpResponse.json({
      items: [
        { id: 'uuid-1', patientName: 'Max', species: 'Dog', status: 'SCHEDULED',
          scheduledAt: '2026-03-10T09:00:00Z', vetName: 'Dr. Sarah' },
        { id: 'uuid-2', patientName: 'Luna', species: 'Cat', status: 'IN_PROGRESS',
          scheduledAt: '2026-03-10T10:30:00Z', vetName: 'Dr. Ahmed' },
      ],
      totalCount: 2, page: 1, pageSize: 10
    })
  }),

  http.post('/api/appointments', async ({ request }) => {
    const body = await request.json()
    return HttpResponse.json({ id: 'uuid-new', ...body, status: 'SCHEDULED' }, { status: 201 })
  }),

  http.patch('/api/appointments/:id/transition', ({ params }) => {
    return HttpResponse.json({ id: params.id, status: 'CHECKED_IN' })
  }),
]
```

### Étape 2 — Activer MSW en développement

```typescript
// vetolib-frontend/src/mocks/browser.ts
import { setupWorker } from 'msw/browser'
import { appointmentHandlers } from './handlers/appointments'
// ... autres handlers

export const worker = setupWorker(...appointmentHandlers, ...authHandlers, ...)

// vetolib-frontend/src/app/layout.tsx (uniquement si process.env.NODE_ENV === 'development')
if (typeof window !== 'undefined' && process.env.NODE_ENV === 'development') {
  const { worker } = await import('@/mocks/browser')
  await worker.start({ onUnhandledRequest: 'bypass' })
}
```

### Étape 3 — Implémenter l'UI

Ordre :
1. Types TypeScript dans `lib/api/{module}.ts` (interfaces uniquement, pas de fetch)
2. Composants UI avec `data-testid` sur tous les éléments interactifs
3. `lib/api/{module}.ts` : fonctions fetch qui appellent le vrai endpoint (iront vers MSW en dev)
4. Pages Next.js

Les appels `fetch` pointent sur `NEXT_PUBLIC_API_URL` — MSW intercepte en dev, le vrai backend en prod.
**Aucun if/else "si MSW alors ... sinon ..."** dans les composants. MSW est transparent.

### Étape 4 — Tests Playwright (contre MSW)

```typescript
// vetolib-frontend/e2e/{module}/{feature}.spec.ts
// Playwright s'exécute contre next dev (qui utilise MSW)
// Les tests décrivent le comportement UI, pas les appels réseau

test('receptionist sees appointment list', async ({ page }) => {
  await page.goto('/appointments')
  await expect(page.getByTestId('appointments-table')).toBeVisible()
  await expect(page.getByText('Max')).toBeVisible() // donnée MSW
  await expect(page.getByText('SCHEDULED')).toBeVisible()
})

test('vet can start consultation', async ({ page }) => {
  await page.goto('/appointments/uuid-1')
  await page.getByTestId('btn-checkin').click()
  await page.getByTestId('confirm-dialog-ok').click()
  await expect(page.getByTestId('status-badge')).toContainText('CHECKED_IN')
})
```

---

## Si ta tâche est WIRE (branchement)

1. Supprime les handlers MSW du module dans `src/mocks/handlers/{module}.ts`
2. Lance `next dev` + backend Aspire en parallèle
3. Lance les tests Playwright — ils doivent rester verts (même comportement, vrai API)
4. Si un test casse : le contrat API diverge → crée `questions/wire-{module}-{timestamp}.md`

---

## Blocage → fail-fast

Crée `questions/{task-id}-{timestamp}.md` et rename `wip-*.md` → `todo-*.md` si :
- Edge case non couvert par les Gherkins
- Ambiguïté métier dans `openspec/`
- Besoin de modifier `Shared/` (gelé)
- 2 implémentations ont échoué

---

## Checklist avant PR

**TOUTES les étapes doivent être EXÉCUTÉES (pas juste cochées). Coller la sortie console comme preuve.**

```
□ Étape 0 lue (CLAUDE.md, archi-spec, openspec, task, features, skills)
□ Bindings Reqnroll écrits en RED avant l'implémentation (backend)
□ MSW handlers écrits en premier (frontend)
□ Tous les Gherkins/Playwright de la tâche sont VERTS
□ dotnet build → 0 erreur (backend) | npm run build → 0 erreur (frontend)
□ Tests unitaires du module passent (backend)
□ Aucun IgnoreQueryFilters(), aucun Controller, aucun throw business
□ data-testid sur tous les éléments interactifs (frontend)
□ PR body rempli avec le template CLAUDE.md
```

## Règles de commit et PR

### Commit immédiat
- **Dès que les tests sont GREEN → commit + push immédiatement.**
- Ne JAMAIS laisser des fixes en local non commités. Un fix non commité n'existe pas.
- Si tu as fait un changement et que les tests passent, commit AVANT toute autre action.

### Une PR par tâche, vers develop
- Chaque tâche = 1 branche = 1 PR vers `develop`.
- **INTERDIT de pousser sur la branche d'un autre agent ou d'une autre tâche.**
- Si tu es dans un worktree, crée ta propre PR vers develop.
- Max ~30 fichiers par PR. Si tu dépasses, tu fais trop de choses.

### Validation post-PR
- Après `gh pr create`, vérifie que la CI se lance (`gh pr checks <num>`).
- Si un check est rouge, corrige AVANT de marquer la tâche comme done.
