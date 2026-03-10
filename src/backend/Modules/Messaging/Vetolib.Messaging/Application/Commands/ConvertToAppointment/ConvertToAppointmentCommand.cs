using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Messaging.Application.Commands.ConvertToAppointment;

/// <summary>
/// Converts a messaging conversation into an appointment request.
/// Extracts PatientId, OwnerId and last owner message from the conversation,
/// then delegates appointment creation to the Agenda module via its Contracts interface.
/// Policy: ClinicStaff (Receptionist, Vet, Admin).
/// </summary>
internal record ConvertToAppointmentCommand(
    Guid ConversationId,
    DateOnly? PreferredDate,
    string? Notes
) : IRequest<Result<CreateAppointmentFromMessageRequest>>;
