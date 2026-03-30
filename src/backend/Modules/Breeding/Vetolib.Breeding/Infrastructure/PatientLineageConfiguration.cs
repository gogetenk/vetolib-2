using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Breeding.Application.Domain;

namespace Vetolib.Breeding.Infrastructure;

internal class PatientLineageConfiguration : IEntityTypeConfiguration<PatientLineage>
{
    public void Configure(EntityTypeBuilder<PatientLineage> builder)
    {
        builder.ToTable("patient_lineages", "breeding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClinicId).IsRequired();
        builder.Property(x => x.PatientId).IsRequired();
        builder.Property(x => x.MotherPatientId);
        builder.Property(x => x.FatherPatientId);
        builder.Property(x => x.RegistryNumber).HasMaxLength(100);
        builder.Property(x => x.RegistryType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => new { x.ClinicId, x.PatientId })
            .IsUnique()
            .HasDatabaseName("ix_patient_lineages_clinic_patient");

        builder.HasIndex(x => new { x.ClinicId, x.MotherPatientId })
            .HasDatabaseName("ix_patient_lineages_clinic_mother");

        builder.HasIndex(x => new { x.ClinicId, x.FatherPatientId })
            .HasDatabaseName("ix_patient_lineages_clinic_father");
    }
}
