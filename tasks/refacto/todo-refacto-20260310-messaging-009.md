# todo-refacto-20260310-messaging-009 -- Endpoint /portal/categories duplique la logique d'auth magic link
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/Api/PortalEndpoints.cs` (lignes 62-85)
**Violation** : Le endpoint `GET /api/v1/portal/categories` fait sa propre verification de token (lecture query param, lookup IgnoreQueryFilters, validation) au lieu d'utiliser le `MagicLinkEndpointFilter` comme les autres endpoints du group portal. Cela duplique la logique d'auth, augmente la surface de bugs, et bypasse le `IPortalContext` standard.
**Correction attendue** :
1. Deplacer le endpoint `/categories` dans le group protege par `MagicLinkEndpointFilter` (ligne 87)
2. Utiliser `IPortalContext` pour acceder au clinicId/ownerId au lieu de re-lookup le token manuellement
3. Supprimer le code duplique de verification de token
**Critere** : Aucun endpoint portal ne fait de lookup de token en dehors de `MagicLinkEndpointFilter`
