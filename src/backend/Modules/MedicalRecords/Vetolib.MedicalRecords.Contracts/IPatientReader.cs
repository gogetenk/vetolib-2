using Ardalis.Result;

namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Allows other modules to read patient data without referencing the MedicalRecords runtime assembly.
/// </summary>
public interface IPatientReader
{
    Task<Result<IReadOnlyList<PatientDto>>> GetPatientsByOwnerIdAsync(Guid ownerId, Guid clinicId, CancellationToken cancellationToken = default);
}
