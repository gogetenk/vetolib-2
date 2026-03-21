using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Infrastructure;

internal class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClinicId)
            .IsRequired();

        builder.Property(c => c.OwnerId)
            .IsRequired();

        builder.Property(c => c.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Channel)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ConversationChannel.Portal);

        builder.Property(c => c.AssignedToRole)
            .HasMaxLength(100);

        builder.Property(c => c.AiTriageConfidence)
            .HasPrecision(5, 4);

        builder.Property(c => c.IsSpam)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance: index for date-range queries filtered by clinic
        builder.HasIndex(c => new { c.ClinicId, c.CreatedAt });
    }
}
