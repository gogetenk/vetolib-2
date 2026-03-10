# todo-infra-git-cleanup-001.md — Nettoyer git, commit, push, PRs GitHub

**Module** : Infra
**Dépendances** : aucune
**Priorité** : CRITIQUE
**Skills à lire** : aucun

---

## Contexte

La branche `feat/back-patients-001` accumule ~180 fichiers modifiés sans commit. Les PRs sont "local-only". Il faut nettoyer, committer proprement, push, et créer les PRs GitHub.

## Étapes

### 1. Analyser les changements accumulés
- `git status` pour lister tous les fichiers modifiés/ajoutés/supprimés
- Regrouper par catégorie logique (module, type de changement)

### 2. Commits atomiques par thème
Créer des commits séparés et bien nommés :
- `feat(sentry): upgrade to Sentry 6.1.0 with EnableLogs support`
- `feat(messaging): add owner portal, SSE, templates, notifications`
- `feat(prescriptions): add drug catalog, interactions, weight-based dosing`
- `feat(stock): add stock management module`
- `feat(infra): add CI/CD pipelines, Docker, monitoring`
- `test(integration): add WebApplicationFactory integration test project`
- `chore: cleanup deleted files, refacto tasks`

### 3. Push et créer les PRs
- Push la branche sur origin
- Créer une PR via `gh pr create` ciblant `main`
- Titre : `[ALL] Consolidation MVP — modules Messaging, Stock, Prescriptions, Infra`

### 4. Vérifier la CI
- Attendre que la CI passe
- Si des checks échouent, fixer et re-push

## Critère de complétion

```
□ Tous les fichiers modifiés sont commités (pas de dirty state)
□ Commits atomiques par thème
□ Branche pushée sur origin
□ PR créée sur GitHub
□ CI vérifiée (au moins build OK)
□ Renommer en done
```
