# todo-infra-cicd-001.md — CI/CD GitHub Actions

**Module** : Infrastructure
**Dépendances** : done-back-migrations-001
**Skills à lire** : aucun skill spécifique

---

## Contexte

Aucune automatisation CI/CD. Les tests ne tournent que localement. Pour un produit vendable, chaque push doit être validé automatiquement.

## Périmètre exact

### 1. Workflow principal — `.github/workflows/ci.yml`

Déclenché sur : push main + PR vers main

```yaml
jobs:
  backend-build:
    runs-on: ubuntu-latest
    steps:
      - dotnet restore
      - dotnet build --no-restore
      - dotnet test (unit tests uniquement, pas Testcontainers en CI pour l'instant)

  backend-bdd:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_USER: test
          POSTGRES_PASSWORD: test
          POSTGRES_DB: vetolib_test
        ports: ["5432:5432"]
    steps:
      - dotnet test --filter "Category=Acceptance"
      # Les tests Reqnroll utilisent Testcontainers,
      # mais en CI on peut aussi utiliser le service postgres directement

  frontend-build:
    runs-on: ubuntu-latest
    steps:
      - npm ci
      - npm run lint
      - npm run build
      # TypeScript compilation = validation des types

  frontend-e2e:
    runs-on: ubuntu-latest
    steps:
      - npm ci
      - npx playwright install --with-deps chromium
      - npm run test:e2e
      # Tests contre MSW (pas besoin du backend en CI)

  docker-build:
    runs-on: ubuntu-latest
    needs: [backend-build, frontend-build]
    steps:
      - docker compose build
      # Vérifie que les images Docker se construisent
```

### 2. Workflow de déploiement — `.github/workflows/deploy.yml`

Déclenché sur : push main (après CI vert)

Pour l'instant, juste builder et pusher les images Docker :
```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - Login to GitHub Container Registry (ghcr.io)
      - docker compose build
      - docker push ghcr.io/$REPO/vetolib-backend:latest
      - docker push ghcr.io/$REPO/vetolib-frontend:latest
```

Le déploiement réel (Cloud Run, Kubernetes, VPS) sera configuré plus tard selon l'hébergeur choisi.

### 3. Protection de branche

Documenter dans le README les settings GitHub recommandés :
- main : protected branch
- Require status checks (ci.yml) before merge
- Require PR review (au moins 1)

## Critère de complétion

```
□ .github/workflows/ci.yml créé et fonctionnel
□ Backend build + unit tests passent en CI
□ Frontend build + lint passent en CI
□ Playwright tests (MSW) passent en CI
□ Docker images se construisent en CI
□ .github/workflows/deploy.yml créé (push images)
□ Renommer en done-infra-cicd-001.md
```
