# todo-back-events-refacto-001.md — Refacto : Domain Events + Integration Events + MassTransit Outbox

**Module** : Shared + tous les modules
**Dépendances** : done-back-email-001, done-back-audit-001
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `ardalis-modular-monolith`
**MODIF_SHARED: autorisé**
**Priorité** : HAUTE — architecture fondamentale

---

## Contexte

L'implémentation email actuelle appelle `IEmailSender` directement depuis les handlers (couplage fort, pas de garantie de livraison, pas de retry). Il faut passer à une architecture DDD propre :

- **Domain Events** : événements internes au module, dispatchés dans la même transaction
- **Integration Events** : événements publiés vers d'autres modules via MassTransit, avec Outbox transactionnel
- **Module Notifications** : consomme les integration events et envoie les emails/SMS

## Architecture cible

```
Aggregate Root (ex: User)
  └── _domainEvents.Add(new UserInvitedDomainEvent(...))
        │
        ▼ (SaveChangesAsync → Unit of Work)
  1. Persist entity changes
  2. Dispatch domain events (MediatR INotification, in-process, same transaction)
  3. Persist integration events in Outbox (MassTransit EF Outbox, same transaction)
  4. COMMIT
        │
        ▼ (MassTransit Outbox relay — async, retries)
  Integration Event → RabbitMQ (ou InMemory en dev)
        │
        ▼
  Module Notifications → IEmailSender.SendAsync()
```

## Périmètre exact

### 1. Domain Events dans BaseEntity

Modifier `Shared/Vetolib.Shared.Kernel/BaseEntity.cs` :

```csharp
public abstract class BaseEntity
{
    // ... existing Id, CreatedAt, UpdatedAt

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Marker interface
public interface IDomainEvent : INotification { }
```

### 2. Unit of Work — dispatch domain events au SaveChanges

Modifier `Shared/Vetolib.Shared.Infrastructure/MultiTenantDbContext.cs` :

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    // 1. Collecter les domain events de toutes les entités modifiées
    var entities = ChangeTracker.Entries<BaseEntity>()
        .Where(e => e.Entity.DomainEvents.Any())
        .ToList();

    var domainEvents = entities
        .SelectMany(e => e.Entity.DomainEvents)
        .ToList();

    // 2. Clear events AVANT le save (évite boucle infinie si un handler modifie une entité)
    entities.ForEach(e => e.Entity.ClearDomainEvents());

    // 3. Persist (inclut l'outbox MassTransit dans la même transaction)
    var result = await base.SaveChangesAsync(ct);

    // 4. Dispatch domain events APRÈS le save (garantit la persistance)
    foreach (var domainEvent in domainEvents)
    {
        await _mediator.Publish(domainEvent, ct);
    }

    return result;
}
```

### 3. Integration Events via MassTransit

Installer MassTransit + EF Core Outbox :

```xml
<!-- Shared.Infrastructure -->
<PackageReference Include="MassTransit" Version="8.*" />
<PackageReference Include="MassTransit.EntityFrameworkCore" Version="8.*" />

<!-- AppHost ou Vetolib.Api pour le transport -->
<PackageReference Include="MassTransit.RabbitMQ" Version="8.*" />
```

Configuration dans `Program.cs` :

```csharp
builder.Services.AddMassTransit(x =>
{
    // Consumers de chaque module
    x.AddConsumers(typeof(AuthModuleServiceRegistrar).Assembly);
    x.AddConsumers(typeof(AgendaModuleServiceRegistrar).Assembly);
    // etc.

    // Outbox EF Core (une config par DbContext)
    x.AddEntityFrameworkOutbox<AuthDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    // Répéter pour chaque DbContext

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq") ?? "localhost");
        cfg.ConfigureEndpoints(context);
    });
});
```

En dev : utiliser `UsingInMemory` ou RabbitMQ via Aspire :
```csharp
// AppHost/Program.cs
var rabbitmq = builder.AddRabbitMQ("rabbitmq");
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithEndpoint(1025, 1025, name: "smtp")
    .WithEndpoint(8025, 8025, name: "ui");
```

### 4. Refactorer les modules pour publier des Integration Events

**Auth module** — `UserInvitedDomainEvent` :

```csharp
// Domain event (interne au module)
internal record UserInvitedDomainEvent(Guid UserId, string Email, string FullName,
    string TemporaryPassword, string ClinicName) : IDomainEvent;

// Dans User.Invite() :
public static Result<User> Invite(...)
{
    var user = new User { ... };
    user.AddDomainEvent(new UserInvitedDomainEvent(user.Id, email, fullName, tempPassword, clinicName));
    return Result.Success(user);
}

// Domain event handler → publie l'integration event
internal class UserInvitedDomainEventHandler : INotificationHandler<UserInvitedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task Handle(UserInvitedDomainEvent notification, CancellationToken ct)
    {
        await _publishEndpoint.Publish(new UserInvitedIntegrationEvent
        {
            Email = notification.Email,
            FullName = notification.FullName,
            TemporaryPassword = notification.TemporaryPassword,
            ClinicName = notification.ClinicName,
        }, ct);
    }
}
```

**Integration event** (dans `Vetolib.Auth.Contracts`) :

```csharp
// Public — consommable par d'autres modules
public record UserInvitedIntegrationEvent
{
    public string Email { get; init; }
    public string FullName { get; init; }
    public string TemporaryPassword { get; init; }
    public string ClinicName { get; init; }
}
```

**Agenda module** — `AppointmentReminderDueIntegrationEvent` :

Remplacer le background service qui envoie des emails directement par un qui publie un integration event :

```csharp
public record AppointmentReminderDueIntegrationEvent
{
    public string OwnerEmail { get; init; }
    public string OwnerName { get; init; }
    public string PatientName { get; init; }
    public string VetName { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string ClinicName { get; init; }
}
```

### 5. Module Notifications (nouveau)

Créer un module léger qui consomme les integration events :

```
Modules/Notifications/
  Vetolib.Notifications.Contracts/
    (vide pour l'instant — pas de DTOs publics)
  Vetolib.Notifications/
    Consumers/
      UserInvitedConsumer.cs     → envoie email invitation
      AppointmentReminderConsumer.cs → envoie email rappel
    Templates/
      InvitationEmailTemplate.cs
      ReminderEmailTemplate.cs
    ModuleServiceRegistrar.cs
```

Chaque consumer :
1. Reçoit l'integration event via MassTransit
2. Construit le template email (HTML + plain text)
3. Appelle `IEmailSender.SendAsync()`
4. MassTransit gère les retries automatiquement (exponential backoff)

### 6. MailHog en dev

Configurer dans Aspire `AppHost/Program.cs` :

```csharp
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithEndpoint(1025, 1025, name: "smtp")
    .WithHttpEndpoint(8025, 8025, name: "ui");
```

Remplacer `ConsoleEmailSender` par défaut par `SmtpEmailSender` pointant vers MailHog :

```json
// appsettings.Development.json
"Email": {
  "Provider": "smtp",
  "Smtp": {
    "Host": "localhost",
    "Port": 1025,
    "EnableSsl": false
  }
}
```

UI MailHog accessible sur http://localhost:8025 — tous les emails y apparaissent.

### 7. Supprimer l'implémentation naïve

- Supprimer `ConsoleEmailSender` (remplacé par SmtpEmailSender + MailHog)
- Supprimer `UserInvitedNotification` du module Auth (remplacé par domain event + integration event)
- Supprimer `SendInviteEmailHandler` du module Auth (remplacé par consumer dans Notifications)
- Modifier `AppointmentReminderService` pour publier un event au lieu d'envoyer un email
- Garder `IEmailSender` et `SmtpEmailSender` dans Shared (le module Notifications les utilise)

### 8. Migration MassTransit Outbox

Chaque DbContext a besoin des tables outbox. Ajouter une migration :

```bash
dotnet ef migrations add AddMassTransitOutbox --context AuthDbContext
dotnet ef migrations add AddMassTransitOutbox --context AgendaDbContext
# etc.
```

## Critère de complétion

```
□ BaseEntity a _domainEvents + AddDomainEvent + ClearDomainEvents
□ MultiTenantDbContext dispatch les domain events au SaveChanges
□ MassTransit configuré avec EF Core Outbox
□ RabbitMQ dans Aspire AppHost
□ MailHog dans Aspire AppHost, UI sur :8025
□ Auth : User.Invite() ajoute un domain event → integration event → consumer envoie email
□ Agenda : reminder service publie un integration event → consumer envoie email
□ Module Notifications créé avec les consumers
□ ConsoleEmailSender supprimé, SmtpEmailSender pointe vers MailHog en dev
□ Tables outbox MassTransit dans les migrations
□ dotnet build → 0 erreur
□ Tests existants toujours verts
□ Renommer en done-back-events-refacto-001.md
```
