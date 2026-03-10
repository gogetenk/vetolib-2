# todo-back-messaging-scaffold-001.md — Scaffold module Vetolib.Messaging

**Module** : Messaging (nouveau)
**Dependances** : done-back-ai-scaffold-001
**Priorite** : BASSE (Phase 4)
**Skills a lire** : `ardalis-result`, `ardalis-modular-monolith`, `multitenant-efcore`
MODIF_GELE: autorise (AppHost/Program.cs, Vetolib.Api/Program.cs)

---

## Objectif

Creer le module Messaging avec la structure 2 assemblies. Ce module porte le domaine metier de la messagerie clinique.

## Spec de reference

- `docs/MESSAGING-AI-BRIEF.md` (brief fonctionnel complet)
- `questions/messaging-module-scope-001.md` (decision PO: module separe)

## Implementation

### Structure

```
Modules/Messaging/
  Vetolib.Messaging.Contracts/
    Vetolib.Messaging.Contracts.csproj
    ConversationDto.cs
    MessageDto.cs
    MessageCategory.cs (enum)
    ConversationStatus.cs (enum)
    MessageSender.cs (enum: Owner, AI, Vet)
  Vetolib.Messaging/
    Vetolib.Messaging.csproj
    Application/
      Domain/
        Conversation.cs (aggregate root)
        Message.cs (entity)
    Infrastructure/
      MessagingDbContext.cs (herite MultiTenantDbContext)
    Api/
      MessagingEndpoints.cs
    ModuleServiceRegistrar.cs (public)
```

### Dependencies

- Reference `Vetolib.AI.Contracts` pour triage/suggestions
- Reference `Vetolib.Agenda.Contracts` pour conversion en RDV
- Reference `Vetolib.MedicalRecords.Contracts` pour contexte patient

### Fichiers geles modifies

- `AppHost/Program.cs` : reference projet Messaging
- `Vetolib.Api/Program.cs` : `AddMessagingModule()` + `MapMessagingEndpoints()`

## Critere

```
[] Structure 2 assemblies creee
[] MessagingDbContext herite MultiTenantDbContext
[] Entites Conversation + Message avec Result<T>
[] ModuleServiceRegistrar seule classe public
[] dotnet build passe
[] Renommer en done
```
