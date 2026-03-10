using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Vetolib.Notifications;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Testcontainers.PostgreSql;
using Vetolib.Agenda.Infrastructure;
using Vetolib.AI.Infrastructure;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// WebApplicationFactory for integration tests.
/// Manages Testcontainers PostgreSQL lifecycle and overrides services for testing.
/// </summary>
public sealed class VetolibWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;

    public IntegrationTestClinicContext TestClinicContext { get; } = new IntegrationTestClinicContext();
    public FakeIntegrationChatClient FakeChatClient { get; } = new FakeIntegrationChatClient();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("vetolib_integration_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _postgres.StartAsync();
        await InitializeDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await authDb.Database.EnsureCreatedAsync();

        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        var agendaCreator = agendaDb.GetService<IRelationalDatabaseCreator>()!;
        try { await agendaCreator.CreateTablesAsync(); } catch { }

        var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        var billingCreator = billingDb.GetService<IRelationalDatabaseCreator>()!;
        try { await billingCreator.CreateTablesAsync(); } catch { }

        var medicalDb = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        var medicalCreator = medicalDb.GetService<IRelationalDatabaseCreator>()!;
        try { await medicalCreator.CreateTablesAsync(); } catch { }

        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var auditCreator = auditDb.GetService<IRelationalDatabaseCreator>()!;
        try { await auditCreator.CreateTablesAsync(); } catch { }

        var notificationsDb = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var notificationsCreator = notificationsDb.GetService<IRelationalDatabaseCreator>()!;
        try { await notificationsCreator.CreateTablesAsync(); } catch { }

        var stockDb = scope.ServiceProvider.GetRequiredService<StockDbContext>();
        var stockCreator = stockDb.GetService<IRelationalDatabaseCreator>()!;
        try { await stockCreator.CreateTablesAsync(); } catch { }

        var aiDb = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        try { await aiDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS ai"); } catch { }
        var aiCreator = aiDb.GetService<IRelationalDatabaseCreator>()!;
        try { await aiCreator.CreateTablesAsync(); } catch { }

        var messagingDb = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        try { await messagingDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS messaging"); } catch { }
        var messagingCreator = messagingDb.GetService<IRelationalDatabaseCreator>()!;
        try { await messagingCreator.CreateTablesAsync(); } catch { }

        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();
        var preferencesCreator = preferencesDb.GetService<IRelationalDatabaseCreator>()!;
        try { await preferencesCreator.CreateTablesAsync(); } catch { }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:vetolibdb", _postgres?.GetConnectionString() ?? "Host=localhost;Database=placeholder");

        builder.ConfigureServices(services =>
        {
            // Disable rate limiting for tests
            var rateLimiterDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(IConfigureOptions<RateLimiterOptions>) ||
                    d.ServiceType == typeof(IOptionsChangeTokenSource<RateLimiterOptions>))
                .ToList();
            foreach (var d in rateLimiterDescriptors)
                services.Remove(d);

            services.Configure<RateLimiterOptions>(options =>
            {
                options.AddPolicy("signup", _ => RateLimitPartition.GetNoLimiter("test"));
                options.AddPolicy("auth", _ => RateLimitPartition.GetNoLimiter("test"));
                options.AddPolicy("api", _ => RateLimitPartition.GetNoLimiter("test"));
            });

            // Remove all DbContext registrations (Aspire registers many things)
            var knownDbContextTypes = new[]
            {
                typeof(AuthDbContext), typeof(AgendaDbContext), typeof(BillingDbContext),
                typeof(MedicalRecordsDbContext), typeof(AuditDbContext), typeof(NotificationsDbContext),
                typeof(StockDbContext), typeof(AIDbContext), typeof(MessagingDbContext),
                typeof(PreferencesDbContext)
            };

            var descriptorsToRemove = services
                .Where(d =>
                    knownDbContextTypes.Any(t =>
                        d.ServiceType == typeof(DbContextOptions<>).MakeGenericType(t) ||
                        d.ServiceType == t ||
                        (d.ServiceType.IsGenericType && d.ServiceType.GenericTypeArguments.Contains(t))) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType.FullName?.Contains("DbContextPool") == true ||
                    d.ServiceType.FullName?.Contains("ScopedDbContextLease") == true ||
                    d.ServiceType.FullName?.Contains("NpgsqlDataSource") == true ||
                    d.ServiceType.FullName?.Contains("Npgsql") == true)
                .ToList();

            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            var cs = _postgres?.GetConnectionString() ?? "Host=localhost;Database=placeholder";
            services.AddDbContext<AuthDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<AgendaDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<BillingDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<MedicalRecordsDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<AuditDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<NotificationsDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<StockDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<AIDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<MessagingDbContext>(opts => opts.UseNpgsql(cs));
            services.AddDbContext<PreferencesDbContext>(opts => opts.UseNpgsql(cs));

            // Replace IChatClient with fake
            var chatClientDescriptors = services
                .Where(d => d.ServiceType == typeof(IChatClient))
                .ToList();
            foreach (var d in chatClientDescriptors)
                services.Remove(d);
            services.AddSingleton<IChatClient>(FakeChatClient);

            // Replace IClinicContext with test version (singleton, fixed GUID)
            var clinicContextDescriptors = services
                .Where(d => d.ServiceType == typeof(IClinicContext) ||
                            d.ImplementationType == typeof(ClinicContext))
                .ToList();
            foreach (var d in clinicContextDescriptors)
                services.Remove(d);

            services.AddSingleton<IntegrationTestClinicContext>(_ => TestClinicContext);
            services.AddSingleton<IClinicContext>(sp => sp.GetRequiredService<IntegrationTestClinicContext>());

            // Replace MassTransit transport with InMemory test harness.
            // Only remove services from the MassTransit namespace to avoid accidentally
            // removing application services like IBusinessHoursChecker (which contains "IBus").
            var massTransitDescriptors = services
                .Where(d => d.ServiceType.Namespace?.StartsWith("MassTransit") == true)
                .ToList();
            foreach (var d in massTransitDescriptors)
                services.Remove(d);

            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumers(typeof(NotificationsModuleServiceRegistrar).Assembly);
            });
        });
    }
}
