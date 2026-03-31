using System.Globalization;
using System.Text;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.ExportInvoicesCsv;

internal static class InvoiceCsvGenerator
{
    private static readonly string[] Headers =
    [
        "InvoiceNumber", "Date", "ClientName", "ClientEmail", "Description",
        "Amount", "TaxRate", "TaxAmount", "Total", "Status", "PaidAt", "CurrencyCode"
    ];

    internal static string Generate(IReadOnlyList<InvoiceDto> invoices)
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", Headers));

        foreach (var invoice in invoices)
        {
            var description = invoice.Items.Count > 0
                ? string.Join("; ", invoice.Items.Select(i => i.Description))
                : string.Empty;

            var amount = invoice.Subtotal;
            var taxRate = invoice.VatRate;
            var taxAmount = invoice.VatAmount;

            sb.AppendLine(string.Join(",",
                EscapeCsvField(invoice.InvoiceNumber),
                invoice.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EscapeCsvField(invoice.BuyerName),
                EscapeCsvField(string.Empty), // ClientEmail not stored on invoice
                EscapeCsvField(description),
                amount.ToString("F2", CultureInfo.InvariantCulture),
                taxRate.ToString("F4", CultureInfo.InvariantCulture),
                taxAmount.ToString("F2", CultureInfo.InvariantCulture),
                invoice.Total.ToString("F2", CultureInfo.InvariantCulture),
                invoice.Status.ToString(),
                invoice.PaidAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                EscapeCsvField(invoice.CurrencyCode)));
        }

        return sb.ToString();
    }

    internal static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
