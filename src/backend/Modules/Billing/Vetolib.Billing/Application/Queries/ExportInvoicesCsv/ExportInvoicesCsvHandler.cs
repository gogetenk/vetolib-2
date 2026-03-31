using System.Text;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Billing.Infrastructure;

namespace Vetolib.Billing.Application.Queries.ExportInvoicesCsv;

internal class ExportInvoicesCsvHandler : IRequestHandler<ExportInvoicesCsvQuery, Result<CsvExportResult>>
{
    private readonly BillingDbContext _context;

    public ExportInvoicesCsvHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CsvExportResult>> Handle(ExportInvoicesCsvQuery query, CancellationToken ct)
    {
        if (query.From >= query.To)
            return Result<CsvExportResult>.Invalid(
                new ValidationError(nameof(query.From), "'From' date must be before 'To' date"));

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Where(i => i.CreatedAt >= query.From && i.CreatedAt < query.To)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

        var dtos = invoices.Select(i => i.ToDto()).ToList();
        var csvContent = InvoiceCsvGenerator.Generate(dtos);
        var csvBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvContent)).ToArray();

        var fileName = $"invoices-{query.From:yyyy-MM-dd}-to-{query.To:yyyy-MM-dd}.csv";

        return Result<CsvExportResult>.Success(new CsvExportResult(csvBytes, fileName));
    }
}
