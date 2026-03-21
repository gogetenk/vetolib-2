# todo-infra-sentry-impl-001 — Implement Sentry (traces + logs + errors)

**Module** : Infra / Observability
**Priority** : HIGH
**Dependencies** : done-infra-sentry-study-001
**Skills to read** : `dotnet-aspire`, study result `docs/sentry-setup-study.md`
**MODIF_SHARED** : no (ServiceDefaults only, not Shared/)

---

## Context

The study `docs/sentry-setup-study.md` recommends **Option B**: Sentry SDK + OpenTelemetry bridge + Serilog sink. This task implements that recommendation.

**Architecture**: Sentry SDK captures unhandled exceptions and provides error grouping. The OpenTelemetry bridge (`Sentry.OpenTelemetry`) connects to the existing OTel `TracerProvider` from Aspire ServiceDefaults. The Serilog sink (`Sentry.Serilog`) sends Warning+ events as Sentry issues and Info+ as breadcrumbs. All integration is centralized — zero module changes.

---

## Step 1 — Add NuGet packages

### `src/backend/Vetolib.Api/Vetolib.Api.csproj`

```xml
<PackageReference Include="Sentry.AspNetCore" Version="5.*" />
<PackageReference Include="Sentry.Serilog" Version="5.*" />
```

### `src/backend/ServiceDefaults/Vetolib.ServiceDefaults.csproj`

```xml
<PackageReference Include="Sentry.OpenTelemetry" Version="5.*" />
```

Run `dotnet restore` to verify package resolution.

---

## Step 2 — Wire Sentry OpenTelemetry bridge in ServiceDefaults

**File**: `src/backend/ServiceDefaults/Extensions.cs`

In the `ConfigureOpenTelemetry` method, add `Sentry.OpenTelemetry` to the tracing pipeline:

```csharp
using Sentry.OpenTelemetry;

// Inside ConfigureOpenTelemetry():
.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSentry();  // <-- NEW: Sentry span processor
})
```

The `.AddSentry()` call registers Sentry as a span processor. It only activates when the Sentry SDK is initialized (i.e., when a DSN is configured). In development without a DSN, this is a no-op.

---

## Step 3 — Initialize Sentry SDK in Program.cs

**File**: `src/backend/Vetolib.Api/Program.cs`

Add `UseSentry()` on `builder.WebHost` **before** `builder.Build()`:

```csharp
// After builder.AddServiceDefaults() and before builder.Build():
builder.WebHost.UseSentry(options =>
{
    // DSN from config — empty string = SDK disabled (safe for dev)
    options.Dsn = builder.Configuration["Sentry:Dsn"] ?? "";
    options.Environment = builder.Environment.EnvironmentName;

    // Sampling: 30% in prod (free tier = 10K transactions/month)
    // 100% in dev (data stays in Aspire, Sentry disabled without DSN anyway)
    options.TracesSampleRate = builder.Environment.IsProduction() ? 0.3 : 1.0;

    // PII protection — do NOT send user IPs, cookies, form data
    options.SendDefaultPii = false;

    // Bridge to existing OpenTelemetry SDK
    options.UseOpenTelemetry();
});
```

---

## Step 4 — Add Serilog Sentry sink in Program.cs

**File**: `src/backend/Vetolib.Api/Program.cs`

In the existing `UseSerilog(...)` block, add the Sentry sink:

```csharp
using Sentry.Serilog;

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Vetolib.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Sentry();  // <-- NEW: sends Warning+ as issues, Info+ as breadcrumbs

    // ... existing console sinks unchanged ...
});
```

The Sentry sink reads its configuration (MinimumBreadcrumbLevel, MinimumEventLevel) from the `Sentry` section in `appsettings.json`.

---

## Step 5 — Configure appsettings

### `appsettings.json` — Add Sentry section (DSN empty = disabled)

```json
{
  "Sentry": {
    "Dsn": "",
    "MinimumBreadcrumbLevel": "Information",
    "MinimumEventLevel": "Warning"
  }
}
```

### `appsettings.Production.json` — DSN via env var

```json
{
  "Sentry": {
    "Dsn": "__OVERRIDE_VIA_ENV__"
  }
}
```

### `appsettings.Development.json` — No changes needed

No Sentry DSN in dev = SDK disabled. Aspire Dashboard handles observability locally.

---

## Step 6 — Docker / Production configuration

### `docker-compose.prod.yml` — Add SENTRY_DSN env var

```yaml
services:
  backend:
    environment:
      - Sentry__Dsn=${SENTRY_DSN}
```

### `.env.example` — Document SENTRY_DSN

Add at the end:

```bash
# -- Sentry (error tracking + traces) -----------------------------------------

# Sentry DSN for the backend. Get it from https://sentry.io > Project Settings > Client Keys.
# Leave empty to disable Sentry (safe for local dev).
SENTRY_DSN=
```

---

## Step 7 — Verification checklist

1. **Local dev (no DSN)**: Run `dotnet run` — app starts normally, no Sentry errors in logs, Aspire Dashboard still works.
2. **With DSN**: Set `Sentry__Dsn` env var to a real Sentry DSN.
   - Trigger a 500 error (e.g., invalid endpoint) -> verify it appears in Sentry Issues.
   - Make normal API calls -> verify traces appear in Sentry Performance.
   - Log a Warning via Serilog -> verify it appears as a Sentry event.
3. **CI**: Ensure all existing tests pass (Sentry SDK is disabled without DSN, so tests are unaffected).

---

## Files to modify

| File | Change |
|---|---|
| `src/backend/Vetolib.Api/Vetolib.Api.csproj` | Add `Sentry.AspNetCore`, `Sentry.Serilog` |
| `src/backend/ServiceDefaults/Vetolib.ServiceDefaults.csproj` | Add `Sentry.OpenTelemetry` |
| `src/backend/ServiceDefaults/Extensions.cs` | Add `.AddSentry()` to tracing pipeline |
| `src/backend/Vetolib.Api/Program.cs` | Add `UseSentry()` + `.WriteTo.Sentry()` |
| `src/backend/Vetolib.Api/appsettings.json` | Add `Sentry` config section |
| `src/backend/Vetolib.Api/appsettings.Production.json` | Add `Sentry.Dsn` placeholder |
| `docker-compose.prod.yml` | Add `Sentry__Dsn` env var |
| `.env.example` | Document `SENTRY_DSN` |

## Files NOT to modify

- No module code (`Modules/*/`)
- No Shared code (`Shared/`)
- No frontend code (`src/frontend/`)
- No test code (Sentry is disabled without DSN)

---

## Completion criteria

- [ ] `Sentry.AspNetCore`, `Sentry.OpenTelemetry`, `Sentry.Serilog` packages added
- [ ] Sentry SDK initialized in Program.cs with `UseOpenTelemetry()` bridge
- [ ] Serilog Sentry sink active (Warning+ as issues, Info+ as breadcrumbs)
- [ ] OTel tracing pipeline includes `.AddSentry()` span processor
- [ ] DSN configurable via env var `Sentry__Dsn` (not hardcoded)
- [ ] `SendDefaultPii = false` (no PII sent to Sentry)
- [ ] `TracesSampleRate = 0.3` in production (free tier quota protection)
- [ ] App starts normally without DSN (dev mode, Sentry disabled)
- [ ] CI GREEN (all existing tests pass)
- [ ] `docker-compose.prod.yml` and `.env.example` updated
- [ ] PR opened with Sentry dashboard screenshot showing a captured error
