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

        builder.Property(c => c.SubscriptionPlan)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.TrialEndsAt)
            .IsRequired();

        // Clinic name uniqueness is enforced at application level (cross-tenant check)
        // No unique index needed here as two clinics can theoretically have the same name
    }
}
