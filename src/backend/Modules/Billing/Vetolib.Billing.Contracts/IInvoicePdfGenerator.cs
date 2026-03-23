namespace Vetolib.Billing.Contracts;

/// <summary>
/// Generates a PDF representation of an invoice.
/// Implementations vary by country (e.g., standard PDF for UAE, Factur-X for France).
/// </summary>
public interface IInvoicePdfGenerator
{
    byte[] Generate(InvoiceDto invoice, string clinicName, string taxRegistrationNumber);
}
