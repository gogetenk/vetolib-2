namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Drug interaction severity levels.
/// Values are explicitly assigned so that numeric ordering (ascending) equals
/// clinical priority ordering (Critical first). Do NOT insert new values between
/// existing ones without updating all OrderBy(Severity) / Sort comparisons.
/// </summary>
public enum InteractionSeverity
{
    Critical = 0,
    Moderate = 10,
    Info = 20
}
