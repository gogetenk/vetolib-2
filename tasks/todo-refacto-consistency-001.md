# todo-refacto-consistency-001.md — Harmoniser conventions et consistency

**Module** : Cross-module
**Dépendances** : todo-audit-arch-001
**Priorité** : MOYENNE
**Skills à lire** : `ardalis-result`, `aspnet-minimal-api`

---

## Objectif

Harmoniser les conventions de nommage, le format des réponses d'erreur, et la structure des DTOs à travers tous les modules.

## Checks

### 1. DTOs
- Tous les DTOs suivent le pattern `{Entity}Dto` (pas `{Entity}Response`, `{Entity}Model`, etc.)
- Les DTOs de réponse de commande utilisent `{Verb}{Entity}Response` uniquement si différent du DTO standard
- Tous les DTOs sont dans `.Contracts` (jamais dans le runtime)

### 2. Endpoints
- Routes RESTful cohérentes : `/api/v1/{resource}` (pluriel)
- Verbes HTTP corrects (GET=lecture, POST=création, PUT=modification, DELETE=suppression)
- Tous les endpoints retournent `ToMinimalApiResult()` — pas de `Results.Ok()` / `Results.NotFound()` manuels
- Tags Swagger/OpenAPI cohérents

### 3. Format d'erreurs
- Toutes les erreurs passent par `Result.Error()` / `Result.NotFound()` / `Result.Invalid()`
- Pas de `ProblemDetails` manuels
- Les messages d'erreur sont en anglais (UAE market)

### 4. Validators
- Chaque Command a un Validator FluentValidation
- Les messages de validation sont descriptifs et en anglais
- Pas de validation dans les handlers (uniquement dans les validators via le behavior pipeline)

## Critère de complétion

```
□ Audit DTOs — tous conformes ou tâches créées
□ Audit endpoints — routes et verbes cohérents
□ Audit erreurs — format uniforme
□ Audit validators — tous présents et conformes
□ Renommer en done
```
