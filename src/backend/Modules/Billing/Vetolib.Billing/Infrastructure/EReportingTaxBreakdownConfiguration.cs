using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Billing.Domain;

namespace Vetolib.Billing.Infrastructure;

internal class EReportingTaxBreakdownConfiguration : IEntityTypeConfiguration<EReportingTaxBreakdown>
{
    public void Configure(EntityTypeBuilder<EReportingTaxBreakdown> builder)
    {
        builder.ToTable("ereporting_tax_breakdowns", "billing");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.EReportingPeriodId)
            .IsRequired();

        builder.Property(b => b.TaxRate)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(b => b.TaxCategory)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.BaseAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.TransactionCount)
            .IsRequired();
    }
}
