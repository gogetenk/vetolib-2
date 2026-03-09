using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords.Infrastructure;
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
        builder.ConfigureServices(services =>
        {
            // Remove ALL registrations related to DbContexts (Aspire registers many things)
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AuthDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions<AgendaDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions<BillingDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions<MedicalRecordsDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(AuthDbContext) ||
                    d.ServiceType == typeof(AgendaDbContext) ||
                    d.ServiceType == typeof(BillingDbContext) ||
                    d.ServiceType == typeof(MedicalRecordsDbContext) ||
                    (d.ServiceType.IsGenericType && d.ServiceType.GenericTypeArguments
                        .Any(t => t == typeof(AuthDbContext) || t == typeof(AgendaDbContext) || t == typeof(BillingDbContext) || t == typeof(MedicalRecordsDbContext))) ||
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
