using Ardalis.Result;

namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Allows other modules to read patient data without referencing the MedicalRecords runtime assembly.
/// </summary>
public interface IPatientReader
{
    Task<Result<IReadOnlyList<PatientDto>>> GetPatientsByOwnerIdAsync(Guid ownerId, Guid clinicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the patient context for display in an inbox conversation.
    /// The <paramref name="includeFullMedicalContext"/> flag controls whether sensitive medical
    /// details (allergies, medications, vaccinations) are included.
    /// Receptionist: false. Vet/Admin: true.
    /// </summary>
    Task<Result<PatientContextDto>> GetPatientContextAsync(Guid patientId, bool includeFullMedicalContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the sex of a patient. Used by the Breeding module to validate reproductive eligibility.
    /// </summary>
    Task<Result<Sex>> GetPatientSexAsync(Guid patientId, CancellationToken cancellationToken = default);
}
