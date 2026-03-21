# todo-cleanup-drug-catalog-seed-001.md

**Module** : MedicalRecords
**Priorité** : BASSE
**Skills à lire** : aucun

---

## Objectif

Extraire le catalogue de médicaments de `DrugCatalogSeedData.cs` (848 lignes) vers une migration EF Core dédiée.

## Contexte

Le fichier suivant contient 848 lignes de données de catalogue médicaments hardcodées en C# :

`src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Infrastructure/DrugCatalogSeedData.cs`

Ce pattern pose plusieurs problèmes :
- Le fichier est chargé à chaque démarrage de l'application (appel à `HasData` ou seed manuel)
- Les modifications du catalogue nécessitent une recompilation
- Le fichier est difficile à maintenir et à versionner proprement

## Actions

1. Analyser comment `DrugCatalogSeedData` est appelé (dans `OnModelCreating` ou dans un `DbInitializer`).
2. Si appelé via `HasData` dans `OnModelCreating` : le laisser tel quel (EF Core gère ça correctement via migrations).
3. Si appelé via un seed manuel au démarrage : envisager de le déplacer dans une migration EF Core
   ou dans un fichier JSON chargé une seule fois avec un flag `IsSeedApplied`.
4. Si le fichier reste en C# : le découper en régions ou fichiers partiels par catégorie thérapeutique
   pour faciliter la maintenance.

## Note

Ne pas supprimer les données — uniquement réorganiser leur chargement.
Si la solution est `HasData` dans EF Core, aucune action n'est nécessaire (c'est le bon pattern).

## Critères de complétion

```
□ Méthode de chargement du catalogue documentée (commentaire dans le fichier ou PR description)
□ Si refactoring effectué : dotnet build → 0 erreurs
□ Si migration créée : dotnet ef database update fonctionne
```
