# todo-refacto-20260309-back-001 — IgnoreQueryFilters usage in Auth handlers
**Priorite** : importante
**Fichiers concernes** :
- `src/Modules/Auth/Vetolib.Auth/Application/Commands/Login/LoginHandler.cs` (ligne 26)
- `src/Modules/Auth/Vetolib.Auth/Application/Commands/RefreshToken/RefreshTokenHandler.cs` (ligne 35)
**Violation** : Regle 4 (Multi-tenancy) — `IgnoreQueryFilters()` interdit sauf seeds/migrations
**Contexte** : Login et RefreshToken utilisent `IgnoreQueryFilters()` pour trouver les users cross-tenant. C'est justifie fonctionnellement (pre-authentification, on ne connait pas encore le tenant), mais viole la regle stricte du CLAUDE.md.
**Correction attendue** :
- Option A : Extraire User/RefreshToken dans un DbContext separe sans global query filter (AuthIdentityDbContext) dedie a l'authentification pre-tenant.
- Option B : Documenter dans `disputes.md` que Login/RefreshToken sont des exceptions acceptees.
- Recommandation : Option B dans l'immediat, Option A si d'autres usages emergent.
**Critere** : [] grep `IgnoreQueryFilters` dans Modules/ ne retourne plus de resultats OU une entree disputes.md documente l'exception
