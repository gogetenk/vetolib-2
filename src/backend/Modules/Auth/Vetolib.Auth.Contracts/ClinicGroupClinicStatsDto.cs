namespace Vetolib.Auth.Contracts;

/// <summary>
/// Per-clinic stats within a clinic group.
/// </summary>
public record ClinicGroupClinicStatsDto(
    Guid ClinicId,
    string ClinicName,
    int PatientCount,
    int AppointmentCount,
    decimal Revenue,
    string Currency);
