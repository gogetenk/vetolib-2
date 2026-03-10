using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;

namespace Vetolib.Messaging.Infrastructure;

internal class ReplyAuditConfiguration : IEntityTypeConfiguration<ReplyAudit>
{
    public void Configure(EntityTypeBuilder<ReplyAudit> builder)
    {
        builder.ToTable("reply_audits");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.MessageId)
            .IsRequired();

        builder.Property(r => r.OriginalOwnerMessageId)
            .IsRequired();

        builder.Property(r => r.AiSuggestedReply)
            .HasMaxLength(2000);

        builder.Property(r => r.WasSuggestedReplyUsed)
            .IsRequired();

        builder.Property(r => r.ActualReply)
            .IsRequired()
            .HasMaxLength(2000);
    }
}
