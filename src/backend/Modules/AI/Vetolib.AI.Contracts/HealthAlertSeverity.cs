namespace Vetolib.AI.Contracts;

/// <summary>
/// Health alert severity levels.
/// Values are explicitly assigned so that numeric ordering (ascending) equals
/// priority ordering (Low &lt; Medium &lt; High). Do NOT insert new values between
/// existing ones without updating all OrderBy(Severity) queries.
/// </summary>
public enum HealthAlertSeverity
{
    Low = 0,
    Medium = 10,
    High = 20
}
