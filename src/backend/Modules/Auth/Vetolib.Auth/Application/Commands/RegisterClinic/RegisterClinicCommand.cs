using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.RegisterClinic;

internal record RegisterClinicCommand(
    string ClinicName,
    string Email,
    string Password,
    string Phone,
    string Country) : IRequest<Result<RegisterClinicResponse>>;
