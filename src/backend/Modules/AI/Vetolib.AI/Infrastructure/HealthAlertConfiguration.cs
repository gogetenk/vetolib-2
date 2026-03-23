using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Infrastructure;

internal class HealthAlertConfiguration : IEntityTypeConfiguration<HealthAlert>
{
    public void Configure(EntityTypeBuilder<HealthAlert> builder)
    {
        builder.ToTable("HealthAlerts");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ClinicId)
            .IsRequired();

        builder.Property(h => h.PatientId)
            .IsRequired();

        builder.Property(h => h.AlertType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(h => h.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(h => h.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(h => h.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(h => h.RecommendedAction)
            .HasMaxLength(1000);

        builder.Property(h => h.RuleId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.RiskScore)
            .IsRequired();

        builder.Property(h => h.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(h => h.GeneratedAt)
            .IsRequired();

        builder.Property(h => h.DismissedReason)
            .HasMaxLength(1000);

        builder.Property(h => h.DismissedByName)
            .HasMaxLength(200);

        // Composite index for querying patient alerts by rule (dedup)
        builder.HasIndex(h => new { h.ClinicId, h.PatientId, h.RuleId, h.Status });

        // Index for dashboard queries (active alerts by severity)
        builder.HasIndex(h => new { h.ClinicId, h.Status, h.Severity });
    }
}
