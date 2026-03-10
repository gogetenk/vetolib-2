using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.DismissChecklist;

internal record DismissChecklistCommand(Guid UserId) : IRequest<Result>;
