using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Agenda.Application.Domain;

namespace Vetolib.Agenda.Infrastructure;

internal class VisitFeedbackConfiguration : IEntityTypeConfiguration<VisitFeedback>
{
    public void Configure(EntityTypeBuilder<VisitFeedback> builder)
    {
        builder.ToTable("visit_feedbacks", "agenda");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.ClinicId)
            .IsRequired();

        builder.Property(f => f.AppointmentId)
            .IsRequired();

        builder.Property(f => f.Rating)
            .IsRequired();

        builder.Property(f => f.Comment)
            .HasMaxLength(1000);

        builder.Property(f => f.IsPublic)
            .IsRequired()
            .HasDefaultValue(false);

        // One feedback per appointment (unique constraint)
        builder.HasIndex(f => f.AppointmentId)
            .IsUnique();

        // Performance: query feedback by clinic
        builder.HasIndex(f => new { f.ClinicId, f.CreatedAt });
    }
}
