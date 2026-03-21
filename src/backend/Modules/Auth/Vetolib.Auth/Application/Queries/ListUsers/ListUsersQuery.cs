using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.ListUsers;

internal record ListUsersQuery(int Page = 1, int PageSize = 20) : IRequest<Result<UserPagedResultDto>>;
