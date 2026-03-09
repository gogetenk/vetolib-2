using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Commands.AddInvoiceItem;

internal record AddInvoiceItemCommand(
    Guid InvoiceId,
    string Description,
    decimal UnitPrice) : IRequest<Result<InvoiceDto>>;
