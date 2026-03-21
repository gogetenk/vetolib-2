# todo-infra-semver-release-001 — Versioning SemVer + Release Notes automatiques

**Module** : Infrastructure
**Dependances** : aucune
**Priorite** : HAUTE

---

## Objectif

Mettre en place le versioning SemVer automatique basé sur les conventional commits, avec génération automatique de release notes et création de GitHub Release.

## Implementation

### 1. Workflow `.github/workflows/release.yml`

Déclenché quand un push arrive sur `main` (après merge de develop → main).

```yaml
on:
  push:
    branches: [main]
```

### 2. Versioning automatique avec conventional commits

Utiliser l'action `google-github-actions/release-please-action@v4` (ou alternative simple) :

**Option recommandée : release-please**
- Crée automatiquement une PR "Release" avec le changelog
- Bumpe la version selon les conventional commits :
  - `feat(...)` → minor bump (0.X.0)
  - `fix(...)` → patch bump (0.0.X)
  - `feat(...)!` ou `BREAKING CHANGE:` → major bump (X.0.0)
  - `chore`, `refactor`, `test`, `docs` → pas de bump
- Quand la PR est mergée → crée le tag + GitHub Release

### 3. Configuration release-please

Créer `.release-please-manifest.json` :
```json
{
  ".": "0.1.0"
}
```

Créer `release-please-config.json` :
```json
{
  "packages": {
    ".": {
      "release-type": "simple",
      "bump-minor-pre-major": true,
      "bump-patch-for-minor-pre-major": true,
      "changelog-sections": [
        {"type": "feat", "section": "Features"},
        {"type": "fix", "section": "Bug Fixes"},
        {"type": "refactor", "section": "Code Refactoring"},
        {"type": "perf", "section": "Performance"},
        {"type": "test", "section": "Tests"},
        {"type": "chore", "section": "Maintenance"},
        {"type": "docs", "section": "Documentation"}
      ]
    }
  }
}
```

### 4. Tag des images Docker

Quand une release est créée, tagger les images Docker avec le numéro de version :
- `ghcr.io/gogetenk/vetolib-2/vetolib-backend:v0.1.0`
- `ghcr.io/gogetenk/vetolib-2/vetolib-frontend:v0.1.0`

### 5. CHANGELOG.md

release-please génère automatiquement le CHANGELOG.md. Ne pas le créer manuellement.

## Règles

- Version initiale : 0.1.0 (pre-1.0, on est en développement)
- Le workflow release.yml est SÉPARÉ de ci.yml et deploy.yml
- Pas de tag manuel — tout est automatique via conventional commits
- Les release notes sont générées automatiquement, pas écrites à la main

## Critère

```
[] .github/workflows/release.yml créé
[] release-please configuré (manifest + config)
[] Version initiale 0.1.0
[] Conventional commits → bump automatique
[] Release notes générées automatiquement
[] Images Docker taguées avec la version
[] CHANGELOG.md généré automatiquement
[] Renommer en done
```
