# Skill: Ardalis Modular Monolith (.NET Core 10)

## Concept fondamental

Un monolithe modulaire = un seul déploiement, plusieurs modules isolés par projets C#.
L'isolation est garantie par les access modifiers C# (`internal`) et la structure en 2 assemblies.

## Les 2 assemblies par module

### `.Contracts` (public)
Ce projet contient UNIQUEMENT les types que les autres modules ont le droit de voir.

```
Vetolib.Agenda.Contracts/
├── IAppointmentService.cs        ← interface publique
├── DTOs/
│   ├── AppointmentDto.cs         ← DTO de réponse
│   └── CreateAppointmentRequest.cs
├── Events/
│   └── IAppointmentConfirmedEvent.cs  ← event interface
└── Enums/
    └── AppointmentStatus.cs
```

Règle : **aucune logique**. Que des types (interfaces, records, enums).

### Runtime (internal)
Tout est `internal` sauf `ModuleServiceRegistrar`.

```
Vetolib.Agenda/
├── AgendaModuleServiceRegistrar.cs   ← public (seul exception)
├── Api/
│   └── AppointmentEndpoints.cs       ← internal
├── Application/
│   └── Commands/
│       └── CreateAppointment/
│           ├── CreateAppointmentCommand.cs   ← internal record
│           ├── CreateAppointmentHandler.cs   ← internal class
│           └── CreateAppointmentValidator.cs ← internal class
└── Infrastructure/
    └── AgendaDbContext.cs             ← internal
```

## ModuleServiceRegistrar — pattern obligatoire

```csharp
// AgendaModuleServiceRegistrar.cs
public static class AgendaModuleServiceRegistrar
{
    public static IServiceCollection AddAgendaModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MediatR — uniquement cet assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AgendaModuleServiceRegistrar).Assembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(AgendaModuleServiceRegistrar).Assembly);

        // EF Core
        services.AddDbContext<AgendaDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("vetolibdb")));

        // Repositories et services internes
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        return services;
    }
}
```

## Graphe de dépendances — règles strictes

```
✅ Autorisé :
  Vetolib.Agenda          → Vetolib.Agenda.Contracts
  Vetolib.Agenda          → Vetolib.Shared.Kernel
  Vetolib.Agenda          → Vetolib.Shared.Infrastructure
  Vetolib.Agenda.Contracts→ Vetolib.Shared.Kernel
  Vetolib.Agenda.Contracts→ Vetolib.Auth.Contracts  (ex: pour ClinicId)
  Vetolib.Api             → Vetolib.Agenda.Contracts

❌ Interdit :
  Vetolib.Api             → Vetolib.Agenda  (jamais le runtime)
  Vetolib.Auth            → Vetolib.Agenda  (jamais cross-runtime)
  Vetolib.Agenda.Contracts→ Vetolib.Agenda  (le Contracts ne peut pas dépendre du runtime)
```

## Communication inter-modules

### Via interface Contracts (in-process, synchrone)

```csharp
// Dans Vetolib.Auth.Contracts
public interface IClinicService
{
    Task<Result<ClinicDto>> GetClinicAsync(Guid clinicId, CancellationToken ct);
}

// Dans Vetolib.Billing, on dépend de Auth.Contracts et on injecte IClinicService
// L'implémentation est dans Vetolib.Auth et enregistrée dans AuthModuleServiceRegistrar
```

### Via Domain Events (MediatR notifications, découplé)

```csharp
// Dans Vetolib.Agenda.Contracts/Events/
public interface IAppointmentConfirmedEvent : INotification
{
    Guid AppointmentId { get; }
    Guid ClinicId { get; }
    DateTime ConfirmedAt { get; }
}

// Dans Vetolib.Billing, un handler écoute cet event pour créer la facture
internal class CreateInvoiceOnAppointmentConfirmedHandler
    : INotificationHandler<IAppointmentConfirmedEvent>
{ ... }
```

## Enregistrement des endpoints dans l'Api

```csharp
// Vetolib.Agenda/Api/AppointmentEndpoints.cs
internal static class AppointmentEndpoints
{
    internal static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").RequireAuthorization();
        group.MapGet("/", GetAll);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}/confirm", Confirm);
        group.MapDelete("/{id:guid}", Cancel);
        return app;
    }

    private static async Task<IResult> Create(
        CreateAppointmentCommand cmd, ISender sender)
        => (await sender.Send(cmd)).ToMinimalApiResult();
}

// Extension publique exposée dans Contracts ou via AgendaModuleServiceRegistrar
public static IEndpointRouteBuilder MapAgendaEndpoints(this IEndpointRouteBuilder app)
    => AppointmentEndpoints.MapAppointmentEndpoints(app);
```

## Anti-patterns à éviter

```
❌ Ajouter des types publics dans le runtime (assemblies non-Contracts)
❌ Référencer un autre module par son runtime
❌ Mettre de la logique dans les DTOs Contracts
❌ Partager un DbContext entre modules (chaque module a le sien)
❌ Utiliser static coupling entre modules (events uniquement)
```

## Checklist avant de créer un nouveau module

```
□ Créer les 2 projets : ModuleName.Contracts + ModuleName
□ Contracts référence Shared.Kernel uniquement
□ Runtime déclare tous les types internal
□ ModuleServiceRegistrar est la seule classe public du runtime
□ DbContext hérite de MultiTenantDbContext
□ Module enregistré dans Vetolib.Api/Program.cs
□ Endpoints mappés dans app.Map{Module}Endpoints()
□ Dossier Infrastructure/Migrations/ créé
```
