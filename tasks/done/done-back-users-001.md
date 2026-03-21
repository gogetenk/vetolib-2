# todo-back-users-001.md — Backend : Gestion utilisateurs clinique

**Module** : Auth (extension)
**Dépendances** : done-back-auth-001
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`, `aspnet-minimal-api`
**Gherkins** : `features/users/users.feature`

---

## Périmètre exact

Endpoints ADMIN uniquement dans `Modules/Auth/` :

- `GET /api/users` — liste les utilisateurs de la clinique
- `POST /api/users` — inviter un nouvel utilisateur
- `PATCH /api/users/{id}/role` — changer le rôle
- `DELETE /api/users/{id}` — désactiver un utilisateur (soft delete, pas suppression physique)

## Règles métier

- Seul ADMIN peut accéder à ces endpoints → 403 pour VET/ASSISTANT/RECEPTIONIST
- Un ADMIN ne peut pas se désactiver lui-même
- Un ADMIN ne peut pas rétrograder son propre rôle
- Invitation : crée le compte avec un mot de passe temporaire aléatoire (8 chars)
  et retourne le mot de passe dans la réponse (à afficher une seule fois)
- Pas d'envoi d'email (hors MVP) — le mot de passe est communiqué manuellement
- Soft delete : `is_active = false`, le compte ne peut plus se connecter

## Contracts à ajouter dans `Vetolib.Auth.Contracts/`

```csharp
public record UserListItemDto(Guid Id, string Email, string FullName, UserRole Role, bool IsActive);
public record InviteUserRequest(string Email, string FullName, UserRole Role);
public record ChangeRoleRequest(UserRole NewRole);
```

## Commands/Queries à créer

- `InviteUserCommand` → `Result<InviteUserResponse>` (avec mot de passe temporaire)
- `ChangeUserRoleCommand` → `Result`
- `DeactivateUserCommand` → `Result`
- `ListUsersQuery` → `Result<IReadOnlyList<UserListItemDto>>`

## Critère de complétion

```
□ Bindings Reqnroll ROUGES avant implémentation
□ ADMIN peut lister, inviter, changer rôle, désactiver
□ VET/ASSISTANT/RECEPTIONIST → 403 sur tous ces endpoints
□ Un ADMIN ne peut pas se désactiver lui-même → 400
□ Tests unitaires dans Vetolib.Auth.Tests.Unit/
□ dotnet build → 0 erreur
□ Renommer en done-back-users-001.md
```
