namespace Vetolib.Messaging.Contracts;

/// <summary>
/// Request body for POST /api/v1/messaging/conversations/{id}/convert-to-appointment.
/// Staff provides optional preferred date and notes; the handler fills in patient/owner from the conversation.
/// </summary>
public record ConvertToAppointmentRequest(
    DateOnly? PreferredDate,
    string? Notes);
