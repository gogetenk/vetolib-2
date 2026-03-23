using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.GenerateInvoicePdf;

internal class GenerateInvoicePdfHandler : IRequestHandler<GenerateInvoicePdfQuery, Result<InvoicePdfResult>>
{
    private readonly BillingDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly InvoicePdfGeneratorFactory _generatorFactory;

    public GenerateInvoicePdfHandler(BillingDbContext context, IConfiguration configuration, InvoicePdfGeneratorFactory generatorFactory)
    {
        _context = context;
        _configuration = configuration;
        _generatorFactory = generatorFactory;
    }

    public async Task<Result<InvoicePdfResult>> Handle(GenerateInvoicePdfQuery query, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);

        if (invoice is null)
            return Result<InvoicePdfResult>.NotFound("INVOICE_NOT_FOUND:Invoice not found");

        var dto = invoice.ToDto();

        if (dto.Status == InvoiceStatus.Draft)
            return Result<InvoicePdfResult>.Error("INVOICE_DRAFT:Invoice must be sent before downloading");

        if (dto.Status == InvoiceStatus.Cancelled)
            return Result<InvoicePdfResult>.Error("INVOICE_CANCELLED:Cancelled invoices cannot be downloaded");

        var clinicName = _configuration["ClinicName"] ?? "Vetolib Veterinary Clinic";
        var trn = _configuration["TaxRegistrationNumber"] ?? string.Empty;

        var generator = _generatorFactory.GetGenerator(dto.CountryCode);
        var pdfBytes = generator.Generate(dto, clinicName, trn);
        return Result<InvoicePdfResult>.Success(new InvoicePdfResult(pdfBytes, dto.InvoiceNumber));
    }
}
