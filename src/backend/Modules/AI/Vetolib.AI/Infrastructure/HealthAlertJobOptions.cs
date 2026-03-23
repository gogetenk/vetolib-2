namespace Vetolib.AI.Infrastructure;

internal class HealthAlertJobOptions
{
    public const string SectionName = "AI:HealthAlertJob";

    /// <summary>
    /// Time of day (UTC) when the health alert generation job runs. Default: 06:00.
    /// </summary>
    public TimeSpan ScheduledTimeUtc { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Whether the background job is enabled. Default: true.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
