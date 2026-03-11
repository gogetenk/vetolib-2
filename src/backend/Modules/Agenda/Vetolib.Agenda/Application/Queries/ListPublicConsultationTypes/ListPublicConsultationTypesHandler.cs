using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListPublicConsultationTypes;

internal class ListPublicConsultationTypesHandler
    : IRequestHandler<ListPublicConsultationTypesQuery, Result<List<ConsultationTypeDto>>>
{
    private readonly AgendaDbContext _context;

    public ListPublicConsultationTypesHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ConsultationTypeDto>>> Handle(
        ListPublicConsultationTypesQuery query,
        CancellationToken ct)
    {
        if (query.ClinicId == Guid.Empty)
            return Result<List<ConsultationTypeDto>>.Error("INVALID_CLINIC_ID:ClinicId is required");

        // IgnoreQueryFilters() is intentional: this is a public endpoint with no tenant context.
        // We explicitly filter by ClinicId to restrict data to the requested clinic.
        var types = await _context.ConsultationTypes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => c.ClinicId == query.ClinicId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => c.ToDto())
            .ToListAsync(ct);

        return Result<List<ConsultationTypeDto>>.Success(types);
    }
}
