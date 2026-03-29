# CLAUDE.md — Vetolib Factory Rules

> Lu automatiquement par tous les agents au démarrage. Règles non négociables.
> En cas de doute sur une règle → .claude/disputes.md, jamais d'improvisation.

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

## Arborescence racine

```
vetolib2/
├── CLAUDE.md                 ← Regles du projet (lu par tous les agents)
├── README.md
├── docker-compose.yml        ← Dev compose
├── sonar-project.properties  ← Config SonarCloud (doit rester a la racine)
├── release-please-config.json
├── .claude/                  ← Config Claude Code + fichiers internes orchestrateur
│   ├── settings.json
│   ├── hooks/
│   ├── commands/
│   ├── agents/
│   ├── disputes.md           ← Arbitrages en attente
│   ├── pr-status.md          ← Suivi PRs
│   └── progress.md           ← Progression orchestrateur
├── src/
│   ├── backend/              ← .NET Aspire + Modules
│   └── frontend/             ← Next.js 15
├── tests/                    ← TU + TI + TF (backend .NET)
├── tasks/                    ← Backlog file-based (todo/done)
├── questions/                ← Questions PO en attente
├── skills/                   ← Skills de reference pour les agents
├── docs/
│   ├── specs/                ← Specs fonctionnelles + archi
│   ├── studies/              ← Etudes techniques
│   └── technical/            ← Docs techniques
├── infra/                    ← Deploiement (Caddyfile, compose prod, scripts)
└── .github/workflows/        ← CI/CD
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

**Frontend** — exécuter dans cet ordre, STOPPER au premier échec :
```bash
cd src/frontend && npm run lint    # 0 errors (warnings OK) — attrape react-hooks, a11y, etc.
cd src/frontend && npm run build   # 0 erreurs TypeScript
```

**Si un test échoue → corriger AVANT de committer.** Pas de `git push` avec des tests rouges.
Pas de "je committe et je fix après". Pas de `--filter` pour exclure les tests qui cassent.

Un agent dev qui ouvre une PR sans avoir exécuté ces commandes = PR rejetée.

### 3f. Audit des migrations EF Core (ajout v3.5)

Après chaque `dotnet ef migrations add`, l'agent DOIT vérifier :

1. **Lire le fichier `.cs` généré** — vérifier :
   - Pas de `AlterColumn` sur des colonnes jamais créées (doit être `AddColumn`)
   - Pas de `DropTable`/`DropColumn` non intentionnels (ex: tables MassTransit outbox)
   - Le fichier `.Designer.cs` compagnon existe
2. **Valider** : `dotnet ef migrations has-pending-model-changes -c {Context} -p {project} -s src/backend/Vetolib.Api` → doit retourner "No changes"
3. **Si des doutes** → lire les migrations précédentes pour comprendre l'historique du schéma

**Pourquoi** : les migrations auto-générées peuvent produire des `AlterColumn` fantômes quand le snapshot diverge du schéma réel. 7 commits de fix en une session à cause de ça.

### 3c. Commit immédiat après GREEN (ajout v3.2 — post-mortem 2026-03-11)

**Dès que les tests sont GREEN → `git add` + `git commit` + `git push` IMMÉDIATEMENT.**

Un fix vérifié localement mais non commité **n'existe pas**. Il peut être perdu par :
- Un revert automatique (linter, hook, autre agent)
- Un checkout de branche
- Un merge qui écrase les changements locaux

Séquence obligatoire :
```
1. Faire le changement
2. Exécuter les tests (voir 3b)
3. Si GREEN → commit + push dans la minute
4. Ne JAMAIS faire autre chose entre le GREEN et le commit (pas de /forge, pas de dispatch)
```

### 3e. Merge-based sync, pas rebase (ajout v3.3 — post-mortem 2026-03-13)

**Quand une PR a des conflits avec develop, utiliser `git merge origin/develop` au lieu de `git rebase`.**

Le repo GitHub interdit le force-push sur toutes les branches. Un rebase nécessite un force-push → bloqué → obligation de créer une nouvelle branche + nouvelle PR. C'est du gaspillage.

```bash
# ✅ CORRECT — merge-based sync
git fetch origin develop
git merge origin/develop    # résoudre les conflits, commit merge
git push                    # push normal, pas de force-push

# ❌ INTERDIT — rebase + force-push
git rebase origin/develop   # crée un historique divergent
git push --force-with-lease # BLOQUÉ par les repo rules
```

### 3d. Hygiène des PRs (ajout v3.2)

- **1 tâche = 1 branche = 1 PR vers `develop`**
- Max ~30 fichiers modifiés par PR. Au-delà = PR monstre → split obligatoire.
- Un agent worktree crée sa PROPRE PR. INTERDIT de pousser sur la branche d'un autre agent.
- Après chaque merge de PR → vérifier que `develop` CI est GREEN dans les 2 minutes.
- Si develop RED après merge → fix immédiat, AVANT toute autre action.
- **Tâches qui touchent la même entité** : les worktrees sont mergés par l'orchestrateur en **1 seule branche + 1 seule PR** pour éviter les conflits en cascade. L'orchestrateur fait le merge local des worktrees, résout les conflits, vérifie le build, puis push + PR.

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

**Contexte rapide** : Branche `{branch}`, tâche `{task-id}`, module {Module}. L'agent travaillait sur : {description en 1 ligne}.
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

### 7b. Circuit breaker — 3 tentatives max (ajout v3.4)

Un agent qui debug un problème a **3 tentatives maximum** pour le résoudre.
Après 3 échecs consécutifs sur le même problème :
- **STOP immédiat** — ne pas continuer à deviner
- Créer un fichier `questions/{task-id}-debug-{timestamp}.md` avec :
  - Ce qui a été tenté (les 3 approches)
  - Les résultats/erreurs de chaque tentative
  - Hypothèse sur la cause racine
- Se bloquer et attendre une réponse humaine ou PO

**Pourquoi** : un agent qui boucle sur un fix consomme du contexte et du budget sans progresser.
Un humain ou un autre agent avec un regard frais résout souvent le problème en 1 tentative.

### 7c. Fallback Bash pour les subagents (ajout v3.5)

Les subagents dans les worktrees peuvent perdre l'accès Bash (limitation connue).
Si un agent est bloqué sur des commandes git/push/PR :
- Terminer avec le statut `BLOCKED`
- Lister les **commandes exactes** à exécuter
- L'orchestrateur les exécutera à sa place

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

Ne jamais modifier sans arbitrage humain (`.claude/disputes.md`) :

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

`CLAUDE.md` et `docs/specs/archi-spec.md` sont lus uniquement si la tâche est la première du module
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
