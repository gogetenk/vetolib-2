using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Agenda.Application.Domain;

namespace Vetolib.Agenda.Infrastructure;

internal class ConsultationTypeConfiguration : IEntityTypeConfiguration<ConsultationType>
{
    public void Configure(EntityTypeBuilder<ConsultationType> builder)
    {
        builder.ToTable("consultation_types", "agenda");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClinicId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.DurationMinutes)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.RequiresVetSelection)
            .IsRequired()
            .HasDefaultValue(false);

        // Unique index on (ClinicId, Name) for active types only
        // Note: partial unique indexes are not supported by EF Core in a cross-DB manner,
        // so we add a unique index on (ClinicId, Name) and enforce IsActive filtering in application logic.
        builder.HasIndex(c => new { c.ClinicId, c.Name })
            .IsUnique()
            .HasFilter("is_active = true");
    }
}
