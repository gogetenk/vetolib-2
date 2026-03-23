using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Billing.Infrastructure;

internal class BillingDbContext : MultiTenantDbContext
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    public BillingDbContext(
        DbContextOptions<BillingDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);

        // Register MassTransit outbox tables in the "billing" schema
        builder.AddInboxStateEntity(b => b.ToTable("inbox_state", "billing"));
        builder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", "billing"));
        builder.AddOutboxStateEntity(b => b.ToTable("outbox_state", "billing"));
    }
}
