# todo-audit-arch-001.md — Audit architectural complet

**Module** : Cross-module
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : `ardalis-result`, `ardalis-modular-monolith`, `cqrs-mediatr`

---

## Contexte

Le projet a grandi rapidement (175+ tâches done, 10 modules). Il faut un audit complet pour s'assurer que l'architecture reste propre et cohérente.

**Référence** : s'inspirer du projet Vibora (`C:\Repos\Perso\vibora-monorepo`) qui est très bien structuré techniquement :
- Clean modular monolith avec isolation stricte
- Tests d'intégration WebApplicationFactory exemplaires
- Infrastructure bien factorisée (ServiceDefaults, cross-module communication)
- Patterns Result<T> + MediatR bien appliqués
- Conventions de nommage cohérentes

## Checks à effectuer

### 1. Result<T> partout — zéro throw
Scanner tous les modules pour :
- `throw new` dans les handlers → VIOLATION (sauf ArgumentException dans les constructeurs)
- `return null` dans les handlers → VIOLATION
- Méthodes Domain qui ne retournent pas `Result<T>` → VIOLATION
- Endpoints qui ne font pas `.ToMinimalApiResult()` → VIOLATION

```bash
# Patterns à chercher
grep -rn "throw new" src/backend/Modules/ --include="*.cs" | grep -v "Migrations" | grep -v "obj/"
grep -rn "return null" src/backend/Modules/ --include="*.cs" | grep -v "Migrations"
```

### 2. Isolation 2-assembly
Pour chaque module vérifier :
- L'assembly `.Contracts` ne contient QUE des interfaces, DTOs, events, enums (pas de handlers, pas de DbContext)
- L'assembly runtime n'a AUCUNE classe public sauf `ModuleServiceRegistrar`
- Aucun module ne référence le runtime d'un autre module (seulement `.Contracts`)

```bash
# Vérifier les classes publiques dans les assemblies runtime
grep -rn "public class\|public record\|public interface" src/backend/Modules/*/Vetolib.*/ --include="*.cs" | grep -v Contracts | grep -v Migrations | grep -v obj
```

### 3. Dead code
- Classes/méthodes jamais référencées
- Usings inutiles
- Fichiers `.cs` vides ou avec seulement un namespace

### 4. Conventions de nommage (comparer avec Vibora)
- DTOs : `{Entity}Dto` (pas `{Entity}Response` sauf pour les commandes)
- Commands : `{Verb}{Entity}Command` + `{Verb}{Entity}Handler`
- Queries : `{Verb}{Entity}Query` + `{Verb}{Entity}Handler`
- Endpoints : verbe HTTP correct, routes RESTful `/api/v1/{resource}`
- Validators : `{Command}Validator` avec FluentValidation

### 5. DbContext isolation
- Chaque module a son propre DbContext (pas de DbContext partagé)
- Les global query filters multi-tenant sont appliqués sur toutes les entités avec `ClinicId`
- Pas de `IgnoreQueryFilters()` en production (seulement dans les seeds/migrations/tests)

### 6. Performances (inspiré Vibora)
- Requêtes N+1 : `Include()` manquants dans les queries
- Projections : utilise `.Select()` plutôt que charger l'entité complète
- Pagination : tous les endpoints de liste supportent `skip/take`

## Output attendu

Créer des tâches `tasks/todo-refacto-*` pour chaque violation trouvée, classées par sévérité :
- **CRITIQUE** : violations Result<T>, throw dans handlers, classes publiques runtime
- **IMPORTANT** : dead code, N+1, conventions
- **MINEUR** : nommage, usings

## Critère de complétion

```
□ Scan Result<T> effectué — 0 violations ou tâches refacto créées
□ Scan isolation 2-assembly effectué
□ Scan dead code effectué
□ Scan conventions effectué
□ Scan DbContext effectué
□ Scan performances effectué
□ Rapport d'audit écrit dans progress.md
□ Tâches refacto créées pour chaque violation
□ Renommer en done
```
