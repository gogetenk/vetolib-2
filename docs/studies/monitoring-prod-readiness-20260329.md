# Monitoring & Observability — Production Readiness Report

**Date**: 2026-03-29
**Author**: Architecture study (Claude)
**Status**: Assessment only — no implementation

---

## 1. Current State

### 1.1 What is already in place

| Pillar | Technology | Status | Notes |
|---|---|---|---|
| Structured logging | Serilog (Console sink dev, JSON prod) + Sentry.Serilog sink | **Active** | JSON on stdout in production; human-readable in dev |
| Distributed tracing | OpenTelemetry SDK (ASP.NET Core + HttpClient instrumentation) | **Active** | Spans exported via OTLP; Sentry span processor attached |
| Metrics | OpenTelemetry (ASP.NET Core + HttpClient + Runtime) | **Active** | OTLP export if `OTEL_EXPORTER_OTLP_ENDPOINT` is set |
| Error tracking | Sentry SDK v6.1.0 (AspNetCore + OpenTelemetry bridge + Serilog) | **Active** | DSN empty by default; injected via `Sentry__Dsn` env var in prod |
| Health checks | ASP.NET Core health checks via ServiceDefaults | **Partial** | Only a self-check ("self" = always healthy); no DB/RabbitMQ checks |
| Dev dashboard | .NET Aspire Dashboard (traces, metrics, logs) | **Active** | Dev only; receives OTLP data automatically |
| Rate limiting | ASP.NET Core sliding/fixed window (auth, signup, api) | **Active** | 3 policies configured |
| CI quality gates | SonarCloud + security scan + code quality | **Active** | Coverage from 3 test layers merged |

### 1.2 Sentry configuration (Program.cs)

```
DSN: empty by default, injected via Sentry__Dsn env var
Environment: auto from ASPNETCORE_ENVIRONMENT
TracesSampleRate: 0.3 in Production, 1.0 in Development
SendDefaultPii: false
EnableLogs: true
OpenTelemetry bridge: active (.UseOpenTelemetry())
Serilog sink: active (.WriteTo.Sentry())
```

**Packages installed** (Vetolib.Api.csproj): `Sentry.AspNetCore 6.1.0`, `Sentry.OpenTelemetry 6.1.0`, `Sentry.Serilog 6.1.0`
**Package installed** (ServiceDefaults.csproj): `Sentry.OpenTelemetry 6.1.0`

### 1.3 OpenTelemetry instrumentation (ServiceDefaults/Extensions.cs)

- **Tracing**: ASP.NET Core + HttpClient + Sentry span processor
- **Metrics**: ASP.NET Core + HttpClient + .NET Runtime
- **Logging**: OTel logging bridge with formatted messages and scopes
- **Export**: OTLP exporter conditionally enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is set

### 1.4 Health checks (ServiceDefaults/Extensions.cs)

Two endpoints, **only exposed in Development**:
- `GET /health` — runs all health checks (currently just "self")
- `GET /alive` — liveness check (tag: "live")

The "self" check always returns `Healthy` — it is a placeholder, not a real dependency check.

### 1.5 Logging coverage across modules

Of the 10 modules (AI, Agenda, Auth, Billing, Breeding, MedicalRecords, Messaging, Notifications, Preferences, Stock), structured `ILogger<T>` usage was found in **6 modules** (AI, Agenda, Auth, Billing, Messaging, Notifications, Preferences). The following modules have **zero or minimal logging**:

- **Breeding** — no ILogger usage found
- **MedicalRecords** — no ILogger usage found
- **Stock** — no ILogger usage found

Modules with logging concentrate it in background services, consumers, and external integrations (Claude API, WhatsApp, SMTP). Most CQRS handlers do not log — they rely on the Result pattern for business flow.

### 1.6 Production docker-compose (infra/docker-compose.prod.yml)

The production override injects `Sentry__Dsn` from `${SENTRY_DSN}` environment variable. No other monitoring infrastructure is defined (no Prometheus, no Grafana, no Loki, no alerting).

### 1.7 Deploy pipeline (.github/workflows/deploy.yml)

- Builds and pushes images to GHCR
- Staging deploy + smoke test (both are **mock/TODO** — not real)
- Production deploy with manual approval gate (also **mock/TODO**)
- Rollback job (also **mock/TODO**)
- No post-deploy health verification against actual endpoints
- No Sentry release tracking integration

---

## 2. Gaps for Production

### Gap 1: Health checks are incomplete and dev-only (CRITICAL)

**Problem**: Health check endpoints are only mapped in Development (`if (app.Environment.IsDevelopment())`). In production, `/health` and `/alive` return 404. Furthermore, all 11 DbContext registrations explicitly set `DisableHealthChecks = true`, and RabbitMQ has no health check.

**Impact**: No way for load balancers, orchestrators, or external monitors to verify the application is alive and functional. A database outage would go undetected until users complain.

**Evidence**: `ServiceDefaults/Extensions.cs` line 83: `if (app.Environment.IsDevelopment())` wraps both health endpoints. `Program.cs` lines 108-233: every `EnrichNpgsqlDbContext` call disables health checks.

### Gap 2: No production-grade log aggregation (HIGH)

**Problem**: In production, Serilog writes JSON to stdout. There is no configured destination for these logs — no Loki, no ELK, no CloudWatch. The Serilog OpenTelemetry sink (`Serilog.Sinks.OpenTelemetry`) mentioned in `done-infra-otel-serilog-sink-001.md` was planned but the package is **not present** in `Vetolib.Api.csproj`. Logs only go to container stdout and Sentry (Warning+).

**Impact**: No searchable log history in production. Debugging incidents requires SSH-ing into containers and tailing stdout. Sentry only captures Warning+ events, so Information-level diagnostic logs are lost.

### Gap 3: No BeforeSend PII filter in Sentry (MEDIUM)

**Problem**: The existing Sentry study (`docs/technical/sentry-setup-study.md`) recommends a `BeforeSend` callback to strip ClinicId, UserId, and patient data from breadcrumbs. This is **not implemented** — `Program.cs` has `SendDefaultPii = false` but no `BeforeSend` callback.

**Impact**: Structured log properties (ClinicId, patient names) may leak into Sentry breadcrumbs if a module logs them at Info level before an error occurs. GDPR/data protection risk for UAE operations.

### Gap 4: No alerting rules configured (HIGH)

**Problem**: Zero alerting configuration exists anywhere in the codebase. The monitoring doc (`docs/technical/monitoring.md`) documents recommended thresholds (5xx rate > 1%, P99 > 2s, etc.) but nothing implements them. Sentry has built-in alert rules but none are configured via the project.

**Impact**: Production errors, performance degradation, and outages are only discovered when users report them or when someone manually checks Sentry/logs.

### Gap 5: No uptime monitoring (HIGH)

**Problem**: No external uptime check exists (no UptimeRobot, Pingdom, Healthchecks.io, or similar). The deploy pipeline's smoke tests are mocked TODOs that echo strings.

**Impact**: If the entire application goes down (server crash, DNS failure, certificate expiry), nobody is notified.

### Gap 6: No EF Core / database instrumentation in OTel (MEDIUM)

**Problem**: OpenTelemetry tracing instruments ASP.NET Core and HttpClient, but **not Entity Framework Core**. The `AddEntityFrameworkCoreInstrumentation()` call is absent from `ServiceDefaults/Extensions.cs`.

**Impact**: Database query performance is invisible in traces. Slow queries, N+1 problems, and connection pool exhaustion cannot be diagnosed from trace data.

### Gap 7: No MassTransit / RabbitMQ instrumentation (MEDIUM)

**Problem**: MassTransit has built-in OpenTelemetry support, but no `.AddSource("MassTransit")` or `AddMassTransitInstrumentation()` is configured. RabbitMQ message processing is invisible in traces.

**Impact**: Integration event delivery failures, slow consumers, and message processing latency are not observable. The Notifications, Messaging, and Billing modules rely heavily on MassTransit consumers.

### Gap 8: No custom business metrics (LOW)

**Problem**: Only standard ASP.NET Core / HttpClient / Runtime metrics are collected. No custom metrics for business KPIs (appointments created/day, invoices generated, AI triage response time, message classification accuracy).

**Impact**: No data-driven insight into business health. Product decisions must rely on database queries rather than real-time dashboards.

### Gap 9: Frontend has no observability (MEDIUM)

**Problem**: The Next.js frontend has zero Sentry or error tracking integration. No `@sentry/nextjs` package is installed. Client-side JavaScript errors, slow page loads, and failed API calls are invisible.

**Impact**: Frontend bugs are only discovered when users report them. No visibility into client-side performance or error rates.

### Gap 10: Deploy pipeline has no real smoke tests or Sentry release (LOW)

**Problem**: The deploy workflow's smoke tests are TODO stubs. There is no Sentry release creation (`sentry-cli releases`) to correlate deployments with error rates.

**Impact**: Cannot use Sentry's "release health" feature to detect regressions after deployment. No automated validation that a deploy succeeded.

### Gap 11: No readiness endpoint distinct from liveness (MEDIUM)

**Problem**: The documentation (`docs/technical/monitoring.md`) describes `/health/ready` (checks DB connectivity) and `/health/live` (process alive), but the actual code only has `/health` (all checks) and `/alive` (live tag). There is no "ready" tag and no DB connectivity check registered.

**Impact**: Kubernetes/orchestrator readiness probes cannot properly gate traffic until the database is reachable.

---

## 3. Recommended Setup

### Target architecture

```
                        External
                     +-------------+
                     | UptimeRobot |---> GET /health (every 60s)
                     +-------------+
                           |
                           v (alert on failure)
                     +-------------+
                     |   PagerDuty |<--- Sentry webhook alerts
                     |   / Slack   |<--- Grafana alert rules
                     +-------------+

   Frontend (Next.js)                    Backend (ASP.NET Core)
   +------------------+                  +------------------------+
   | @sentry/nextjs   |---errors------->|  Sentry Cloud          |
   | (client + server)|                 |  - Errors & issues     |
   +------------------+                 |  - Traces (30% sample) |
                                        |  - Breadcrumbs         |
                                        |  - Release health      |
                                        +------------------------+
                                                  ^
                                                  | Sentry SDK
                                                  |
                                        +------------------------+
                                        | Vetolib.Api            |
                                        | - Sentry.AspNetCore    |
                                        | - Sentry.OpenTelemetry |
                                        | - Sentry.Serilog       |
                                        +------------------------+
                                                  |
                                          OTel SDK (OTLP)
                                                  |
                                                  v
                                        +------------------------+
                                        | Grafana Cloud (free)   |
                                        | OR self-hosted stack   |
                                        | - Grafana (dashboards) |
                                        | - Loki (logs)          |
                                        | - Prometheus (metrics) |
                                        | - Tempo (traces)       |
                                        +------------------------+
```

### Option A: Sentry + Grafana Cloud Free Tier (recommended for MVP)

- **Sentry** (already integrated): errors, traces, release health
- **Grafana Cloud free tier**: 50 GB logs, 10K metrics series, 50 GB traces — sufficient for 10 clinics
- **UptimeRobot free tier**: 50 monitors, 5-minute intervals
- **Total cost**: $0/month for the monitoring stack itself

### Option B: Sentry + Self-hosted Grafana/Loki/Prometheus

- Same as Option A but self-hosted on the same VPS
- More control, no data leaves your infrastructure
- Requires ~1 GB additional RAM for the Grafana stack
- Higher maintenance burden

### Option C: Sentry + Aspire Dashboard in production

- .NET Aspire Dashboard can run as a standalone OTLP collector
- Lightweight, no additional infrastructure
- **Not recommended**: no persistent storage, no alerting, no multi-user access

---

## 4. Priority Implementation Order

### P0 — Must have before first paying customer

| # | Item | Effort | Description |
|---|---|---|---|
| 1 | **Fix health checks for production** | 1h | Remove the `IsDevelopment()` guard. Add PostgreSQL health check. Add RabbitMQ health check. Register proper `/health`, `/health/ready`, `/health/live` endpoints. |
| 2 | **External uptime monitoring** | 30m | Set up UptimeRobot (free) to ping `https://api.vetolib.ae/health` every 60 seconds. Configure email/Slack alert on failure. |
| 3 | **Sentry alert rules** | 1h | Configure in Sentry dashboard: (a) alert on new issue, (b) alert if error rate > 1% over 5 minutes, (c) alert if P95 transaction duration > 3 seconds. Connect to Slack/email. |
| 4 | **Sentry BeforeSend PII filter** | 30m | Add `SetBeforeSend` callback to strip sensitive properties (patient names, email, phone) from breadcrumbs before they leave the server. |
| 5 | **Sentry release tracking in deploy pipeline** | 30m | Add `sentry-cli releases new` and `sentry-cli releases finalize` steps to `deploy.yml`. Tag each deploy with the git SHA. |

### P1 — Should have within first month of production

| # | Item | Effort | Description |
|---|---|---|---|
| 6 | **EF Core OTel instrumentation** | 30m | Add `OpenTelemetry.Instrumentation.EntityFrameworkCore` to ServiceDefaults. Call `.AddEntityFrameworkCoreInstrumentation()` in tracing config. Enables DB query visibility in traces. |
| 7 | **MassTransit OTel instrumentation** | 30m | MassTransit auto-instruments when OTel is present, but verify `.AddSource("MassTransit")` is registered so consumer spans appear in Sentry/Grafana. |
| 8 | **Log aggregation (Grafana Cloud or Loki)** | 2h | Add `Serilog.Sinks.OpenTelemetry` or `Serilog.Sinks.Grafana.Loki` to ship logs to a searchable store. Alternative: configure `OTEL_EXPORTER_OTLP_ENDPOINT` to point to Grafana Cloud OTLP endpoint. |
| 9 | **Frontend Sentry integration** | 2h | Install `@sentry/nextjs`. Configure in `next.config.js`. Create a separate Sentry project for the frontend. Captures client-side errors, slow navigations, web vitals. |
| 10 | **Real smoke tests in deploy pipeline** | 1h | Replace mock steps with real `curl` calls to staging/production health endpoints. Fail the deploy if health check returns non-200. |

### P2 — Nice to have for operational maturity

| # | Item | Effort | Description |
|---|---|---|---|
| 11 | **Grafana dashboards** | 3h | Build dashboards for: request rate/latency (RED method), DB query performance, MassTransit consumer lag, and business KPIs. |
| 12 | **Custom business metrics** | 2h | Add `Meter` + `Counter`/`Histogram` instruments for: appointments created, invoices issued, AI triage calls, message classification accuracy. |
| 13 | **Logging coverage in silent modules** | 1h | Add structured logging to Breeding, MedicalRecords, and Stock modules — at minimum for command handlers that mutate data. |
| 14 | **Structured log correlation IDs** | 1h | Ensure every log entry includes `TraceId` and `SpanId` from the current OTel context, enabling log-to-trace correlation in Grafana/Sentry. |
| 15 | **SLA monitoring** | 2h | Track Messaging module SLA targets (emergency: 15 min, medical question: 8h, etc.) as custom metrics. Alert when SLA breach rate exceeds threshold. |

---

## 5. Quick Wins (< 30 minutes each)

1. **Remove `IsDevelopment()` guard** from `MapDefaultEndpoints` in `ServiceDefaults/Extensions.cs` (line 83). Health endpoints must be available in all environments.

2. **Enable at least one DbContext health check** — change one `DisableHealthChecks = true` to `false` (e.g., AuthDbContext). This gives a real PostgreSQL connectivity check.

3. **Sign up for UptimeRobot** (free) and add a monitor pointing to the production health endpoint.

4. **Create a Sentry alert rule** via the Sentry web UI: "Alert me on every new issue" — takes 2 minutes.

---

## 6. Files Reviewed

| File | Key finding |
|---|---|
| `src/backend/Vetolib.Api/Program.cs` | Sentry fully configured (SDK + OTel bridge + Serilog sink). Health checks disabled on all DbContexts. |
| `src/backend/ServiceDefaults/Extensions.cs` | OTel tracing/metrics/logging active. Health endpoints gated behind `IsDevelopment()`. Only "self" check registered. |
| `src/backend/AppHost/Program.cs` | Aspire orchestration for dev: Postgres + RabbitMQ + MailHog. No monitoring services. |
| `src/backend/Vetolib.Api/appsettings.json` | Sentry DSN empty by default. Breadcrumb level: Information, Event level: Warning. |
| `src/backend/Vetolib.Api/appsettings.Production.json` | Sentry DSN + Serilog levels configured for production. |
| `infra/docker-compose.prod.yml` | Injects `SENTRY_DSN` env var. No monitoring stack (no Grafana/Loki/Prometheus). |
| `docker-compose.yml` | Dev compose with Postgres + Caddy. No monitoring containers. |
| `.github/workflows/deploy.yml` | Smoke tests are TODO mocks. No Sentry release integration. |
| `.github/workflows/ci.yml` | SonarCloud + security scan. No monitoring-related CI steps. |
| `docs/technical/monitoring.md` | Documents architecture but describes endpoints (`/health/ready`) that don't exist in code. |
| `docs/technical/sentry-setup-study.md` | Thorough Sentry study from 2026-03-10. Recommendations largely implemented. BeforeSend PII filter recommended but not implemented. |

---

## 7. Summary

The Vetolib backend has a **solid foundation** for observability: Serilog structured logging, OpenTelemetry tracing+metrics, and Sentry error tracking are all wired up correctly. The Sentry integration follows the recommended SDK + OTel bridge pattern with appropriate sampling (30% in production).

However, there are **critical gaps** that must be addressed before production:

1. **Health checks are invisible in production** (dev-only + no real dependency checks)
2. **No external uptime monitoring** (nobody is alerted if the app goes down)
3. **No alerting rules** (errors accumulate silently in Sentry)
4. **No PII filtering in Sentry breadcrumbs** (data protection risk)
5. **No log aggregation beyond Sentry** (Information-level logs are lost)

The recommended path is: fix health checks (1h) + UptimeRobot (30m) + Sentry alerts (1h) = **~2.5 hours to reach minimum production viability** for monitoring. The remaining items (Grafana, EF Core instrumentation, frontend Sentry, business metrics) can be tackled incrementally in the first month.
