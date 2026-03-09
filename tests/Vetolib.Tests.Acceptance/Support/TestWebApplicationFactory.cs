using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Vetolib.Agenda.Infrastructure;
using Vetolib.AI.Application.Services;
using Vetolib.AI.Infrastructure;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Shared.Infrastructure;
using Vetolib.Stock.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Tests.Acceptance.Support;

internal class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    public readonly FakeChatClient FakeChatClient = new();

    public TestWebApplicationFactory(string connectionString)
        => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Disable rate limiting for tests: the "signup" policy allows only 3 req/h,
            // which is exceeded by the 5 BDD scenarios that hit POST /clinics/register.
            // Remove existing rate limiter configure options, then re-register with no-op policies.
            var rateLimiterDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(IConfigureOptions<RateLimiterOptions>) ||
                    d.ServiceType == typeof(IOptionsChangeTokenSource<RateLimiterOptions>))
                .ToList();
            foreach (var d in rateLimiterDescriptors)
                services.Remove(d);

            // Re-register rate limiter options with unlimited no-op policies.
            // Using Configure<RateLimiterOptions> instead of AddRateLimiter() because
            // the AddRateLimiter extension on IServiceCollection is not accessible
            // from Microsoft.NET.Sdk test projects in .NET 10 without the web SDK.
            services.Configure<RateLimiterOptions>(options =>
            {
                options.AddPolicy("signup", _ => RateLimitPartition.GetNoLimiter("test"));
                options.AddPolicy("auth", _ => RateLimitPartition.GetNoLimiter("test"));
                options.AddPolicy("api", _ => RateLimitPartition.GetNoLimiter("test"));
            });

            // Remove ALL registrations related to DbContexts (Aspire registers many things)
            var knownDbContextTypes = new[]
            {
                typeof(AuthDbContext), typeof(AgendaDbContext), typeof(BillingDbContext),
                typeof(MedicalRecordsDbContext), typeof(AuditDbContext), typeof(NotificationsDbContext),
                typeof(StockDbContext), typeof(AIDbContext)
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
            services.AddDbContext<StockDbContext>(opts =>
                opts.UseNpgsql(_connectionString));
            services.AddDbContext<AIDbContext>(opts =>
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

            // Replace IChatClient with FakeChatClient for AI triage tests
            var chatClientDescriptors = services
                .Where(d => d.ServiceType == typeof(IChatClient))
                .ToList();
            foreach (var d in chatClientDescriptors)
                services.Remove(d);

            services.AddSingleton<IChatClient>(FakeChatClient);

            // Replace INoShowPredictionService with FakeNoShowPredictionService.
            // In tests, no ML model file is available so NoShowPredictionService would
            // return ML_MODEL_NOT_LOADED. The fake returns deterministic predictions
            // based solely on input features, making BDD scenarios predictable.
            var noShowDescriptors = services
                .Where(d => d.ServiceType == typeof(INoShowPredictionService))
                .ToList();
            foreach (var d in noShowDescriptors)
                services.Remove(d);

            services.AddScoped<INoShowPredictionService, FakeNoShowPredictionService>();

            // Register OutputCache so IOutputCacheStore is available.
            // EditAppointmentHandler and ImportPatientsHandler depend on it to invalidate caches.
            services.AddOutputCache();
        });
    }
}
