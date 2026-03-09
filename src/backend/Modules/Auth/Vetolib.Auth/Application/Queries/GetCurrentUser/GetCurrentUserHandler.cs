using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Queries.GetCurrentUser;

internal class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly AuthDbContext _context;

    public GetCurrentUserHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery query, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.UserId, ct);

        if (user is null)
            return Result<UserDto>.NotFound("User not found");

        return Result<UserDto>.Success(user.ToDto());
    }
}
