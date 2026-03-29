using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Infrastructure;

internal class BreedingDbContext : MultiTenantDbContext
{
    public DbSet<Litter> Litters => Set<Litter>();
    public DbSet<LitterOffspring> LitterOffspring => Set<LitterOffspring>();

    public BreedingDbContext(
        DbContextOptions<BreedingDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(BreedingDbContext).Assembly);
    }
}
