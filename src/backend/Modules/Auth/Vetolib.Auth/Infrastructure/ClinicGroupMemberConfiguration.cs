using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class ClinicGroupMemberConfiguration : IEntityTypeConfiguration<ClinicGroupMember>
{
    public void Configure(EntityTypeBuilder<ClinicGroupMember> builder)
    {
        builder.ToTable("clinic_group_members", "auth");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ClinicGroupId)
            .IsRequired();

        builder.Property(m => m.ClinicId)
            .IsRequired();

        builder.HasIndex(m => new { m.ClinicGroupId, m.ClinicId })
            .IsUnique();
    }
}
