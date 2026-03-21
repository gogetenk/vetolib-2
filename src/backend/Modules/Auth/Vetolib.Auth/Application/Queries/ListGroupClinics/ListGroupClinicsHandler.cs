using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Queries.ListGroupClinics;

internal class ListGroupClinicsHandler : IRequestHandler<ListGroupClinicsQuery, Result<ClinicGroupDto>>
{
    private readonly AuthDbContext _context;

    public ListGroupClinicsHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClinicGroupDto>> Handle(ListGroupClinicsQuery query, CancellationToken ct)
    {
        var group = await _context.ClinicGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == query.GroupId, ct);

        if (group is null)
            return Result<ClinicGroupDto>.NotFound("Clinic group not found");

        var dto = new ClinicGroupDto(
            group.Id,
            group.Name,
            group.OwnerUserId,
            group.Members.Select(m => new ClinicGroupMemberDto(m.ClinicId)).ToList());

        return Result<ClinicGroupDto>.Success(dto);
    }
}
