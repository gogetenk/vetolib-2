using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Queries.SearchClinics;

internal class SearchClinicsHandler : IRequestHandler<SearchClinicsQuery, Result<ClinicSearchPagedResultDto>>
{
    private readonly AuthDbContext _context;

    public SearchClinicsHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClinicSearchPagedResultDto>> Handle(SearchClinicsQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        // Clinic is cross-tenant (not IMultiTenant), so no ClinicId filter is applied.
        // We use IgnoreQueryFilters to bypass the tenant filter explicitly for this public endpoint.
        var clinicsQuery = _context.Clinics
            .IgnoreQueryFilters()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var nameFilter = query.Name.Trim().ToLowerInvariant();
            clinicsQuery = clinicsQuery.Where(c => c.Name.ToLower().Contains(nameFilter));
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var cityFilter = query.City.Trim().ToLowerInvariant();
            clinicsQuery = clinicsQuery.Where(c => c.City != null && c.City.ToLower() == cityFilter);
        }

        if (!string.IsNullOrWhiteSpace(query.Species))
        {
            var speciesFilter = query.Species.Trim().ToLowerInvariant();
            clinicsQuery = clinicsQuery.Where(c => c.SupportedSpecies.Any(s => s.ToLower() == speciesFilter));
        }

        var totalCount = await clinicsQuery.CountAsync(ct);

        var items = await clinicsQuery
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClinicSearchResultDto(
                c.Id,
                c.Name,
                c.City,
                c.SupportedSpecies,
                c.LogoUrl,
                c.Slug))
            .ToListAsync(ct);

        return Result<ClinicSearchPagedResultDto>.Success(
            new ClinicSearchPagedResultDto(items, totalCount, page, pageSize));
    }
}
