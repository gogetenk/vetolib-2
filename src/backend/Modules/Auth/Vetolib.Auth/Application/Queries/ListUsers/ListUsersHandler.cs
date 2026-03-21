using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Queries.ListUsers;

internal class ListUsersHandler : IRequestHandler<ListUsersQuery, Result<UserPagedResultDto>>
{
    private readonly AuthDbContext _context;

    public ListUsersHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserPagedResultDto>> Handle(ListUsersQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

        var totalCount = await _context.Users
            .AsNoTracking()
            .CountAsync(ct);

        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => u.ToListItemDto())
            .ToListAsync(ct);

        return Result<UserPagedResultDto>.Success(
            new UserPagedResultDto(users, totalCount, page, pageSize));
    }
}
