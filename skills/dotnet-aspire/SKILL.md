# Skill: .NET Aspire — Orchestration locale et transition microservices

## Rôle d'Aspire dans le projet

.NET Aspire est l'orchestrateur local. Il remplace docker-compose.
Un seul `F5` démarre : l'API, PostgreSQL, pgAdmin, et le dashboard Aspire.
Il injecte automatiquement les connection strings dans chaque projet.

## Structure des projets Aspire

```
Vetolib.sln
├── AppHost/                        ← orchestrateur (lance tout)
│   └── Program.cs
└── ServiceDefaults/                ← shared config (telemetry, health, discovery)
    └── Extensions.cs
```

## AppHost — configuration

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL — Aspire crée le container Docker automatiquement
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()                  // UI pgAdmin locale accessible
    .WithDataVolume("vetolib-data") // persistance entre redémarrages
    .WithEnvironment("POSTGRES_PASSWORD", "dev-password");

// Base de données
var db = postgres.AddDatabase("vetolibdb");

// API — référence la DB (Aspire injecte la connection string)
var api = builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WaitFor(db)                    // attend que PostgreSQL soit prêt
    .WithEnvironment("JWT_SECRET", "dev-secret-32chars-minimum");

// Frontend Next.js — si développé en parallèle
// builder.AddNpmApp("frontend", "../vetolib-frontend")
//     .WithReference(api)
//     .WithHttpEndpoint(port: 3000);

builder.Build().Run();
```

## ServiceDefaults — extensions partagées

```csharp
// ServiceDefaults/Extensions.cs
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // OpenTelemetry (traces, metrics, logs) → Aspire Dashboard
        builder.ConfigureOpenTelemetry();

        // Health checks → /health et /alive
        builder.AddDefaultHealthChecks();

        // Service Discovery (pour la transition microservices)
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
        return app;
    }
}
```

## Vetolib.Api — intégration Aspire

```csharp
// Vetolib.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// OBLIGATOIRE — doit être appelé en premier
builder.AddServiceDefaults();

// PostgreSQL — Aspire injecte la connection string via le nom "vetolibdb"
// En dev : la string est générée automatiquement par Aspire
// En prod : définie dans appsettings ou variables d'environnement
builder.AddNpgsqlDbContext<AuthDbContext>("vetolibdb",
    configureDbContextOptions: opts => opts.UseNpgsql(npgsqlOpts =>
        npgsqlOpts.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

builder.AddNpgsqlDbContext<AgendaDbContext>("vetolibdb",
    configureDbContextOptions: opts => opts.UseNpgsql(npgsqlOpts =>
        npgsqlOpts.MigrationsHistoryTable("__EFMigrationsHistory", "agenda")));

// ... autres DbContexts
```

## Aspire Dashboard — ce qu'on voit

Le dashboard Aspire (http://localhost:18888 par défaut) affiche :
- **Resources** : état de chaque service (api, postgres)
- **Console logs** : logs structurés de tous les services centralisés
- **Traces** : distributed traces avec spans (requête → handler → DB)
- **Metrics** : CPU, mémoire, requêtes/sec
- **Endpoints** : URLs de chaque service

C'est le seul outil de monitoring en développement. Pas besoin de Sentry/DataDog en local.

## Migrations EF Core avec Aspire

En développement, appliquer les migrations au démarrage :

```csharp
// AppHost/Program.cs — pour apply auto en dev
var api = builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WaitFor(db);

// Vetolib.Api/Program.cs — appliquer au démarrage en dev uniquement
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<AgendaDbContext>().Database.Migrate();
    // ...
}
```

## Transition vers microservices — comment Aspire aide

Quand un module doit devenir un microservice indépendant :

```csharp
// AVANT (monolithe) : AppHost/Program.cs
var api = builder.AddProject<Projects.Vetolib_Api>("api").WithReference(db);

// APRÈS (Agenda extrait en microservice) : AppHost/Program.cs
var agendaApi = builder.AddProject<Projects.Vetolib_Agenda_Api>("agenda")
    .WithReference(db)
    .WithReference(bus);  // si on ajoute un message bus

var mainApi = builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WithReference(agendaApi);  // Service Discovery injecte l'URL
```

Les autres modules utilisent déjà `IAppointmentService` via les Contracts.
L'implémentation passe de in-process à HTTP sans changer le code des modules.

## Health checks — conventions

```csharp
// Dans chaque ModuleServiceRegistrar, ajouter un health check DB
services.AddHealthChecks()
    .AddDbContextCheck<AgendaDbContext>("agenda-db", tags: new[] { "db", "ready" });
```

## Commandes utiles

```bash
# Lancer tout le projet (AppHost)
dotnet run --project AppHost

# Dashboard Aspire (par défaut)
# http://localhost:18888

# Publier pour déploiement (génère docker-compose, k8s manifests, bicep)
dotnet publish AppHost --output ./deploy
aspire publish  # si CLI installée

# Ajouter Aspire à un projet existant
dotnet add package Aspire.Hosting.AppHost
```

## NuGet packages requis

```xml
<!-- AppHost/AppHost.csproj -->
<PackageReference Include="Aspire.Hosting.AppHost" Version="9.*" />
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.*" />

<!-- ServiceDefaults/ServiceDefaults.csproj -->
<PackageReference Include="Microsoft.Extensions.ServiceDiscovery" Version="9.*" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.*" />

<!-- Vetolib.Api/Vetolib.Api.csproj -->
<PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
```
