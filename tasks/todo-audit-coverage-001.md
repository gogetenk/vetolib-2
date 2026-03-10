# todo-audit-coverage-001.md — Audit couverture Gherkin vs endpoints

**Module** : Cross-module
**Dépendances** : aucune
**Priorité** : MOYENNE
**Skills à lire** : `reqnroll-bindings`

---

## Objectif

Comparer les scénarios Gherkin existants avec les endpoints API réels pour identifier les trous de couverture.

## Étapes

### 1. Lister tous les endpoints API
Scanner tous les fichiers `*Endpoints.cs` dans chaque module pour extraire les routes :
```bash
grep -rn "Map(Get\|Post\|Put\|Delete\|Patch)" src/backend/Modules/ --include="*.cs"
```

### 2. Lister tous les scénarios Gherkin
Scanner tous les fichiers `.feature` :
```bash
find tests/ -name "*.feature" -exec grep -l "Scenario" {} \;
```

### 3. Croiser les deux listes
Pour chaque endpoint, vérifier qu'au moins un scénario Gherkin le couvre.

### 4. Identifier les trous
- Endpoints sans scénario → créer un `todo-test-*` pour chaque
- Scénarios qui testent des endpoints inexistants → supprimer ou marquer @wip

### 5. Vérifier les edge cases
Pour chaque endpoint couvert :
- Happy path testé ?
- Validation error testé (400) ?
- Not found testé (404) ?
- Unauthorized testé (401/403) ?
- Tenant isolation testé ?

## Critère de complétion

```
□ Liste complète des endpoints API (par module)
□ Liste complète des scénarios Gherkin (par module)
□ Matrice de couverture endpoint ↔ scénario
□ Tâches créées pour les trous de couverture
□ Rapport dans progress.md
□ Renommer en done
```
