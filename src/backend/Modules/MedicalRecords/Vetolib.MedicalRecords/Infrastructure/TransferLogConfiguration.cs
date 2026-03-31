using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class TransferLogConfiguration : IEntityTypeConfiguration<TransferLog>
{
    public void Configure(EntityTypeBuilder<TransferLog> builder)
    {
        builder.ToTable("transfer_logs", "medical");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.SourceClinicId)
            .IsRequired();

        builder.Property(t => t.TargetClinicId)
            .IsRequired();

        builder.Property(t => t.PatientId)
            .IsRequired();

        builder.Property(t => t.TransferredAt)
            .IsRequired();

        builder.Property(t => t.TransferredBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.FailureReason)
            .HasMaxLength(2000);

        builder.Property(t => t.ClinicId)
            .IsRequired();

        builder.HasIndex(t => new { t.SourceClinicId, t.PatientId })
            .HasDatabaseName("IX_transfer_logs_SourceClinicId_PatientId");
    }
}
