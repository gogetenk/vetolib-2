# CLAUDE.md — Vetolib Factory Rules

> Lu automatiquement par tous les agents au démarrage. Règles non négociables.
> En cas de doute sur une règle → disputes.md, jamais d'improvisation.

---

## Stack

| Couche | Technologie |
|---|---|
| Orchestration locale | .NET Aspire 9.x |
| Backend | ASP.NET Core 10, Minimal APIs uniquement |
| Architecture | Ardalis Modular Monolith (2 assemblies par module) |
| Pattern résultat | **Ardalis.Result PARTOUT — Domain inclus** |
| ORM | Entity Framework Core 10 + Npgsql (PostgreSQL 16) |
| Intra-couche | Clean Architecture + MediatR + FluentValidation |
| Frontend | Next.js 15 App Router, TypeScript, shadcn/ui, Tailwind |
| Tests BDD | Reqnroll + xUnit + Testcontainers.PostgreSql |
| Tests E2E | Playwright |

## Structure de solution

```
Vetolib.sln
├── AppHost/                          ← Aspire — orchestre tout
├── ServiceDefaults/                  ← OpenTelemetry, health checks, service discovery
├── Vetolib.Api/                      ← Host ASP.NET Core 10 (Minimal APIs)
├── Modules/
│   ├── Auth/
│   │   ├── Vetolib.Auth.Contracts/   ← PUBLIC : interfaces, DTOs, events, enums
│   │   └── Vetolib.Auth/             ← INTERNAL : handlers, DbContext, entities
│   ├── Agenda/
│   │   ├── Vetolib.Agenda.Contracts/
│   │   └── Vetolib.Agenda/
│   ├── MedicalRecords/
│   │   ├── Vetolib.MedicalRecords.Contracts/
│   │   └── Vetolib.MedicalRecords/
│   └── Billing/
│       ├── Vetolib.Billing.Contracts/
│       └── Vetolib.Billing/
└── Shared/                           ← GELÉ — ne jamais modifier sans arbitrage humain
    ├── Vetolib.Shared.Kernel/        ← BaseEntity, IMultiTenant, IClinicContext
    └── Vetolib.Shared.Infrastructure/← MultiTenantDbContext, ClinicContext
```

---

## Règles absolues

### 1. Ardalis.Result PARTOUT

Zéro exception pour le control flow business. Chaque méthode qui peut échouer retourne `Result<T>` ou `Result`.

```csharp
// ✅ Domain
public static Result<Appointment> Create(...) { ... }
public Result Confirm() { ... }

// ✅ Handler
public async Task<Result<AppointmentDto>> Handle(...) { ... }

// ✅ Endpoint
group.MapPost("/", async (CreateAppointmentCommand cmd, ISender sender) =>
    (await sender.Send(cmd)).ToMinimalApiResult());

// ❌ INTERDIT
throw new NotFoundException();
throw new ValidationException();
return null;
if (result.IsSuccess) return Ok(result.Value); else return NotFound();
```

### 2. 2 assemblies par module — isolation stricte

- L'assembly `.Contracts` est public. L'assembly runtime est entièrement `internal`.
- La seule classe `public` du runtime est `ModuleServiceRegistrar`.
- **Un module ne peut jamais référencer le runtime d'un autre module.**
- Communication inter-modules : uniquement via `.Contracts` (interfaces) ou Domain Events (MediatR notifications).

```csharp
// ✅ Vetolib.Api référence uniquement les Contracts
using Vetolib.Agenda.Contracts;

// ❌ INTERDIT — jamais de référence au runtime depuis un autre module
using Vetolib.Agenda;  // INTERDIT
```

### 3. BDD-first obligatoire

```
Étape 1 : Lire les fichiers .feature liés à la tâche
Étape 2 : Écrire les step definitions Reqnroll → vérifier RED
Étape 3 : Implémenter jusqu'au GREEN
Étape 4 : PR uniquement quand tous les Gherkins sont GREEN
```

Un agent qui ouvre une PR avec des tests Reqnroll rouges = PR rejetée automatiquement.

### 3b. Vérification locale obligatoire AVANT commit/push (ajout v3.1 — post-mortem 2026-03-11)

**Aucun code ne quitte la machine sans vérification locale.** C'est la règle la plus importante du pipeline.

**Backend** — exécuter dans cet ordre, STOPPER au premier échec :
```bash
dotnet build src/backend/Vetolib.sln -c Release        # DOIT retourner 0 erreurs
dotnet test tests/Vetolib.Tests.Unit/ --no-build -c Release  # DOIT passer
dotnet test tests/Vetolib.Tests.Integration/ --no-build -c Release --filter "Category!=wip"  # DOIT passer
```

**Frontend** — exécuter dans cet ordre :
```bash
cd src/frontend && npm run build   # 0 erreurs TypeScript
```

**Si un test échoue → corriger AVANT de committer.** Pas de `git push` avec des tests rouges.
Pas de "je committe et je fix après". Pas de `--filter` pour exclure les tests qui cassent.

Un agent dev qui ouvre une PR sans avoir exécuté ces commandes = PR rejetée.

### 4. Multi-tenancy — Global Query Filter

Le `MultiTenantDbContext` applique automatiquement `WHERE ClinicId = @current` sur toutes les requêtes.

```csharp
// ✅ Le filter est automatique — ne pas réécrire le WHERE
var appointments = await _context.Appointments.ToListAsync();

// ❌ INTERDIT — le filter le fait déjà, et tu risques de l'avoir deux fois
var appointments = await _context.Appointments
    .Where(a => a.ClinicId == clinicId)  // INTERDIT
    .ToListAsync();

// ❌ INTERDIT en production
_context.Appointments.IgnoreQueryFilters()  // INTERDIT sauf seeds/migrations
```

### 5. Minimal APIs uniquement — pas de Controllers

```csharp
// ✅ Minimal API avec ToMinimalApiResult
group.MapPost("/", async (CreateAppointmentCommand cmd, ISender sender) =>
    (await sender.Send(cmd)).ToMinimalApiResult());

// ❌ INTERDIT
[ApiController]
public class AppointmentController : ControllerBase { ... }
```

### 6. Périmètres isolés

Chaque agent ne touche qu'aux fichiers de son module. Un agent Agenda ne modifie jamais `Modules/Auth/` ou `Shared/`.
Si un besoin inter-module apparaît → créer un fichier `questions/` et se bloquer.

### 7. Fail-fast obligatoire

Se bloquer immédiatement et créer `questions/{task-id}-{timestamp}.md` si :
- Edge case non couvert par les Gherkins
- Ambiguïté sur une règle métier
- Besoin de modifier `Shared/` (GELÉ)
- Deux approches ont échoué

Format du fichier question :

```markdown
# Question — {task-id}

**Module** : Agenda / Auth / MedicalRecords / Billing
**Bloquant** : Oui

## Problème
Description précise.

## Options
**Option A** : ... Impact : ...
**Option B** : ... Impact : ...

## Recommandation
Option X parce que...
```

### 8. Convention commits

```
feat(agenda): add conflict detection on appointment creation
fix(billing): correct VAT 5% calculation
test(auth): add reqnroll bindings for refresh token rotation
refactor(medical-records): extract prescription to value object
```

### 9. Convention PR

Titre : `[MODULE] Description courte`

Body obligatoire :
```
## Tâche
tasks/{task-id}.md

## Gherkins couverts
- Scénario 1 : ...
- Scénario 2 : ...

## Blockers / Questions
Aucun / ou lien vers questions/

## Vidéo démo
Lien artefact Playwright
```

### 10. Skills à lire selon le rôle

Avant de coder, lire les skills correspondants dans `skills/` :

| Tâche | Skills à lire obligatoirement |
|---|---|
| Toute tâche backend | `ardalis-result` + `cqrs-mediatr` + `ardalis-modular-monolith` |
| Entité avec clinicId | + `multitenant-efcore` |
| Endpoint API | + `aspnet-minimal-api` |
| AppHost / infra | `dotnet-aspire` |
| Bindings Reqnroll | `reqnroll-bindings` |
| Tests E2E | `playwright-e2e` |
| Composant frontend | `shadcn-nextjs` |
| Règle métier vétérinaire | `veterinary-domain` |

---

## Fichiers GELÉS

Ne jamais modifier sans arbitrage humain (`disputes.md`) :

- `Shared/Vetolib.Shared.Kernel/` — BaseEntity, IMultiTenant, IClinicContext
- `Shared/Vetolib.Shared.Infrastructure/` — MultiTenantDbContext, ClinicContext
- `AppHost/Program.cs` — orchestration Aspire
- `Vetolib.Api/Program.cs` — registration des modules

---

## Règles Frontend (ajout v2.1)

### MSW — Mock Service Worker

- **Toutes les tâches frontend démarrent avec des handlers MSW** — le backend n'a pas besoin d'exister
- MSW est transparent : `lib/api/*.ts` utilise le même `fetch` en dev (intercepté MSW) et en prod (vrai API)
- Zéro code conditionnel `if (process.env.NODE_ENV === 'development')` dans les composants — uniquement dans `src/mocks/browser.ts`
- Les données mockées sont réalistes : noms arabes/anglais UAE, devise AED, timezone Asia/Dubai
- Skill obligatoire : `skills/msw-mock-api/SKILL.md`

### Tests frontend

- **Playwright contre MSW** (next dev) : tests du comportement UI complet
- `data-testid` obligatoire sur TOUS les éléments interactifs
- Les tests Playwright décrivent le comportement utilisateur, pas les appels réseau
- Pas de `cy.intercept()` ou `page.route()` pour mocker — MSW gère ça

### Features Gherkin — purement fonctionnelles

- **Toujours en anglais** — zéro français dans les .feature
- Les features décrivent le **comportement observable par l'utilisateur** (pas les endpoints API)
- **Zéro technique** : pas de status codes HTTP, pas d'URLs, pas de JSON dans les steps
- Scénarios écrits par/avec le PO en langage naturel
- Un scénario = un use case métier complet
- Les steps techniques (HTTP codes, tenant isolation) vont dans les TI, PAS dans les TF

### Tâches de branchement (wire)

- Une tâche `wire-{module}` est créée automatiquement par l'orchestrator quand `done-back-{module}` ET `done-front-{module}` existent
- La tâche wire : supprime les handlers MSW, exécute Playwright contre le vrai backend
- Si les tests Playwright cassent lors du wire → le contrat API diverge → question au PO

---

## Stratégie de tests — modèle en sablier (v2.3)

3 couches complémentaires, chacune avec un rôle distinct. **Aucune couche ne duplique les tests d'une autre.**

### TU — Tests Unitaires (xUnit + NSubstitute)
- **Rôle** : Edge cases, mutations, setups complexes impossibles à reproduire en intégration
- Testent : Domain factories, Handler branches (error paths, edge cases), Validators, Value Objects
- Tout est mocké (NSubstitute) — PAS de Testcontainers, PAS de HTTP
- Rapides (< 5s pour tout le projet)

### TI — Tests d'Intégration (xUnit + Testcontainers)
- **Rôle** : Wiring technique pur et contract testing — **pas de logique métier**
- Testent : 1 TI par endpoint minimum (le contrat HTTP fonctionne), sérialisation JSON, auth/authz, multi-tenancy isolation
- Vérifient que le pipeline technique fonctionne (DI → handler → DB → response)
- Ne testent PAS les règles métier (c'est le rôle des TF)

### TF — Tests Fonctionnels / BDD (Reqnroll + Testcontainers)
- **Rôle** : Comportement fonctionnel observable, écrit par/avec le PO
- Testent : Tous les use cases métier via Gherkin, langage naturel
- **Zéro technique** dans les .feature : pas de status codes, pas d'URLs, pas de JSON
- Les .feature sont **la spec vivante** — si un scénario n'est pas couvert, c'est un bug
- **Toujours en anglais** (marché UAE)

### Ce que ça implique pour SonarCloud
Chaque fichier backend est couvert directement ou indirectement par au moins une des 3 couches.
Si SonarCloud montre un fichier non couvert → il manque des tests OU c'est du code mort à supprimer.
**Aucune exclusion de coverage** — tout doit être couvert.

### Structure
```
Tests/
├── Vetolib.Tests.Acceptance/         ← TF : Reqnroll + Testcontainers (BDD fonctionnel)
│   ├── StepDefinitions/
│   ├── Support/
│   └── Features/                     ← .feature EN ANGLAIS uniquement
├── Vetolib.Tests.Integration/        ← TI : contract testing, wiring, 1 par endpoint
├── Vetolib.Tests.Unit/               ← TU : edge cases, mutations, validators
```

Les projets TU ne dépendent JAMAIS de Testcontainers — tout est mocké (NSubstitute).

---

## Commandes disponibles (v2.2)

Dans n'importe quelle session Claude Code lancée à la racine du repo :

| Commande | Effet |
|---|---|
| `/forge` | Cycle orchestrateur complet (scan → dispatch → wire → progress) |
| `/status` | État rapide en < 10 lignes, sans lancer d'agents |
| `/dev tasks/todo-front-auth-001.md` | Lancer un agent dev sur une tâche précise |
| `/po` | Traiter toutes les questions métier en attente dans `questions/` |

## Optimisations de contexte (v2.2)

**Règle des 3 fichiers max au démarrage :**
Un agent dev doit démarrer avec 3 lectures maximum avant de coder :
1. Sa tâche `tasks/wip-*.md` (scope + règles métier + skills requis)
2. Le skill principal listé dans la tâche
3. Le `.feature` correspondant

`CLAUDE.md` et `archi-spec.md` sont lus uniquement si la tâche est la première du module
ou si un doute architectural émerge. Pas systématiquement.

**Tasks auto-suffisantes :**
Chaque fichier `todo-*.md` doit contenir tout ce dont l'agent a besoin :
- Règles métier non-triviales (pas un renvoi vers openspec qui n'existe plus)
- Liste exacte des skills à lire
- Critères de complétion binaires (checkbox)

**Ne jamais polluter le contexte avec :**
- Des fichiers déjà lus qui ne changent pas (archi-spec après la première lecture)
- Des listes de tâches complètes (seule la tâche courante compte)
- Des specs de modules autres que celui en cours

## Hooks actifs

| Hook | Déclencheur | Effet |
|---|---|---|
| `guard-shared.sh` | Toute écriture Write/Edit | Bloque modification de `Shared/` sans autorisation |
| `verify-before-push.sh` | Bash `git push` | Build + tests unitaires DOIVENT passer avant push |
| `log-cost.sh` | Fin de session (Stop) | Log dans `.claude/cost-log.csv`, alerte si > $2/session |

## Flags dans les tâches

| Flag | Signification |
|---|---|
| `[MSW: oui]` | Tâche frontend — développer avec MSW, pas de dépendance backend |
| `[Branchement ultérieur]` | Indique quelle tâche wire sera créée par l'orchestrator |
| `MODIF_SHARED: autorisé` | Autorise le hook guard-shared pour cette tâche |
