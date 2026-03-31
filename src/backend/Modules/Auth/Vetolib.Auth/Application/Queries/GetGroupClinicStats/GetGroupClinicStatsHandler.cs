using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Auth.Application.Queries.GetGroupClinicStats;

internal class GetGroupClinicStatsHandler
    : IRequestHandler<GetGroupClinicStatsQuery, Result<IReadOnlyList<ClinicGroupClinicStatsDto>>>
{
    private readonly AuthDbContext _context;
    private readonly IPatientStatsReader _patientStats;
    private readonly IAppointmentStatsReader _appointmentStats;
    private readonly IRevenueStatsReader _revenueStats;

    public GetGroupClinicStatsHandler(
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

    public async Task<Result<IReadOnlyList<ClinicGroupClinicStatsDto>>> Handle(
        GetGroupClinicStatsQuery query, CancellationToken ct)
    {
        var group = await _context.ClinicGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == query.GroupId, ct);

        if (group is null)
            return Result<IReadOnlyList<ClinicGroupClinicStatsDto>>.NotFound("Clinic group not found");

        if (group.OwnerUserId != query.RequestingUserId)
            return Result<IReadOnlyList<ClinicGroupClinicStatsDto>>.Forbidden();

        var clinicIds = group.Members.Select(m => m.ClinicId).ToList();

        // Fetch clinic names from the Clinics table (cross-tenant, no filter needed)
        var clinics = await _context.Clinics
            .Where(c => clinicIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var patientResult = await _patientStats.GetPatientCountsByClinicIdsAsync(clinicIds, ct);
        if (!patientResult.IsSuccess)
            return Result<IReadOnlyList<ClinicGroupClinicStatsDto>>.Error(string.Join("; ", patientResult.Errors));

        var appointmentResult = await _appointmentStats.GetAppointmentCountsByClinicIdsAsync(clinicIds, ct);
        if (!appointmentResult.IsSuccess)
            return Result<IReadOnlyList<ClinicGroupClinicStatsDto>>.Error(string.Join("; ", appointmentResult.Errors));

        var revenueResult = await _revenueStats.GetRevenueByClinicIdsAsync(clinicIds, ct);
        if (!revenueResult.IsSuccess)
            return Result<IReadOnlyList<ClinicGroupClinicStatsDto>>.Error(string.Join("; ", revenueResult.Errors));

        var dtos = clinicIds.Select(id => new ClinicGroupClinicStatsDto(
            id,
            clinics.GetValueOrDefault(id, "Unknown"),
            patientResult.Value.GetValueOrDefault(id),
            appointmentResult.Value.GetValueOrDefault(id),
            revenueResult.Value.GetValueOrDefault(id),
            "AED")).ToList();

        return Result<IReadOnlyList<ClinicGroupClinicStatsDto>>.Success(dtos);
    }
}
