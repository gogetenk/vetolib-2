# todo-test-dashboard-analytics-001.md — Couverture Gherkin pour GET /analytics

**Module** : Dashboard (Vetolib.Api host)
**Dépendances** : Agenda, Billing, MedicalRecords
**Priorité** : MOYENNE
**Skills à lire** : `ardalis-result`, `reqnroll-bindings`

---

## Contexte

L'audit `done-audit-coverage-001.md` a révélé que `GET /api/dashboard/analytics` est implémenté dans `src/backend/Vetolib.Api/Dashboard/DashboardEndpoints.cs` mais ne dispose d'**aucun scénario Gherkin**.

Les autres endpoints dashboard (`/stats`, `/today-appointments`, `/recent-activity`) sont couverts par `Dashboard.feature`.

## Travail à faire

### Étape 1 — Ajouter des scénarios dans Dashboard.feature

Fichier : `tests/Vetolib.Tests.Acceptance/Features/Dashboard/Dashboard.feature`

Ajouter :

```gherkin
Scenario: Admin sees analytics data
  Given there are 5 completed appointments this month
  And there are 3 invoices totaling 1500 AED this month
  When I request dashboard analytics
  Then the response status is 200
  And the analytics include appointments count for current month
  And the analytics include revenue in AED for current month

Scenario: Analytics endpoint requires authentication
  When I request dashboard analytics without authentication
  Then the response status is 401

Scenario: Non-admin cannot access analytics
  Given I am authenticated as RECEPTIONIST
  When I request dashboard analytics
  Then the response status is 403
```

### Étape 2 — Vérifier ce que retourne l'endpoint analytics

Lire `src/backend/Vetolib.Api/Dashboard/DashboardEndpoints.cs` et le handler sous-jacent pour comprendre exactement quelles données sont retournées (appointments by month, revenue by month, patients by species, etc.) afin d'aligner les assertions Gherkin sur la réponse réelle.

### Étape 3 — Bindings Reqnroll (RED d'abord)

Fichier : `tests/Vetolib.Tests.Acceptance/StepDefinitions/Dashboard/DashboardSteps.cs`

Ajouter les steps manquants. Vérifier RED.

### Étape 4 — Vérifier GREEN

```bash
dotnet test tests/Vetolib.Tests.Acceptance/ --filter "Feature[Dashboard]"
dotnet build src/backend/Vetolib.sln
```

## Règles métier

- Les analytics sont en lecture seule
- Accès : Admin et Vet uniquement (pas Receptionist, pas Assistant)
- Les données sont filtrées par clinique (tenant isolation)
- Les montants sont en AED

## Critères de complétion

```
[ ] Scénarios Gherkin ajoutés dans Dashboard.feature
[ ] Steps Reqnroll écrits et ROUGES confirmés
[ ] Tous les scénarios VERTS
[ ] dotnet build -> 0 erreur
```
