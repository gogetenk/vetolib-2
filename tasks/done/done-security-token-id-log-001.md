# todo-security-token-id-log-001 — TokenId loggué dans ChangePasswordHandler

**Module** : Auth
**Priorité** : BASSE
**Skills à lire** : aucun

---

## Problème détecté

Dans `src/backend/Modules/Auth/Vetolib.Auth/Application/Commands/ChangePassword/ChangePasswordHandler.cs`,
le log suivant est émis :

```csharp
_logger.LogWarning("ChangePasswordHandler: failed to revoke token {TokenId} for user {UserId} — {Errors}",
```

Le `TokenId` est un identifiant de refresh token. Le logger Serilog avec le sink OpenTelemetry
exporte ces logs vers des backends d'observabilité (Seq, OTLP, etc.). Si un attaquant obtient
l'accès aux logs, il peut énumérer des identifiants de tokens.

Bien que le `TokenId` seul ne suffise pas pour rejouer un token (le secret reste dans la DB),
sa présence dans les logs est une surface inutile et va à l'encontre du principe de
minimisation des données dans les logs (PDPL UAE / GDPR équivalent).

## Correction requise

Remplacer le `TokenId` dans le message de log par une forme tronquée ou redactée :

```csharp
// Avant
_logger.LogWarning("ChangePasswordHandler: failed to revoke token {TokenId} for user {UserId} — {Errors}",
    tokenId, userId, errors);

// Après — tronquer à 8 chars pour la tracabilité sans exposer l'ID complet
_logger.LogWarning("ChangePasswordHandler: failed to revoke token {TokenIdPrefix} for user {UserId} — {Errors}",
    tokenId.ToString()[..8] + "...", userId, errors);
```

Alternativement, supprimer `TokenId` du log si la tracabilité n'est pas requise.

## Critères de complétion

```
□ TokenId n'apparaît plus en clair dans les logs de ChangePasswordHandler
□ dotnet build → 0 erreur
□ Tests unitaires ChangePassword restent GREEN
```
