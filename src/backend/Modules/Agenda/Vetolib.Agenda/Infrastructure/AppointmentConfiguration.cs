using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Agenda.Application.Domain;

namespace Vetolib.Agenda.Infrastructure;

internal class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments", "agenda");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ClinicId)
            .IsRequired();

        builder.Property(a => a.VeterinarianId)
            .IsRequired();

        builder.Property(a => a.VeterinarianName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.AnimalId)
            .IsRequired();

        builder.Property(a => a.AnimalName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.OwnerName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Date)
            .IsRequired();

        builder.Property(a => a.StartTime)
            .IsRequired();

        builder.Property(a => a.DurationMinutes)
            .IsRequired();

        builder.Property(a => a.EndTime)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Reason)
            .HasMaxLength(1000);

        // Index for conflict detection queries
        builder.HasIndex(a => new { a.ClinicId, a.VeterinarianId, a.Date });
    }
}
