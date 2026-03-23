using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts.EInvoicing;

namespace Vetolib.Billing.Application.Queries.GetEInvoicingStatus;

internal record GetEInvoicingStatusQuery(Guid InvoiceId) : IRequest<Result<EInvoiceStatus>>;
