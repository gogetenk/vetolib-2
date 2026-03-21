using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Commands.CreateClinicGroup;

internal record CreateClinicGroupCommand(string Name, Guid OwnerUserId) : IRequest<Result<ClinicGroupDto>>;
