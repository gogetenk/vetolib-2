using Ardalis.Result;
using MediatR;

namespace Vetolib.Billing.Contracts;

/// <summary>
/// Public query dispatched from Vetolib.Api for the dashboard.
/// Handler lives in Vetolib.Billing (internal).
/// </summary>
public record GetUnpaidInvoicesTotalQuery : IRequest<Result<decimal>>;
