# todo-test-auth-handlers-001 — Unit tests for Auth command handlers

**Module** : Auth
**Priorité** : HAUTE
**Skills à lire** : `skills/ardalis-result/SKILL.md`, `skills/cqrs-mediatr/SKILL.md`

---

## Contexte

Le module Auth contient des handlers critiques pour la sécurité, sans aucun test unitaire :

- `LoginHandler` — logique JWT, hachage de mot de passe
- `InviteUserHandler` — publication d'event, email
- `ChangeUserRoleHandler` — RBAC, transitions de rôle
- `DeactivateUserHandler` — désactivation soft-delete

Seuls `UserDomainTests.cs` et `RegisterClinicValidatorTests.cs` existent actuellement.

## Travail à faire

Créer `tests/Vetolib.Tests.Unit/Auth/LoginHandlerTests.cs` couvrant :
- Happy path : token JWT retourné, `Result.Success`
- Email inexistant : `Result.NotFound`
- Mot de passe incorrect : `Result.Invalid`
- Utilisateur désactivé : `Result.Invalid` avec message dédié

Créer `tests/Vetolib.Tests.Unit/Auth/ChangeUserRoleHandlerTests.cs` couvrant :
- Promotion réussie (Receptionist → Vet)
- Utilisateur introuvable : `Result.NotFound`
- Rôle identique (no-op ou `Result.Invalid` selon règle métier)

Créer `tests/Vetolib.Tests.Unit/Auth/DeactivateUserHandlerTests.cs` couvrant :
- Désactivation réussie : `IsActive = false`
- Utilisateur déjà désactivé : `Result.Invalid`
- Utilisateur introuvable : `Result.NotFound`

## Contraintes techniques

- NSubstitute pour `IJwtTokenService`, `IEmailSender`, `IPublisher`
- InMemory EF Core avec ClinicId fixe `11111111-1111-1111-1111-111111111111`
- Ne pas tester la génération JWT elle-même (déjà couverte par `JwtTokenService`)
- Zéro Testcontainers dans les tests unitaires

## Critères de complétion

```
□ LoginHandlerTests.cs créé avec minimum 4 scénarios
□ ChangeUserRoleHandlerTests.cs créé avec minimum 3 scénarios
□ DeactivateUserHandlerTests.cs créé avec minimum 3 scénarios
□ dotnet test tests/Vetolib.Tests.Unit/ → 0 erreur, 0 échec
```
