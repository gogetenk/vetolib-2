using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Breeding.Application.Domain;

namespace Vetolib.Breeding.Infrastructure;

internal class LitterOffspringConfiguration : IEntityTypeConfiguration<LitterOffspring>
{
    public void Configure(EntityTypeBuilder<LitterOffspring> builder)
    {
        builder.ToTable("litter_offspring", "breeding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClinicId).IsRequired();
        builder.Property(x => x.LitterId).IsRequired();
        builder.Property(x => x.PatientId).IsRequired();
        builder.Property(x => x.BirthOrder);

        builder.HasIndex(x => new { x.LitterId, x.PatientId })
            .IsUnique()
            .HasDatabaseName("ix_litter_offspring_litter_patient");
    }
}
