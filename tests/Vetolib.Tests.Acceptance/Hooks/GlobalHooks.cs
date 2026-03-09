using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Testcontainers.PostgreSql;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.Hooks;

[Binding]
internal class GlobalHooks
{
    private static PostgreSqlContainer _postgres = null!;
    private static TestWebApplicationFactory _factory = null!;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("vetolib_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
        await _postgres.StartAsync();

        _factory = new TestWebApplicationFactory(_postgres.GetConnectionString());

        // Apply schema creation for all DbContexts.
        // EnsureCreatedAsync() only works for the first DbContext because once the DB
        // exists, subsequent calls are no-ops. We must use GetService<IRelationalDatabaseCreator>()
        // to create tables for each context's model independently.
        using var scope = _factory.Services.CreateScope();

        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await authDb.Database.EnsureCreatedAsync();

        // For subsequent contexts, the DB already exists so EnsureCreatedAsync() is a no-op.
        // Use CreateTablesAsync via the relational creator to add tables from each context's model.
        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        var agendaCreator = agendaDb.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>()!;
        try { await agendaCreator.CreateTablesAsync(); } catch { /* tables may already exist */ }

        var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        var billingCreator = billingDb.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>()!;
        try { await billingCreator.CreateTablesAsync(); } catch { /* tables may already exist */ }

        var medicalDb = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        var medicalCreator = medicalDb.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>()!;
        try { await medicalCreator.CreateTablesAsync(); } catch { /* tables may already exist */ }
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        if (_postgres is not null)
            await _postgres.DisposeAsync();
    }

    [BeforeScenario(Order = 0)]
    public void BeforeScenario(ScenarioContext scenarioContext)
    {
        scenarioContext.Set(_factory);
        scenarioContext.Set(_factory.CreateClient());
    }

    [AfterScenario]
    public async Task AfterScenario()
    {
        // Clean database between scenarios
        using var scope = _factory.Services.CreateScope();

        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await authDb.RefreshTokens.IgnoreQueryFilters().ExecuteDeleteAsync();
        await authDb.Users.IgnoreQueryFilters().ExecuteDeleteAsync();

        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        await agendaDb.Appointments.IgnoreQueryFilters().ExecuteDeleteAsync();

        var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        await billingDb.InvoiceItems.IgnoreQueryFilters().ExecuteDeleteAsync();
        await billingDb.Invoices.IgnoreQueryFilters().ExecuteDeleteAsync();

        var medicalDb = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        await medicalDb.PatientOwners.IgnoreQueryFilters().ExecuteDeleteAsync();
        await medicalDb.Patients.IgnoreQueryFilters().ExecuteDeleteAsync();
        await medicalDb.Owners.IgnoreQueryFilters().ExecuteDeleteAsync();
    }
}
