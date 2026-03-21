# Monitoring et observabilité — Vetolib

## Architecture de monitoring

Vetolib utilise .NET Aspire comme couche d'orchestration locale. Le **Aspire Dashboard** (accessible sur `http://localhost:18888` par défaut) centralise :
- Traces distribuées (OpenTelemetry OTLP)
- Métriques (histogrammes, compteurs)
- Logs structurés

En production (docker compose ou déploiement cloud), les logs JSON sont émis sur stdout et peuvent être collectés par n'importe quel aggregateur compatible (Loki, ELK, Datadog…).

---

## Logging — Serilog

### Configuration

| Environnement | Format | Destination |
|---|---|---|
| Development | Template humain-lisible | Console |
| Production | JSON structuré | stdout (container) |

Le niveau de log est configuré via `appsettings.json` / `appsettings.{env}.json` :

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Propriétés enrichies automatiquement

Chaque log inclut :
- `Application` : `"Vetolib.Api"`
- `Environment` : `"Development"` / `"Production"`
- `SourceContext` : nom complet de la classe qui log
- Les propriétés du contexte (`LogContext.PushProperty(...)`)

### Exemple — ajouter une propriété au contexte

```csharp
using (LogContext.PushProperty("ClinicId", clinicId))
using (LogContext.PushProperty("UserId", userId))
{
    _logger.LogInformation("Appointment {AppointmentId} created", appointmentId);
}
```

---

## Health Checks

Trois endpoints sont disponibles :

| Endpoint | Rôle | Checks inclus |
|---|---|---|
| `GET /health` | Santé globale | Tous |
| `GET /health/ready` | Readiness (prêt à recevoir du trafic) | PostgreSQL connectivity (tag `ready`) |
| `GET /health/live` | Liveness (processus vivant) | Self check (tag `live`) |

### Format de réponse

```json
{
  "status": "Healthy",
  "results": {
    "self": { "status": "Healthy", "duration": "00:00:00.001" },
    "postgres": { "status": "Healthy", "duration": "00:00:00.012" }
  }
}
```

### Intégration Docker

Le `docker-compose.yml` peut utiliser `/health/live` pour le liveness et `/health/ready` pour le readiness :

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health/ready"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 15s
```

### Intégration Kubernetes

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
```

---

## Métriques OpenTelemetry

Les métriques suivantes sont collectées automatiquement :

| Métrique | Type | Description |
|---|---|---|
| `http.server.request.duration` | Histogramme | Durée des requêtes HTTP (ms) |
| `http.server.active_requests` | UpDownCounter | Requêtes en cours |
| `http.client.request.duration` | Histogramme | Durée des appels HTTP sortants |
| `dotnet.process.cpu.time` | Counter | Temps CPU consommé |
| `dotnet.process.memory.working_set` | UpDownCounter | Mémoire utilisée (bytes) |
| `dotnet.gc.collections` | Counter | Nombre de GC par génération |

### Visualisation

**En développement** : métriques et traces exportées vers la console ET vers l'Aspire Dashboard via OTLP.

**En production** : métriques exportées via OTLP vers le collecteur configuré dans `OTEL_EXPORTER_OTLP_ENDPOINT`.

### Aspire Dashboard

Accessible via l'AppHost Aspire :

```bash
cd src/backend/AppHost
dotnet run
# Dashboard disponible sur http://localhost:18888
```

---

## Traces distribuées

Chaque requête HTTP génère une trace avec :
- Span de la requête entrante (ASP.NET Core instrumentation)
- Spans des appels HTTP sortants (HttpClient instrumentation)

Les traces sont visualisables dans l'Aspire Dashboard sous l'onglet "Traces".

---

## Alerting

En l'absence de Seq dans ce setup (remplacé par l'Aspire Dashboard), les patterns d'alerte sont configurables dans l'aggregateur de logs de production.

### Requêtes utiles (format structlog / Loki LogQL)

```logql
# Toutes les erreurs
{app="vetolib-api"} | json | level="Error"

# Requêtes lentes (> 1s)
{app="vetolib-api"} | json | Elapsed > 1000

# Erreurs serveur (5xx)
{app="vetolib-api"} | json | StatusCode >= 500
```

### Seuils recommandés pour les alertes

| Métrique | Seuil warning | Seuil critical |
|---|---|---|
| Taux d'erreurs 5xx | > 1% des requêtes | > 5% des requêtes |
| P99 latence | > 2s | > 5s |
| Health check `/health/ready` | Dégradé | Unhealthy |
| Mémoire processus | > 500 MB | > 1 GB |

---

## Variables d'environnement

| Variable | Description | Exemple |
|---|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Endpoint OTLP (injecté par Aspire) | `http://localhost:4317` |
| `OTEL_SERVICE_NAME` | Nom du service dans les traces | `vetolib-api` |
| `ASPNETCORE_ENVIRONMENT` | Contrôle le format de log | `Development` / `Production` |
