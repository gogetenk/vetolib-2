using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure;

public abstract class MultiTenantDbContext : DbContext
{
    protected readonly IClinicContext ClinicContext;

    protected MultiTenantDbContext(
        DbContextOptions options,
        IClinicContext clinicContext) : base(options)
    {
        ClinicContext = clinicContext;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
            {
                ApplyTenantFilter(builder, entityType.ClrType);
            }
        }
    }

    private void ApplyTenantFilter(ModelBuilder builder, Type entityType)
    {
        var param = Expression.Parameter(entityType, "e");
        var property = Expression.Property(param, nameof(IMultiTenant.ClinicId));
        var clinicIdGetter = Expression.Property(
            Expression.Constant(ClinicContext),
            nameof(IClinicContext.ClinicId));
        var equals = Expression.Equal(property, clinicIdGetter);
        var lambda = Expression.Lambda(equals, param);

        builder.Entity(entityType).HasQueryFilter(lambda);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.GetType().GetProperty(nameof(BaseEntity.UpdatedAt))!
                    .SetValue(entry.Entity, DateTime.UtcNow);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
