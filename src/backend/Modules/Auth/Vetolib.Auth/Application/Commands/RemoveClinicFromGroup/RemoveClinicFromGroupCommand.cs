using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.RemoveClinicFromGroup;

internal record RemoveClinicFromGroupCommand(Guid GroupId, Guid ClinicId) : IRequest<Result>;
