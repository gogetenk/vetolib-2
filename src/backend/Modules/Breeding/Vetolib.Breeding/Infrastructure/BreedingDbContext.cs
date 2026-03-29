using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Infrastructure;

internal class BreedingDbContext : MultiTenantDbContext
{
    public DbSet<HeatCycle> HeatCycles => Set<HeatCycle>();

    public BreedingDbContext(
        DbContextOptions<BreedingDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(BreedingDbContext).Assembly);
    }
}
