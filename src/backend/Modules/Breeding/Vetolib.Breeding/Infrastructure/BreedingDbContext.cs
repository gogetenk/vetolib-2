using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Infrastructure;

internal class BreedingDbContext : MultiTenantDbContext
{
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
