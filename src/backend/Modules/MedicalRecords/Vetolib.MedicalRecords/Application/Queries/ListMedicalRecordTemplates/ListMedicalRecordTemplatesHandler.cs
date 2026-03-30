using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Queries.ListMedicalRecordTemplates;

internal class ListMedicalRecordTemplatesHandler : IRequestHandler<ListMedicalRecordTemplatesQuery, Result<List<MedicalRecordTemplateDto>>>
{
    private readonly MedicalRecordsDbContext _context;
    private readonly IClinicContext _clinicContext;

    public ListMedicalRecordTemplatesHandler(MedicalRecordsDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<List<MedicalRecordTemplateDto>>> Handle(ListMedicalRecordTemplatesQuery query, CancellationToken ct)
    {
        // Ensure system templates are seeded for this clinic
        await MedicalRecordTemplateSeedData.SeedAsync(_context, _clinicContext.ClinicId, ct);

        var baseQuery = _context.MedicalRecordTemplates.AsNoTracking().AsQueryable();

        if (query.Category is not null)
            baseQuery = baseQuery.Where(t => t.Category == query.Category.Value);

        if (query.Species is not null)
            baseQuery = baseQuery.Where(t => t.Species == null || t.Species == query.Species.Value);

        var templates = await baseQuery
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .Select(t => t.ToDto())
            .ToListAsync(ct);

        return Result<List<MedicalRecordTemplateDto>>.Success(templates);
    }
}
