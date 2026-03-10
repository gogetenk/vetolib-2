using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Preferences.Application.Domain;

namespace Vetolib.Preferences.Infrastructure;

internal class ConsentAuditConfiguration : IEntityTypeConfiguration<ConsentAuditEntry>
{
    public void Configure(EntityTypeBuilder<ConsentAuditEntry> builder)
    {
        builder.ToTable("consent_audit_entries", "preferences");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClinicId)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Key)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(e => e.PreviousValue)
            .HasMaxLength(500);

        builder.Property(e => e.NewValue)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Source)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Performance index for querying audit history per user
        builder.HasIndex(e => new { e.ClinicId, e.UserId, e.CreatedAt })
            .HasDatabaseName("ix_consent_audit_entries_clinic_user_created");
    }
}
