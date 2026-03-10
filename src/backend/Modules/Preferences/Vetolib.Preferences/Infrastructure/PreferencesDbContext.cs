using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Infrastructure;

internal class PreferencesDbContext : MultiTenantDbContext
{
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<ClinicPreferenceDefault> ClinicPreferenceDefaults => Set<ClinicPreferenceDefault>();
    public DbSet<ConsentAuditEntry> ConsentAuditEntries => Set<ConsentAuditEntry>();

    public PreferencesDbContext(
        DbContextOptions<PreferencesDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(PreferencesDbContext).Assembly);
    }
}
