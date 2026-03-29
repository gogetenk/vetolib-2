using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Breeding.Domain;

namespace Vetolib.Breeding.Infrastructure;

internal class PregnancyCheckConfiguration : IEntityTypeConfiguration<PregnancyCheck>
{
    public void Configure(EntityTypeBuilder<PregnancyCheck> builder)
    {
        builder.ToTable("pregnancy_checks", "breeding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PregnancyId).IsRequired();
        builder.Property(x => x.ScheduledDate).IsRequired();
        builder.Property(x => x.CheckType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.CompletedAt);
        builder.Property(x => x.Result).HasMaxLength(2000);

        builder.HasIndex(x => x.PregnancyId)
            .HasDatabaseName("ix_pregnancy_checks_pregnancy_id");
    }
}
