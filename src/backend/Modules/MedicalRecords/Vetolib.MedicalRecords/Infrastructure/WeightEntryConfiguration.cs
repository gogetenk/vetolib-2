using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class WeightEntryConfiguration : IEntityTypeConfiguration<WeightEntry>
{
    public void Configure(EntityTypeBuilder<WeightEntry> builder)
    {
        builder.ToTable("weight_entries", "medical");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.ClinicId).IsRequired();
        builder.Property(w => w.PatientId).IsRequired();

        builder.Property(w => w.WeightKg)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(w => w.RecordedAt).IsRequired();

        builder.Property(w => w.RecordedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Note)
            .HasMaxLength(500);

        builder.HasIndex(w => new { w.PatientId, w.RecordedAt })
            .HasDatabaseName("IX_weight_entries_PatientId_RecordedAt");

        builder.HasIndex(w => w.ClinicId)
            .HasDatabaseName("IX_weight_entries_ClinicId");
    }
}
