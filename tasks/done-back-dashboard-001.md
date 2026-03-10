# todo-back-dashboard-001.md — Backend : API Dashboard stats

**Dépendances** : done-back-agenda-001, done-back-billing-001, done-back-patients-001
**Skills** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`, `reqnroll-bindings`

---

## Périmètre

Nouveau module léger `Vetolib.Dashboard` (lecture seule, agrège les autres modules via leurs Contracts).

Endpoints :
- `GET /api/dashboard/stats` — métriques clés du jour
- `GET /api/dashboard/today-appointments` — RDVs du jour triés par heure
- `GET /api/dashboard/recent-activity` — 10 dernières actions de la clinique

## Contracts — `Vetolib.Dashboard.Contracts/`

```csharp
public record DashboardStatsDto(
    int AppointmentsToday,
    int PendingCheckin,
    decimal UnpaidInvoicesAed,
    int TotalPatients
);

public record ActivityDto(
    DateTimeOffset OccurredAt,
    string ActivityType,   // "APPOINTMENT" | "MEDICAL_RECORD" | "INVOICE"
    string Description,
    Guid? EntityId
);
```

## Règles métier

- Stats filtrées par `clinicId` (IMultiTenant via IClinicContext)
- `AppointmentsToday` : count RDVs dont `scheduledAt.Date == today` (timezone Asia/Dubai)
- `PendingCheckin` : count RDVs status = SCHEDULED dont heure <= now + 30min
- `UnpaidInvoicesAed` : somme total des factures status = SENT
- `TotalPatients` : count patients actifs de la clinique
- `RecentActivity` : union des 10 derniers événements (RDV créé/transitionné, dossier créé, facture créée/payée) triés par date desc
- RBAC sur les stats (voir tâche front-dashboard-001 pour les règles par rôle)

## Implémentation

Le module Dashboard ne dépend que des **Contracts** des autres modules (jamais des runtimes).
Les queries font des appels directs à la DB via un `DashboardDbContext` en lecture seule
(ou via les interfaces des Contracts si disponibles).

## Critère de complétion

```
□ Bindings Reqnroll ROUGES avant implémentation
□ GET /api/dashboard/stats retourne les bonnes métriques
□ Isolation clinique vérifiée
□ dotnet build → 0 erreur
□ Renommer en done-back-dashboard-001.md
```
