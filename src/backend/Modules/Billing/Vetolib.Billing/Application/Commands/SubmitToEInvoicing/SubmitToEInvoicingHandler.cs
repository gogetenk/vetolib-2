using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Vetolib.Billing.Application.Queries.GenerateInvoicePdf;
using Vetolib.Billing.Contracts;
using Vetolib.Billing.Contracts.EInvoicing;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Commands.SubmitToEInvoicing;

internal class SubmitToEInvoicingHandler : IRequestHandler<SubmitToEInvoicingCommand, Result<InvoiceDto>>
{
    private readonly BillingDbContext _context;
    private readonly IEInvoicingGateway _gateway;
    private readonly InvoicePdfGeneratorFactory _generatorFactory;
    private readonly IConfiguration _configuration;

    public SubmitToEInvoicingHandler(
        BillingDbContext context,
        IEInvoicingGateway gateway,
        InvoicePdfGeneratorFactory generatorFactory,
        IConfiguration configuration)
    {
        _context = context;
        _gateway = gateway;
        _generatorFactory = generatorFactory;
        _configuration = configuration;
    }

    public async Task<Result<InvoiceDto>> Handle(SubmitToEInvoicingCommand cmd, CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);

        if (invoice is null)
            return Result<InvoiceDto>.NotFound("INVOICE_NOT_FOUND:Invoice not found");

        // Only FR invoices are eligible for e-invoicing
        if (invoice.CountryCode != "FR")
            return Result<InvoiceDto>.Error("EINVOICING_NOT_APPLICABLE:E-invoicing is only applicable for French invoices");

        // Must be in Sent status
        if (invoice.Status != InvoiceStatus.Sent)
            return Result<InvoiceDto>.Error("EINVOICING_INVALID_STATUS:Invoice must be in Sent status to submit to e-invoicing");

        // Already submitted
        if (invoice.EInvoicingStatus is not null)
            return Result<InvoiceDto>.Error("EINVOICING_ALREADY_SUBMITTED:Invoice has already been submitted to e-invoicing platform");

        var dto = invoice.ToDto();
        var clinicName = _configuration["ClinicName"] ?? "Vetolib Veterinary Clinic";
        var trn = _configuration["TaxRegistrationNumber"] ?? string.Empty;

        // Generate Factur-X PDF and CII XML
        var generator = _generatorFactory.GetGenerator(dto.CountryCode);
        var pdfBytes = generator.Generate(dto, clinicName, trn);
        var xmlBytes = FacturXXmlGenerator.Generate(dto, clinicName, trn);

        var payload = new EInvoicePayload(
            InvoiceNumber: dto.InvoiceNumber,
            FacturXPdf: pdfBytes,
            CiiXml: xmlBytes,
            SellerSiren: dto.SellerSiren ?? string.Empty,
            SellerVatNumber: dto.SellerVatNumber ?? string.Empty,
            BuyerName: dto.BuyerName,
            BuyerSiren: dto.BuyerSiren);

        var submissionResult = await _gateway.SubmitInvoiceAsync(payload, ct);
        if (!submissionResult.IsSuccess)
            return Result<InvoiceDto>.Error(string.Join("; ", submissionResult.Errors));

        var markResult = invoice.MarkEInvoicingSubmitted(
            submissionResult.Value.PlatformInvoiceId,
            submissionResult.Value.Status);

        if (!markResult.IsSuccess)
            return Result<InvoiceDto>.Error(string.Join("; ", markResult.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
