# todo-infra-cd-pipeline-001 — Pipeline CD staging + prod (mock deploy)

**Module** : Infrastructure
**Dependances** : aucune
**Priorite** : HAUTE

---

## Objectif

Refondre `.github/workflows/deploy.yml` pour avoir un pipeline CD professionnel avec environnements staging et prod, en mockant le déploiement réel pour l'instant.

## Ce qui existe déjà

Le fichier `.github/workflows/deploy.yml` existe avec un simple push d'images Docker sur GHCR quand on push sur main. Pas d'environnements, pas de staging.

## Modifications requises

### 1. Flow CD complet

```
develop (CI) → merge PR → main → staging auto → approval gate → prod
```

### 2. Triggers

```yaml
on:
  push:
    branches: [main]
  workflow_dispatch:
    inputs:
      environment:
        description: 'Target environment'
        required: true
        default: 'staging'
        type: choice
        options:
          - staging
          - production
```

### 3. Jobs

#### Job 1 : `build-and-push` (existant, garder)
- Build + push images Docker sur GHCR
- Tagger avec `latest`, `sha-{commit}`, et la version SemVer si tag existe

#### Job 2 : `deploy-staging`
- **Environment** : `staging` (configurer dans GitHub repo settings)
- Needs : `build-and-push`
- Se déclenche automatiquement sur push main
- Steps mockés :
  ```yaml
  - name: Deploy to staging
    run: |
      echo "🚀 Deploying to staging..."
      echo "Backend image: ${{ needs.build-and-push.outputs.backend_image }}:${{ github.sha }}"
      echo "Frontend image: ${{ needs.build-and-push.outputs.frontend_image }}:${{ github.sha }}"
      echo "TODO: Replace with real deployment command (kubectl apply / docker-compose up / terraform apply)"
      echo "✅ Staging deployment complete (mock)"
  ```
- Ajouter un step de smoke test mocké :
  ```yaml
  - name: Smoke test staging
    run: |
      echo "🔍 Running smoke tests against staging..."
      echo "TODO: curl https://staging.vetolib.ae/health"
      echo "TODO: curl https://staging-api.vetolib.ae/health"
      echo "✅ Smoke tests passed (mock)"
  ```

#### Job 3 : `deploy-production`
- **Environment** : `production` (avec required reviewers dans GitHub settings)
- Needs : `deploy-staging`
- Steps mockés identiques mais pour prod
- Ajouter un step de notification :
  ```yaml
  - name: Notify deployment
    run: |
      echo "📢 Production deployment notification"
      echo "Version: ${{ github.sha }}"
      echo "Deployed by: ${{ github.actor }}"
      echo "TODO: Send Slack/Teams notification"
  ```

### 4. Outputs du job build-and-push

Exporter les noms d'images pour les jobs suivants :
```yaml
outputs:
  backend_image: ${{ steps.meta.outputs.backend_image }}
  frontend_image: ${{ steps.meta.outputs.frontend_image }}
  version: ${{ steps.meta.outputs.sha_tag }}
```

### 5. Rollback step (mocké)

Ajouter un job `rollback` qui peut être déclenché manuellement via workflow_dispatch :
```yaml
rollback:
  if: github.event.inputs.environment == 'production' && failure()
  ...
```

## Règles

- Ne PAS toucher au code applicatif ni à ci.yml
- Tous les steps de déploiement sont mockés avec des echo
- Les vrais déploiements seront ajoutés quand l'infra sera prête
- Utiliser les GitHub Environments pour staging et production
- Production nécessite une approbation manuelle (configurer dans les settings du repo)

## Critère

```
[] deploy.yml refait avec staging + prod
[] Environment staging (auto-deploy sur push main)
[] Environment production (manual approval gate)
[] Smoke tests mockés
[] Notification mockée
[] workflow_dispatch pour déploiement manuel
[] Rollback mocké
[] Outputs images partagés entre jobs
[] Renommer en done
```
