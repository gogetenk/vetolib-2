using Ardalis.Result;
using MediatR;

namespace Vetolib.Billing.Application.Queries.ExportInvoicesCsv;

internal record ExportInvoicesCsvQuery(DateTime From, DateTime To) : IRequest<Result<CsvExportResult>>;

internal record CsvExportResult(byte[] CsvBytes, string FileName);
