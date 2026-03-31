using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;

namespace Vetolib.Auth.Application.Queries.GetGroupRevenueComparison;

internal class GetGroupRevenueComparisonHandler
    : IRequestHandler<GetGroupRevenueComparisonQuery, Result<ClinicGroupRevenueComparisonDto>>
{
    private readonly AuthDbContext _context;
    private readonly IRevenueStatsReader _revenueStats;

    public GetGroupRevenueComparisonHandler(
        AuthDbContext context,
        IRevenueStatsReader revenueStats)
    {
        _context = context;
        _revenueStats = revenueStats;
    }

    public async Task<Result<ClinicGroupRevenueComparisonDto>> Handle(
        GetGroupRevenueComparisonQuery query, CancellationToken ct)
    {
        var group = await _context.ClinicGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == query.GroupId, ct);

        if (group is null)
            return Result<ClinicGroupRevenueComparisonDto>.NotFound("Clinic group not found");

        if (group.OwnerUserId != query.RequestingUserId)
            return Result<ClinicGroupRevenueComparisonDto>.Forbidden();

        var clinicIds = group.Members.Select(m => m.ClinicId).ToList();

        // Fetch clinic names from the Clinics table (cross-tenant, no filter needed)
        var clinics = await _context.Clinics
            .Where(c => clinicIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var revenueResult = await _revenueStats.GetRevenueByClinicIdsAsync(clinicIds, ct);
        if (!revenueResult.IsSuccess)
            return Result<ClinicGroupRevenueComparisonDto>.Error(string.Join("; ", revenueResult.Errors));

        var clinicRevenues = clinicIds.Select(id => new ClinicRevenueDto(
            id,
            clinics.GetValueOrDefault(id, "Unknown"),
            revenueResult.Value.GetValueOrDefault(id))).ToList();

        var dto = new ClinicGroupRevenueComparisonDto(
            group.Id,
            group.Name,
            clinicRevenues,
            "AED");

        return Result<ClinicGroupRevenueComparisonDto>.Success(dto);
    }
}
