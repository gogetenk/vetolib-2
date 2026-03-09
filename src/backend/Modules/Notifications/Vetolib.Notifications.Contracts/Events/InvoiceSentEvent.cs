namespace Vetolib.Notifications.Contracts.Events;

/// <summary>
/// Published when an invoice is sent to the owner.
/// Maps to the invoice-sent template.
/// </summary>
public record InvoiceSentEvent
{
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "AED";
    public string ClinicName { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
}
