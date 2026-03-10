# todo-refacto-20260309-stock-004 — ListStockItemsHandler charge tout en memoire avant filtrage
**Priorite** : importante
**Fichiers concernes** : `src/backend/Modules/Stock/Vetolib.Stock/Application/Queries/ListStockItems/ListStockItemsHandler.cs`
**Violation** : Performance — `ToListAsync()` suivi de `Where()` en memoire. Le filtre Category peut etre pousse en SQL. Les filtres LowStock et ExpiringSoon necessitent une comparaison sur les colonnes Quantity/MinThreshold et ExpiryDate qui peuvent aussi etre des predicats IQueryable.
**Correction attendue** : Construire l'IQueryable avec predicats conditionnels avant d'appeler `ToListAsync()`. Exemple : `query = query.Where(i => i.Category == cat)` au lieu de charger puis filtrer.
**Critere** : `ToListAsync()` est appele une seule fois a la fin de la chaine de filtres IQueryable.
