# todo-refacto-20260309-stock-003 — UpdateStockItemValidator manquant
**Priorite** : importante
**Fichiers concernes** : `src/backend/Modules/Stock/Vetolib.Stock/Application/Commands/UpdateStockItem/` (fichier a creer)
**Violation** : CLAUDE.md stack — FluentValidation requis. Les commands CreateStockItem et RecordStockMovement ont des validators, mais UpdateStockItem n'en a pas. Le ValidationBehavior pipeline ne valide pas cette commande.
**Correction attendue** : Creer `UpdateStockItemValidator` avec regles : StockItemId non vide, au moins un champ (Name ou MinThreshold) non null, MinThreshold >= 0 si present.
**Critere** : Le fichier `UpdateStockItemValidator.cs` existe et est enregistre via l'assembly scan.
