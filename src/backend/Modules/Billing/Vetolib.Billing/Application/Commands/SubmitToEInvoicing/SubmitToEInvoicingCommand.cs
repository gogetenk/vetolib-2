using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Commands.SubmitToEInvoicing;

internal record SubmitToEInvoicingCommand(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
