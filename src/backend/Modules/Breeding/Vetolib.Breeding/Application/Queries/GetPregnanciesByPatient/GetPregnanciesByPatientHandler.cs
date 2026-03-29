using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Breeding.Contracts;
using Vetolib.Breeding.Infrastructure;

namespace Vetolib.Breeding.Application.Queries.GetPregnanciesByPatient;

internal class GetPregnanciesByPatientHandler : IRequestHandler<GetPregnanciesByPatientQuery, Result<IReadOnlyList<PregnancyDto>>>
{
    private readonly BreedingDbContext _context;

    public GetPregnanciesByPatientHandler(BreedingDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<PregnancyDto>>> Handle(GetPregnanciesByPatientQuery query, CancellationToken ct)
    {
        var pregnancies = await _context.Pregnancies
            .AsNoTracking()
            .Include(p => p.ScheduledChecks)
            .Where(p => p.PatientId == query.PatientId)
            .OrderByDescending(p => p.MatingDate)
            .ToListAsync(ct);

        return Result<IReadOnlyList<PregnancyDto>>.Success(
            pregnancies.Select(p => p.ToDto()).ToList());
    }
}
