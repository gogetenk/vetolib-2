using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.GetUsers;

internal record GetUsersQuery() : IRequest<Result<IReadOnlyList<UserDto>>>;
