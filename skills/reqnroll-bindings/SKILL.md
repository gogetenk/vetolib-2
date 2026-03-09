# Skill: Reqnroll — BDD-first avec Gherkin

## Processus BDD-first OBLIGATOIRE

```
1. Lire le fichier .feature correspondant à la task
2. Écrire les step definitions (bindings) → vérifier que les tests sont RED
3. Implémenter le code jusqu'à GREEN
4. Ouvrir la PR uniquement quand tous les scénarios du .feature passent
```

Un agent qui ouvre une PR avec des scénarios Reqnroll qui ne passent pas = PR rejetée automatiquement.

## Structure des projets de tests

```
Tests/
├── Vetolib.Tests.Acceptance/        ← Tests BDD Reqnroll (acceptance tests)
│   ├── Features/
│   │   ├── Auth/
│   │   │   └── Login.feature
│   │   └── Agenda/
│   │       └── Appointments.feature
│   ├── StepDefinitions/
│   │   ├── Auth/
│   │   │   └── LoginSteps.cs
│   │   └── Agenda/
│   │       └── AppointmentSteps.cs
│   ├── Support/
│   │   ├── TestWebApplicationFactory.cs
│   │   ├── DatabaseFixture.cs
│   │   └── ScenarioContext.cs
│   └── Hooks/
│       └── GlobalHooks.cs
└── Vetolib.Tests.Unit/              ← Tests unitaires xUnit
```

## NuGet packages

```xml
<PackageReference Include="Reqnroll" Version="2.*" />
<PackageReference Include="Reqnroll.xUnit" Version="2.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.*" />
<PackageReference Include="FluentAssertions" Version="7.*" />
```

## Configuration Reqnroll

```json
// reqnroll.json
{
    "bindingCulture": {
        "name": "en-US"
    },
    "language": {
        "feature": "en"
    }
}
```

## TestWebApplicationFactory — base de tous les tests

```csharp
// Support/TestWebApplicationFactory.cs
internal class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString)
        => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remplacer tous les DbContexts par les versions de test
            ReplaceDbContext<AuthDbContext>(services);
            ReplaceDbContext<AgendaDbContext>(services);
            ReplaceDbContext<MedicalRecordsDbContext>(services);
            ReplaceDbContext<BillingDbContext>(services);

            // IClinicContext de test — configurable par scénario
            services.AddScoped<IClinicContext>(sp =>
                sp.GetRequiredService<TestClinicContext>());
            services.AddScoped<TestClinicContext>();
        });
    }

    private void ReplaceDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor is not null) services.Remove(descriptor);

        services.AddDbContext<TContext>(opts =>
            opts.UseNpgsql(_connectionString));
    }
}
```

## Hooks globaux — setup/teardown

```csharp
// Hooks/GlobalHooks.cs
[Binding]
internal class GlobalHooks
{
    private static PostgreSqlContainer _postgres = null!;
    private static TestWebApplicationFactory _factory = null!;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        // Container PostgreSQL via Testcontainers
        _postgres = new PostgreSqlBuilder()
            .WithDatabase("vetolib_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
        await _postgres.StartAsync();

        _factory = new TestWebApplicationFactory(_postgres.GetConnectionString());

        // Appliquer les migrations
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.Migrate();
        scope.ServiceProvider.GetRequiredService<AgendaDbContext>().Database.Migrate();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        _factory.Dispose();
        await _postgres.DisposeAsync();
    }

    [BeforeScenario]
    public void BeforeScenario(ScenarioContext scenarioContext)
    {
        // Injecter la factory dans le contexte du scénario
        scenarioContext.Set(_factory);
        scenarioContext.Set(_factory.CreateClient());
    }

    [AfterScenario]
    public async Task AfterScenario()
    {
        // Nettoyer les données entre scénarios
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        await db.Appointments.IgnoreQueryFilters().ExecuteDeleteAsync();
        // ... autres tables
    }
}
```

## Step Definitions — pattern exact

```csharp
// StepDefinitions/Agenda/AppointmentSteps.cs
[Binding]
internal class AppointmentSteps
{
    private readonly ScenarioContext _ctx;
    private readonly HttpClient _client;
    private HttpResponseMessage _response = null!;
    private AppointmentDto? _lastCreatedAppointment;

    public AppointmentSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
        _client = ctx.Get<HttpClient>();
    }

    // GIVEN steps — setup de l'état
    [Given(@"I am authenticated as a vet at clinic ""(.*)""")]
    public async Task GivenIAmAuthenticatedAsAVetAtClinic(string clinicName)
    {
        var clinic = await CreateClinicAsync(clinicName);
        var token = GenerateJwtToken(clinic.Id, role: "Vet");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        _ctx.Set(clinic.Id, "CurrentClinicId");
    }

    [Given(@"the following vets exist:")]
    public async Task GivenTheFollowingVetsExist(Table table)
    {
        foreach (var row in table.Rows)
        {
            await CreateVetAsync(
                name: row["Name"],
                speciality: row["Speciality"],
                clinicId: _ctx.Get<Guid>("CurrentClinicId"));
        }
    }

    // WHEN steps — actions
    [When(@"I create an appointment for vet ""(.*)"" on ""(.*)"" at ""(.*)""")]
    public async Task WhenICreateAnAppointment(string vetName, string date, string time)
    {
        var scheduledAt = DateTime.Parse($"{date} {time}");
        var vet = await GetVetByNameAsync(vetName);

        _response = await _client.PostAsJsonAsync("/api/appointments", new
        {
            VetId = vet.Id,
            PatientId = _ctx.Get<Guid>("CurrentPatientId"),
            ScheduledAt = scheduledAt,
            DurationMinutes = 30
        });

        if (_response.IsSuccessStatusCode)
            _lastCreatedAppointment = await _response.Content.ReadFromJsonAsync<AppointmentDto>();
    }

    [When(@"I confirm the appointment")]
    public async Task WhenIConfirmTheAppointment()
    {
        _response = await _client.PutAsync(
            $"/api/appointments/{_lastCreatedAppointment!.Id}/confirm",
            null);
    }

    // THEN steps — assertions
    [Then(@"the appointment should be created successfully")]
    public void ThenTheAppointmentShouldBeCreatedSuccessfully()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _lastCreatedAppointment.Should().NotBeNull();
    }

    [Then(@"I should receive a conflict error")]
    public void ThenIShouldReceiveAConflictError()
    {
        _response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Conflict,
            HttpStatusCode.UnprocessableEntity);
    }

    [Then(@"the appointment status should be ""(.*)""")]
    public async Task ThenTheAppointmentStatusShouldBe(string expectedStatus)
    {
        var response = await _client.GetFromJsonAsync<AppointmentDto>(
            $"/api/appointments/{_lastCreatedAppointment!.Id}");
        response!.Status.Should().Be(expectedStatus);
    }

    [Then(@"appointments from other clinics should not be visible")]
    public async Task ThenAppointmentsFromOtherClinicsShouldNotBeVisible()
    {
        var response = await _client.GetFromJsonAsync<List<AppointmentDto>>("/api/appointments");
        var otherClinicId = _ctx.Get<Guid>("OtherClinicId");
        response!.Should().AllSatisfy(a => a.ClinicId.Should().NotBe(otherClinicId));
    }
}
```

## Conventions de nommage des steps

```
GIVEN → état initial du monde
  "Given a clinic exists with {N} vets"
  "Given I am authenticated as {role}"
  "Given the following {entities} exist:"  (avec DataTable)

WHEN → action déclenchée
  "When I create/update/delete/confirm/cancel {entity}"
  "When I request {endpoint}"

THEN → assertion sur le résultat
  "Then the response status should be {code}"
  "Then I should receive {N} {entities}"
  "Then {entity} should be {status/value}"
  "Then I should receive an error containing {message}"
```

## Règles

```
✅ Écrire les bindings AVANT l'implémentation
✅ Vérifier que les tests sont RED avant de coder
✅ Un step = une chose (pas de logique complexe dans les steps)
✅ Utiliser des DataTables pour les jeux de données multiples
✅ Nettoyer les données après chaque scénario (AfterScenario)
✅ Testcontainers pour PostgreSQL (pas de mock, pas de InMemory)

❌ Pas de Thread.Sleep dans les steps (utiliser retry ou polling)
❌ Pas de logique métier dans les steps
❌ Pas d'assertions dans les When steps
❌ Pas de Given dans les Then steps
❌ Pas de base de données in-memory (SQLite ou InMemory EF) — utiliser Testcontainers PostgreSQL
```
