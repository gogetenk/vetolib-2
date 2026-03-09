using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.Logout;

internal record LogoutCommand(Guid UserId) : IRequest<Result>;
