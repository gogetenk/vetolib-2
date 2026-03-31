using MassTransit;
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
    public DbSet<ClinicGroup> ClinicGroups => Set<ClinicGroup>();
    public DbSet<ClinicGroupMember> ClinicGroupMembers => Set<ClinicGroupMember>();
    public DbSet<ReferralCode> ReferralCodes => Set<ReferralCode>();
    public DbSet<OwnerAccount> OwnerAccounts => Set<OwnerAccount>();
    public DbSet<VetInvitationLog> VetInvitationLogs => Set<VetInvitationLog>();

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

        // Register MassTransit outbox tables in the "auth" schema
        builder.AddInboxStateEntity(b => b.ToTable("inbox_state", "auth"));
        builder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", "auth"));
        builder.AddOutboxStateEntity(b => b.ToTable("outbox_state", "auth"));
    }
}
