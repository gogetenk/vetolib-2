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
