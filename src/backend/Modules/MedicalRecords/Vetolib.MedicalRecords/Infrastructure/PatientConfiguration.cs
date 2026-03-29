using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients", "medical");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Species)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Breed)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.BirthDate)
            .IsRequired();

        builder.Property(p => p.ClinicId)
            .IsRequired();

        builder.Property(p => p.MicrochipNumber)
            .HasMaxLength(15);

        builder.HasIndex(p => new { p.ClinicId, p.MicrochipNumber })
            .IsUnique()
            .HasFilter("\"MicrochipNumber\" IS NOT NULL")
            .HasDatabaseName("IX_patients_ClinicId_MicrochipNumber");

        builder.HasMany(p => p.PatientOwners)
            .WithOne(po => po.Patient)
            .HasForeignKey(po => po.PatientId);

        builder.HasMany(p => p.WeightEntries)
            .WithOne()
            .HasForeignKey(w => w.PatientId);
    }
}
