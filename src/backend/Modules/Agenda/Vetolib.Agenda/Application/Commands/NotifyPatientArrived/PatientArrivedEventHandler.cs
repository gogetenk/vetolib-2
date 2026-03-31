using MediatR;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.NotifyPatientArrived;

internal class PatientArrivedEventHandler : INotificationHandler<PatientArrivedEvent>
{
    private readonly ILogger<PatientArrivedEventHandler> _logger;

    public PatientArrivedEventHandler(ILogger<PatientArrivedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PatientArrivedEvent notification, CancellationToken ct)
    {
        // Log the notification for the assigned veterinarian.
        // When SSE/SignalR infrastructure is wired, this handler will push
        // real-time notifications to the vet's connected client.
        _logger.LogInformation(
            "Patient arrived: {PatientName} ({OwnerName}) for {AppointmentTime} appointment — notifying vet {VetName} ({VetId})",
            notification.PatientName,
            notification.OwnerName,
            notification.AppointmentTime,
            notification.VeterinarianName,
            notification.VeterinarianId);

        return Task.CompletedTask;
    }
}
