using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.GetCurrentUser;

internal record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserDto>>;
