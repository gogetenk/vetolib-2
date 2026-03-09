using Ardalis.Result;
using MediatR;

namespace Vetolib.Billing.Contracts;

/// <summary>
/// Returns revenue totals (PAID invoices) grouped by month for the last 12 months.
/// Dispatched from Vetolib.Api for the analytics dashboard endpoint.
/// </summary>
public record GetRevenueByMonthQuery : IRequest<Result<IReadOnlyList<RevenueByMonthDto>>>;

public record RevenueByMonthDto(
    string Month,   // "2026-01"
    decimal Total,
    string Currency // "AED"
);
