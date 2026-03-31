using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class ReferralCodeConfiguration : IEntityTypeConfiguration<ReferralCode>
{
    public void Configure(EntityTypeBuilder<ReferralCode> builder)
    {
        builder.ToTable("referral_codes", "auth");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.OwnerUserId)
            .IsRequired();

        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.UsageCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Code must be globally unique
        builder.HasIndex(r => r.Code)
            .IsUnique();

        // One referral code per user
        builder.HasIndex(r => r.OwnerUserId)
            .IsUnique();

        // Exclude from tenant filter — referrals are global
        builder.HasQueryFilter(_ => true);
    }
}
