using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Vetolib.Agenda.Infrastructure;
using Vetolib.AI.Infrastructure;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Infrastructure;
using Vetolib.Breeding.Infrastructure;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// Base class for integration tests. Provides HttpClient, auth helpers, and database cleanup.
/// Uses IClassFixture so the factory (and its Testcontainer) is shared across all tests in a class.
/// </summary>
[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly VetolibWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly Guid TestClinicId = IntegrationTestClinicContext.PrimaryClinicId;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    protected IntegrationTestBase(VetolibWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        // Reset clinic context to primary clinic before each test
        factory.TestClinicContext.ClinicId = TestClinicId;
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await CleanDatabaseAsync();
        Client.Dispose();
    }

    /// <summary>
    /// Truncate all tenant-owned tables. Uses IgnoreQueryFilters() to bypass multi-tenancy.
    /// Order matters: child tables before parent tables to avoid FK violations.
    /// </summary>
    protected async Task CleanDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();

        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await authDb.RefreshTokens.IgnoreQueryFilters().ExecuteDeleteAsync();
        await authDb.OnboardingStates.IgnoreQueryFilters().ExecuteDeleteAsync();
        await authDb.Users.IgnoreQueryFilters().ExecuteDeleteAsync();
        await authDb.Clinics.ExecuteDeleteAsync();

        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        await agendaDb.VisitFeedbacks.IgnoreQueryFilters().ExecuteDeleteAsync();
        await agendaDb.WaitlistEntries.IgnoreQueryFilters().ExecuteDeleteAsync();
        await agendaDb.Appointments.IgnoreQueryFilters().ExecuteDeleteAsync();

        var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        await billingDb.InvoiceItems.IgnoreQueryFilters().ExecuteDeleteAsync();
        await billingDb.Invoices.IgnoreQueryFilters().ExecuteDeleteAsync();

        var medicalDb = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        await medicalDb.MedicalRecordTemplates.IgnoreQueryFilters().ExecuteDeleteAsync();
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

        var messagingDb = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        await messagingDb.ReplyAudits.IgnoreQueryFilters().ExecuteDeleteAsync();
        await messagingDb.MessageAttachments.IgnoreQueryFilters().ExecuteDeleteAsync();
        await messagingDb.Messages.IgnoreQueryFilters().ExecuteDeleteAsync();
        await messagingDb.Conversations.IgnoreQueryFilters().ExecuteDeleteAsync();
        await messagingDb.OwnerPortalTokens.IgnoreQueryFilters().ExecuteDeleteAsync();
        await messagingDb.ResponseTemplates.IgnoreQueryFilters().ExecuteDeleteAsync();
        await messagingDb.MessagingHours.IgnoreQueryFilters().ExecuteDeleteAsync();

        var notificationsDb = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        await notificationsDb.ReminderLogs.IgnoreQueryFilters().ExecuteDeleteAsync();
        await notificationsDb.ReminderConfigs.IgnoreQueryFilters().ExecuteDeleteAsync();

        var breedingDb = scope.ServiceProvider.GetRequiredService<BreedingDbContext>();
        await breedingDb.LitterOffspring.IgnoreQueryFilters().ExecuteDeleteAsync();
        await breedingDb.Litters.IgnoreQueryFilters().ExecuteDeleteAsync();
        await breedingDb.PregnancyChecks.IgnoreQueryFilters().ExecuteDeleteAsync();
        await breedingDb.Pregnancies.IgnoreQueryFilters().ExecuteDeleteAsync();
        await breedingDb.HeatCycles.IgnoreQueryFilters().ExecuteDeleteAsync();
        await breedingDb.PatientLineages.IgnoreQueryFilters().ExecuteDeleteAsync();

        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();
        await preferencesDb.ClinicWorkingHours.IgnoreQueryFilters().ExecuteDeleteAsync();
        await preferencesDb.UserPreferences.IgnoreQueryFilters().ExecuteDeleteAsync();
        await preferencesDb.ClinicPreferenceDefaults.IgnoreQueryFilters().ExecuteDeleteAsync();
        await preferencesDb.ConsentAuditEntries.IgnoreQueryFilters().ExecuteDeleteAsync();
    }

    /// <summary>
    /// Creates a new HttpClient pre-authenticated as Admin for the test clinic.
    /// </summary>
    protected HttpClient CreateAdminClient()
        => Factory.CreateClient().WithAdminAuth(TestClinicId);

    /// <summary>
    /// Creates a new HttpClient pre-authenticated as Vet for the test clinic.
    /// </summary>
    protected HttpClient CreateVetClient(Guid? userId = null)
        => Factory.CreateClient().WithVetAuth(TestClinicId, userId);

    /// <summary>
    /// Creates a new HttpClient pre-authenticated as Receptionist for the test clinic.
    /// </summary>
    protected HttpClient CreateReceptionistClient()
        => Factory.CreateClient().WithReceptionistAuth(TestClinicId);
}
