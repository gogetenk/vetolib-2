namespace Vetolib.Messaging.Contracts;

/// <summary>
/// Patient context returned when staff opens a conversation linked to a patient.
/// Messaging-specific type to avoid a transitive dependency on MedicalRecords.Contracts.
/// </summary>
public record ConversationPatientContextDto(
    Guid PatientId,
    string Name,
    string Species,
    int AgeYears,
    DateTime? LastExaminedAt,
    IReadOnlyList<string>? ActiveMedications,
    IReadOnlyList<string>? KnownAllergies,
    IReadOnlyList<string>? VaccinationHistory);
