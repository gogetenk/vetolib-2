using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Vetolib.Notifications.Infrastructure;

/// <summary>
/// Dedicated DbContext for MassTransit Outbox pattern.
/// Stores outbox messages to guarantee at-least-once delivery
/// even across process restarts or broker unavailability.
/// </summary>
internal class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Register MassTransit outbox tables in the "notifications" schema
        modelBuilder.AddInboxStateEntity(b => b.ToTable("inbox_state", "notifications"));
        modelBuilder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", "notifications"));
        modelBuilder.AddOutboxStateEntity(b => b.ToTable("outbox_state", "notifications"));
    }
}
