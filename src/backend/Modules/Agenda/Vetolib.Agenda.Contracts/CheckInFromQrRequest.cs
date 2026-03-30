namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Request body sent when scanning a QR code at the reception desk.
/// </summary>
public record CheckInFromQrRequest(
    Guid AppointmentId,
    string PatientName,
    string OwnerName,
    DateTime ScheduledTime,
    Guid ClinicId,
    string Signature);
