# todo-refacto-nightly-002 — Stock: AsNoTracking manquant sur les 3 query handlers

**Module** : Stock
**Priorité** : MOYENNE — perf
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

Les 3 query handlers du module Stock n'utilisent pas `.AsNoTracking()`. EF Core charge les entités dans le change tracker inutilement alors que ces handlers ne font aucune modification.

Fichiers concernés :
- `src/backend/Modules/Stock/Vetolib.Stock/Application/Queries/ListStockItems/ListStockItemsHandler.cs`
- `src/backend/Modules/Stock/Vetolib.Stock/Application/Queries/GetStockAlerts/GetStockAlertsHandler.cs`
- `src/backend/Modules/Stock/Vetolib.Stock/Application/Queries/CheckStockAvailability/CheckStockAvailabilityHandler.cs`

## Fix attendu

Ajouter `.AsNoTracking()` sur chaque query EF dans ces 3 handlers, après `.AsQueryable()` ou directement sur le DbSet avant le `.Where()`.

Exemple :
```csharp
// Avant
var items = await _context.StockItems.Where(...).ToListAsync(ct);

// Après
var items = await _context.StockItems.AsNoTracking().Where(...).ToListAsync(ct);
```

## Critères de complétion

```
□ AsNoTracking() présent sur toutes les queries EF des 3 handlers
□ dotnet build → 0 erreur
□ Tests acceptance Stock toujours verts (si existants)
```
