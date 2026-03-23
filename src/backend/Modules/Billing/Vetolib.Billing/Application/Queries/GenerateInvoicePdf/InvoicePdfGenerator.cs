using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.GenerateInvoicePdf;

/// <summary>
/// Backward-compatible static entry point. Delegates to <see cref="StandardPdfGenerator"/>.
/// Kept so existing callers (including unit tests) continue to compile without changes.
/// </summary>
internal static class InvoicePdfGenerator
{
    private static readonly StandardPdfGenerator Instance = new();

    public static byte[] Generate(InvoiceDto invoice, string clinicName, string taxRegistrationNumber)
        => Instance.Generate(invoice, clinicName, taxRegistrationNumber);
}
