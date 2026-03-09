using Ardalis.Result;
using MediatR;

namespace Vetolib.Billing.Application.Queries.GenerateInvoicePdf;

internal record InvoicePdfResult(byte[] PdfBytes, string InvoiceNumber);

internal record GenerateInvoicePdfQuery(Guid InvoiceId) : IRequest<Result<InvoicePdfResult>>;
