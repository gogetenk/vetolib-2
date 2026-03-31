using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class WebhookRegistrationConfiguration : IEntityTypeConfiguration<WebhookRegistration>
{
    public void Configure(EntityTypeBuilder<WebhookRegistration> builder)
    {
        builder.ToTable("webhook_registrations", "auth");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.ClinicId)
            .IsRequired();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(w => w.Secret)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(w => w.EventTypes)
            .HasField("_eventTypes")
            .HasColumnType("text[]");

        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(w => w.LastCalledAt);

        builder.HasMany(w => w.Logs)
            .WithOne()
            .HasForeignKey(l => l.WebhookRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.ClinicId, w.IsActive });
    }
}
