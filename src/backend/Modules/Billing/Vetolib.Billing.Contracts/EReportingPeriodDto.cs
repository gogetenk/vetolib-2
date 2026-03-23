namespace Vetolib.Billing.Contracts;

public record EReportingPeriodDto(
    Guid Id,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    EReportingStatus Status,
    DateTime? SubmittedAt,
    int TransactionCount,
    decimal TotalExclTax,
    decimal TotalTax,
    decimal TotalInclTax,
    IReadOnlyList<EReportingTaxBreakdownDto> TaxBreakdowns);
