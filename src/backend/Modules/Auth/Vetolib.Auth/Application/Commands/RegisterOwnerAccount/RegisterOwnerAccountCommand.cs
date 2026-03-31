using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.RegisterOwnerAccount;

internal record RegisterOwnerAccountCommand(
    string Email,
    string Phone,
    string FullName,
    string Password) : IRequest<Result<OwnerAccountDto>>;
