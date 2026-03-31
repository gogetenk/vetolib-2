using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class WebhookLogConfiguration : IEntityTypeConfiguration<WebhookLog>
{
    public void Configure(EntityTypeBuilder<WebhookLog> builder)
    {
        builder.ToTable("webhook_logs", "auth");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ClinicId)
            .IsRequired();

        builder.Property(l => l.WebhookRegistrationId)
            .IsRequired();

        builder.Property(l => l.EventType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(l => l.Payload)
            .IsRequired();

        builder.Property(l => l.ReceivedAt)
            .IsRequired();

        builder.Property(l => l.ProcessedAt);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.HasIndex(l => new { l.WebhookRegistrationId, l.ReceivedAt });
    }
}
