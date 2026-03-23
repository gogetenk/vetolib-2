using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Commands.AddInvoiceItem;

internal class AddInvoiceItemHandler : IRequestHandler<AddInvoiceItemCommand, Result<InvoiceDto>>
{
    private readonly BillingDbContext _context;
    private readonly ICountryTaxResolver _taxResolver;

    public AddInvoiceItemHandler(BillingDbContext context, ICountryTaxResolver taxResolver)
    {
        _context = context;
        _taxResolver = taxResolver;
    }

    public async Task<Result<InvoiceDto>> Handle(AddInvoiceItemCommand cmd, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound("INVOICE_NOT_FOUND:Invoice not found");

        var taxRate = _taxResolver.GetTaxRate(invoice.CountryCode, cmd.TaxCategory);

        var itemResult = invoice.AddItem(cmd.Description, cmd.UnitPrice, taxRate, cmd.TaxCategory);
        if (!itemResult.IsSuccess)
            return Result<InvoiceDto>.Error(string.Join("; ", itemResult.Errors));

        // Explicitly add the new item to the context to ensure proper tracking
        _context.InvoiceItems.Add(itemResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
