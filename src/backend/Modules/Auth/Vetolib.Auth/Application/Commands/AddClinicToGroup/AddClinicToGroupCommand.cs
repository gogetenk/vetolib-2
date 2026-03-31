using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.AddClinicToGroup;

internal record AddClinicToGroupCommand(Guid GroupId, Guid ClinicId, Guid RequestingUserId) : IRequest<Result>;
