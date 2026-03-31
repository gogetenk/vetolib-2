using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.OwnerPortalLogin;

internal record OwnerPortalLoginCommand(string Email, string Password) : IRequest<Result<OwnerPortalTokenDto>>;
