# todo-refacto-nightly-007 — Messaging: AsNoTracking manquant sur GetConversationById et GetOwnerConversationById

**Module** : Messaging
**Priorité** : MOYENNE — perf
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

Deux query handlers du module Messaging lisent des conversations (avec `.Include(c => c.Messages)`) sans `.AsNoTracking()`. Ces handlers ne modifient jamais les entités chargées, mais EF Core les maintient dans le change tracker inutilement.

Fichiers concernés :

- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/GetConversationById/GetConversationByIdHandler.cs` (122 lignes)
  — charge `Conversations` + `Messages` sans AsNoTracking.
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Queries/GetOwnerConversationById/GetOwnerConversationByIdHandler.cs`
  — utilise `IgnoreQueryFilters()` (justifié — portail owner), mais pas de AsNoTracking.

## Fix attendu

Ajouter `.AsNoTracking()` sur chaque query EF dans les 2 handlers.

```csharp
// GetConversationByIdHandler — avant
var conversation = await _context.Conversations
    .Include(c => c.Messages)
    .FirstOrDefaultAsync(c => c.Id == query.ConversationId, ct);

// Après
var conversation = await _context.Conversations
    .AsNoTracking()
    .Include(c => c.Messages)
    .FirstOrDefaultAsync(c => c.Id == query.ConversationId, ct);
```

Pour `GetOwnerConversationByIdHandler`, placer `.AsNoTracking()` après `.IgnoreQueryFilters()`.

## Critères de complétion

```
□ AsNoTracking() présent sur les 2 handlers
□ dotnet build → 0 erreur
□ Tests acceptance Messaging toujours verts
```
