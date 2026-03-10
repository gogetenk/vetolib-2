using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Queries.GetConsentAudit;

internal class GetConsentAuditHandler : IRequestHandler<GetConsentAuditQuery, Result<ConsentAuditPagedResultDto>>
{
    private readonly PreferencesDbContext _context;

    public GetConsentAuditHandler(PreferencesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ConsentAuditPagedResultDto>> Handle(GetConsentAuditQuery query, CancellationToken ct)
    {
        var q = _context.ConsentAuditEntries.IgnoreQueryFilters().AsQueryable();

        if (query.UserId.HasValue)
            q = q.Where(e => e.UserId == query.UserId.Value);

        if (query.Category.HasValue)
            q = q.Where(e => e.Category == query.Category.Value);

        if (query.From.HasValue)
            q = q.Where(e => e.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            q = q.Where(e => e.CreatedAt <= query.To.Value);

        var totalCount = await q.CountAsync(ct);

        var items = await q
            .OrderBy(e => e.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(e => e.ToDto()).ToList().AsReadOnly();

        return Result<ConsentAuditPagedResultDto>.Success(
            new ConsentAuditPagedResultDto(dtos, totalCount, query.Page, query.PageSize));
    }
}
