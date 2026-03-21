# todo-refacto-nightly-001 — Messaging: pagination SQL-side dans ListConversations

**Module** : Messaging
**Priorité** : HAUTE — correctness + perf
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

`ListConversationsHandler` charge TOUTES les conversations en mémoire via `.ToListAsync(ct)` à la ligne 106, puis applique `.Skip()/.Take()` en LINQ-to-Objects. Résultat : toutes les conversations de la clinique sont chargées en RAM à chaque requête, la pagination n'est jamais faite côté SQL.

Fichier : `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/ListConversations/ListConversationsHandler.cs`

**Violation secondaire** : pas de `.AsNoTracking()` sur la query.

## Fix attendu

1. Déplacer `.Skip()/.Take()` avant `.ToListAsync()` pour que la pagination soit faite par PostgreSQL.
2. Supprimer le tri in-memory sur `priorityOrder` (Dictionary) — le traduire en `.OrderBy()` EF-compatible (ex: CASE WHEN via `switch expression` sur `c.Category`).
3. Ajouter `.AsNoTracking()` après le `.Include()`.

## Critères de complétion

```
□ .Skip() et .Take() appelés avant .ToListAsync()
□ .AsNoTracking() présent sur la query
□ dotnet build → 0 erreur
□ Tests acceptance Messaging toujours verts
```
