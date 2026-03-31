namespace Vetolib.Auth.Contracts;

/// <summary>
/// Revenue comparison across clinics in a group.
/// </summary>
public record ClinicGroupRevenueComparisonDto(
    Guid GroupId,
    string GroupName,
    IReadOnlyList<ClinicRevenueDto> Clinics,
    string Currency);

public record ClinicRevenueDto(
    Guid ClinicId,
    string ClinicName,
    decimal Revenue);
