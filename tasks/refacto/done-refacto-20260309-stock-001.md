# todo-refacto-20260309-stock-001 — StockMovement.Create() ne retourne pas Result<StockMovement>
**Priorite** : critique
**Fichiers concernes** : `src/backend/Modules/Stock/Vetolib.Stock/Domain/StockMovement.cs`
**Violation** : CLAUDE.md regle 1 — "Ardalis.Result PARTOUT — Domain inclus". Chaque methode qui peut echouer retourne `Result<T>` ou `Result`. Les factory methods domain doivent retourner `Result<T>`.
**Correction attendue** : Modifier `StockMovement.Create()` pour retourner `Result<StockMovement>` avec validation (StockItemId non vide, Quantity > 0, Reason non vide). Mettre a jour `RecordStockMovementHandler` pour propager le Result.
**Critere** : La methode `StockMovement.Create` retourne `Result<StockMovement>` et le handler propage correctement les erreurs.
