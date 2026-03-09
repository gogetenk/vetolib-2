using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.ChangeUserRole;

internal record ChangeUserRoleCommand(
    Guid RequestingUserId,
    Guid TargetUserId,
    UserRole NewRole) : IRequest<Result>;
