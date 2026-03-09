using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.DeactivateUser;

internal record DeactivateUserCommand(
    Guid RequestingUserId,
    Guid TargetUserId) : IRequest<Result>;
