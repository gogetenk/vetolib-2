namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Action-based transition request aligned with the frontend convention.
/// Action values: CHECK_IN, START, COMPLETE, CANCEL, NO_SHOW
/// </summary>
public record TransitionAppointmentRequest(
    string Action,
    string? Reason);
