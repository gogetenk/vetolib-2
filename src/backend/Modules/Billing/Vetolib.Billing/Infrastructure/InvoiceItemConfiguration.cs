using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Billing.Domain;

namespace Vetolib.Billing.Infrastructure;

internal class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items", "billing");

        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.InvoiceId)
            .IsRequired();

        builder.Property(ii => ii.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ii => ii.UnitPriceExclTax)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.TotalInclTax)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.TaxRate)
            .IsRequired()
            .HasPrecision(5, 4)
            .HasDefaultValue(0.05m);

        builder.Property(ii => ii.TaxCategory)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Contracts.TaxCategory.Standard);
    }
}
