namespace Vetolib.Agenda.Contracts;

public record CreateWaitlistEntryRequest(
    Guid PatientId,
    string OwnerName,
    string OwnerPhone,
    string? OwnerEmail,
    DateOnly PreferredDate,
    PreferredTimeSlot PreferredTimeSlot,
    Guid? VetPreference,
    string? Reason);
