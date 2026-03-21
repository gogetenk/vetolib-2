# todo-back-noshow-contracts-001.md — Interfaces inter-modules pour prediction no-show

**Module** : Agenda.Contracts + MedicalRecords.Contracts
**Dependances** : done-back-ai-scaffold-001
**Priorite** : MOYENNE (Phase 3)
**Skills a lire** : `ardalis-result`, `ardalis-modular-monolith`

---

## Objectif

Creer les interfaces de lecture inter-modules necessaires au module AI pour la prediction no-show.

## Spec de reference

`docs/AI-FEATURES-SPEC.md` section 5.6

## Implementation

### Vetolib.Agenda.Contracts

1. `IAppointmentReader.cs` — interface publique
   ```csharp
   public interface IAppointmentReader
   {
       Task<IReadOnlyList<AppointmentHistoryDto>> GetOwnerHistoryAsync(
           Guid ownerId, int limit, CancellationToken ct);
   }
   ```
2. `AppointmentHistoryDto.cs` — DTO avec Status, Date, Duration, Type, WasNoShow

### Vetolib.Agenda (runtime, internal)

3. `AppointmentReader.cs` — implementation interne de `IAppointmentReader`
   - Lit depuis `AgendaDbContext` les RDV d'un owner
   - Enregistre dans DI via `ModuleServiceRegistrar`

### Vetolib.MedicalRecords.Contracts (si necessaire)

4. `IOwnerReader.cs` — interface pour lire les donnees owner (nombre total RDV, dernier RDV)

## Critere

```
[] IAppointmentReader dans Agenda.Contracts
[] AppointmentHistoryDto dans Agenda.Contracts
[] AppointmentReader (internal) dans Agenda runtime
[] DI registration dans ModuleServiceRegistrar
[] dotnet build passe
[] Renommer en done
```
