using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Breeding.Infrastructure;
using Vetolib.Shared.Infrastructure;

namespace Vetolib.Api;

/// <summary>
/// Runs EF Core migrations for all DbContexts and seeds demo data.
/// Called once at startup — idempotent (safe to call on every restart).
/// </summary>
public static class DbInitializer
{
    // Fixed IDs so the seed is deterministic and idempotent across restarts.
    private static readonly Guid DemoClinicId = new("00000000-0000-0000-0001-000000000001");
    private static readonly Guid AdminUserId  = new("00000000-0000-0000-0002-000000000001");
    private static readonly Guid VetUserId    = new("00000000-0000-0000-0002-000000000002");

    public static async Task MigrateAllAsync(IServiceProvider services)
    {
        await MigrateContextAsync<AuthDbContext>(services);
        await MigrateContextAsync<AgendaDbContext>(services);
        await MigrateContextAsync<MedicalRecordsDbContext>(services);
        await MigrateContextAsync<BillingDbContext>(services);
        await MigrateContextAsync<AuditDbContext>(services);
        await MigrateContextAsync<NotificationsDbContext>(services);
        await MigrateContextAsync<BreedingDbContext>(services);
    }

    /// <summary>
    /// Seeds the global drug catalog (medications, vaccines, supplements) into the database.
    /// Idempotent — safe to call on every restart; does nothing if catalog already exists.
    ///
    /// NOT wired in Program.cs by default (2026-03-10). To activate automatic seeding at startup,
    /// add the following after MigrateAllAsync in Program.cs:
    ///   await DbInitializer.SeedDrugCatalogAsync(app.Services);
    /// </summary>
    public static async Task SeedDrugCatalogAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        await DrugCatalogSeedData.SeedAsync(db);
    }

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        // IgnoreQueryFilters is allowed in seed/migration scenarios (see CLAUDE.md)
        var adminExists = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Id == AdminUserId);

        if (adminExists)
            return;

        // C-01: In production, Seed:AdminPassword and Seed:VetPassword MUST be set — fail-fast.
        // In development, generate if absent and print ONLY to the local console (not structured logs).
        var adminPassword = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            if (!env.IsDevelopment())
                throw new InvalidOperationException(
                    "Seed:AdminPassword is required in non-development environments. " +
                    "Set it via environment variable Seed__AdminPassword or User Secrets.");

            adminPassword = GenerateSeedPassword();
            // Intentional string concatenation — avoids structured-log parameter capture
            Console.WriteLine(
                "[DbInitializer] Seed:AdminPassword not set. Generated one-time admin password: "
                + adminPassword
                + " — store this value in a secret manager immediately.");
        }

        var vetPassword = config["Seed:VetPassword"];
        if (string.IsNullOrWhiteSpace(vetPassword))
        {
            if (!env.IsDevelopment())
                throw new InvalidOperationException(
                    "Seed:VetPassword is required in non-development environments. " +
                    "Set it via environment variable Seed__VetPassword or User Secrets.");

            vetPassword = GenerateSeedPassword();
            // Intentional string concatenation — avoids structured-log parameter capture
            Console.WriteLine(
                "[DbInitializer] Seed:VetPassword not set. Generated one-time vet password: "
                + vetPassword
                + " — store this value in a secret manager immediately.");
        }

        var now = DateTime.UtcNow;

        // Raw SQL to bypass EF entity constructors while respecting the table schema.
        // ON CONFLICT DO NOTHING ensures idempotency on replays.
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO auth.users
                (""Id"", ""ClinicId"", ""Email"", ""FullName"", ""PasswordHash"", ""Role"",
                 ""VetLicenseNumber"", ""IsActive"", ""IsLocked"", ""FailedLoginAttempts"",
                 ""CreatedAt"", ""UpdatedAt"")
            VALUES
                ({0}, {1}, {2}, {3}, {4}, {5}, NULL,  true, false, 0, {6}, {7}),
                ({8}, {9}, {10}, {11}, {12}, {13}, {14}, true, false, 0, {15}, {16})
            ON CONFLICT (""Id"") DO NOTHING",
            AdminUserId,
            DemoClinicId,
            "admin@desertpaws.ae",
            "Omar Al-Rashid",
            BCrypt.Net.BCrypt.HashPassword(adminPassword),
            UserRole.Admin.ToString(),
            now, now,
            VetUserId,
            DemoClinicId,
            "dr.sarah@desertpaws.ae",
            "Dr. Sarah Johnson",
            BCrypt.Net.BCrypt.HashPassword(vetPassword),
            UserRole.Vet.ToString(),
            "VET-UAE-2024-001",
            now, now);
    }

    /// <summary>
    /// Generates a cryptographically secure password for seed users when none is configured.
    /// Logged once at startup; must be stored securely immediately.
    /// </summary>
    private static string GenerateSeedPassword()
    {
        var bytes = RandomNumberGenerator.GetBytes(12);
        return Convert.ToBase64String(bytes).Replace("+", "A").Replace("/", "B")[..16] + "1!";
    }

    // -------------------------------------------------------------------------

    private static async Task MigrateContextAsync<TContext>(IServiceProvider services)
        where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DbInitializer");

        try
        {
            logger.LogInformation("Running migrations for {DbContext}...", contextName);
            var db = scope.ServiceProvider.GetRequiredService<TContext>();
            await db.Database.MigrateAsync();
            logger.LogInformation("Migrations completed for {DbContext}", contextName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration failed for {DbContext}", contextName);
            throw;
        }
    }
}
