# todo-refacto-20260309-stock-002 — Domain events StockLowEvent et StockExpiringEvent absents
**Priorite** : critique
**Fichiers concernes** :
- `src/backend/Modules/Stock/Vetolib.Stock.Contracts/` (events a creer)
- `src/backend/Modules/Stock/Vetolib.Stock/Application/Commands/RecordStockMovement/RecordStockMovementHandler.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/` (consumers a creer)
**Violation** : Task spec done-back-stock-management-001 exige `StockLowEvent` (notification quand qty < MinThreshold) et `StockExpiringEvent` (notification 30 jours avant expiry). Aucun de ces events n'existe.
**Correction attendue** :
1. Creer `StockLowEvent` et `StockExpiringEvent` dans `Vetolib.Stock.Contracts`
2. Publier `StockLowEvent` dans `RecordStockMovementHandler` quand `item.IsLowStock` apres mouvement
3. Creer un background service ou scheduled job pour publier `StockExpiringEvent` pour items expirant dans 30 jours
4. Creer les consumers MassTransit dans le module Notifications
**Critere** : `grep -r "StockLowEvent\|StockExpiringEvent" src/backend/` retourne des resultats dans Contracts, Handler et Consumer.
