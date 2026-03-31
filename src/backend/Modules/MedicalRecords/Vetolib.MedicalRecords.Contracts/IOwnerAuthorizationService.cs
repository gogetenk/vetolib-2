using Ardalis.Result;

namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Verifies that an owner (identified by their cross-clinic OwnerAccountId)
/// is linked to a given patient. Used by the owner portal endpoints.
/// </summary>
public interface IOwnerAuthorizationService
{
    Task<Result<bool>> IsOwnerLinkedToPatient(Guid ownerAccountId, Guid patientId, CancellationToken ct);
}
