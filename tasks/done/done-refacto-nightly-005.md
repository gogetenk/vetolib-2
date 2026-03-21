# todo-refacto-nightly-005 — Auth: GetOnboardingStateHandler > 80 lignes

**Module** : Auth
**Priorité** : MOYENNE — maintenabilité
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

`GetOnboardingStateHandler` fait 120 lignes. La méthode `Handle()` contient une logique de calcul de l'état d'onboarding en plusieurs étapes (comptage d'entités, évaluation de conditions) qui devrait être extraite.

Fichier : `src/backend/Modules/Auth/Vetolib.Auth/Application/Queries/GetOnboardingState/GetOnboardingStateHandler.cs`

## Fix attendu

Extraire des méthodes privées pour réduire `Handle()` à < 40 lignes :

- `private async Task<OnboardingDataSnapshot> LoadSnapshotAsync(Guid clinicId, CancellationToken ct)` — toutes les queries EF regroupées
- `private static OnboardingStateDto ComputeState(OnboardingDataSnapshot snapshot)` — logique de calcul pure (testable sans DB)

`OnboardingDataSnapshot` peut être un record interne à la classe ou dans le même namespace.

## Critères de complétion

```
□ Méthode Handle() <= 40 lignes
□ Logique de calcul extraite dans une méthode statique pure (sans I/O)
□ Aucun changement de comportement observable
□ dotnet build → 0 erreur
□ Tests unitaires Auth toujours verts
```
