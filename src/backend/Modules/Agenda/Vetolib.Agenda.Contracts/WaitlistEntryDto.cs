namespace Vetolib.Agenda.Contracts;

public record WaitlistEntryDto(
    Guid Id,
    Guid ClinicId,
    Guid PatientId,
    string OwnerName,
    string OwnerPhone,
    string? OwnerEmail,
    DateOnly PreferredDate,
    PreferredTimeSlot PreferredTimeSlot,
    Guid? VetPreference,
    string? Reason,
    WaitlistEntryStatus Status,
    DateTime CreatedAt,
    DateTime? NotifiedAt,
    DateTime? ExpiresAt);
