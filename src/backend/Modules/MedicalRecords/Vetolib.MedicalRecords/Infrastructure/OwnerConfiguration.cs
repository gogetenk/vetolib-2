using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("owners", "medical");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.Phone)
            .HasMaxLength(50);

        builder.Property(o => o.ClinicId)
            .IsRequired();

        builder.HasIndex(o => new { o.ClinicId, o.Email })
            .IsUnique();

        builder.HasIndex(o => new { o.ClinicId, o.Phone })
            .HasFilter("\"Phone\" IS NOT NULL")
            .HasDatabaseName("IX_owners_ClinicId_Phone");

        builder.HasMany(o => o.PatientOwners)
            .WithOne(po => po.Owner)
            .HasForeignKey(po => po.OwnerId);
    }
}
