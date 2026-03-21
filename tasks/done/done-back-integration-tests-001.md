# todo-back-integration-tests-001.md — Tests d'intégration WebApplicationFactory

**Module** : Infra / Cross-module
**Dépendances** : aucune
**Priorité** : HAUTE (qualité, fiabilité)
**Skills à lire** : `reqnroll-bindings`, `ardalis-modular-monolith`

---

## Contexte

Vetolib possède déjà des tests BDD (Reqnroll + Testcontainers) dans `tests/Vetolib.Tests.Acceptance/` et des tests unitaires (NSubstitute + InMemory EF Core) dans `tests/Vetolib.Tests.Unit/`.

Cependant, il manque un projet de **tests d'intégration HTTP purs** (sans BDD/Gherkin) qui teste les endpoints API directement via `WebApplicationFactory<Program>` + `HttpClient`, à la manière du projet Vibora (`Vibora.Integration.Tests`).

## Objectif

Créer un projet `Vetolib.Tests.Integration` avec :
1. Une infrastructure de test réutilisable (factory, seeders, auth helpers)
2. Des tests d'intégration HTTP pour chaque module critique
3. Des tests d'événements MassTransit (publish/consume)

## Référence : Pattern Vibora

Le projet s'inspire de `vibora-monorepo/backend/tests/Vibora.Integration.Tests/` :

### Infrastructure (à créer)

| Fichier | Rôle |
|---|---|
| `Infrastructure/VetolibWebApplicationFactory.cs` | Étend `WebApplicationFactory<Program>` + IAsyncLifetime, démarre Testcontainers PostgreSQL, applique les migrations, override les services |
| `Infrastructure/IntegrationTestBase.cs` | Base class xUnit avec `IClassFixture<VetolibWebApplicationFactory>`, HttpClient, auth helpers, cleanup TRUNCATE CASCADE |
| `Infrastructure/TestJwtGenerator.cs` | Génère des JWT de test avec claims (ClinicId, Role, UserId) |
| `Infrastructure/HttpClientAuthExtensions.cs` | Extensions `.WithUser()`, `.WithRole()`, `.WithoutAuth()` sur HttpClient |
| `Infrastructure/TestDataSeeder.cs` | Seeders fluent (ClinicBuilder, UserBuilder, PatientBuilder, AppointmentBuilder) |
| `Infrastructure/EventIntegrationTestBase.cs` | Base class pour tester MassTransit events via `ITestHarness` |

### Différences avec Vibora (adapter pour Vetolib)

| Aspect | Vibora | Vetolib |
|---|---|---|
| Multi-tenancy | Non | Oui — ClinicId fixe `11111111-...` via `TestClinicContext` (réutiliser le pattern existant) |
| DbContexts | 3 (Games, Users, Notifications) | 10 (Auth, Agenda, Billing, MedicalRecords, AI, Messaging, Stock, Preferences, Audit, Notifications) |
| Auth | Supabase JWT (sub claim) | JWT interne (ClinicId + Role + UserId claims) |
| Events | MassTransit InMemory | MassTransit RabbitMQ (override InMemory pour tests) |
| BDD existant | Non | Oui — les tests d'intégration complètent les BDD, pas de duplication |

### Tests à écrire (par module, priorité décroissante)

**Auth (5 tests)**
- POST `/api/v1/auth/register-clinic` → 201 avec JWT
- POST `/api/v1/auth/login` → 200 avec tokens
- POST `/api/v1/auth/refresh` → 200 avec nouveau token
- POST `/api/v1/auth/change-password` → 200
- GET `/api/v1/users` → 403 pour Receptionist

**Agenda (5 tests)**
- POST `/api/v1/appointments` → 201 créé
- GET `/api/v1/appointments` → 200 avec pagination
- PUT `/api/v1/appointments/{id}/status` → 200 transition valide
- PUT `/api/v1/appointments/{id}/status` → 400 transition invalide
- GET `/api/v1/appointments/today` → filtre par date

**MedicalRecords (4 tests)**
- POST `/api/v1/patients` → 201 avec owner inline
- GET `/api/v1/patients?search=Rex` → filtre par nom
- POST `/api/v1/patients/{id}/medical-records` → 201
- POST `/api/v1/patients/import/csv` → 200 avec rapport

**Billing (4 tests)**
- POST `/api/v1/invoices` → 201
- POST `/api/v1/invoices/{id}/items` → 200
- PUT `/api/v1/invoices/{id}/status` → 200 (Draft → Sent)
- GET `/api/v1/invoices/{id}/pdf` → 200 application/pdf

**Multi-tenancy isolation (2 tests)**
- Patient créé par Clinic A invisible pour Clinic B
- Appointment créé par Clinic A invisible pour Clinic B

**MassTransit Events (3 tests)**
- Appointment created → AppointmentReminderEvent published
- Invoice status → Sent → InvoiceSentIntegrationEvent published
- User invited → UserInvitedEvent published

## Structure cible

```
tests/
├── Vetolib.Tests.Acceptance/     ← existant (BDD Reqnroll)
├── Vetolib.Tests.Unit/           ← existant (domaine + handlers)
└── Vetolib.Tests.Integration/    ← NOUVEAU
    ├── Vetolib.Tests.Integration.csproj
    ├── Infrastructure/
    │   ├── VetolibWebApplicationFactory.cs
    │   ├── IntegrationTestBase.cs
    │   ├── EventIntegrationTestBase.cs
    │   ├── TestJwtGenerator.cs
    │   ├── HttpClientAuthExtensions.cs
    │   ├── TestDataSeeder.cs
    │   └── TestDataBuilders/
    │       ├── ClinicBuilder.cs
    │       ├── UserBuilder.cs
    │       ├── PatientBuilder.cs
    │       └── AppointmentBuilder.cs
    ├── Auth/
    │   └── AuthEndpointsTests.cs
    ├── Agenda/
    │   └── AppointmentEndpointsTests.cs
    ├── MedicalRecords/
    │   └── PatientEndpointsTests.cs
    ├── Billing/
    │   └── InvoiceEndpointsTests.cs
    ├── MultiTenancy/
    │   └── TenantIsolationTests.cs
    └── Events/
        └── IntegrationEventTests.cs
```

## .csproj minimal

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentAssertions" Version="8.*" />
    <PackageReference Include="MassTransit.TestFramework" Version="8.*" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.*" />
    <PackageReference Include="Testcontainers.PostgreSql" Version="4.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\backend\Vetolib.Api\Vetolib.Api.csproj" />
  </ItemGroup>
</Project>
```

## Critère de complétion

```
□ Projet Vetolib.Tests.Integration créé et ajouté à la solution
□ VetolibWebApplicationFactory fonctionne (Testcontainers + migrations)
□ IntegrationTestBase avec auth helpers et cleanup
□ TestDataSeeder avec builders fluent
□ 23+ tests d'intégration GREEN
□ Tests de tenant isolation GREEN
□ Tests d'événements MassTransit GREEN
□ xunit.runner.json : parallelizeTestCollections = false
□ Renommer en done
```

## Notes architecte

- **Ne pas dupliquer les tests BDD existants** — les tests d'intégration testent les edges cases HTTP (status codes, validation, headers) que les Gherkins ne couvrent pas
- **Réutiliser TestClinicContext** (GUID fixe) — même pattern que les tests acceptance
- **TRUNCATE CASCADE** pour le cleanup (pas ExecuteDeleteAsync qui est plus lent avec 10 DbContexts)
- **MassTransit.Testing** avec `AddMassTransitTestHarness()` pour remplacer RabbitMQ par InMemory
