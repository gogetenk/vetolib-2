# todo-back-ai-scaffold-001.md — Scaffold module Vetolib.AI

**Module** : AI (nouveau)
**Dependances** : aucune
**Priorite** : HAUTE (Phase 2 — debloque AI Triage + No-show + Messaging)
**Skills a lire** : `ardalis-result`, `ardalis-modular-monolith`, `dotnet-aspire`
MODIF_GELE: autorise (AppHost/Program.cs, Vetolib.Api/Program.cs)

---

## Objectif

Creer le module AI avec la structure 2 assemblies, integrer Microsoft.Extensions.AI + Aspire Ollama.

## Spec de reference

`docs/AI-FEATURES-SPEC.md` sections 2.1-2.5

## Implementation

### Structure

```
Modules/AI/
  Vetolib.AI.Contracts/
    Vetolib.AI.Contracts.csproj
    TriageSuggestionDto.cs
    AISeverity.cs (enum: Emergency, Normal, Routine)
    IAITriageService.cs (interface)
  Vetolib.AI/
    Vetolib.AI.csproj
    Application/ (vide pour l'instant)
    Infrastructure/
      AIDbContext.cs (herite MultiTenantDbContext)
    ModuleServiceRegistrar.cs (public, seule classe public)
```

### Fichiers geles modifies

1. **AppHost/Program.cs** : ajout Ollama
   ```csharp
   var ollama = builder.AddOllama("ollama")
       .AddModel("llama3.1")
       .WithDataVolume();
   ```
   + `.WithReference(ollama)` sur le projet API

2. **Vetolib.Api/Program.cs** : ajout module AI
   ```csharp
   builder.AddNpgsqlDbContext<AIDbContext>("vetolibdb");
   builder.Services.AddAIModule(builder.Configuration);
   app.MapAIEndpoints();
   ```

### NuGet packages

- `Microsoft.Extensions.AI.Abstractions` → AI.Contracts
- `Microsoft.Extensions.AI.OpenAI` → AI runtime
- `Aspire.Hosting.Ollama` → AppHost

### Registration IChatClient

Dans `ModuleServiceRegistrar.cs` :
- Pipeline : UseOpenTelemetry → UseFunctionInvocation → UseDistributedCache → provider concret
- Dual mode : Ollama (dev) / Azure OpenAI (prod) via configuration

### Telemetrie

- Custom meter `Vetolib.AI` dans ServiceDefaults/Extensions.cs : `metrics.AddMeter("Vetolib.AI")`

## Critere

```
[] Structure 2 assemblies creee
[] AIDbContext herite MultiTenantDbContext
[] ModuleServiceRegistrar seule classe public
[] IChatClient enregistre avec pipeline OTel
[] Aspire orchestre Ollama (AppHost)
[] Module enregistre dans Program.cs
[] dotnet build passe
[] Renommer en done
```
