using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Agenda.Application.Domain;

namespace Vetolib.Agenda.Infrastructure;

internal class FollowUpRuleConfiguration : IEntityTypeConfiguration<FollowUpRule>
{
    public void Configure(EntityTypeBuilder<FollowUpRule> builder)
    {
        builder.ToTable("follow_up_rules", "agenda");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.ClinicId)
            .IsRequired();

        builder.Property(f => f.ConsultationType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FollowUpDays)
            .IsRequired();

        builder.Property(f => f.FollowUpReason)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Partial unique index on (ClinicId, ConsultationType) for active rules only
        builder.HasIndex(f => new { f.ClinicId, f.ConsultationType })
            .IsUnique()
            .HasFilter("\"IsActive\" = true");
    }
}
