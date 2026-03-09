using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Domain;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Commands.CreateInvoice;

internal class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly BillingDbContext _context;

    public CreateInvoiceHandler(BillingDbContext context)
    {
        _context = context;
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

        var invoiceResult = Invoice.Create(
            cmd.ClinicId,
            cmd.AnimalId,
            numberResult.Value.Value,
            cmd.ItemDescription,
            cmd.ItemUnitPrice);

        if (!invoiceResult.IsSuccess)
            return Result<InvoiceDto>.Invalid(invoiceResult.ValidationErrors.ToList());

        _context.Invoices.Add(invoiceResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<InvoiceDto>.Success(invoiceResult.Value.ToDto());
    }
}
