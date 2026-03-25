# todo-back-breeding-scaffold-007.md -- Scaffold Vetolib.Breeding module

**Module** : Breeding (NEW)
**Priority** : Critique
**Dependencies** : todo-back-patient-sex-001 (Sex enum must exist in MedicalRecords.Contracts)
**Skills** : `ardalis-modular-monolith`, `multitenant-efcore`, `cqrs-mediatr`
**MODIF_SHARED: autorise** (modification de Vetolib.Api/Program.cs necessaire)

## Context

Phase 2 features (Litter, Lineage, Pregnancy, HeatCycle) live in a new Breeding module following the Ardalis modular monolith pattern (2 assemblies). This task creates the skeleton -- no business logic yet.

## Scope

### 1. Create projects

```
src/backend/Modules/Breeding/
    Vetolib.Breeding.Contracts/
        Vetolib.Breeding.Contracts.csproj
    Vetolib.Breeding/
        Vetolib.Breeding.csproj
```

#### Vetolib.Breeding.Contracts.csproj

References:
- `Vetolib.Shared.Kernel`
- `Vetolib.MedicalRecords.Contracts` (for Sex, Species, PatientDto, IPatientReader)
- `Ardalis.Result`

#### Vetolib.Breeding.csproj

References:
- `Vetolib.Breeding.Contracts`
- `Vetolib.Shared.Kernel`
- `Vetolib.Shared.Infrastructure`
- `MediatR`
- `FluentValidation`
- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

### 2. Add to solution

```bash
dotnet sln Vetolib.sln add src/backend/Modules/Breeding/Vetolib.Breeding.Contracts/Vetolib.Breeding.Contracts.csproj
dotnet sln Vetolib.sln add src/backend/Modules/Breeding/Vetolib.Breeding/Vetolib.Breeding.csproj
```

### 3. Create BreedingDbContext

File: `src/backend/Modules/Breeding/Vetolib.Breeding/Infrastructure/BreedingDbContext.cs`

```csharp
internal class BreedingDbContext : MultiTenantDbContext
{
    public BreedingDbContext(DbContextOptions<BreedingDbContext> options, IClinicContext clinicContext)
        : base(options, clinicContext) { }

    // DbSets will be added by subsequent tasks

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(BreedingDbContext).Assembly);
    }
}
```

### 4. Create ModuleServiceRegistrar

File: `src/backend/Modules/Breeding/Vetolib.Breeding/BreedingModuleServiceRegistrar.cs`

```csharp
public static class BreedingModuleServiceRegistrar
{
    public static IServiceCollection AddBreedingModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BreedingModuleServiceRegistrar).Assembly));
        services.AddValidatorsFromAssembly(typeof(BreedingModuleServiceRegistrar).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapBreedingEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoints will be added by subsequent tasks
        return app;
    }
}
```

### 5. Create directory structure

```
Vetolib.Breeding/
    Api/                       (empty, endpoints added later)
    Application/
        Commands/              (empty)
        Queries/               (empty)
        Domain/                (empty)
    Infrastructure/
        BreedingDbContext.cs
        Configurations/        (empty)
        Migrations/            (empty)
    BreedingModuleServiceRegistrar.cs
```

### 6. Register in Vetolib.Api/Program.cs (FROZEN FILE -- MODIF_GELE)

Add to `Program.cs`:

```csharp
using Vetolib.Breeding;
using Vetolib.Breeding.Infrastructure;

// After existing module registrations:
builder.Services.AddBreedingModule(builder.Configuration);
builder.AddNpgsqlDbContext<BreedingDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);

// After existing endpoint mappings:
app.MapBreedingEndpoints();
```

Also add to `DbInitializer.MigrateAllAsync()` if applicable.

### 7. Extend IPatientReader (in MedicalRecords.Contracts)

The Breeding module needs to read patient Sex and Species for validation. Add to `IPatientReader`:

```csharp
Task<Result<PatientDto>> GetPatientByIdAsync(Guid patientId, CancellationToken cancellationToken = default);
```

This method already returns PatientDto which will include Sex after task 001. Implement in MedicalRecords runtime.

## BDD

No specific .feature for scaffolding. Validation = builds and tests pass.

## Completion criteria

- [ ] `Vetolib.Breeding.Contracts` project created and added to solution
- [ ] `Vetolib.Breeding` project created and added to solution
- [ ] `BreedingDbContext` extends `MultiTenantDbContext`
- [ ] `BreedingModuleServiceRegistrar` is the only public class in runtime
- [ ] Registered in `Vetolib.Api/Program.cs`
- [ ] `IPatientReader.GetPatientByIdAsync()` added (or equivalent)
- [ ] Directory structure follows module convention
- [ ] `dotnet build` GREEN (no business logic to test yet)
- [ ] `dotnet test` GREEN (no regressions)
