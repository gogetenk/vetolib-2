using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class VetInvitationLogConfiguration : IEntityTypeConfiguration<VetInvitationLog>
{
    public void Configure(EntityTypeBuilder<VetInvitationLog> builder)
    {
        builder.ToTable("vet_invitation_logs", "auth");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VetEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(v => v.OwnerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.PetName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Message)
            .HasMaxLength(1000);

        builder.Property(v => v.SentAt)
            .IsRequired();

        builder.Property(v => v.ClinicId);

        // Index for rate limiting: count invitations per email per day
        builder.HasIndex(v => new { v.VetEmail, v.SentAt });
    }
}
