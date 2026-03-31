using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Infrastructure;

internal class StaffScheduleConfiguration : IEntityTypeConfiguration<StaffSchedule>
{
    public void Configure(EntityTypeBuilder<StaffSchedule> builder)
    {
        builder.ToTable("staff_schedules", "agenda");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClinicId)
            .IsRequired();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Date)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.ShiftType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for date-range queries by clinic
        builder.HasIndex(s => new { s.ClinicId, s.Date });

        // Index for user-specific queries
        builder.HasIndex(s => new { s.ClinicId, s.UserId, s.Date });
    }
}
