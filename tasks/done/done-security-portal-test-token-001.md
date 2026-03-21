# todo-security-portal-test-token-001 — Endpoint test-token non protégé en environnements non-Production

**Module** : Messaging
**Priorité** : HAUTE
**Skills à lire** : `aspnet-minimal-api`

---

## Problème détecté

L'endpoint `POST /api/v1/portal/test-token` dans `PortalEndpoints.cs` est accessible sans
authentification dans tous les environnements sauf Production (`env.IsProduction()`).

```csharp
app.MapPost("/api/v1/portal/test-token", async (..., IHostEnvironment env) =>
{
    if (env.IsProduction())
        return Results.NotFound();
    // ... crée un OwnerPortalToken valide avec consentement auto-accepté
})
```

Si le backend est déployé en staging, UAT, ou preview (où `IsProduction()` retourne `false`),
n'importe qui peut appeler cet endpoint sans credentials et obtenir un token de portail valide.
Ce token donne accès à toutes les conversations et données d'un owner fictif — mais le
`ClinicId` est hardcodé (`11111111-...`) donc le risque est limité aux données de test.

Le risque réel est la possibilité d'injecter des tokens valides en base d'un environnement
partagé, ce qui peut polluer les données de test et tromper les assertions BDD.

## Correction recommandée

Deux options :
- **Option A** : Ajouter `RequireAuthorization("Admin")` sur cet endpoint (seul un admin peut
  générer des tokens de test). Simple, préserve la fonctionnalité BDD.
- **Option B** : Conditionner l'enregistrement de la route à `env.IsDevelopment() || env.IsEnvironment("Test")`
  de sorte que la route n'existe pas du tout en staging/UAT.

Option B préférable : l'endpoint disparaît en staging, le problème est éliminé structurellement.

## Critères de complétion

```
□ L'endpoint /portal/test-token n'est enregistré que si IsDevelopment() || IsEnvironment("Test")
□ Les tests BDD Messaging passent toujours (Testcontainers lance en env "Test")
□ dotnet build → 0 erreur
```
