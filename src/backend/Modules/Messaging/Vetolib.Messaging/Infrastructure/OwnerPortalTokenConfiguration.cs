using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;

namespace Vetolib.Messaging.Infrastructure;

internal class OwnerPortalTokenConfiguration : IEntityTypeConfiguration<OwnerPortalToken>
{
    public void Configure(EntityTypeBuilder<OwnerPortalToken> builder)
    {
        builder.ToTable("owner_portal_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ClinicId)
            .IsRequired();

        builder.Property(t => t.OwnerId)
            .IsRequired();

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.ConsentVersion)
            .HasMaxLength(50);
    }
}
