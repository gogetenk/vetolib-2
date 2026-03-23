namespace Vetolib.Billing.Contracts;

public record EReportingTaxBreakdownDto(
    decimal TaxRate,
    TaxCategory TaxCategory,
    decimal BaseAmount,
    decimal TaxAmount,
    int TransactionCount);
