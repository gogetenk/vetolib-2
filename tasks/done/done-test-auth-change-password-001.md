# todo-test-auth-change-password-001.md — Endpoint ChangePassword + Gherkin

**Module** : Auth
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`, `reqnroll-bindings`

---

## Contexte

L'audit `done-audit-coverage-001.md` a révélé que `ChangePasswordCommand` est entièrement implémentée :
- `Application/Commands/ChangePassword/ChangePasswordCommand.cs`
- `Application/Commands/ChangePassword/ChangePasswordHandler.cs`
- `Application/Commands/ChangePassword/ChangePasswordValidator.cs`
- `Vetolib.Auth.Contracts/ChangePasswordRequest.cs`

Mais **aucun endpoint n'est exposé** dans `AuthEndpoints.cs` ou `UserEndpoints.cs`, et il n'existe **aucun scénario Gherkin**.

## Travail à faire

### Étape 1 — Écrire les scénarios Gherkin d'abord

Créer `tests/Vetolib.Tests.Acceptance/Features/Auth/ChangePassword.feature` :

```gherkin
Feature: Change password
  As a logged-in user
  I want to change my password
  So that I can maintain account security

  Background:
    Given une clinique "Happy Paws"

  Scenario: Successful password change
    Given I am authenticated as VET with password "Secure@1234567!"
    When I change my password from "Secure@1234567!" to "NewSecure@7654321!"
    Then the response status is 200
    And I can log in with the new password "NewSecure@7654321!"

  Scenario: Wrong current password is rejected
    Given I am authenticated as VET with password "Secure@1234567!"
    When I change my password from "WrongPassword@!" to "NewSecure@7654321!"
    Then the response status is 400
    And the error code is "INVALID_CURRENT_PASSWORD"

  Scenario: Weak new password is rejected
    Given I am authenticated as VET with password "Secure@1234567!"
    When I change my password from "Secure@1234567!" to "weak"
    Then the response status is 400

  Scenario: Unauthenticated request is rejected
    When I send a change password request without authentication
    Then the response status is 401
```

### Étape 2 — Bindings Reqnroll (RED d'abord)

Créer `tests/Vetolib.Tests.Acceptance/StepDefinitions/Auth/ChangePasswordSteps.cs`

Vérifier que les tests sont ROUGES avant d'implémenter l'endpoint.

### Étape 3 — Exposer l'endpoint

Fichier : `src/backend/Modules/Auth/Vetolib.Auth/Api/AuthEndpoints.cs`

Ajouter dans le groupe `authGroup` (authentifié) :

```csharp
authGroup.MapPost("/change-password", ChangePassword)
    .WithName("ChangePassword");
```

Route finale : `POST /api/auth/change-password`

### Étape 4 — Vérifier GREEN

```bash
dotnet test tests/Vetolib.Tests.Acceptance/ --filter "Feature[Auth]"
dotnet build src/backend/Vetolib.sln
```

## Règles métier

- Le mot de passe actuel doit être vérifié (non juste un token valide)
- Le nouveau mot de passe doit respecter la politique : min 10 caractères + 1 caractère spécial
- Après changement, l'ancien mot de passe ne doit plus fonctionner
- Si `MustChangePassword = true`, le flag doit être mis à `false` après changement réussi

## Critères de complétion

```
[ ] ChangePassword.feature créé avec 4 scénarios minimum
[ ] ChangePasswordSteps.cs créé et tests ROUGES confirmés
[ ] POST /api/auth/change-password exposé et fonctionnel
[ ] Tous les scénarios VERTS
[ ] dotnet build -> 0 erreur
```
