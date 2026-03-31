using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;

namespace Vetolib.MedicalRecords.Application.Queries.ListShareLinks;

internal class ListShareLinksHandler : IRequestHandler<ListShareLinksQuery, Result<IReadOnlyList<SharedRecordLinkDto>>>
{
    private const string ShareBaseUrl = "https://vetara.ae/shared/";
    private readonly MedicalRecordsDbContext _context;

    public ListShareLinksHandler(MedicalRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<SharedRecordLinkDto>>> Handle(ListShareLinksQuery query, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var links = await _context.SharedRecordLinks
            .AsNoTracking()
            .Where(l => l.OwnerAccountId == query.OwnerAccountId)
            .Where(l => l.RevokedAt == null && l.ExpiresAt > now)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        // Get patient names for the DTOs
        var patientIds = links.Select(l => l.PatientId).Distinct().ToList();
        var patientNames = await _context.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name })
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var dtos = links.Select(l => new SharedRecordLinkDto(
            l.Id,
            l.PatientId,
            patientNames.GetValueOrDefault(l.PatientId, "Unknown"),
            l.Token,
            $"{ShareBaseUrl}{l.Token}",
            l.ExpiresAt,
            l.RevokedAt,
            l.AccessCount,
            l.CreatedAt)).ToList();

        return Result<IReadOnlyList<SharedRecordLinkDto>>.Success(dtos);
    }
}
