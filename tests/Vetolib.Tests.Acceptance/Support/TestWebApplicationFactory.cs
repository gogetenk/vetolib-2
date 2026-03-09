using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Tests.Acceptance.Support;

internal class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString)
        => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Rate limiting: tests run sequentially inside a TestServer (in-process),
        // all using loopback IP. The built-in limits (100 req/min "api", 10 req/min "auth",
        // 3/h "signup") are high enough that sequential BDD scenarios will not be rejected.
        // No override needed.

        builder.ConfigureServices(services =>
        {
            // Remove ALL registrations related to DbContexts (Aspire registers many things)
            var knownDbContextTypes = new[]
            {
                typeof(AuthDbContext), typeof(AgendaDbContext), typeof(BillingDbContext),
                typeof(MedicalRecordsDbContext), typeof(AuditDbContext), typeof(NotificationsDbContext)
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

            // Add test databases (simple, no pooling)
            services.AddDbContext<AuthDbContext>(opts =>
                opts.UseNpgsql(_connectionString));
            services.AddDbContext<AgendaDbContext>(opts =>
                opts.UseNpgsql(_connectionString));
            services.AddDbContext<BillingDbContext>(opts =>
                opts.UseNpgsql(_connectionString));
            services.AddDbContext<MedicalRecordsDbContext>(opts =>
                opts.UseNpgsql(_connectionString));
            services.AddDbContext<AuditDbContext>(opts =>
                opts.UseNpgsql(_connectionString));
            services.AddDbContext<NotificationsDbContext>(opts =>
                opts.UseNpgsql(_connectionString));

            // Replace IClinicContext with test version
            var clinicContextDescriptors = services
                .Where(d => d.ServiceType == typeof(IClinicContext) ||
                            d.ImplementationType == typeof(ClinicContext))
                .ToList();
            foreach (var d in clinicContextDescriptors)
                services.Remove(d);

            services.AddSingleton<TestClinicContext>();
            services.AddSingleton<IClinicContext>(sp => sp.GetRequiredService<TestClinicContext>());
        });
    }
}
