using Ardalis.Result;
using Vetolib.Billing.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Billing.Domain;

internal class EReportingPeriod : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public EReportingStatus Status { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public string? PlatformSubmissionId { get; private set; }

    private readonly List<EReportingTaxBreakdown> _taxBreakdowns = [];
    public IReadOnlyList<EReportingTaxBreakdown> TaxBreakdowns => _taxBreakdowns.AsReadOnly();

    public int TransactionCount { get; private set; }
    public decimal TotalExclTax { get; private set; }
    public decimal TotalTax { get; private set; }
    public decimal TotalInclTax { get; private set; }

    private EReportingPeriod() { } // EF Core

    public static Result<EReportingPeriod> Create(
        Guid clinicId,
        DateOnly periodStart,
        DateOnly periodEnd,
        IReadOnlyList<EReportingTaxBreakdownData> breakdowns,
        int transactionCount,
        decimal totalExclTax,
        decimal totalTax,
        decimal totalInclTax)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (periodEnd < periodStart)
            errors.Add(new ValidationError(nameof(periodEnd), "PeriodEnd must be >= PeriodStart"));

        if (transactionCount < 0)
            errors.Add(new ValidationError(nameof(transactionCount), "TransactionCount cannot be negative"));

        if (errors.Count > 0)
            return Result<EReportingPeriod>.Invalid(errors);

        var period = new EReportingPeriod
        {
            ClinicId = clinicId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = EReportingStatus.Draft,
            TransactionCount = transactionCount,
            TotalExclTax = totalExclTax,
            TotalTax = totalTax,
            TotalInclTax = totalInclTax
        };

        foreach (var bd in breakdowns)
        {
            period._taxBreakdowns.Add(EReportingTaxBreakdown.Create(
                period.Id,
                bd.TaxRate,
                bd.TaxCategory,
                bd.BaseAmount,
                bd.TaxAmount,
                bd.TransactionCount));
        }

        return Result<EReportingPeriod>.Success(period);
    }

    public Result MarkSubmitted(string platformSubmissionId)
    {
        if (Status == EReportingStatus.Submitted)
            return Result.Error("ALREADY_SUBMITTED:This period has already been submitted");

        if (Status == EReportingStatus.Accepted)
            return Result.Error("ALREADY_ACCEPTED:This period has already been accepted");

        Status = EReportingStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        PlatformSubmissionId = platformSubmissionId;

        return Result.Success();
    }

    public Result MarkAccepted()
    {
        if (Status != EReportingStatus.Submitted)
            return Result.Error("NOT_SUBMITTED:Period must be in Submitted status to be accepted");

        Status = EReportingStatus.Accepted;
        return Result.Success();
    }

    public Result MarkRejected()
    {
        if (Status != EReportingStatus.Submitted)
            return Result.Error("NOT_SUBMITTED:Period must be in Submitted status to be rejected");

        Status = EReportingStatus.Rejected;
        return Result.Success();
    }

    public EReportingPeriodDto ToDto() => new(
        Id,
        PeriodStart,
        PeriodEnd,
        Status,
        SubmittedAt,
        TransactionCount,
        TotalExclTax,
        TotalTax,
        TotalInclTax,
        _taxBreakdowns.Select(b => b.ToDto()).ToList().AsReadOnly());
}

/// <summary>
/// Data transfer object for creating tax breakdowns (avoids exposing internal entity constructors).
/// </summary>
internal record EReportingTaxBreakdownData(
    decimal TaxRate,
    TaxCategory TaxCategory,
    decimal BaseAmount,
    decimal TaxAmount,
    int TransactionCount);
