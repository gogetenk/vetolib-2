using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class SharedRecordLinkConfiguration : IEntityTypeConfiguration<SharedRecordLink>
{
    public void Configure(EntityTypeBuilder<SharedRecordLink> builder)
    {
        builder.ToTable("shared_record_links", "medical");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClinicId)
            .IsRequired();

        builder.Property(s => s.PatientId)
            .IsRequired();

        builder.Property(s => s.OwnerAccountId)
            .IsRequired();

        builder.Property(s => s.Token)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.RevokedAt)
            .IsRequired(false);

        builder.Property(s => s.AccessCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(s => s.Token)
            .IsUnique()
            .HasDatabaseName("IX_shared_record_links_Token");

        builder.HasIndex(s => new { s.OwnerAccountId, s.RevokedAt, s.ExpiresAt })
            .HasDatabaseName("IX_shared_record_links_OwnerAccountId_Active");

        builder.HasIndex(s => new { s.ClinicId, s.PatientId })
            .HasDatabaseName("IX_shared_record_links_ClinicId_PatientId");
    }
}
