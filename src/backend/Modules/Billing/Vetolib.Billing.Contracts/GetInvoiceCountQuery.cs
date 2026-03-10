using Ardalis.Result;
using MediatR;

namespace Vetolib.Billing.Contracts;

/// <summary>
/// Returns the total number of invoices for the current clinic.
/// Handler lives in Vetolib.Billing (internal).
/// Used by Auth module for onboarding auto-completion.
/// </summary>
public record GetInvoiceCountQuery : IRequest<Result<int>>;
