using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Auth.Application.Queries.GetGroupDashboardStats;

internal class GetGroupDashboardStatsHandler
    : IRequestHandler<GetGroupDashboardStatsQuery, Result<ClinicGroupDashboardStatsDto>>
{
    private readonly AuthDbContext _context;
    private readonly IPatientStatsReader _patientStats;
    private readonly IAppointmentStatsReader _appointmentStats;
    private readonly IRevenueStatsReader _revenueStats;

    public GetGroupDashboardStatsHandler(
        AuthDbContext context,
        IPatientStatsReader patientStats,
        IAppointmentStatsReader appointmentStats,
        IRevenueStatsReader revenueStats)
    {
        _context = context;
        _patientStats = patientStats;
        _appointmentStats = appointmentStats;
        _revenueStats = revenueStats;
    }

    public async Task<Result<ClinicGroupDashboardStatsDto>> Handle(
        GetGroupDashboardStatsQuery query, CancellationToken ct)
    {
        var group = await _context.ClinicGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == query.GroupId, ct);

        if (group is null)
            return Result<ClinicGroupDashboardStatsDto>.NotFound("Clinic group not found");

        if (group.OwnerUserId != query.RequestingUserId)
            return Result<ClinicGroupDashboardStatsDto>.Forbidden();

        var clinicIds = group.Members.Select(m => m.ClinicId).ToList();

        var patientResult = await _patientStats.GetPatientCountsByClinicIdsAsync(clinicIds, ct);
        if (!patientResult.IsSuccess)
            return Result<ClinicGroupDashboardStatsDto>.Error(string.Join("; ", patientResult.Errors));

        var appointmentResult = await _appointmentStats.GetAppointmentCountsByClinicIdsAsync(clinicIds, ct);
        if (!appointmentResult.IsSuccess)
            return Result<ClinicGroupDashboardStatsDto>.Error(string.Join("; ", appointmentResult.Errors));

        var revenueResult = await _revenueStats.GetRevenueByClinicIdsAsync(clinicIds, ct);
        if (!revenueResult.IsSuccess)
            return Result<ClinicGroupDashboardStatsDto>.Error(string.Join("; ", revenueResult.Errors));

        var dto = new ClinicGroupDashboardStatsDto(
            group.Id,
            group.Name,
            clinicIds.Count,
            patientResult.Value.Values.Sum(),
            appointmentResult.Value.Values.Sum(),
            revenueResult.Value.Values.Sum(),
            "AED");

        return Result<ClinicGroupDashboardStatsDto>.Success(dto);
    }
}
