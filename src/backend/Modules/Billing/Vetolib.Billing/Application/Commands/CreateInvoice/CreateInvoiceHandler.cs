using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Commands.CreateInvoice;

internal class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly BillingDbContext _context;
    private readonly ICountryTaxResolver _taxResolver;
    private readonly BillingOptions _options;

    public CreateInvoiceHandler(BillingDbContext context, ICountryTaxResolver taxResolver, IOptions<BillingOptions> options)
    {
        _context = context;
        _taxResolver = taxResolver;
        _options = options.Value;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand cmd, CancellationToken ct)
    {
        // Generate sequential invoice number
        var year = DateTime.UtcNow.Year;
        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}-"))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        var nextSeq = 1;
        if (lastInvoice is not null)
        {
            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                nextSeq = lastSeq + 1;
        }

        var numberResult = InvoiceNumber.Create(year, nextSeq);
        if (!numberResult.IsSuccess)
            return Result<InvoiceDto>.Invalid(numberResult.ValidationErrors.ToList());

        var taxRate = _taxResolver.GetTaxRate(cmd.CountryCode, cmd.ItemTaxCategory);

        var invoiceResult = Invoice.Create(
            cmd.ClinicId,
            cmd.AnimalId,
            numberResult.Value.Value,
            cmd.ItemDescription,
            cmd.ItemUnitPrice,
            taxRate,
            _options.CurrencyCode,
            cmd.CountryCode,
            cmd.ItemTaxCategory,
            buyerName: cmd.BuyerName,
            invoiceTypeCode: cmd.InvoiceTypeCode,
            sellerSiren: cmd.SellerSiren,
            sellerVatNumber: cmd.SellerVatNumber,
            buyerSiren: cmd.BuyerSiren,
            buyerVatNumber: cmd.BuyerVatNumber,
            buyerAddress: cmd.BuyerAddress,
            operationType: cmd.OperationType,
            paymentTerms: cmd.PaymentTerms,
            purchaseOrderReference: cmd.PurchaseOrderReference);

        if (!invoiceResult.IsSuccess)
            return Result<InvoiceDto>.Invalid(invoiceResult.ValidationErrors.ToList());

        _context.Invoices.Add(invoiceResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<InvoiceDto>.Success(invoiceResult.Value.ToDto());
    }
}
