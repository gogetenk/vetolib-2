using MassTransit;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Domain.Events;

/// <summary>
/// Handles the EmailVerificationRequestedDomainEvent by publishing an integration event
/// to RabbitMQ via MassTransit. The Notifications module consumes it
/// and sends the verification email with the token link.
/// </summary>
internal class EmailVerificationRequestedDomainEventHandler : INotificationHandler<EmailVerificationRequestedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EmailVerificationRequestedDomainEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(EmailVerificationRequestedDomainEvent notification, CancellationToken ct)
    {
        await _publishEndpoint.Publish(new EmailVerificationRequestedIntegrationEvent
        {
            Email = notification.Email,
            VerificationToken = notification.VerificationToken,
        }, ct);
    }
}
