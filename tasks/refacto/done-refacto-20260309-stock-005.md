# todo-refacto-20260309-stock-005 — StockMovement ne porte pas IMultiTenant
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Stock/Vetolib.Stock/Domain/StockMovement.cs`
- `src/backend/Modules/Stock/Vetolib.Stock/Infrastructure/StockMovementConfiguration.cs`
**Violation** : CLAUDE.md regle 4 — Multi-tenancy Global Query Filter. `StockMovement` herite `BaseEntity` mais pas `IMultiTenant`. Le `MultiTenantDbContext` n'applique le filtre `WHERE ClinicId = @current` que sur les entites implementant `IMultiTenant`. Les StockMovements ne sont donc pas isoles par tenant.
**Correction attendue** : Ajouter `IMultiTenant` sur `StockMovement`, ajouter la propriete `ClinicId`, la peupler dans `StockMovement.Create()` (ajouter le parametre `clinicId`), et mettre a jour la configuration EF et le handler.
**Critere** : `StockMovement` implemente `IMultiTenant` et possede un `ClinicId`.
