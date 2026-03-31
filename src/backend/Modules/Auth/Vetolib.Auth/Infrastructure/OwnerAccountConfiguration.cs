using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class OwnerAccountConfiguration : IEntityTypeConfiguration<OwnerAccount>
{
    public void Configure(EntityTypeBuilder<OwnerAccount> builder)
    {
        builder.ToTable("owner_accounts", "auth");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.Phone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(o => o.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(o => o.Email)
            .IsUnique();

        builder.HasIndex(o => o.Phone)
            .IsUnique();
    }
}
