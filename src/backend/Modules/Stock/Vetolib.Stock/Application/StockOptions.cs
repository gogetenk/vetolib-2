namespace Vetolib.Stock.Application;

internal class StockOptions
{
    public const string SectionName = "Stock";

    /// <summary>
    /// Number of days before expiry to flag a stock item as "expiring soon" (default: 30).
    /// </summary>
    public int ExpiryWarningDays { get; set; } = 30;
}
