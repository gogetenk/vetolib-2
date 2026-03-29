using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Breeding.Application.Domain;

namespace Vetolib.Breeding.Infrastructure;

internal class LitterConfiguration : IEntityTypeConfiguration<Litter>
{
    public void Configure(EntityTypeBuilder<Litter> builder)
    {
        builder.ToTable("litters", "breeding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClinicId).IsRequired();
        builder.Property(x => x.MotherPatientId).IsRequired();
        builder.Property(x => x.FatherPatientId);
        builder.Property(x => x.ExternalFatherName).HasMaxLength(200);
        builder.Property(x => x.BirthDate).IsRequired();
        builder.Property(x => x.BornCount).IsRequired();
        builder.Property(x => x.AliveCount).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasMany(x => x.Offspring)
            .WithOne()
            .HasForeignKey(o => o.LitterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ClinicId, x.MotherPatientId })
            .HasDatabaseName("ix_litters_clinic_mother");
    }
}
