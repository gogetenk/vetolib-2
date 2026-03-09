# openspec/agenda/spec.md — Module Agenda

## Assemblies

```
Vetolib.Agenda.Contracts/   ← public : AppointmentDto, CreateAppointmentRequest, AppointmentStatus, IAppointmentConfirmedEvent
Vetolib.Agenda/             ← internal : Appointment entity, handlers, AgendaDbContext
```

## Responsabilités

Gestion complète des rendez-vous vétérinaires : création, confirmation, annulation, disponibilités.

---

## Entités Domain (internal)

### Appointment

```csharp
internal class Appointment : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid VetId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public int DurationMinutes { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string? CancelReason { get; private set; }

    // Factory statique — retourne Result<Appointment>
    public static Result<Appointment> Create(...) { ... }

    // Transitions d'état — retournent Result
    public Result Confirm() { ... }
    public Result Start() { ... }      // → InProgress
    public Result Complete() { ... }   // → Done, publie AppointmentCompletedEvent
    public Result Cancel(string reason) { ... }
    public Result MarkNoShow() { ... }
}
```

### Statuts et transitions

```
Pending → Confirmed → InProgress → Done
Pending → Cancelled
Confirmed → Cancelled
Confirmed → NoShow
```

- `Done` et `NoShow` : états terminaux, aucune modification possible
- `Confirmed` → `InProgress` : uniquement par `Vet` ou `Admin`
- `InProgress` → `Done` : déclenche `AppointmentCompletedEvent` (pour créer la facture dans Billing)

---

## Règles métier

- Un vétérinaire ne peut pas avoir deux rendez-vous qui se chevauchent (`ScheduledAt` à `ScheduledAt + DurationMinutes`)
- Durée : min 15 min, max 120 min
- Impossible de créer un rendez-vous dans le passé
- Un rendez-vous `Done` est immuable
- L'API de disponibilité retourne les créneaux libres pour un vétérinaire sur une plage de dates
- Durées standard (défaults UI) : voir skill `veterinary-domain`

---

## Endpoints

```
POST   /api/appointments                    → Result<AppointmentDto>              (Vet, Admin, Receptionist)
GET    /api/appointments                    → Result<PagedResult<AppointmentDto>> (filtre: from, to, vetId, status)
GET    /api/appointments/{id}               → Result<AppointmentDto>
PUT    /api/appointments/{id}               → Result<AppointmentDto>              (Pending/Confirmed seulement)
PATCH  /api/appointments/{id}/confirm       → Result                              (Admin, Receptionist)
PATCH  /api/appointments/{id}/start         → Result                              (Vet, Admin)
PATCH  /api/appointments/{id}/complete      → Result                              (Vet, Admin)
PATCH  /api/appointments/{id}/cancel        → Result                              (body: { reason })
PATCH  /api/appointments/{id}/no-show       → Result                              (Admin, Receptionist)
GET    /api/appointments/availability       → Result<IReadOnlyList<TimeSlotDto>>  (query: vetId, date)
```

**Note** : chaque PATCH de transition d'état = un Command dédié.

---

## DTOs (Contracts, public)

```csharp
public record AppointmentDto(
    Guid Id,
    Guid VetId,
    string VetName,
    Guid PatientId,
    string PatientName,
    DateTime ScheduledAt,
    int DurationMinutes,
    AppointmentStatus Status,
    string? Notes,
    string? CancelReason);

public record CreateAppointmentRequest(
    Guid VetId,
    Guid PatientId,
    DateTime ScheduledAt,
    int DurationMinutes,
    string? Notes);

public record TimeSlotDto(DateTime Start, DateTime End, bool IsAvailable);
```

---

## Domain Events (Contracts, public)

```csharp
// Consommé par Billing pour créer la facture
public record AppointmentCompletedEvent(
    Guid AppointmentId,
    Guid ClinicId,
    Guid VetId,
    Guid PatientId) : INotification;
```

---

## Handlers attendus

| Command/Query | Retour |
|---|---|
| `CreateAppointmentCommand` | `Result<AppointmentDto>` |
| `UpdateAppointmentCommand` | `Result<AppointmentDto>` |
| `ConfirmAppointmentCommand` | `Result` |
| `StartAppointmentCommand` | `Result` |
| `CompleteAppointmentCommand` | `Result` |
| `CancelAppointmentCommand` | `Result` |
| `MarkNoShowCommand` | `Result` |
| `GetAppointmentsQuery` | `Result<PagedResult<AppointmentDto>>` |
| `GetAppointmentByIdQuery` | `Result<AppointmentDto>` |
| `GetAvailabilityQuery` | `Result<IReadOnlyList<TimeSlotDto>>` |

---

## Gherkins liés

`features/agenda/appointments.feature`

---

## Dépendances

- `Vetolib.Shared.Kernel` + `Vetolib.Shared.Infrastructure`
- `Vetolib.Auth.Contracts` : `UserDto` (pour afficher `VetName`)
