using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.CreateClinicGroup;

internal class CreateClinicGroupHandler : IRequestHandler<CreateClinicGroupCommand, Result<ClinicGroupDto>>
{
    private readonly AuthDbContext _context;

    public CreateClinicGroupHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClinicGroupDto>> Handle(CreateClinicGroupCommand cmd, CancellationToken ct)
    {
        // Verify the owner user exists
        var userExists = await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Id == cmd.OwnerUserId && u.IsActive, ct);

        if (!userExists)
            return Result<ClinicGroupDto>.NotFound("User not found");

        var groupResult = ClinicGroup.Create(cmd.Name, cmd.OwnerUserId);
        if (!groupResult.IsSuccess)
            return Result<ClinicGroupDto>.Invalid(groupResult.ValidationErrors.ToList());

        var group = groupResult.Value;
        _context.ClinicGroups.Add(group);
        await _context.SaveChangesAsync(ct);

        return Result<ClinicGroupDto>.Success(new ClinicGroupDto(
            group.Id,
            group.Name,
            group.OwnerUserId,
            []));
    }
}
