using Vetolib.Billing.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Billing.Domain;

internal class EReportingTaxBreakdown : BaseEntity
{
    public Guid EReportingPeriodId { get; private set; }
    public decimal TaxRate { get; private set; }
    public TaxCategory TaxCategory { get; private set; }
    public decimal BaseAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public int TransactionCount { get; private set; }

    private EReportingTaxBreakdown() { } // EF Core

    internal static EReportingTaxBreakdown Create(
        Guid periodId,
        decimal taxRate,
        TaxCategory taxCategory,
        decimal baseAmount,
        decimal taxAmount,
        int transactionCount)
    {
        return new EReportingTaxBreakdown
        {
            EReportingPeriodId = periodId,
            TaxRate = taxRate,
            TaxCategory = taxCategory,
            BaseAmount = baseAmount,
            TaxAmount = taxAmount,
            TransactionCount = transactionCount
        };
    }

    public EReportingTaxBreakdownDto ToDto() => new(
        TaxRate,
        TaxCategory,
        BaseAmount,
        TaxAmount,
        TransactionCount);
}
