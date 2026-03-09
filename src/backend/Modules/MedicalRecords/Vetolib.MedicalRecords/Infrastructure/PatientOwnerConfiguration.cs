using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class PatientOwnerConfiguration : IEntityTypeConfiguration<PatientOwner>
{
    public void Configure(EntityTypeBuilder<PatientOwner> builder)
    {
        builder.ToTable("patient_owners", "medical");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.ClinicId)
            .IsRequired();

        builder.HasIndex(po => new { po.PatientId, po.OwnerId })
            .IsUnique();
    }
}
