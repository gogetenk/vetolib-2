using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetVetWorkload;

internal class GetVetWorkloadHandler
    : IRequestHandler<GetVetWorkloadQuery, Result<IReadOnlyList<VetWorkloadDto>>>
{
    private readonly AgendaDbContext _context;

    public GetVetWorkloadHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<VetWorkloadDto>>> Handle(
        GetVetWorkloadQuery query, CancellationToken ct)
    {
        // Current week: Monday to Sunday (ISO 8601)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7; // Monday=0, Sunday=6
        var monday = today.AddDays(-daysSinceMonday);
        var nextMonday = monday.AddDays(7);

        var rows = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date >= monday && a.Date < nextMonday)
            .GroupBy(a => new { a.VeterinarianId, a.VeterinarianName })
            .Select(g => new
            {
                g.Key.VeterinarianId,
                g.Key.VeterinarianName,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .ToListAsync(ct);

        var result = rows
            .Select(r => new VetWorkloadDto(
                VeterinarianId: r.VeterinarianId,
                VeterinarianName: r.VeterinarianName,
                AppointmentCount: r.Count))
            .ToList();

        return Result<IReadOnlyList<VetWorkloadDto>>.Success(result);
    }
}
