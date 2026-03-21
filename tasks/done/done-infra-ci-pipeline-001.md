# todo-infra-ci-pipeline-001 — Pipeline CI complète avec tests auto

**Module** : Infrastructure
**Dependances** : aucune
**Priorite** : HAUTE

---

## Objectif

Refondre `.github/workflows/ci.yml` pour avoir une CI professionnelle déclenchée sur push develop + PR vers develop/main.

## Ce qui existe déjà

Le fichier `.github/workflows/ci.yml` existe avec : backend-build, backend-bdd, frontend-build, frontend-e2e, docker-build. Mais il ne se déclenche que sur main.

## Modifications requises

### 1. Triggers

```yaml
on:
  push:
    branches: [develop, main]
  pull_request:
    branches: [develop, main]
```

### 2. Jobs existants à garder (ajuster si besoin)

- `backend-build` : build + unit tests
- `backend-bdd` : BDD acceptance tests (Testcontainers)
- `frontend-build` : lint + build
- `frontend-e2e` : Playwright E2E (MSW)
- `docker-build` : verify Docker images build

### 3. Jobs à ajouter

- **`code-quality`** : job parallèle
  - Lint backend : `dotnet format --verify-no-changes`
  - Check for TODO/FIXME/HACK dans le code source (warning, pas bloquant)

- **`security-scan`** : job parallèle
  - `dotnet list package --vulnerable` pour les CVE .NET
  - `npm audit --audit-level=high` pour le frontend

- **`status-check`** : job final qui dépend de tous les autres
  - Juste un job gate qui ne fait rien sauf confirmer que tout est vert
  - Permet de configurer une branch protection rule sur ce seul job

### 4. Optimisations

- Cache NuGet : `actions/cache@v4` avec `~/.nuget/packages`
- Cache npm : déjà en place
- Concurrency : déjà en place, garder
- Timeout : 15 min max par job

### 5. Status badges

- Ajouter le badge CI dans le README.md existant (en haut du fichier)

## Règles

- Ne PAS toucher au code applicatif
- Ne PAS modifier deploy.yml (c'est la tâche CD)
- Le fichier ci.yml doit être autonome et complet

## Critère

```
[] CI se déclenche sur push develop + PR develop/main
[] backend-build + backend-bdd + frontend-build + frontend-e2e + docker-build
[] Job code-quality ajouté
[] Job security-scan ajouté
[] Job status-check gate final
[] Cache NuGet configuré
[] Timeout 15 min sur chaque job
[] Badge CI dans README.md
[] Renommer en done
```
