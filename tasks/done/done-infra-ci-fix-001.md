# todo-infra-ci-fix-001.md — Fixer la CI et les PRs GitHub

**Module** : Infra
**Dépendances** : aucune
**Priorité** : CRITIQUE
**Skills à lire** : aucun

---

## Contexte

La CI GitHub Actions ne fonctionne pas correctement. Les PRs dans pr-status.md sont marquées "local-only" alors qu'un remote existe (`origin → gogetenk/vetolib-2`). Aucune PR n'a jamais été créée sur GitHub.

## Problèmes identifiés

### 1. CI workflow cible les mauvaises branches
- `ci.yml` trigger sur `push: [develop, main]` et `pull_request: [develop, main]`
- La branche `develop` n'existe peut-être pas sur le remote
- Vérifier et créer `develop` si nécessaire

### 2. Nouveau projet de tests non inclus
- `Vetolib.Tests.Integration` vient d'être créé mais n'est pas dans `ci.yml`
- Ajouter un job `backend-integration` qui lance ces tests (nécessite Docker pour Testcontainers)

### 3. SonarCloud
- Vérifier si `SONAR_TOKEN` est configuré dans les secrets GitHub
- Si non, documenter comment le configurer ou rendre le job optionnel (continue-on-error)

### 4. Branche sale
- `feat/back-patients-001` a ~180 fichiers modifiés non-commités
- Il faut un commit propre avant de pouvoir push et créer des PRs

## Étapes

1. Vérifier l'état des branches remote (`git branch -r`)
2. Créer `develop` depuis `main` si elle n'existe pas
3. Ajouter le job `backend-integration` dans `ci.yml` :
   ```yaml
   backend-integration:
     name: Backend — integration tests
     runs-on: ubuntu-latest
     timeout-minutes: 15
     needs: backend-build
     steps:
       - Checkout, Setup .NET 10, Cache, Restore
       - Build tests/Vetolib.Tests.Integration
       - Run with Docker socket for Testcontainers
   ```
4. Rendre SonarCloud non-bloquant si pas de token (`continue-on-error: true`)
5. Ajouter `backend-integration` dans les `needs` du `status-check`
6. Push la CI fixée et vérifier qu'elle passe

## Critère de complétion

```
□ CI passe sur GitHub Actions (au moins backend-build + frontend-build)
□ Job backend-integration ajouté et fonctionnel
□ SonarCloud non-bloquant si pas de token
□ Branche develop existe sur remote
□ Renommer en done
```
