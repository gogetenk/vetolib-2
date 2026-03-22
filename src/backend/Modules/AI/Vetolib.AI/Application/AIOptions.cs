namespace Vetolib.AI.Application;

internal class AIOptions
{
    public const string SectionName = "AI";

    /// <summary>
    /// Minimum number of historical completed appointments needed before no-show prediction is reliable (default: 50).
    /// </summary>
    public int MinimumHistoricalAppointments { get; set; } = 50;

    /// <summary>
    /// Timeout in seconds for AI SOAP note generation calls (default: 30).
    /// </summary>
    public int SoapTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for AI SOAP note generation (default: 2).
    /// </summary>
    public int SoapRetryAttempts { get; set; } = 2;

    /// <summary>
    /// Timeout in seconds for message classification AI calls (default: 15).
    /// </summary>
    public int ClassificationTimeoutSeconds { get; set; } = 15;
}
