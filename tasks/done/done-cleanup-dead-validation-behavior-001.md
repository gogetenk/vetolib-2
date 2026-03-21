# todo-cleanup-dead-validation-behavior-001.md

**Module** : Agenda, Auth, Billing
**Priorité** : BASSE
**Skills à lire** : aucun

---

## Objectif

Supprimer 3 fichiers `ValidationBehavior.cs` locaux qui sont du code mort.

## Contexte

Les modules Agenda, Auth et Billing possèdent chacun une copie locale de `ValidationBehavior.cs` :

- `src/backend/Modules/Agenda/Vetolib.Agenda/Application/Behaviors/ValidationBehavior.cs`
- `src/backend/Modules/Auth/Vetolib.Auth/Application/Behaviors/ValidationBehavior.cs`
- `src/backend/Modules/Billing/Vetolib.Billing/Application/Behaviors/ValidationBehavior.cs`

Ces fichiers ne sont **jamais utilisés**. Chaque `ModuleServiceRegistrar` importe
`using Vetolib.Shared.Infrastructure.Behaviors;` et enregistre la version partagée.
La seule référence à ces namespaces locaux provient des fichiers eux-mêmes (déclaration `namespace`).

La version partagée se trouve dans :
`src/backend/Shared/Vetolib.Shared.Infrastructure/Behaviors/ValidationBehavior.cs`

## Actions

1. Supprimer les 3 fichiers locaux listés ci-dessus.
2. Supprimer les dossiers `Application/Behaviors/` s'ils deviennent vides.
3. Vérifier `dotnet build src/backend/Vetolib.sln` — 0 erreurs attendues.

## Critères de complétion

```
□ 3 fichiers ValidationBehavior.cs locaux supprimés
□ dotnet build → 0 erreurs
□ dotnet test Tests/Vetolib.Tests.Acceptance/ → passe (ou était déjà en échec pour d'autres raisons)
```
