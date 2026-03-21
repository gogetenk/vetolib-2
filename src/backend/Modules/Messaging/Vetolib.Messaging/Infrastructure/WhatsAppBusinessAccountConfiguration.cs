using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;

namespace Vetolib.Messaging.Infrastructure;

internal class WhatsAppBusinessAccountConfiguration : IEntityTypeConfiguration<WhatsAppBusinessAccount>
{
    public void Configure(EntityTypeBuilder<WhatsAppBusinessAccount> builder)
    {
        builder.ToTable("whatsapp_business_accounts");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.ClinicId)
            .IsRequired();

        builder.Property(w => w.WabaId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.PhoneNumberId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.EncryptedAccessToken)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(w => w.ClinicId)
            .IsUnique();
    }
}
