using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.InviteUser;

internal record InviteUserCommand(
    Guid ClinicId,
    Guid RequestingUserId,
    string Email,
    string FullName,
    UserRole Role) : IRequest<Result<InviteUserResponse>>;
