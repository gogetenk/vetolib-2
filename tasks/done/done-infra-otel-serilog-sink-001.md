# todo-infra-otel-serilog-sink-001 -- Bridge Serilog vers OpenTelemetry OTLP

**Module** : Infrastructure / ServiceDefaults / Vetolib.Api
**Priorite** : importante
**Dependances** : done-infra-monitoring-001, todo-infra-log-redaction-001
**Skills a lire** : `dotnet-aspire`

---

## Contexte

Actuellement, Serilog ecrit sur stdout (console dev / JSON prod) et OpenTelemetry est configure
separement dans `ServiceDefaults/Extensions.cs` pour les traces et metriques uniquement.
Les **logs** ne sont pas envoyes via OTLP -- ils ne remontent pas dans le dashboard Aspire ni
dans un collecteur OpenTelemetry externe (Grafana Loki, Datadog, etc.).

Le pipeline OTel Aspire existant (`AddOpenTelemetry().WithMetrics().WithTracing()`) ne doit
**pas etre casse**. Le sink Serilog OpenTelemetry s'ajoute **en parallele** des sinks existants.

## Perimetre exact

### 1. NuGet a ajouter

Dans `Vetolib.Api.csproj` :
```xml
<PackageReference Include="Serilog.Sinks.OpenTelemetry" Version="4.*" />
```

### 2. Configuration Serilog dans Program.cs

Ajouter le sink OpenTelemetry dans le bloc `builder.Host.UseSerilog(...)`, **apres** les sinks
console existants :

```csharp
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Vetolib.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithSensitiveDataMasking(...); // de todo-infra-log-redaction-001

    if (context.HostingEnvironment.IsDevelopment())
    {
        config.WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}");
    }
    else
    {
        config.WriteTo.Console(new JsonFormatter());
    }

    // NOUVEAU : Bridge vers OpenTelemetry OTLP
    // L'endpoint est injecte par Aspire via OTEL_EXPORTER_OTLP_ENDPOINT.
    // Si l'env var n'est pas definie (ex: tests), le sink est simplement ignore.
    var otlpEndpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        config.WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = otlpEndpoint;
            options.Protocol = OtlpProtocol.Grpc;
            options.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"] = "Vetolib.Api",
                ["service.version"] = typeof(Program).Assembly
                    .GetName().Version?.ToString() ?? "0.0.0"
            };
        });
    }
});
```

### 3. Interaction avec le pipeline OTel existant dans ServiceDefaults

**Ne rien modifier dans `ServiceDefaults/Extensions.cs`.**

Le pipeline `AddOpenTelemetry().WithMetrics().WithTracing()` continue de gerer les traces et
metriques via le SDK OpenTelemetry standard. Le sink Serilog OpenTelemetry envoie les **logs**
directement au collecteur OTLP -- c'est l'approche recommandee par Serilog (pas de double
bridge ILogger -> OTel -> OTLP).

Architecture resultante :
```
Traces/Metriques : ASP.NET Core -> OTel SDK (ServiceDefaults) -> OTLP endpoint
Logs            : Serilog -> Serilog.Sinks.OpenTelemetry       -> OTLP endpoint (meme)
                          -> Console (dev) / JSON stdout (prod)
```

Les deux convergent vers le meme `OTEL_EXPORTER_OTLP_ENDPOINT` (dashboard Aspire en dev,
collecteur en prod).

### 4. Verification dans appsettings

Aucune modification necessaire dans `appsettings.json`. L'endpoint OTLP est injecte par Aspire
via variable d'environnement, pas par configuration fichier.

## Tests

### Test fonctionnel

```
1. Lancer `dotnet run --project src/backend/AppHost`
2. Ouvrir le dashboard Aspire (https://localhost:XXXXX)
3. Naviguer vers l'onglet "Structured logs"
4. Verifier que les logs Serilog apparaissent avec :
   - La propriete "Application" = "Vetolib.Api"
   - La propriete "Environment" = "Development"
   - Les proprietes sensibles redactees (si todo-infra-log-redaction-001 est fait)
5. Faire un POST /api/auth/login avec des identifiants invalides
6. Verifier que le log d'echec apparait dans le dashboard sans email en clair
```

### Test unitaire

```csharp
[Fact]
public void OpenTelemetry_sink_is_configured_only_when_OTLP_endpoint_exists()
{
    // Verify that when OTEL_EXPORTER_OTLP_ENDPOINT is not set,
    // no OpenTelemetry sink is added (no exception, no crash).
    // This ensures tests and non-Aspire environments work correctly.
}
```

## Critere de completion

```
[] NuGet Serilog.Sinks.OpenTelemetry ajoute dans Vetolib.Api.csproj
[] Sink OpenTelemetry configure dans Program.cs (conditionnel sur OTEL_EXPORTER_OTLP_ENDPOINT)
[] ServiceDefaults/Extensions.cs NON modifie
[] Dashboard Aspire affiche les logs structures Serilog
[] Sans OTEL_EXPORTER_OTLP_ENDPOINT : app demarre sans erreur (tests, docker compose sans Aspire)
[] Les logs dans le dashboard respectent la redaction (depend de todo-infra-log-redaction-001)
[] Renommer en done-infra-otel-serilog-sink-001.md
```
