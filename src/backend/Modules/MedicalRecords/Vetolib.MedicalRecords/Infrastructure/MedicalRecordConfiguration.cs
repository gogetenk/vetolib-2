using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("medical_records", "medical");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ClinicId).IsRequired();
        builder.Property(r => r.PatientId).IsRequired();

        builder.Property(r => r.Diagnosis)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.Treatment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.VetName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ExaminedAt).IsRequired();

        builder.HasMany(r => r.Prescriptions)
            .WithOne()
            .HasForeignKey(p => p.MedicalRecordId);

        builder.HasIndex(r => new { r.PatientId, r.ExaminedAt })
            .HasDatabaseName("IX_medical_records_PatientId_ExaminedAt")
            .IsDescending(false, true);
    }
}
