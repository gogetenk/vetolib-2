# Task: Keycloak OpenTelemetry tracing + metrics

**Module:** AppHost + ServiceDefaults
**Priority:** MEDIUM (Keycloak Phase 1)
**Source:** docs/studies/keycloak-aspire-migration-20260401.md — 1.2
**Depends on:** done-kc-poc-aspire-setup-061

## Scope
1. Add OTel env vars to Keycloak container in AppHost:
   - KC_TRACING_ENABLED=true
   - OTEL_EXPORTER_OTLP_ENDPOINT (point to Aspire collector)
   - OTEL_SERVICE_NAME=keycloak
2. Verify Keycloak auth flow traces appear in Aspire Dashboard
3. Enable Prometheus metrics endpoint on Keycloak (KC_METRICS_ENABLED already set)

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] Keycloak OTel env vars configured in AppHost
