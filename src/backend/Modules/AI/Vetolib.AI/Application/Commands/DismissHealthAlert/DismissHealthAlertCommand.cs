using Ardalis.Result;
using MediatR;

namespace Vetolib.AI.Application.Commands.DismissHealthAlert;

internal record DismissHealthAlertCommand(
    Guid AlertId,
    string Reason,
    string DismissedByName) : IRequest<Result>;
