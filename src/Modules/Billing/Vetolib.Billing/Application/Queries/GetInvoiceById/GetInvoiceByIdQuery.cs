using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.GetInvoiceById;

internal record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
