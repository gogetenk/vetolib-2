using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class MedicalRecordTemplateConfiguration : IEntityTypeConfiguration<MedicalRecordTemplate>
{
    public void Configure(EntityTypeBuilder<MedicalRecordTemplate> builder)
    {
        builder.ToTable("medical_record_templates", "medical");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ClinicId).IsRequired();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.DiagnosisTemplate)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.TreatmentTemplate)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.NotesTemplate)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.Species)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.IsSystemTemplate)
            .IsRequired();

        builder.Property(t => t.SortOrder)
            .IsRequired();

        builder.HasIndex(t => new { t.ClinicId, t.Category })
            .HasDatabaseName("IX_medical_record_templates_ClinicId_Category");

        builder.HasIndex(t => new { t.ClinicId, t.IsSystemTemplate })
            .HasDatabaseName("IX_medical_record_templates_ClinicId_IsSystemTemplate");
    }
}
