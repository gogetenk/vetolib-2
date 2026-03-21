# todo-refacto-nightly-009 — AI: CheckInteractionsHandler > 200 lignes — extraire méthodes privées

**Module** : AI
**Priorité** : MOYENNE — maintenabilité
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

`CheckInteractionsHandler` fait 244 lignes. La méthode `Handle()` mélange plusieurs responsabilités : récupération du médicament via MediatR, chargement des prescriptions actives du patient, analyse des interactions (boucles), construction du résultat.

Fichier : `src/backend/Modules/AI/Vetolib.AI/Application/Queries/CheckInteractions/CheckInteractionsHandler.cs`

## Fix attendu

Extraire des méthodes privées pour réduire `Handle()` à < 50 lignes :

- `private static IReadOnlyList<InteractionAlert> DetectInteractions(DrugCatalogEntryDto newDrug, IEnumerable<ActivePrescriptionDto> activePrescriptions)`
  — logique d'analyse des interactions (pure, sans I/O, testable en isolation)
- `private static InteractionCheckResult BuildResult(DrugCatalogEntryDto drug, IReadOnlyList<InteractionAlert> alerts)`
  — construction du DTO résultat

Les méthodes restent `private` dans le même handler. Aucun nouveau service ne doit être créé.

## Critères de complétion

```
□ Méthode Handle() <= 50 lignes
□ Méthode DetectInteractions() extraite et statique (sans dépendances externes)
□ Aucun changement de comportement observable
□ dotnet build → 0 erreur
□ Tests unitaires AI toujours verts (si existants)
```
