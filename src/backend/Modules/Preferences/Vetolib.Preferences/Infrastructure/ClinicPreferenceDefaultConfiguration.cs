using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Preferences.Application.Domain;

namespace Vetolib.Preferences.Infrastructure;

internal class ClinicPreferenceDefaultConfiguration : IEntityTypeConfiguration<ClinicPreferenceDefault>
{
    public void Configure(EntityTypeBuilder<ClinicPreferenceDefault> builder)
    {
        builder.ToTable("clinic_preference_defaults", "preferences");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClinicId)
            .IsRequired();

        builder.Property(p => p.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Key)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(p => p.Value)
            .IsRequired()
            .HasMaxLength(500);

        // Unique constraint: one default per (clinic, key)
        builder.HasIndex(p => new { p.ClinicId, p.Key })
            .IsUnique()
            .HasDatabaseName("ix_clinic_preference_defaults_clinic_key");
    }
}
