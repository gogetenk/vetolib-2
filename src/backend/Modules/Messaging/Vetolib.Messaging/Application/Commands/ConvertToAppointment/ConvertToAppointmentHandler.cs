using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Commands.ConvertToAppointment;

internal class ConvertToAppointmentHandler
    : IRequestHandler<ConvertToAppointmentCommand, Result<CreateAppointmentFromMessageRequest>>
{
    private readonly MessagingDbContext _context;
    private readonly IPatientReader _patientReader;
    private readonly IClinicContext _clinicContext;

    public ConvertToAppointmentHandler(
        MessagingDbContext context,
        IPatientReader patientReader,
        IClinicContext clinicContext)
    {
        _context = context;
        _patientReader = patientReader;
        _clinicContext = clinicContext;
    }

    public async Task<Result<CreateAppointmentFromMessageRequest>> Handle(
        ConvertToAppointmentCommand cmd,
        CancellationToken ct)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == cmd.ConversationId, ct);

        if (conversation is null)
            return Result<CreateAppointmentFromMessageRequest>.NotFound(
                $"Conversation {cmd.ConversationId} not found");

        if (conversation.PatientId is null)
            return Result<CreateAppointmentFromMessageRequest>.Error(
                "MISSING_PATIENT:This conversation is not linked to a patient. Cannot convert to appointment.");

        // Get the last message sent by the owner as context/reason
        var lastOwnerMessage = conversation.Messages
            .Where(m => m.Sender == Messaging.Contracts.MessageSender.Owner && !m.IsInternalNote)
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        var notes = cmd.Notes ?? lastOwnerMessage?.Body;

        // Resolve patient and owner names via MedicalRecords.Contracts (no runtime cross-ref)
        var clinicId = _clinicContext.ClinicId;
        var patientsResult = await _patientReader.GetPatientsByOwnerIdAsync(
            conversation.OwnerId, clinicId, ct);

        string animalName;
        string ownerName;

        if (patientsResult.IsSuccess)
        {
            var patient = patientsResult.Value.FirstOrDefault(p => p.Id == conversation.PatientId.Value);
            animalName = patient?.Name ?? "Unknown";
            ownerName = patient?.OwnerName ?? "Unknown";
        }
        else
        {
            animalName = "Unknown";
            ownerName = "Unknown";
        }

        var request = new CreateAppointmentFromMessageRequest(
            ConversationId: conversation.Id,
            PatientId: conversation.PatientId.Value,
            OwnerId: conversation.OwnerId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Notes: notes,
            PreferredDate: cmd.PreferredDate);

        return Result<CreateAppointmentFromMessageRequest>.Success(request);
    }
}
