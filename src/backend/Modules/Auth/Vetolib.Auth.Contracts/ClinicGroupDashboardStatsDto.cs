namespace Vetolib.Auth.Contracts;

/// <summary>
/// Aggregated stats across all clinics in a clinic group.
/// </summary>
public record ClinicGroupDashboardStatsDto(
    Guid GroupId,
    string GroupName,
    int TotalClinics,
    int TotalPatients,
    int TotalAppointments,
    decimal TotalRevenue,
    string Currency);
