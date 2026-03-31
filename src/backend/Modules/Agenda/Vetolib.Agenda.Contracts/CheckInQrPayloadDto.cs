namespace Vetolib.Agenda.Contracts;

/// <summary>
/// The data payload embedded in the check-in QR code.
/// Includes an HMAC signature to prevent tampering.
/// </summary>
public record CheckInQrPayloadDto(
    Guid AppointmentId,
    string PatientName,
    string OwnerName,
    DateTime ScheduledTime,
    Guid ClinicId,
    string Signature);
