using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Queries.ListUsers;

internal class ListUsersHandler : IRequestHandler<ListUsersQuery, Result<IReadOnlyList<UserListItemDto>>>
{
    private readonly AuthDbContext _context;

    public ListUsersHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<UserListItemDto>>> Handle(ListUsersQuery query, CancellationToken ct)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Select(u => u.ToListItemDto())
            .ToListAsync(ct);

        return Result<IReadOnlyList<UserListItemDto>>.Success(users);
    }
}
