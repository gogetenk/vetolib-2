using Ardalis.Result;
using MediatR;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Commands.CreateInvoice;

internal record CreateInvoiceCommand(
    Guid ClinicId,
    Guid AnimalId,
    string ItemDescription,
    decimal ItemUnitPrice,
    string CountryCode = "AE",
    TaxCategory ItemTaxCategory = TaxCategory.Standard) : IRequest<Result<InvoiceDto>>;
