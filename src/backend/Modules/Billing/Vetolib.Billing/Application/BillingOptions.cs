namespace Vetolib.Billing.Application;

internal class BillingOptions
{
    public const string SectionName = "Billing";

    /// <summary>
    /// UAE VAT rate (default: 5%).
    /// </summary>
    public decimal TaxRate { get; set; } = 0.05m;

    /// <summary>
    /// Number of days after invoice is sent before it is due (default: 30).
    /// </summary>
    public int DueDateDays { get; set; } = 30;

    /// <summary>
    /// ISO 4217 currency code for the clinic (default: AED for UAE).
    /// </summary>
    public string CurrencyCode { get; set; } = "AED";
}
