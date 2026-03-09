using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.RefreshToken;

internal record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokenDto>>;
