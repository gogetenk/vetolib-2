using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.ListGroupClinics;

internal record ListGroupClinicsQuery(Guid GroupId, Guid RequestingUserId) : IRequest<Result<ClinicGroupDto>>;
