# todo-back-messaging-hours-config-001.md — Configuration heures de messagerie + stats admin

**Module** : Messaging
**Dependances** : todo-back-messaging-domain-001
**Priorite** : MOYENNE
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`

---

## Objectif

Permettre a l'admin de configurer les heures de messagerie et consulter les statistiques de triage.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 2.5 (Out-of-Hours), section 4.2 (Triage statistics), Appendix A (RBAC)

## Implementation

### 1. Configuration des heures de messagerie

**GetMessagingHoursQuery** + Handler
- Retourne les heures configurees pour la clinique (7 jours)
- Policy : AdminOnly

**UpdateMessagingHoursCommand** + Handler + Validator
- Met a jour les heures pour un ou plusieurs jours
- Valide les horaires (OpenTime < CloseTime, sauf si IsClosed)
- Default UAE : Sunday-Thursday 08:00-20:00, Friday 08:00-12:00, Saturday closed
- Policy : AdminOnly

Endpoints :
```
GET  /api/v1/messaging/settings/hours → GetMessagingHoursQuery
PUT  /api/v1/messaging/settings/hours → UpdateMessagingHoursCommand
```

### 2. Service interne : IsWithinBusinessHours

Creer `IBusinessHoursChecker` (interne) :
- `Task<bool> IsWithinBusinessHoursAsync(Guid clinicId, DateTime utcNow)`
- Utilise par les handlers owner pour decider du auto-acknowledgment
- Timezone : Asia/Dubai (UTC+4)

### 3. Statistiques de triage (Admin dashboard)

**GetTriageStatsQuery** + Handler
- Average first response time
- Messages by category (count par categorie)
- AI triage accuracy (% de messages recategorises par le staff)
- Volume per day (trend sur 30 jours)
- Conversion rate : message -> appointment
- Policy : AdminOnly

Endpoint :
```
GET /api/v1/messaging/stats → GetTriageStatsQuery
```

## Regles

- Seul l'Admin peut configurer les heures et voir les stats
- Le calcul du SLA ne compte que les heures de bureau
- Timezone par defaut : Asia/Dubai

## Critere

```
[] CRUD heures de messagerie (GET + PUT)
[] IBusinessHoursChecker implemente
[] GetTriageStatsQuery avec 5 metriques
[] 3 endpoints mappes
[] Default UAE work week pre-configure
[] dotnet build passe
[] Renommer en done
```
