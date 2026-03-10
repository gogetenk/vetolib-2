using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Testcontainers.PostgreSql;
using Vetolib.Agenda.Infrastructure;
using Vetolib.AI.Infrastructure;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Stock.Infrastructure;
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

        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var auditCreator = auditDb.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>()!;
        try { await auditCreator.CreateTablesAsync(); } catch { /* tables may already exist */ }

        var notificationsDb = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var notificationsCreator = notificationsDb.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>()!;
        try { await notificationsCreator.CreateTablesAsync(); } catch { /* tables may already exist */ }

        var stockDb = scope.ServiceProvider.GetRequiredService<StockDbContext>();
        var stockCreator = stockDb.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>()!;
        try { await stockCreator.CreateTablesAsync(); } catch { /* tables may already exist */ }

        var aiDb = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        // The AIDbContext uses schema "ai" which must exist before CreateTablesAsync() can work.
        try { await aiDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS ai"); } catch { }
        var aiCreator = aiDb.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>()!;
        try { await aiCreator.CreateTablesAsync(); } catch { /* tables may already exist */ }
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
        await authDb.Clinics.ExecuteDeleteAsync();

        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        await agendaDb.Appointments.IgnoreQueryFilters().ExecuteDeleteAsync();

        var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        await billingDb.InvoiceItems.IgnoreQueryFilters().ExecuteDeleteAsync();
        await billingDb.Invoices.IgnoreQueryFilters().ExecuteDeleteAsync();

        var medicalDb = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        await medicalDb.Prescriptions.IgnoreQueryFilters().ExecuteDeleteAsync();
        await medicalDb.MedicalRecords.IgnoreQueryFilters().ExecuteDeleteAsync();
        await medicalDb.PatientOwners.IgnoreQueryFilters().ExecuteDeleteAsync();
        await medicalDb.Patients.IgnoreQueryFilters().ExecuteDeleteAsync();
        await medicalDb.Owners.IgnoreQueryFilters().ExecuteDeleteAsync();
        await medicalDb.DrugCatalogEntries.IgnoreQueryFilters().ExecuteDeleteAsync();

        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        await auditDb.AuditLog.ExecuteDeleteAsync();

        var stockDb = scope.ServiceProvider.GetRequiredService<StockDbContext>();
        await stockDb.StockMovements.IgnoreQueryFilters().ExecuteDeleteAsync();
        await stockDb.StockItems.IgnoreQueryFilters().ExecuteDeleteAsync();

        var aiDb = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        await aiDb.TriageResults.IgnoreQueryFilters().ExecuteDeleteAsync();

        // Reset FakeChatClient state
        _factory.FakeChatClient.SetShouldThrow(false);
    }
}
