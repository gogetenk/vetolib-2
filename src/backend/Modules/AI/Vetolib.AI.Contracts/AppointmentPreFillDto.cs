namespace Vetolib.AI.Contracts;

public record AppointmentPreFillDto(
    Guid PatientId,
    string PatientName,
    string SuggestedNotes,
    string AlertTitle);
