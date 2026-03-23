using Ardalis.Result;
using MediatR;

namespace Vetolib.AI.Application.Commands.AcknowledgeHealthAlert;

internal record AcknowledgeHealthAlertCommand(Guid AlertId) : IRequest<Result>;
