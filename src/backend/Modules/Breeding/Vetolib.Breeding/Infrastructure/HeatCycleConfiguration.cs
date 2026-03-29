using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Breeding.Application.Domain;

namespace Vetolib.Breeding.Infrastructure;

internal class HeatCycleConfiguration : IEntityTypeConfiguration<HeatCycle>
{
    public void Configure(EntityTypeBuilder<HeatCycle> builder)
    {
        builder.ToTable("HeatCycles", "breeding");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ClinicId).IsRequired();
        builder.Property(h => h.PatientId).IsRequired();
        builder.Property(h => h.StartDate).IsRequired();
        builder.Property(h => h.EndDate);
        builder.Property(h => h.Notes).HasMaxLength(500);

        builder.HasIndex(h => new { h.PatientId, h.StartDate });
    }
}
