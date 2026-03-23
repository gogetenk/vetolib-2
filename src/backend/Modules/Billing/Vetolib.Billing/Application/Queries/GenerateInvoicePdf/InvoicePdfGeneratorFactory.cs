using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Application.Queries.GenerateInvoicePdf;

/// <summary>
/// Resolves the appropriate <see cref="IInvoicePdfGenerator"/> based on the invoice's country code.
/// French invoices use <see cref="FacturXPdfGenerator"/> (Factur-X PDF/A-3 + CII XML).
/// All other countries use <see cref="StandardPdfGenerator"/> (simple PDF).
/// </summary>
internal sealed class InvoicePdfGeneratorFactory
{
    private static readonly StandardPdfGenerator Standard = new();
    private static readonly FacturXPdfGenerator FacturX = new();

    /// <summary>
    /// Countries that require Factur-X e-invoicing format.
    /// </summary>
    private static readonly HashSet<string> FacturXCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "FR" // France — mandatory Factur-X since 2024+
    };

    /// <summary>
    /// Returns the appropriate PDF generator for the given country code.
    /// </summary>
    public IInvoicePdfGenerator GetGenerator(string countryCode)
    {
        if (FacturXCountries.Contains(countryCode))
            return FacturX;

        return Standard;
    }
}
