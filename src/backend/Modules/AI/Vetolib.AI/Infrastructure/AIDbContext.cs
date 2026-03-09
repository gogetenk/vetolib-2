using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.AI.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.AI.Infrastructure;

internal class AIDbContext : MultiTenantDbContext
{
    public DbSet<TriageResult> TriageResults => Set<TriageResult>();

    public AIDbContext(
        DbContextOptions<AIDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.HasDefaultSchema("ai");
        builder.ApplyConfigurationsFromAssembly(typeof(AIDbContext).Assembly);
    }
}
