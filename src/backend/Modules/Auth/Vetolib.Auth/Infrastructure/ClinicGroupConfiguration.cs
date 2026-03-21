using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class ClinicGroupConfiguration : IEntityTypeConfiguration<ClinicGroup>
{
    public void Configure(EntityTypeBuilder<ClinicGroup> builder)
    {
        builder.ToTable("clinic_groups", "auth");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(g => g.OwnerUserId)
            .IsRequired();

        builder.HasMany(g => g.Members)
            .WithOne()
            .HasForeignKey(m => m.ClinicGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
