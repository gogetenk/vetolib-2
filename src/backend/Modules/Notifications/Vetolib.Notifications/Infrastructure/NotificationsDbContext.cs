using MassTransit;
using Microsoft.EntityFrameworkCore;
using Vetolib.Notifications.Domain;

namespace Vetolib.Notifications.Infrastructure;

/// <summary>
/// Dedicated DbContext for the Notifications module.
/// Includes MassTransit Outbox tables and reminder-related entities.
/// </summary>
internal class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReminderLog> ReminderLogs => Set<ReminderLog>();
    public DbSet<ReminderConfig> ReminderConfigs => Set<ReminderConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Register MassTransit outbox tables in the "notifications" schema
        modelBuilder.AddInboxStateEntity(b => b.ToTable("inbox_state", "notifications"));
        modelBuilder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", "notifications"));
        modelBuilder.AddOutboxStateEntity(b => b.ToTable("outbox_state", "notifications"));

        modelBuilder.Entity<ReminderLog>(b =>
        {
            b.ToTable("reminder_logs", "notifications");
            b.HasKey(e => e.Id);
            b.Property(e => e.ReminderType).HasConversion<string>();
            b.Property(e => e.Channel).HasConversion<string>();
            b.Property(e => e.DeliveryStatus).HasConversion<string>();
            b.Property(e => e.RecipientEmail).HasMaxLength(320);
            b.HasIndex(e => e.ClinicId);
            b.HasIndex(e => new { e.AppointmentId, e.ReminderType });
            b.HasIndex(e => new { e.PatientId, e.ReminderType });
        });

        modelBuilder.Entity<ReminderConfig>(b =>
        {
            b.ToTable("reminder_configs", "notifications");
            b.HasKey(e => e.Id);
            b.Property(e => e.PreferredReminderChannel).HasConversion<string>().HasDefaultValue(Contracts.Enums.ReminderChannel.Email);
            b.HasIndex(e => e.ClinicId).IsUnique();
        });
    }
}
