using Ardalis.Result;

namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Provides cross-clinic patient count statistics.
/// Used by the clinic group dashboard to aggregate patient counts across multiple clinics.
/// </summary>
public interface IPatientStatsReader
{
    /// <summary>
    /// Returns the total patient count for each of the specified clinic IDs.
    /// Uses IgnoreQueryFilters to bypass tenant filtering — caller must verify ownership.
    /// </summary>
    Task<Result<IReadOnlyDictionary<Guid, int>>> GetPatientCountsByClinicIdsAsync(
        IReadOnlyList<Guid> clinicIds, CancellationToken ct = default);
}
