# todo-test-agenda-status-edge-cases-001.md — Edge cases PATCH /status (400, 404)

**Module** : Agenda
**Dépendances** : aucune
**Priorité** : MOYENNE
**Skills à lire** : `ardalis-result`, `reqnroll-bindings`

---

## Contexte

L'audit `done-audit-coverage-001.md` a révélé que `PATCH /api/v1/appointments/{id}/status` est exposé mais ne dispose que d'une couverture implicite via les scénarios RBAC. Il manque des scénarios dédiés pour :
- Happy path (succès)
- Erreur 400 (statut invalide)
- Erreur 404 (rendez-vous inexistant)

Note : `PATCH /{id}/transition` est bien couvert (check-in, consultation, fin, annulation, transition invalide). Cette tâche couvre spécifiquement le endpoint `/status` qui est distinct de `/transition`.

## Distinction entre les deux endpoints

- `PATCH /{id}/status` — mise à jour directe du statut (admin override, correction manuelle)
- `PATCH /{id}/transition` — machine à états métier (check-in → consultation → terminé → etc.)

## Travail à faire

### Étape 1 — Ajouter des scénarios dans Appointments.feature

Fichier : `tests/Vetolib.Tests.Acceptance/Features/Agenda/Appointments.feature`

Ajouter :

```gherkin
Scenario: Admin updates appointment status directly
  Given a clinic "Happy Paws"
  And I am authenticated as ADMIN
  And an existing appointment with status "SCHEDULED"
  When I update the appointment status to "NO_SHOW"
  Then the response status is 200
  And the appointment status is "NO_SHOW"

Scenario: Status update with invalid status value returns 400
  Given a clinic "Happy Paws"
  And I am authenticated as ADMIN
  And an existing appointment with status "SCHEDULED"
  When I update the appointment status to "INVALID_STATUS"
  Then the response status is 400

Scenario: Status update on non-existent appointment returns 404
  Given a clinic "Happy Paws"
  And I am authenticated as ADMIN
  When I update a non-existent appointment status to "NO_SHOW"
  Then the response status is 404
```

### Étape 2 — Bindings Reqnroll (RED d'abord)

Fichier : `tests/Vetolib.Tests.Acceptance/StepDefinitions/Agenda/AppointmentSteps.cs`

Ajouter les steps manquants. Vérifier RED.

### Étape 3 — Vérifier GREEN

```bash
dotnet test tests/Vetolib.Tests.Acceptance/ --filter "Feature[Agenda]"
dotnet build src/backend/Vetolib.sln
```

## Règles métier

- `PATCH /status` est réservé aux Admin (pour corriger des erreurs)
- Les statuts valides : SCHEDULED, CHECKED_IN, IN_PROGRESS, COMPLETED, CANCELLED, NO_SHOW
- Même en override admin, les contraintes métier fondamentales s'appliquent (ex: ne pas mettre CANCELLED après COMPLETED)

## Critères de complétion

```
[ ] 3 scénarios Gherkin ajoutés dans Appointments.feature
[ ] Steps Reqnroll écrits et ROUGES confirmés
[ ] Tous les scénarios VERTS
[ ] dotnet build -> 0 erreur
```
