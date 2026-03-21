using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.SwitchClinic;

internal record SwitchClinicCommand(Guid UserId, Guid TargetClinicId) : IRequest<Result<AuthTokenDto>>;
