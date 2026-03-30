using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.ListPatients;

internal class ListPatientsHandler : IRequestHandler<ListPatientsQuery, Result<PatientPagedResult>>
{
    private readonly MedicalRecordsDbContext _context;

    public ListPatientsHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientPagedResult>> Handle(ListPatientsQuery query, CancellationToken ct)
    {
        var q = _context.Patients
            .AsNoTracking()
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
            q = q.Where(p => p.Name.ToLower().Contains(query.Name.ToLower()));

        if (query.Species.HasValue)
            q = q.Where(p => p.Species == query.Species.Value);

        if (!string.IsNullOrWhiteSpace(query.Microchip))
            q = q.Where(p => p.MicrochipNumber == query.Microchip);

        if (!string.IsNullOrWhiteSpace(query.OwnerPhone))
            q = q.Where(p => p.PatientOwners.Any(po =>
                po.Owner != null && po.Owner.Phone != null && po.Owner.Phone.Contains(query.OwnerPhone)));

        var total = await q.CountAsync(ct);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var patients = await q
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = patients.Select(p => p.ToDto()).ToList();

        return Result<PatientPagedResult>.Success(new PatientPagedResult(dtos, total, page, pageSize));
    }
}
