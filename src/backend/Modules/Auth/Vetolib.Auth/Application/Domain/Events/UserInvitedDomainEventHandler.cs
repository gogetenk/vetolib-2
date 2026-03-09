using MassTransit;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Domain.Events;

/// <summary>
/// Handles the UserInvitedDomainEvent by publishing an integration event
/// to RabbitMQ via MassTransit. The Notifications module consumes it
/// and sends the invitation email.
/// </summary>
internal class UserInvitedDomainEventHandler : INotificationHandler<UserInvitedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public UserInvitedDomainEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(UserInvitedDomainEvent notification, CancellationToken ct)
    {
        await _publishEndpoint.Publish(new UserInvitedIntegrationEvent
        {
            Email = notification.Email,
            FullName = notification.FullName,
            TemporaryPassword = notification.TemporaryPassword,
            ClinicName = notification.ClinicName,
        }, ct);
    }
}
