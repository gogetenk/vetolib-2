using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure;

public abstract class MultiTenantDbContext : DbContext
{
    // Exposed as internal so AuditSaveChangesInterceptor (same assembly) can read it.
    internal readonly IClinicContext ClinicContext;

    private readonly IPublisher? _publisher;

    protected MultiTenantDbContext(
        DbContextOptions options,
        IClinicContext clinicContext,
        IPublisher? publisher = null) : base(options)
    {
        ClinicContext = clinicContext;
        _publisher = publisher;
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Stamp UpdatedAt on modified entities
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.GetType().GetProperty(nameof(BaseEntity.UpdatedAt))!
                    .SetValue(entry.Entity, DateTime.UtcNow);
            }
        }

        // Collect domain events BEFORE save (clear to avoid infinite loops)
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.Entity.ClearDomainEvents());

        // Persist (includes MassTransit outbox in the same transaction)
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events AFTER successful save
        if (_publisher is not null)
        {
            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }

        return result;
    }
}
