using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Returns the list of active veterinarians for a given clinic.
/// Used by the owner booking portal to let owners pick a vet.
/// Handler lives in Vetolib.Agenda (internal).
/// </summary>
public record ListClinicVeterinariansQuery(Guid ClinicId)
    : IRequest<Result<List<ClinicVeterinarianDto>>>;
