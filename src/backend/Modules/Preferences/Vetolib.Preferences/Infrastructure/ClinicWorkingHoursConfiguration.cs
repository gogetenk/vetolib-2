using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Preferences.Application.Domain;

namespace Vetolib.Preferences.Infrastructure;

internal class ClinicWorkingHoursConfiguration : IEntityTypeConfiguration<ClinicWorkingHours>
{
    public void Configure(EntityTypeBuilder<ClinicWorkingHours> builder)
    {
        builder.ToTable("clinic_working_hours", "preferences");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.ClinicId)
            .IsRequired();

        builder.Property(w => w.DayOfWeek)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(w => w.IsOpen)
            .IsRequired();

        builder.Property(w => w.OpenTime)
            .IsRequired();

        builder.Property(w => w.CloseTime)
            .IsRequired();

        builder.Property(w => w.BreakStartTime);

        builder.Property(w => w.BreakEndTime);

        // Unique constraint: one entry per (clinic, dayOfWeek)
        builder.HasIndex(w => new { w.ClinicId, w.DayOfWeek })
            .IsUnique()
            .HasDatabaseName("ix_clinic_working_hours_clinic_day");
    }
}
