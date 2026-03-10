using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Infrastructure;

internal class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preferences", "preferences");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClinicId)
            .IsRequired();

        builder.Property(p => p.UserId)
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

        // Unique constraint: one value per (clinic, user, key)
        builder.HasIndex(p => new { p.ClinicId, p.UserId, p.Key })
            .IsUnique()
            .HasDatabaseName("ix_user_preferences_clinic_user_key");
    }
}
