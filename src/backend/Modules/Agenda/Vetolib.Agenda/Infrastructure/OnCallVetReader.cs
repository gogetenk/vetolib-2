using Ardalis.Result;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Infrastructure;

/// <summary>
/// Default implementation of IOnCallVetReader.
/// On-call scheduling is not yet implemented; returns NotFound so the caller
/// falls back to notifying all vets.
/// </summary>
internal class OnCallVetReader : IOnCallVetReader
{
    public Task<Result<OnCallVetDto>> GetCurrentOnCallVetAsync(Guid clinicId, CancellationToken ct = default)
    {
        // On-call vet scheduling is not yet configured.
        // Callers must handle NotFound and fall back (e.g., notify all vets).
        return Task.FromResult(Result<OnCallVetDto>.NotFound("No on-call vet configured for this clinic"));
    }
}
