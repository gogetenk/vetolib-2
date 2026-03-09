using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;

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
            .HasMaxLength(100);

        builder.Property(p => p.Breed)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ClinicId)
            .IsRequired();

        builder.HasMany(p => p.PatientOwners)
            .WithOne(po => po.Patient)
            .HasForeignKey(po => po.PatientId);
    }
}
