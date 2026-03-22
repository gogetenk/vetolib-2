using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;

namespace Vetolib.Messaging.Infrastructure;

internal class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ConversationId)
            .IsRequired();

        builder.Property(m => m.Sender)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.Body)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.SentAt)
            .IsRequired();

        builder.HasMany(m => m.Attachments)
            .WithOne()
            .HasForeignKey(a => a.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance: index for ordering messages within a conversation
        builder.HasIndex(m => new { m.ConversationId, m.SentAt });

        // Classification fields
        builder.Property(m => m.ClassifiedUrgency)
            .HasConversion<string>();

        builder.Property(m => m.ClassifiedCategory)
            .HasConversion<string>();

        builder.Property(m => m.ClassifiedConfidence)
            .HasPrecision(5, 4);

        builder.Property(m => m.OriginalAiUrgency)
            .HasConversion<string>();

        builder.Property(m => m.OriginalAiCategory)
            .HasConversion<string>();

        // Index for flagged messages review queue
        builder.HasIndex(m => m.IsFlaggedForReview);

        // Index for stats queries (accuracy report)
        builder.HasIndex(m => m.ClassifiedCategory);
    }
}
