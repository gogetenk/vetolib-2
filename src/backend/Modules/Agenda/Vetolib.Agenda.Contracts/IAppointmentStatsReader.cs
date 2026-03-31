using Ardalis.Result;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Provides cross-clinic appointment count statistics.
/// Used by the clinic group dashboard to aggregate appointment counts across multiple clinics.
/// </summary>
public interface IAppointmentStatsReader
{
    /// <summary>
    /// Returns the total appointment count for each of the specified clinic IDs.
    /// Uses IgnoreQueryFilters to bypass tenant filtering — caller must verify ownership.
    /// </summary>
    Task<Result<IReadOnlyDictionary<Guid, int>>> GetAppointmentCountsByClinicIdsAsync(
        IReadOnlyList<Guid> clinicIds, CancellationToken ct = default);
}
