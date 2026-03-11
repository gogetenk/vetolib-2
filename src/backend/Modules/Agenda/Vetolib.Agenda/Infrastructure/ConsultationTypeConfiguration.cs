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

        // Partial unique index: only active types must have unique names per clinic
        // CRITICAL: Npgsql uses PascalCase column names with double quotes, NOT snake_case
        builder.HasIndex(c => new { c.ClinicId, c.Name })
            .IsUnique()
            .HasFilter("\"IsActive\" = true");
    }
}
