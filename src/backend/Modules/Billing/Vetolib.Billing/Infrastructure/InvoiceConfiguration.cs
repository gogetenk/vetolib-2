using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Billing.Domain;

namespace Vetolib.Billing.Infrastructure;

internal class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices", "billing");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ClinicId)
            .IsRequired();

        builder.Property(i => i.AnimalId)
            .IsRequired();

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.SellerSiren)
            .HasMaxLength(9);

        builder.Property(i => i.SellerVatNumber)
            .HasMaxLength(20);

        builder.Property(i => i.BuyerSiren)
            .HasMaxLength(9);

        builder.Property(i => i.BuyerVatNumber)
            .HasMaxLength(20);

        builder.Property(i => i.BuyerName)
            .IsRequired()
            .HasMaxLength(200)
            .HasDefaultValue("");

        builder.Property(i => i.BuyerAddress)
            .HasMaxLength(500);

        builder.Property(i => i.OperationType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.InvoiceTypeCode)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("380");

        builder.Property(i => i.PaymentTerms)
            .HasMaxLength(500);

        builder.Property(i => i.CountryCode)
            .IsRequired()
            .HasMaxLength(2)
            .HasDefaultValue("AE");

        builder.Property(i => i.PurchaseOrderReference)
            .HasMaxLength(100);

        builder.HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(i => new { i.ClinicId, i.InvoiceNumber })
            .IsUnique();

        // Ignore computed properties
        builder.Ignore(i => i.SubTotal);
        builder.Ignore(i => i.TotalTax);
        builder.Ignore(i => i.Total);
    }
}
