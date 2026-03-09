using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.ListPatients;

internal class ListPatientsHandler : IRequestHandler<ListPatientsQuery, Result<List<PatientDto>>>
{
    private readonly MedicalRecordsDbContext _context;

    public ListPatientsHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PatientDto>>> Handle(ListPatientsQuery query, CancellationToken ct)
    {
        var patients = await _context.Patients
            .AsNoTracking()
            .Include(p => p.PatientOwners)
                .ThenInclude(po => po.Owner)
            .ToListAsync(ct);

        var dtos = patients.Select(p => p.ToDto()).ToList();

        return Result<List<PatientDto>>.Success(dtos);
    }
}
