using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Queries.GetUsers;

internal class GetUsersHandler : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    private readonly AuthDbContext _context;

    public GetUsersHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Select(u => u.ToDto())
            .ToListAsync(ct);

        return Result<IReadOnlyList<UserDto>>.Success(users);
    }
}
