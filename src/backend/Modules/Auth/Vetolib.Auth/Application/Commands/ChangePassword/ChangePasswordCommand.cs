using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.ChangePassword;

internal record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Result>;
