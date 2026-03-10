# todo-refacto-20260309-back-003 — IgnoreQueryFilters in Auth handlers (Login, RefreshToken, ChangePassword)
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/Login/LoginHandler.cs` (ligne 26)
- `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/RefreshToken/RefreshTokenHandler.cs` (ligne 35)
- `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/ChangePassword/ChangePasswordHandler.cs` (ligne 20)
**Violation** : IgnoreQueryFilters utilise dans des handlers de production (hors seeds/migrations). Ces usages sont fonctionnellement justifies (login cross-tenant, refresh token lookup, change password par userId) mais violent la regle stricte de CLAUDE.md.
**Correction attendue** : Formaliser ces exceptions dans `disputes.md` avec justification : "Les endpoints d'authentification sont pre-tenant (l'utilisateur ne connait pas encore son clinic_id au login)". Alternativement, modeliser User et RefreshToken sans IMultiTenant (puisque l'auth est cross-tenant par nature) et retirer le filtre au niveau du modele.
**Critere** : Soit les entites User/RefreshToken ne sont plus IMultiTenant (et le filtre ne s'applique plus), soit l'exception est documentee dans disputes.md.
