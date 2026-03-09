# todo-scaffold-000.md — Scaffolding solution .NET

**Module** : Infrastructure / Solution entière
**Priorité** : CRITIQUE — à faire avant toute autre tâche
**Dépendances** : aucune
**Estimé** : 1-2h

---

## Objectif

Créer la structure complète de la solution .NET 10 avec Aspire, tous les projets vides,
et vérifier que `dotnet build` passe avant de toucher au code métier.

---

## Livrable attendu

```
Vetolib.sln
├── AppHost/
│   └── AppHost.csproj                     ← Aspire AppHost
├── ServiceDefaults/
│   └── ServiceDefaults.csproj             ← Aspire ServiceDefaults
├── Vetolib.Api/
│   └── Vetolib.Api.csproj                 ← ASP.NET Core 10 Minimal APIs
├── Modules/
│   ├── Auth/
│   │   ├── Vetolib.Auth.Contracts/
│   │   │   └── Vetolib.Auth.Contracts.csproj
│   │   └── Vetolib.Auth/
│   │       └── Vetolib.Auth.csproj
│   ├── Agenda/
│   │   ├── Vetolib.Agenda.Contracts/
│   │   │   └── Vetolib.Agenda.Contracts.csproj
│   │   └── Vetolib.Agenda/
│   │       └── Vetolib.Agenda.csproj
│   ├── MedicalRecords/
│   │   ├── Vetolib.MedicalRecords.Contracts/
│   │   │   └── Vetolib.MedicalRecords.Contracts.csproj
│   │   └── Vetolib.MedicalRecords/
│   │       └── Vetolib.MedicalRecords.csproj
│   └── Billing/
│       ├── Vetolib.Billing.Contracts/
│       │   └── Vetolib.Billing.Contracts.csproj
│       └── Vetolib.Billing/
│           └── Vetolib.Billing.csproj
├── Shared/
│   ├── Vetolib.Shared.Kernel/
│   │   └── Vetolib.Shared.Kernel.csproj
│   └── Vetolib.Shared.Infrastructure/
│       └── Vetolib.Shared.Infrastructure.csproj
└── Tests/
    ├── Vetolib.Tests.Acceptance/
    │   └── Vetolib.Tests.Acceptance.csproj ← Reqnroll + xUnit + Testcontainers
    └── Vetolib.Tests.Unit/
        └── Vetolib.Tests.Unit.csproj       ← xUnit + FluentAssertions + NSubstitute
```

---

## Étapes

### 1. Créer la solution et les projets

```bash
dotnet new sln -n Vetolib

# Aspire
dotnet new aspire-apphost -n AppHost -o AppHost
dotnet new aspire-servicedefaults -n ServiceDefaults -o ServiceDefaults

# Api host
dotnet new webapi -n Vetolib.Api -o Vetolib.Api --use-minimal-apis

# Shared
dotnet new classlib -n Vetolib.Shared.Kernel -o Shared/Vetolib.Shared.Kernel --framework net10.0
dotnet new classlib -n Vetolib.Shared.Infrastructure -o Shared/Vetolib.Shared.Infrastructure --framework net10.0

# Modules — Contracts (public)
dotnet new classlib -n Vetolib.Auth.Contracts -o Modules/Auth/Vetolib.Auth.Contracts --framework net10.0
dotnet new classlib -n Vetolib.Agenda.Contracts -o Modules/Agenda/Vetolib.Agenda.Contracts --framework net10.0
dotnet new classlib -n Vetolib.MedicalRecords.Contracts -o Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts --framework net10.0
dotnet new classlib -n Vetolib.Billing.Contracts -o Modules/Billing/Vetolib.Billing.Contracts --framework net10.0

# Modules — Runtime (internal)
dotnet new classlib -n Vetolib.Auth -o Modules/Auth/Vetolib.Auth --framework net10.0
dotnet new classlib -n Vetolib.Agenda -o Modules/Agenda/Vetolib.Agenda --framework net10.0
dotnet new classlib -n Vetolib.MedicalRecords -o Modules/MedicalRecords/Vetolib.MedicalRecords --framework net10.0
dotnet new classlib -n Vetolib.Billing -o Modules/Billing/Vetolib.Billing --framework net10.0

# Tests
dotnet new xunit -n Vetolib.Tests.Acceptance -o Tests/Vetolib.Tests.Acceptance --framework net10.0
dotnet new xunit -n Vetolib.Tests.Unit -o Tests/Vetolib.Tests.Unit --framework net10.0
```

### 2. Ajouter tous les projets à la solution

```bash
dotnet sln add AppHost/AppHost.csproj
dotnet sln add ServiceDefaults/ServiceDefaults.csproj
dotnet sln add Vetolib.Api/Vetolib.Api.csproj
dotnet sln add Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj
dotnet sln add Shared/Vetolib.Shared.Infrastructure/Vetolib.Shared.Infrastructure.csproj
dotnet sln add Modules/Auth/Vetolib.Auth.Contracts/Vetolib.Auth.Contracts.csproj
dotnet sln add Modules/Auth/Vetolib.Auth/Vetolib.Auth.csproj
dotnet sln add Modules/Agenda/Vetolib.Agenda.Contracts/Vetolib.Agenda.Contracts.csproj
dotnet sln add Modules/Agenda/Vetolib.Agenda/Vetolib.Agenda.csproj
dotnet sln add Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts/Vetolib.MedicalRecords.Contracts.csproj
dotnet sln add Modules/MedicalRecords/Vetolib.MedicalRecords/Vetolib.MedicalRecords.csproj
dotnet sln add Modules/Billing/Vetolib.Billing.Contracts/Vetolib.Billing.Contracts.csproj
dotnet sln add Modules/Billing/Vetolib.Billing/Vetolib.Billing.csproj
dotnet sln add Tests/Vetolib.Tests.Acceptance/Vetolib.Tests.Acceptance.csproj
dotnet sln add Tests/Vetolib.Tests.Unit/Vetolib.Tests.Unit.csproj
```

### 3. Configurer les références entre projets

Graphe de dépendances (voir archi-spec.md §3) :

```bash
# ServiceDefaults
dotnet add ServiceDefaults/ServiceDefaults.csproj reference \
  Vetolib.Api/Vetolib.Api.csproj  # non — ServiceDefaults est référencé par les autres

# Shared.Kernel — aucune dépendance interne
# NuGet : Ardalis.Result, MediatR
dotnet add Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj \
  package Ardalis.Result
dotnet add Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj \
  package MediatR

# Shared.Infrastructure → Shared.Kernel + EF Core + Npgsql
dotnet add Shared/Vetolib.Shared.Infrastructure/Vetolib.Shared.Infrastructure.csproj \
  reference Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj
dotnet add Shared/Vetolib.Shared.Infrastructure/Vetolib.Shared.Infrastructure.csproj \
  package Microsoft.EntityFrameworkCore
dotnet add Shared/Vetolib.Shared.Infrastructure/Vetolib.Shared.Infrastructure.csproj \
  package Npgsql.EntityFrameworkCore.PostgreSQL

# Contracts → Shared.Kernel
dotnet add Modules/Auth/Vetolib.Auth.Contracts/Vetolib.Auth.Contracts.csproj \
  reference Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj
dotnet add Modules/Agenda/Vetolib.Agenda.Contracts/Vetolib.Agenda.Contracts.csproj \
  reference Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj
dotnet add Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts/Vetolib.MedicalRecords.Contracts.csproj \
  reference Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj
dotnet add Modules/Billing/Vetolib.Billing.Contracts/Vetolib.Billing.Contracts.csproj \
  reference Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj

# Agenda.Contracts → Auth.Contracts (pour UserDto/ClinicId)
dotnet add Modules/Agenda/Vetolib.Agenda.Contracts/Vetolib.Agenda.Contracts.csproj \
  reference Modules/Auth/Vetolib.Auth.Contracts/Vetolib.Auth.Contracts.csproj

# Runtimes → leurs Contracts + Shared.*
dotnet add Modules/Auth/Vetolib.Auth/Vetolib.Auth.csproj \
  reference Modules/Auth/Vetolib.Auth.Contracts/Vetolib.Auth.Contracts.csproj
dotnet add Modules/Auth/Vetolib.Auth/Vetolib.Auth.csproj \
  reference Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj
dotnet add Modules/Auth/Vetolib.Auth/Vetolib.Auth.csproj \
  reference Shared/Vetolib.Shared.Infrastructure/Vetolib.Shared.Infrastructure.csproj
# (répéter pour Agenda, MedicalRecords, Billing)

# Vetolib.Api → tous les Contracts (jamais les runtimes)
dotnet add Vetolib.Api/Vetolib.Api.csproj \
  reference Modules/Auth/Vetolib.Auth.Contracts/Vetolib.Auth.Contracts.csproj
dotnet add Vetolib.Api/Vetolib.Api.csproj \
  reference Modules/Agenda/Vetolib.Agenda.Contracts/Vetolib.Agenda.Contracts.csproj
dotnet add Vetolib.Api/Vetolib.Api.csproj \
  reference Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts/Vetolib.MedicalRecords.Contracts.csproj
dotnet add Vetolib.Api/Vetolib.Api.csproj \
  reference Modules/Billing/Vetolib.Billing.Contracts/Vetolib.Billing.Contracts.csproj
# + références aux runtimes pour les ModuleServiceRegistrar
dotnet add Vetolib.Api/Vetolib.Api.csproj \
  reference Modules/Auth/Vetolib.Auth/Vetolib.Auth.csproj
dotnet add Vetolib.Api/Vetolib.Api.csproj \
  reference Modules/Agenda/Vetolib.Agenda/Vetolib.Agenda.csproj
dotnet add Vetolib.Api/Vetolib.Api.csproj \
  reference Modules/MedicalRecords/Vetolib.MedicalRecords/Vetolib.MedicalRecords.csproj
dotnet add Vetolib.Api/Vetolib.Api.csproj \
  reference Modules/Billing/Vetolib.Billing/Vetolib.Billing.csproj

# Tests → Acceptance a besoin de tout
dotnet add Tests/Vetolib.Tests.Acceptance/Vetolib.Tests.Acceptance.csproj \
  reference Vetolib.Api/Vetolib.Api.csproj
dotnet add Tests/Vetolib.Tests.Acceptance/Vetolib.Tests.Acceptance.csproj \
  package Reqnroll.xUnit
dotnet add Tests/Vetolib.Tests.Acceptance/Vetolib.Tests.Acceptance.csproj \
  package Microsoft.AspNetCore.Mvc.Testing
dotnet add Tests/Vetolib.Tests.Acceptance/Vetolib.Tests.Acceptance.csproj \
  package Testcontainers.PostgreSql
dotnet add Tests/Vetolib.Tests.Acceptance/Vetolib.Tests.Acceptance.csproj \
  package FluentAssertions
```

### 4. NuGet packages globaux — Directory.Packages.props

Créer `Directory.Packages.props` à la racine pour centraliser les versions :

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Ardalis.Result" Version="10.*" />
    <PackageVersion Include="Ardalis.Result.AspNetCore" Version="10.*" />
    <PackageVersion Include="Ardalis.Result.FluentValidation" Version="10.*" />
    <PackageVersion Include="MediatR" Version="12.*" />
    <PackageVersion Include="FluentValidation" Version="11.*" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.*" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.*" />
    <PackageVersion Include="BCrypt.Net-Next" Version="4.*" />
    <PackageVersion Include="Reqnroll.xUnit" Version="2.*" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />
    <PackageVersion Include="Testcontainers.PostgreSql" Version="3.*" />
    <PackageVersion Include="FluentAssertions" Version="7.*" />
    <PackageVersion Include="NSubstitute" Version="5.*" />
    <PackageVersion Include="xunit" Version="2.*" />
  </ItemGroup>
</Project>
```

### 5. Créer les fichiers Shared.Kernel de base

Créer dans `Shared/Vetolib.Shared.Kernel/` :

- `BaseEntity.cs` — Id (Guid), CreatedAt, UpdatedAt, domain events list
- `IMultiTenant.cs` — `Guid ClinicId { get; }`
- `IClinicContext.cs` — `Guid ClinicId { get; }`
- `IAggregateRoot.cs` — marqueur vide

Créer dans `Shared/Vetolib.Shared.Infrastructure/` :

- `MultiTenantDbContext.cs` — Global Query Filter automatique (voir skill multitenant-efcore)
- `ClinicContext.cs` — implémentation IClinicContext depuis JWT

### 6. AppHost minimal

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);
var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var db = postgres.AddDatabase("vetolibdb");
builder.AddProject<Projects.Vetolib_Api>("api").WithReference(db).WaitFor(db);
builder.Build().Run();
```

### 7. Vérifier

```bash
dotnet build Vetolib.sln
# Doit compiler avec 0 erreurs (projets vides avec les bons NuGet)

dotnet test Tests/Vetolib.Tests.Acceptance/
# 0 tests = OK à ce stade (aucun binding écrit)
```

---

## Critère de complétion

```
□ dotnet build Vetolib.sln → 0 erreurs
□ dotnet run --project AppHost → dashboard Aspire accessible
□ Tous les projets dans la solution
□ Shared.Kernel contient BaseEntity, IMultiTenant, IClinicContext
□ Shared.Infrastructure contient MultiTenantDbContext, ClinicContext
□ Directory.Packages.props créé à la racine
```

## Ensuite

Renommer ce fichier en `done-scaffold-000.md` et notifier dans `progress.md`.
Les tâches `todo-auth-001.md` et les autres peuvent démarrer en parallèle.
