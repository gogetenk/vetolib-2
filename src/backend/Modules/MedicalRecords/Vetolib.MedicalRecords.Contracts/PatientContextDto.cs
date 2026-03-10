namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Patient context returned when staff opens a conversation linked to a patient.
/// The level of detail varies by role: receptionist sees basic info, vet sees full medical context.
/// </summary>
public record PatientContextDto(
    Guid PatientId,
    string Name,
    string Species,
    int AgeYears,
    DateTime? LastExaminedAt,
    // Vet/Admin only fields (null for Receptionist)
    IReadOnlyList<string>? ActiveMedications,
    IReadOnlyList<string>? KnownAllergies,
    IReadOnlyList<string>? VaccinationHistory);
