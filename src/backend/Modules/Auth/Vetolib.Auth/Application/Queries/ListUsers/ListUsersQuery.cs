using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.ListUsers;

internal record ListUsersQuery : IRequest<Result<IReadOnlyList<UserListItemDto>>>;
