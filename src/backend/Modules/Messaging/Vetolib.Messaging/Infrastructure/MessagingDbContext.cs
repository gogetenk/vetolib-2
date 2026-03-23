using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Infrastructure;

internal class MessagingDbContext : MultiTenantDbContext
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ReplyAudit> ReplyAudits => Set<ReplyAudit>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
    public DbSet<ResponseTemplate> ResponseTemplates => Set<ResponseTemplate>();
    public DbSet<OwnerPortalToken> OwnerPortalTokens => Set<OwnerPortalToken>();
    public DbSet<MessagingHours> MessagingHours => Set<MessagingHours>();
    public DbSet<WhatsAppBusinessAccount> WhatsAppBusinessAccounts => Set<WhatsAppBusinessAccount>();
    public DbSet<WhatsAppPhoneMapping> WhatsAppPhoneMappings => Set<WhatsAppPhoneMapping>();
    public DbSet<PendingUpload> PendingUploads => Set<PendingUpload>();

    public MessagingDbContext(
        DbContextOptions<MessagingDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.HasDefaultSchema("messaging");
        builder.ApplyConfigurationsFromAssembly(typeof(MessagingDbContext).Assembly);

        // Register MassTransit outbox tables in the "messaging" schema
        builder.AddInboxStateEntity(b => b.ToTable("inbox_state", "messaging"));
        builder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", "messaging"));
        builder.AddOutboxStateEntity(b => b.ToTable("outbox_state", "messaging"));
    }
}
