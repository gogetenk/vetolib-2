using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.DeactivateWebhook;

internal record DeactivateWebhookCommand(Guid Id) : IRequest<Result>;
