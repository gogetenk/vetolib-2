using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.ListInvoices;

internal record ListInvoicesQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<IReadOnlyList<InvoiceDto>>>;
