using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("clinics", "auth");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.City)
            .HasMaxLength(256);

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(2048);

        builder.Property(c => c.Slug)
            .HasMaxLength(256);

        builder.Property(c => c.SupportedSpecies)
            .HasField("_supportedSpecies")
            .HasColumnType("text[]");

        builder.Property(c => c.SubscriptionPlan)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(c => c.TrialEndsAt)
            .IsRequired();

        // Index for public clinic search (case-insensitive partial match on name + city filter)
        builder.HasIndex(c => c.City);
        builder.HasIndex(c => c.Slug).IsUnique();
    }
}
