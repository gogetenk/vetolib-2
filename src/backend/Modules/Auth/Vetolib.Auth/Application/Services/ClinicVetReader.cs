using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Services;

internal class ClinicVetReader : IClinicVetReader
{
    private readonly AuthDbContext _context;

    public ClinicVetReader(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ClinicVetDto>>> GetVeterinariansForClinic(Guid clinicId, CancellationToken ct)
    {
        var vets = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.ClinicId == clinicId && u.Role == UserRole.Vet && u.IsActive)
            .Select(u => new ClinicVetDto(u.Id, u.FullName))
            .ToListAsync(ct);

        return Result<List<ClinicVetDto>>.Success(vets);
    }
}
