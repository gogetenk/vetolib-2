using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.Login;

internal record LoginCommand(string Email, string Password) : IRequest<Result<AuthTokenDto>>;
