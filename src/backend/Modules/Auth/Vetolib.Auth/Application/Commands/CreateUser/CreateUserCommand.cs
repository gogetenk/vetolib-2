using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.CreateUser;

internal record CreateUserCommand(
    Guid ClinicId,
    string Email,
    string Password,
    UserRole Role,
    string? VetLicenseNumber) : IRequest<Result<UserDto>>;
