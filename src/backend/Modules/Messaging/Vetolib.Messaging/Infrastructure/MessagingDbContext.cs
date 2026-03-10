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
    }
}
