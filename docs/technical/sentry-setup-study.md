# Sentry Setup Study — Vetolib Backend

**Author**: Architect agent
**Date**: 2026-03-10
**Status**: Final recommendation

---

## 1. Current Observability Stack

| Layer | Technology | Status |
|---|---|---|
| Logging | Serilog (Console sink, JSON in prod) | Active |
| Tracing | OpenTelemetry SDK via Aspire ServiceDefaults | Active |
| Metrics | OpenTelemetry SDK (ASP.NET Core + HTTP + Runtime) | Active |
| OTLP Export | `OpenTelemetry.Exporter.OpenTelemetryProtocol` (env-driven) | Active |
| Dashboard | Aspire Dashboard (dev only) | Active |
| Production dashboard | None | **Gap** |

The gap is clear: in production, telemetry data has no destination. Sentry fills this gap for errors, traces, and logs.

---

## 2. Sentry OpenTelemetry Support — Current State

### 2.1 Native OTLP Ingestion

Sentry supports receiving traces via OTLP (HTTP) since late 2024. The endpoint format is:

```
https://{org}.sentry.io/api/{project_id}/otlp/v1/traces
```

Authentication uses the DSN's public key as a bearer token. **However**, OTLP ingestion has important limitations:

- **Traces**: Fully supported via OTLP.
- **Metrics**: Not supported via OTLP. Sentry's metrics product uses its own SDK protocol.
- **Logs**: Not supported via OTLP. Sentry expects errors/issues via its own envelope protocol.
- **Error grouping**: OTLP traces lack Sentry's fingerprinting, stack trace grouping, and issue deduplication. Raw OTLP spans appear as transactions but unhandled exceptions are poorly grouped.

**Verdict**: Pure OTLP is insufficient. We lose error grouping, breadcrumbs, Serilog integration, and release tracking.

### 2.2 Sentry .NET SDK + OpenTelemetry Bridge

The recommended approach for .NET is:

1. **`Sentry.AspNetCore`** (v5.x) — Core SDK with ASP.NET Core integration. Captures unhandled exceptions, request context, user info, PII scrubbing.
2. **`Sentry.OpenTelemetry`** — Bridge that connects the Sentry SDK to the existing OpenTelemetry `TracerProvider`. Sentry becomes a span processor, consuming OTel spans and enriching them with Sentry context (breadcrumbs, tags, user).
3. **`Sentry.Serilog`** — Serilog sink that sends log events (Warning+) as Sentry events/breadcrumbs.

This approach:
- Reuses the existing OpenTelemetry instrumentation (no duplicate spans).
- Gets full Sentry error grouping, stack traces, breadcrumbs.
- Integrates with Serilog naturally.
- Does NOT conflict with the existing OTLP exporter (both can coexist).

### 2.3 Serilog Sink (`Sentry.Serilog`)

The `Sentry.Serilog` package:
- Captures log events at Warning+ as Sentry issues (with full stack trace if an exception is attached).
- Captures log events at Info+ as breadcrumbs (context trail leading to an error).
- Supports structured logging properties as Sentry tags/extra data.
- Is configurable via `appsettings.json` (no code changes to Serilog setup needed).

---

## 3. Sentry Free Tier Limits (Developer Plan)

| Resource | Free Tier Limit | Our Estimate (10 clinics) |
|---|---|---|
| Errors | 5,000 / month | ~200-500/month (healthy app) |
| Performance units (transactions) | 10,000 / month | ~5,000-15,000/month (depends on traffic) |
| Replays | 50 / month | N/A (backend only) |
| Cron monitors | 1 | 0 needed currently |
| Attachments | 1 GB / month | Minimal |
| Data retention | 30 days | Sufficient for MVP |
| Team members | 1 | Sufficient for solo/small team |
| Rate limiting | Yes, auto-applied | Built-in SDK rate limiting too |

### Risk Assessment

- **Errors (5K)**: Comfortable margin. A healthy app with proper Result pattern (no throw for business flow) should produce very few unhandled errors.
- **Transactions (10K)**: This is the tight limit. With 10 clinics, each averaging 50 API calls/day = 500/day = 15,000/month. **We must configure sampling.**
- **Mitigation**: Set `TracesSampleRate` to 0.3-0.5 in production (30-50% of transactions sampled). This keeps us under 10K while still providing representative performance data.

---

## 4. Architectural Recommendation

### Recommended: Option B — SDK Sentry + OpenTelemetry Bridge + Serilog Sink

This is the only option that provides all three pillars (errors, traces, logs) with proper Sentry features (grouping, breadcrumbs, releases).

```
                    +------------------+
                    |   Sentry Cloud   |
                    |   (Free Tier)    |
                    +--------+---------+
                             ^
                             | Sentry Protocol
                             | (errors, traces, breadcrumbs)
                             |
              +--------------+--------------+
              |                             |
    +---------+----------+    +-------------+-----------+
    | Sentry.OpenTelemetry|    | Sentry.Serilog Sink    |
    | (span processor)   |    | (Warning+ -> Issues)   |
    +--------+-----------+    | (Info+ -> Breadcrumbs)  |
             ^                +-------------+-----------+
             |                              ^
    +--------+-----------+    +-------------+-----------+
    | OpenTelemetry SDK  |    | Serilog Pipeline        |
    | (existing, Aspire) |    | (existing)              |
    +--------------------+    +-------------------------+
```

The existing OTLP exporter to Aspire Dashboard continues to work in parallel (dev only).

### Packages Required

| Package | Target Project | Purpose |
|---|---|---|
| `Sentry.AspNetCore` (5.x) | `Vetolib.Api.csproj` | Core SDK, middleware, unhandled exceptions |
| `Sentry.OpenTelemetry` (5.x) | `Vetolib.ServiceDefaults.csproj` | Bridge OTel spans to Sentry |
| `Sentry.Serilog` (5.x) | `Vetolib.Api.csproj` | Serilog sink for errors + breadcrumbs |

### Why NOT the other options

- **Option A (Pure OTLP)**: No error grouping, no breadcrumbs, no Serilog integration, no metrics support. Sentry becomes a dumb trace viewer.
- **Option C (Serilog-only)**: No traces/performance monitoring. Only captures logged errors, misses unhandled exceptions and request context.
- **Option D (Mix)**: Adds complexity without benefit over Option B. The SDK bridge handles both traces and errors in a unified way.

---

## 5. Integration Points

### 5.1 `ServiceDefaults/Extensions.cs` — Add Sentry span processor

```csharp
// In ConfigureOpenTelemetry(), add to WithTracing():
.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSentry();  // <-- Sentry span processor
})
```

**Note**: `Sentry.OpenTelemetry` must be added as a dependency to `Vetolib.ServiceDefaults.csproj`.

### 5.2 `Vetolib.Api/Program.cs` — Initialize Sentry SDK

```csharp
// Add UseSentry() to the Serilog configuration:
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Vetolib.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Sentry();  // <-- Add Sentry sink

    // ... existing console sinks unchanged ...
});

// Add Sentry SDK initialization:
builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["Sentry:Dsn"] ?? "";
    o.Environment = builder.Environment.EnvironmentName;
    o.TracesSampleRate = builder.Environment.IsProduction() ? 0.3 : 1.0;
    o.SendDefaultPii = false;  // GDPR/PII protection
    o.UseOpenTelemetry();      // Bridge to OTel
});
```

### 5.3 `appsettings.json` — DSN placeholder

```json
{
  "Sentry": {
    "Dsn": "",
    "MinimumBreadcrumbLevel": "Information",
    "MinimumEventLevel": "Warning"
  }
}
```

### 5.4 `appsettings.Production.json` — Production config

```json
{
  "Sentry": {
    "Dsn": "__OVERRIDE_VIA_ENV__"
  }
}
```

DSN injected via environment variable `Sentry__Dsn` in docker-compose.prod.yml.

### 5.5 `appsettings.Development.json` — Disabled in dev

No `Sentry.Dsn` entry = Sentry SDK is disabled. The Aspire Dashboard handles observability in dev.

### 5.6 Module impact

**None.** The integration is fully centralized in `Vetolib.Api` and `ServiceDefaults`. No module code changes required.

---

## 6. PII / Sensitive Data Protection

Sentry SDK configuration must include:

- `SendDefaultPii = false` — Do not send user IPs, cookies, or form data.
- `BeforeSend` callback — Strip any `ClinicId`, `UserId`, or patient data from breadcrumbs if accidentally logged.
- The existing `[SensitiveData]` attribute on domain properties should be respected: ensure Serilog destructuring does not serialize marked properties into Sentry breadcrumbs.

```csharp
o.SetBeforeSend((sentryEvent, hint) =>
{
    // Remove any accidental PII from breadcrumbs
    // The SensitiveDataAttribute-based log redaction already handles Serilog,
    // but this is a defense-in-depth measure.
    return sentryEvent;
});
```

---

## 7. Sampling Strategy

To stay within the 10K transaction limit on the free tier:

| Environment | TracesSampleRate | Rationale |
|---|---|---|
| Development | 1.0 (100%) | Full visibility, data stays in Aspire |
| Production | 0.3 (30%) | ~4,500 transactions/month at 500 req/day |

If traffic grows, reduce to 0.1 or implement `TracesSampler` callback for intelligent sampling (always sample errors, downsample health checks).

---

## 8. Frontend Consideration

The Next.js frontend could also use `@sentry/nextjs` to capture client-side errors. This is out of scope for this task but should be a separate `todo-front-sentry-001.md` task. The same Sentry project can be used with a separate DSN for the frontend.

---

## 9. Summary

| Question | Answer |
|---|---|
| Does Sentry support OTLP ingestion? | Yes for traces only. Insufficient alone. |
| Recommended approach? | SDK (`Sentry.AspNetCore`) + OTel bridge (`Sentry.OpenTelemetry`) + Serilog sink (`Sentry.Serilog`) |
| Free tier sufficient? | Yes, with 30% trace sampling |
| Module code changes needed? | No, fully centralized |
| Packages to add | 3 NuGet packages (Sentry.AspNetCore, Sentry.OpenTelemetry, Sentry.Serilog) |
| Risk | Transaction quota at 10K/month requires sampling discipline |
