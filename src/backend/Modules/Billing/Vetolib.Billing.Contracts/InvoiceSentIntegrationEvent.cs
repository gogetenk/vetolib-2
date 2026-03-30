namespace Vetolib.Billing.Contracts;

/// <summary>
/// Integration event published when an invoice status transitions to Sent.
/// Consumed by the Notifications module to send the invoice email.
/// </summary>
public record InvoiceSentIntegrationEvent
{
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "AED";
    public string ClinicName { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
}
