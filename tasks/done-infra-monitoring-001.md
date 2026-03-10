# todo-infra-monitoring-001.md — Monitoring et observabilité

**Module** : Infrastructure / ServiceDefaults
**Dépendances** : done-back-migrations-001
**Skills à lire** : `dotnet-aspire`

---

## Contexte

`ServiceDefaults` a déjà du code OpenTelemetry mais il n'est connecté à rien. En production, sans monitoring, on ne sait pas si le système fonctionne, si les requêtes sont lentes, ou si des erreurs se produisent.

## Périmètre exact

### 1. Structured logging avec Serilog

Remplacer le logging par défaut par Serilog :

```csharp
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Vetolib.Api")
        .WriteTo.Console(new JsonFormatter())  // JSON en production
        .WriteTo.Seq("http://seq:5341");       // Seq pour l'agrégation
});
```

En dev : console humaine-lisible. En production : JSON + Seq.

### 2. Seq dans docker-compose

Ajouter Seq (gratuit pour usage individuel) dans `docker-compose.yml` :

```yaml
  seq:
    image: datalust/seq:latest
    environment:
      ACCEPT_EULA: "Y"
    ports:
      - "5341:5341"  # ingestion
      - "8081:80"    # UI web
    volumes:
      - seqdata:/data
```

### 3. Health checks structurés

Vérifier que les health checks existants dans ServiceDefaults couvrent :
- PostgreSQL connectivity
- Chaque DbContext (migration applied)
- Disk space (si applicable)

Endpoints :
- `GET /health` → global health
- `GET /health/ready` → readiness (DB up + migrations OK)
- `GET /health/live` → liveness (process alive)

### 4. Métriques de base

Via OpenTelemetry (déjà scaffoldé dans ServiceDefaults) :
- Request duration (histogramme)
- Request count par endpoint
- Error rate (4xx, 5xx)
- Active connections DB

Exporter vers la console en dev. En production, Prometheus endpoint (`/metrics`) pour scraping.

### 5. Alerting basique

Documenter dans `docs/monitoring.md` :
- Comment accéder à Seq (http://localhost:8081)
- Requêtes Seq utiles :
  - `@Level = 'Error'` → toutes les erreurs
  - `Elapsed > 1000` → requêtes lentes (> 1s)
  - `StatusCode >= 500` → erreurs serveur
- Comment ajouter des alertes Seq (email/Slack) quand des patterns apparaissent

## Critère de complétion

```
□ Serilog configuré (console dev, JSON + Seq prod)
□ Seq dans docker-compose et accessible sur :8081
□ Health checks /health, /health/ready, /health/live
□ Métriques OpenTelemetry exportées
□ docs/monitoring.md créé
□ docker compose up → Seq reçoit les logs du backend
□ Renommer en done-infra-monitoring-001.md
```
