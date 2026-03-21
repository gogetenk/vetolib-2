# todo-back-messaging-domain-001.md — Enrichir le domaine Messaging (entites manquantes + MessageSender update)

**Module** : Messaging
**Dependances** : done-back-messaging-scaffold-001
**Priorite** : HAUTE
**Skills a lire** : `ardalis-result`, `ardalis-modular-monolith`, `multitenant-efcore`

---

## Objectif

Le scaffold a cree Conversation et Message. Il manque les entites et enums decrits dans la spec `docs/MESSAGING-SPEC.md` section 8.

## Implementation

### 1. Mettre a jour MessageSender enum (Contracts)

```csharp
public enum MessageSender
{
    Owner,      // Pet owner via portal
    Vet,        // Veterinarian reply
    Staff,      // Receptionist or Admin reply
    System      // Auto-acknowledgment (out-of-hours)
}
```

Supprimer `AI` (l'IA ne send pas de messages, elle genere des suggestions).

### 2. Ajouter les entites Domain (runtime, internal)

- **ReplyAudit** : `Id, MessageId, OriginalOwnerMessageId, AiSuggestedReply?, WasSuggestedReplyUsed, ActualReply`
- **MessageAttachment** : `Id, MessageId, FileName, ContentType, FileSizeBytes, StoragePath`
- **ResponseTemplate** (IMultiTenant) : `Id, ClinicId, Name, ContentEn, ContentAr, Category?, CreatedAt, UpdatedAt`
- **OwnerPortalToken** (IMultiTenant) : `Id, ClinicId, OwnerId, Token (unique, signed), ExpiresAt, ConsentAcceptedAt?, ConsentVersion?, CreatedAt`
- **MessagingHours** (IMultiTenant) : `Id, ClinicId, DayOfWeek (0-6), OpenTime, CloseTime, IsClosed`

### 3. Enrichir Conversation

Ajouter les champs manquants :
- `AssignedToUserId?` (Guid)
- `AssignedToRole?` (string)
- `AiTriageConfidence?` (decimal)
- `IsTriageUncertain` (bool)

### 4. Ajouter les DTOs manquants (Contracts)

- `ResponseTemplateDto`
- `OwnerPortalTokenDto`
- `MessagingHoursDto`
- `MessageAttachmentDto`
- `ReplyAuditDto`

### 5. Ajouter les Configurations EF Core

- `ReplyAuditConfiguration.cs`
- `MessageAttachmentConfiguration.cs`
- `ResponseTemplateConfiguration.cs`
- `OwnerPortalTokenConfiguration.cs`
- `MessagingHoursConfiguration.cs`

### 6. Mettre a jour MessagingDbContext

Ajouter les DbSet pour chaque nouvelle entite.

### 7. Migration initiale

Creer la migration EF Core pour le schema `messaging`.

## Regles

- Toutes les entites multi-tenant implementent `IMultiTenant`
- Toutes les factories statiques retournent `Result<T>`
- Toutes les classes domain sont `internal`
- Le body Message reste limite a 2000 caracteres
- OwnerPortalToken.Token doit avoir un index unique

## Critere

```
[] MessageSender enum mis a jour (Owner, Vet, Staff, System)
[] 5 nouvelles entites creees avec factories Result<T>
[] Conversation enrichie (AssignedToUserId, AssignedToRole, AiTriageConfidence, IsTriageUncertain)
[] DTOs correspondants dans Contracts
[] EF Core configurations + DbSet
[] Migration creee
[] dotnet build passe
[] Renommer en done
```
