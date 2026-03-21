using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;

namespace Vetolib.Messaging.Infrastructure;

internal class PendingUploadConfiguration : IEntityTypeConfiguration<PendingUpload>
{
    public void Configure(EntityTypeBuilder<PendingUpload> builder)
    {
        builder.ToTable("pending_uploads");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClinicId)
            .IsRequired();

        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.FileSizeBytes)
            .IsRequired();

        builder.Property(p => p.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.UploadedAt)
            .IsRequired();

        builder.Property(p => p.ExpiresAt)
            .IsRequired();

        builder.HasIndex(p => p.ExpiresAt);
    }
}
