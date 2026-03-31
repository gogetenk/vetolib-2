namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Cross-module interface for linking owner records to a global OwnerAccount.
/// Implemented by MedicalRecords module, consumed by Auth module.
/// </summary>
public interface IOwnerAccountLinker
{
    /// <summary>
    /// Finds all Owner records matching email or phone across ALL clinics (IgnoreQueryFilters)
    /// and links them to the given OwnerAccountId.
    /// Returns the list of ClinicIds where owners were found.
    /// </summary>
    Task<Guid[]> LinkOwnersByEmailOrPhoneAsync(Guid ownerAccountId, string email, string? phone, CancellationToken ct = default);

    /// <summary>
    /// Finds a Patient by microchip number across ALL clinics (IgnoreQueryFilters),
    /// then links that Patient's Owner to the given OwnerAccountId.
    /// Returns the ClinicId if found, or null if not found.
    /// </summary>
    Task<Guid?> LinkOwnerByMicrochipAsync(Guid ownerAccountId, string microchipNumber, CancellationToken ct = default);

    /// <summary>
    /// Returns all ClinicIds where the given OwnerAccountId has linked Owner records.
    /// </summary>
    Task<Guid[]> GetLinkedClinicIdsAsync(Guid ownerAccountId, CancellationToken ct = default);
}
