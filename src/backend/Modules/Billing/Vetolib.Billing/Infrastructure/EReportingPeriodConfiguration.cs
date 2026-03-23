using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Billing.Domain;

namespace Vetolib.Billing.Infrastructure;

internal class EReportingPeriodConfiguration : IEntityTypeConfiguration<EReportingPeriod>
{
    public void Configure(EntityTypeBuilder<EReportingPeriod> builder)
    {
        builder.ToTable("ereporting_periods", "billing");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClinicId)
            .IsRequired();

        builder.Property(p => p.PeriodStart)
            .IsRequired();

        builder.Property(p => p.PeriodEnd)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.SubmittedAt);

        builder.Property(p => p.PlatformSubmissionId)
            .HasMaxLength(200);

        builder.Property(p => p.TransactionCount)
            .IsRequired();

        builder.Property(p => p.TotalExclTax)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalTax)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalInclTax)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasMany(p => p.TaxBreakdowns)
            .WithOne()
            .HasForeignKey(b => b.EReportingPeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.TaxBreakdowns)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => new { p.ClinicId, p.PeriodStart, p.PeriodEnd })
            .IsUnique();
    }
}
