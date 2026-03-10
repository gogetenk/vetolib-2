using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Commands.AddMessageToRecord;

/// <summary>
/// Attaches a message (and its attachments) from a conversation to the patient's medical record.
/// The text of the message and URLs of photos are added as a note in the medical record.
/// Policy: VetOrAdmin.
/// </summary>
internal record AddMessageToRecordCommand(
    Guid ConversationId,
    Guid MessageId
) : IRequest<Result>;
