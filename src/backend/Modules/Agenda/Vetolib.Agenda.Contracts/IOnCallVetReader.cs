using Ardalis.Result;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Allows other modules to query the on-call vet for a clinic without referencing the Agenda runtime assembly.
/// Implement in Vetolib.Agenda, register in AgendaModuleServiceRegistrar.
/// </summary>
public interface IOnCallVetReader
{
    /// <summary>
    /// Returns the on-call vet (userId) for the given clinic at the current moment.
    /// Returns NotFound if no on-call vet is configured.
    /// </summary>
    Task<Result<OnCallVetDto>> GetCurrentOnCallVetAsync(Guid clinicId, CancellationToken ct = default);
}

public record OnCallVetDto(Guid UserId, string Name, string? Phone);
