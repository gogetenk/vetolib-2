using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;

namespace Vetolib.Messaging.Infrastructure;

internal class WhatsAppPhoneMappingConfiguration : IEntityTypeConfiguration<WhatsAppPhoneMapping>
{
    public void Configure(EntityTypeBuilder<WhatsAppPhoneMapping> builder)
    {
        builder.ToTable("whatsapp_phone_mappings");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.ClinicId)
            .IsRequired();

        builder.Property(w => w.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(w => w.OwnerId)
            .IsRequired();

        builder.Property(w => w.OptInDate)
            .IsRequired();

        builder.HasIndex(w => new { w.ClinicId, w.Phone })
            .IsUnique();
    }
}
