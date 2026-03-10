using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Infrastructure;

internal class AuthDbContext : MultiTenantDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<OnboardingState> OnboardingStates => Set<OnboardingState>();

    public AuthDbContext(
        DbContextOptions<AuthDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
