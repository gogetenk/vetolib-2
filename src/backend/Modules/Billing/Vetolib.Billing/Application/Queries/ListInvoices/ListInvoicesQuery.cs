using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.ListInvoices;

internal record ListInvoicesQuery() : IRequest<Result<IReadOnlyList<InvoiceDto>>>;
