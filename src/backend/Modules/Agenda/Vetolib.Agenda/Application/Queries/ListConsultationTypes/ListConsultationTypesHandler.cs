using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListConsultationTypes;

internal class ListConsultationTypesHandler : IRequestHandler<ListConsultationTypesQuery, Result<IReadOnlyList<ConsultationTypeDto>>>
{
    private readonly AgendaDbContext _context;

    public ListConsultationTypesHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ConsultationTypeDto>>> Handle(ListConsultationTypesQuery query, CancellationToken ct)
    {
        var types = await _context.ConsultationTypes
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => c.ToDto())
            .ToListAsync(ct);

        return Result<IReadOnlyList<ConsultationTypeDto>>.Success(types);
    }
}
