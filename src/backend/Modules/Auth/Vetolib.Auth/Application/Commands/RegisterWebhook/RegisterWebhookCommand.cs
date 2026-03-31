using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.RegisterWebhook;

internal record RegisterWebhookCommand(string Name, string Secret, List<string> EventTypes)
    : IRequest<Result<WebhookRegistrationDto>>;
