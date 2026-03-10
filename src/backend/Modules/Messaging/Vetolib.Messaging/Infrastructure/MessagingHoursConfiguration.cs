using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;

namespace Vetolib.Messaging.Infrastructure;

internal class MessagingHoursConfiguration : IEntityTypeConfiguration<MessagingHours>
{
    public void Configure(EntityTypeBuilder<MessagingHours> builder)
    {
        builder.ToTable("messaging_hours");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ClinicId)
            .IsRequired();

        builder.Property(h => h.DayOfWeek)
            .IsRequired();

        builder.Property(h => h.OpenTime)
            .IsRequired();

        builder.Property(h => h.CloseTime)
            .IsRequired();

        builder.Property(h => h.IsClosed)
            .IsRequired();
    }
}
