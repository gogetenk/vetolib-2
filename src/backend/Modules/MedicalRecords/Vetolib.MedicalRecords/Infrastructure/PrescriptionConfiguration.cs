using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("prescriptions", "medical");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClinicId).IsRequired();
        builder.Property(p => p.MedicalRecordId).IsRequired();

        builder.Property(p => p.Medication)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Dosage)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.VetLicenseNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.DrugCatalogEntryId)
            .IsRequired(false);

        builder.Property(p => p.OverrideJustification)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(p => p.OverrideSeverity)
            .HasMaxLength(50)
            .IsRequired(false);
    }
}
