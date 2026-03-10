using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Vetolib.Stock.Domain;

namespace Vetolib.Stock.Infrastructure;

internal class StockDbContext : MultiTenantDbContext
{
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public StockDbContext(
        DbContextOptions<StockDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(StockDbContext).Assembly);
    }
}
