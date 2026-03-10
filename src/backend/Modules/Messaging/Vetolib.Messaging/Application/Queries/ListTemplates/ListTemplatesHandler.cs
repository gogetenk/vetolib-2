using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.ListTemplates;

internal class ListTemplatesHandler : IRequestHandler<ListTemplatesQuery, Result<IReadOnlyList<ResponseTemplateDto>>>
{
    private readonly MessagingDbContext _context;

    public ListTemplatesHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ResponseTemplateDto>>> Handle(ListTemplatesQuery query, CancellationToken ct)
    {
        var templatesQuery = _context.ResponseTemplates.AsNoTracking();

        if (query.Category.HasValue)
            templatesQuery = templatesQuery.Where(t => t.Category == query.Category.Value);

        var templates = await templatesQuery
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

        var dtos = templates.Select(t => t.ToDto()).ToList();

        return Result<IReadOnlyList<ResponseTemplateDto>>.Success(dtos);
    }
}
