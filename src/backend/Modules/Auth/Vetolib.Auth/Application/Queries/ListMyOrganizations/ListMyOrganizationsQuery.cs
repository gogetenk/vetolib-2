using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.ListMyOrganizations;

internal record ListMyOrganizationsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<MyOrganizationDto>>>;
