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
        IClinicContext clinicContext)
        : base(options, clinicContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
    }
}
