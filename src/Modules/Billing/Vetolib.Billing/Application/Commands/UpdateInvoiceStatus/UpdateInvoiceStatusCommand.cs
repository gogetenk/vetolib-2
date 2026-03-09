using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Commands.UpdateInvoiceStatus;

internal record UpdateInvoiceStatusCommand(
    Guid InvoiceId,
    InvoiceStatus NewStatus) : IRequest<Result<InvoiceDto>>;
