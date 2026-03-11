# todo-security-cors-policy-001 — CORS absent : aucune politique explicite configurée

**Module** : Vetolib.Api (host)
**Priorité** : MOYENNE
**Skills à lire** : aucun
**Flag** : MODIF_SHARED: non — concerne uniquement Vetolib.Api/Program.cs

---

## Problème détecté

Le scan complet du backend ne trouve aucun appel à `AddCors`, `UseCors`, ni `WithOrigins`
dans aucun fichier `.cs`. Il n'y a donc aucune politique CORS explicitement configurée.

En l'absence de politique CORS :
- Les requêtes cross-origin depuis le frontend (Next.js sur `localhost:3000` ou le domaine
  de prod) seront bloquées par les navigateurs pour les requêtes avec credentials (cookies,
  Authorization header).
- En production, si le frontend et l'API sont sur des domaines différents, les appels API
  échoueront silencieusement côté browser (CORS error).
- Aucune liste blanche d'origines autorisées → si jamais un middleware ajoute des headers
  permissifs par défaut, le vecteur est ouvert.

## Correction requise

Dans `Vetolib.Api/Program.cs` (fichier gelé — nécessite flag MODIF_GELE ou intervention
architecte), ajouter :

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("VetolibFrontend", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
                ?? ["http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Et dans le pipeline :
app.UseCors("VetolibFrontend");
```

L'origine de production doit être configurée dans `appsettings.Production.json` sous
`Cors:AllowedOrigins`.

## Critères de complétion

```
□ AddCors avec policy nommée "VetolibFrontend" dans Program.cs
□ WithOrigins lit depuis la configuration (pas hardcodé)
□ UseCors("VetolibFrontend") dans le pipeline, avant UseAuthentication
□ appsettings.Production.json contient Cors:AllowedOrigins
□ dotnet build → 0 erreur
□ Les tests d'acceptance existants restent GREEN
```
