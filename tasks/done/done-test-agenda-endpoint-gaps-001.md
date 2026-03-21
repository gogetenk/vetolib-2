# todo-test-agenda-endpoint-gaps-001.md — Expose GetById + EditAppointment + Gherkin

**Module** : Agenda
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`, `reqnroll-bindings`

---

## Contexte

L'audit `done-audit-coverage-001.md` a révélé deux trous critiques dans le module Agenda :

1. **`GetAppointmentByIdQuery`** est implémentée (`Application/Queries/GetAppointmentById/`) mais aucun endpoint `GET /api/v1/appointments/{id}` n'est exposé.
2. **`EditAppointmentCommand`** est implémentée (`Application/Commands/EditAppointment/`) mais aucun endpoint `PUT /api/v1/appointments/{id}` n'est exposé.
3. **Zéro scénario Gherkin** pour ces deux opérations.

## Travail à faire

### Étape 1 — Écrire les scénarios Gherkin d'abord

Ajouter dans `tests/Vetolib.Tests.Acceptance/Features/Agenda/Appointments.feature` :

```gherkin
Scenario: Get appointment by ID
  Given a clinic "Happy Paws"
  And I am authenticated as VET
  And an existing appointment for patient "Max" on "2026-04-01" at "10:00"
  When I request the appointment by its ID
  Then the response status is 200
  And the appointment details include patient "Max" and time "10:00"

Scenario: Get appointment by ID returns 404 when not found
  Given a clinic "Happy Paws"
  And I am authenticated as VET
  When I request appointment with a random non-existent ID
  Then the response status is 404

Scenario: Edit appointment date and time
  Given a clinic "Happy Paws"
  And I am authenticated as VET
  And an existing appointment for patient "Max" on "2026-04-01" at "10:00"
  When I update the appointment to "2026-04-02" at "14:00"
  Then the response status is 200
  And the appointment is now scheduled for "2026-04-02" at "14:00"

Scenario: Edit appointment refused if slot conflict
  Given a clinic "Happy Paws"
  And I am authenticated as VET
  And an existing appointment on "2026-04-02" at "14:00"
  And another appointment for "Max" on "2026-04-01" at "10:00"
  When I update the second appointment to "2026-04-02" at "14:00"
  Then the response status is 409
```

### Étape 2 — Écrire les bindings Reqnroll (RED d'abord)

Fichier : `tests/Vetolib.Tests.Acceptance/StepDefinitions/Agenda/AppointmentSteps.cs`

Ajouter les steps correspondants et vérifier qu'ils sont ROUGES avant d'implémenter.

### Étape 3 — Exposer les endpoints

Fichier : `src/backend/Modules/Agenda/Vetolib.Agenda/Api/AppointmentEndpoints.cs`

```csharp
group.MapGet("/{id:guid}", GetAppointmentById)
    .RequireAuthorization()
    .WithName("GetAppointmentById");

group.MapPut("/{id:guid}", EditAppointment)
    .RequireAuthorization("VetOrAdmin")
    .WithName("EditAppointment");
```

### Étape 4 — Vérifier GREEN

```bash
dotnet test tests/Vetolib.Tests.Acceptance/ --filter "Feature[Agenda]"
dotnet build src/backend/Vetolib.sln
```

## Règles métier

- Un rendez-vous ne peut pas être modifié s'il est dans un état terminal (COMPLETED, CANCELLED, NO_SHOW)
- La modification d'un rendez-vous doit re-vérifier les conflits de créneaux
- Seuls VET et ADMIN peuvent modifier un rendez-vous

## Critères de complétion

```
[ ] Scénarios Gherkin écrits dans Appointments.feature
[ ] Bindings Reqnroll écrits et ROUGES avant implémentation
[ ] GET /api/v1/appointments/{id} exposé et fonctionnel
[ ] PUT /api/v1/appointments/{id} exposé et fonctionnel
[ ] Tous les scénarios VERTS
[ ] dotnet build -> 0 erreur
```
