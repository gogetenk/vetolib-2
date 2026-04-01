using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Queries.ListMyOrganizations;

internal class ListMyOrganizationsHandler : IRequestHandler<ListMyOrganizationsQuery, Result<IReadOnlyList<MyOrganizationDto>>>
{
    private readonly AuthDbContext _context;
    private readonly IKeycloakAdminService _keycloakAdmin;

    public ListMyOrganizationsHandler(AuthDbContext context, IKeycloakAdminService keycloakAdmin)
    {
        _context = context;
        _keycloakAdmin = keycloakAdmin;
    }

    public async Task<Result<IReadOnlyList<MyOrganizationDto>>> Handle(
        ListMyOrganizationsQuery query, CancellationToken ct)
    {
        // Look up the user (cross-clinic) to check for KeycloakUserId
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == query.UserId && u.IsActive, ct);

        if (user is null)
            return Result<IReadOnlyList<MyOrganizationDto>>.NotFound("User not found");

        // If user has a KeycloakUserId, query Keycloak Organizations API
        if (user.KeycloakUserId.HasValue)
        {
            var kcResult = await _keycloakAdmin.ListUserOrganizationsAsync(user.KeycloakUserId.Value, ct);
            if (kcResult.IsSuccess)
            {
                var orgs = kcResult.Value
                    .Select(o => new MyOrganizationDto(o.Id, o.Name, o.ClinicId, Array.Empty<string>()))
                    .ToList();
                return Result<IReadOnlyList<MyOrganizationDto>>.Success(orgs);
            }

            // If Keycloak call fails, fall through to ClinicGroup fallback
        }

        // Fallback: use ClinicGroup membership
        var ownedGroups = await _context.ClinicGroups
            .Include(g => g.Members)
            .Where(g => g.OwnerUserId == query.UserId)
            .ToListAsync(ct);

        var clinicIds = ownedGroups
            .SelectMany(g => g.Members.Select(m => m.ClinicId))
            .Distinct()
            .ToList();

        // Always include the user's own clinic
        if (!clinicIds.Contains(user.ClinicId))
            clinicIds.Add(user.ClinicId);

        // Resolve clinic names
        var clinics = await _context.Clinics
            .IgnoreQueryFilters()
            .Where(c => clinicIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var result = clinicIds
            .Select(cid => new MyOrganizationDto(
                cid,
                clinics.GetValueOrDefault(cid, "Unknown"),
                cid,
                Array.Empty<string>()))
            .ToList();

        return Result<IReadOnlyList<MyOrganizationDto>>.Success(result);
    }
}
