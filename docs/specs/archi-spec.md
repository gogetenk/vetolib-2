# archi-spec.md — Vetolib Architecture Reference

> Document de référence pour l'Agent Architect. Ne pas modifier sans créer un dispute.
> Version : 2.0 — Stack .NET Core 10 + Ardalis Modular Monolith + Aspire

---

## 1. Stack technique

| Couche | Technologie |
|---|---|
| Orchestration locale | .NET Aspire 9.x (AppHost + ServiceDefaults) |
| Backend runtime | ASP.NET Core 10, Minimal APIs |
| Architecture | Ardalis Modular Monolith (2 assemblies par module) |
| Intracouche | Clean Architecture (Jason Taylor) avec MediatR |
| Résultat partout | Ardalis.Result + Ardalis.Result.AspNetCore |
| ORM | Entity Framework Core 10, driver Npgsql |
| Base de données | PostgreSQL 16 |
| Auth | JWT + Refresh Tokens (multi-tenant par clinique) |
| Frontend | Next.js 15 App Router, TypeScript, shadcn/ui, Tailwind CSS |
| Tests BDD | Reqnroll (C#) + xUnit |
| Tests E2E | Playwright |
| Tests unitaires | xUnit + FluentAssertions + NSubstitute |

---

## 2. Structure de solution

```
Vetolib.sln
│
├── AppHost/                              ← .NET Aspire AppHost (orchestrateur local)
│   └── Program.cs
│
├── ServiceDefaults/                      ← .NET Aspire partagés
│   └── Extensions.cs                     ← OpenTelemetry, health checks, service discovery
│
├── Vetolib.Api/                          ← Host ASP.NET Core 10 (Minimal APIs uniquement)
│   └── Program.cs
│
├── Modules/
│   ├── Auth/
│   │   ├── Vetolib.Auth.Contracts/       ← Assembly PUBLIC
│   │   └── Vetolib.Auth/                 ← Assembly INTERNAL
│   │       ├── Api/
│   │       ├── Application/
│   │       │   ├── Commands/
│   │       │   ├── Queries/
│   │       │   └── Domain/
│   │       └── Infrastructure/
│   │
│   ├── Agenda/
│   │   ├── Vetolib.Agenda.Contracts/
│   │   └── Vetolib.Agenda/
│   │
│   ├── MedicalRecords/
│   │   ├── Vetolib.MedicalRecords.Contracts/
│   │   └── Vetolib.MedicalRecords/
│   │
│   └── Billing/
│       ├── Vetolib.Billing.Contracts/
│       └── Vetolib.Billing/
│
└── Shared/                               ← FROZEN — ne pas modifier sans arbitrage humain
    ├── Vetolib.Shared.Kernel/
    └── Vetolib.Shared.Infrastructure/
```

---

## 3. Règle des 2 assemblies (Ardalis Modular Monolith)

### Assembly `.Contracts` (PUBLIC)

- Tous les types sont `public`
- Contient : interfaces de services (`IAppointmentService`), DTOs, domain events interfaces, enums publics
- **INTERDIT** : entités EF Core, DbContext, implémentations, logique métier
- Les autres modules ne référencent QUE `.Contracts`, jamais le runtime

### Assembly runtime (INTERNAL)

- Tous les types sauf `ModuleServiceRegistrar` sont `internal`
- Contient : entités, handlers MediatR, DbContext, repositories, services d'infra
- `ModuleServiceRegistrar` est la seule classe `public`

```csharp
// Vetolib.Auth/AuthModuleServiceRegistrar.cs
public static class AuthModuleServiceRegistrar
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthModuleServiceRegistrar).Assembly));
        services.AddDbContext<AuthDbContext>(...);
        return services;
    }
}
```

### Graphe de dépendances autorisé

```
Vetolib.Auth.Contracts    → Vetolib.Shared.Kernel
Vetolib.Auth              → Vetolib.Auth.Contracts + Vetolib.Shared.*
Vetolib.Api               → Vetolib.Auth.Contracts (JAMAIS Vetolib.Auth directement)
Vetolib.Agenda.Contracts  → Vetolib.Auth.Contracts (pour ClinicId, UserDto)
```

**RÈGLE ABSOLUE** : Module A ne peut jamais référencer le runtime de Module B. Uniquement les Contracts.

---

## 4. Clean Architecture par module

```
Vetolib.Agenda/
├── Api/
│   └── AppointmentEndpoints.cs
│
├── Application/
│   ├── Commands/
│   │   └── CreateAppointment/
│   │       ├── CreateAppointmentCommand.cs
│   │       ├── CreateAppointmentHandler.cs
│   │       └── CreateAppointmentValidator.cs
│   ├── Queries/
│   │   └── GetAppointments/
│   │       ├── GetAppointmentsQuery.cs
│   │       └── GetAppointmentsHandler.cs
│   └── Domain/
│       ├── Appointment.cs
│       ├── AppointmentStatus.cs
│       └── AppointmentDomainService.cs
│
└── Infrastructure/
    ├── AgendaDbContext.cs
    ├── Repositories/
    └── Migrations/
```

---

## 5. Pattern Result — Ardalis.Result PARTOUT

**Règle absolue : zéro exception pour le control flow. Tout retourne `Result<T>` ou `Result`.**

### Dans le Domain

```csharp
internal class Appointment : BaseEntity, IMultiTenant
{
    public Result Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            return Result.Error("Cannot confirm a non-pending appointment.");
        Status = AppointmentStatus.Confirmed;
        return Result.Success();
    }

    public static Result<Appointment> Create(Guid clinicId, Guid vetId, DateTime scheduledAt)
    {
        if (scheduledAt < DateTime.UtcNow)
            return Result<Appointment>.Invalid(new ValidationError("scheduledAt", "Cannot schedule in the past"));
        return Result<Appointment>.Success(new Appointment { ClinicId = clinicId, VetId = vetId, ScheduledAt = scheduledAt });
    }
}
```

### Dans l'Application (Handler)

```csharp
internal class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand cmd, CancellationToken ct)
    {
        var vet = await _vetRepo.GetByIdAsync(cmd.VetId, ct);
        if (vet is null) return Result<AppointmentDto>.NotFound($"Vet {cmd.VetId} not found.");

        var result = Appointment.Create(cmd.ClinicId, cmd.VetId, cmd.ScheduledAt);
        if (!result.IsSuccess) return result.Map(_ => (AppointmentDto)null!);

        await _repo.AddAsync(result.Value, ct);
        return Result<AppointmentDto>.Success(result.Value.ToDto());
    }
}
```

### Dans l'Api (Minimal API endpoint)

```csharp
// Toujours .ToMinimalApiResult() — JAMAIS de try/catch business
group.MapPost("/", async (CreateAppointmentCommand cmd, ISender sender) =>
    (await sender.Send(cmd)).ToMinimalApiResult());
```

### Mapping statuts

| Result | HTTP |
|---|---|
| `Result.Success()` | 200 OK |
| `Result<T>.Success(value)` | 200 OK avec body |
| `Result.NotFound()` | 404 Not Found |
| `Result.Invalid(errors)` | 400 Bad Request |
| `Result.Unauthorized()` | 401 Unauthorized |
| `Result.Forbidden()` | 403 Forbidden |
| `Result.Error(msg)` | 422 Unprocessable Entity |

---

## 6. Multi-tenancy — Global Query Filter EF Core

Toute entité accessible par clinique implémente `IMultiTenant`.

```csharp
// Shared.Kernel/IMultiTenant.cs
public interface IMultiTenant { Guid ClinicId { get; } }

// Shared.Infrastructure/MultiTenantDbContext.cs
public abstract class MultiTenantDbContext : DbContext
{
    protected readonly IClinicContext ClinicContext;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
                ApplyTenantFilter(builder, entityType.ClrType);
        }
    }

    private void ApplyTenantFilter(ModelBuilder builder, Type type)
    {
        var param = Expression.Parameter(type);
        var prop = Expression.Property(param, nameof(IMultiTenant.ClinicId));
        var body = Expression.Equal(prop, Expression.Constant(ClinicContext.ClinicId));
        builder.Entity(type).HasQueryFilter(Expression.Lambda(body, param));
    }
}
```

**INTERDIT** : `IgnoreQueryFilters()` sauf dans les seeds/migrations avec commentaire explicite.

---

## 7. .NET Aspire — AppHost

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var db = postgres.AddDatabase("vetolibdb");

builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
```

```csharp
// Vetolib.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AuthDbContext>("vetolibdb");
builder.AddNpgsqlDbContext<AgendaDbContext>("vetolibdb");

builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddAgendaModule(builder.Configuration);
builder.Services.AddMedicalRecordsModule(builder.Configuration);
builder.Services.AddBillingModule(builder.Configuration);

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapAgendaEndpoints();
app.MapMedicalRecordsEndpoints();
app.MapBillingEndpoints();
app.Run();
```

**Migration vers microservices** : un module s'extrait en ajoutant `builder.AddProject<Projects.Vetolib_Agenda>("agenda-service")` dans AppHost. Les autres modules utilisent déjà les Contracts — zéro changement dans leur code.

---

## 8. CQRS avec MediatR

- Commands → écriture → `Result` ou `Result<T>`
- Queries → lecture → `Result<T>` (zéro effet de bord)
- Behaviors → cross-cutting : validation, logging, performance

```csharp
// ValidationBehavior retourne Result.Invalid() si FluentValidation échoue — jamais d'exception
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

---

## 9. Nommage

| Élément | Convention |
|---|---|
| Commands | `CreateAppointmentCommand` |
| Queries | `GetAppointmentsQuery` |
| Handlers | `CreateAppointmentHandler` |
| Validators | `CreateAppointmentValidator` |
| Entities | PascalCase singulier : `Appointment`, `Patient` |
| DTOs (Contracts) | `AppointmentDto`, `CreateAppointmentRequest` |
| Domain Events | `AppointmentConfirmedEvent` |
| Endpoints | `AppointmentEndpoints` + `MapAppointmentEndpoints(this IEndpointRouteBuilder)` |

---

## 10. Décisions verrouillées

```
1. Ardalis.Result PARTOUT — Domain inclus. Zéro exception pour le business flow.
2. 2 assemblies par module — Contracts (public) + Runtime (internal). Jamais de référence croisée runtime.
3. Global Query Filter obligatoire sur IMultiTenant. IgnoreQueryFilters() interdit sauf seeds/migrations.
4. MediatR pour toute communication intra-module.
5. Inter-module : Contracts uniquement ou Domain Events (MediatR notifications). Jamais de runtime import.
6. .NET Aspire AppHost = seul orchestrateur local. Pas de docker-compose manuel.
7. Minimal APIs uniquement. Toujours .ToMinimalApiResult() en fin d'endpoint.
8. BDD-first obligatoire : Reqnroll bindings écrits avant l'implémentation. PR bloquée sinon.
9. Chaque module a son propre DbContext héritant de MultiTenantDbContext.
10. Migrations EF Core : une migration par module, dans son Infrastructure/Migrations/.
```

---

## 11. Shared.Kernel — FROZEN

- `BaseEntity` : `Id (Guid)`, `CreatedAt`, `UpdatedAt`, `AddDomainEvent()`
- `IAggregateRoot` : marqueur
- `IMultiTenant` : `Guid ClinicId { get; }`
- `IClinicContext` : `Guid ClinicId { get; }` (résolu depuis JWT par middleware)

**Ne jamais ajouter de logique métier dans Shared.Kernel. Si en doute → disputes.md.**
