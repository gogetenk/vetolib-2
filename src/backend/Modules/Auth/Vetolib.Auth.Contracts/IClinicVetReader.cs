using Ardalis.Result;

namespace Vetolib.Auth.Contracts;

/// <summary>
/// Allows other modules to query the list of veterinarians for a clinic
/// without referencing the Auth runtime assembly.
/// Implemented in Vetolib.Auth, registered in AuthModuleServiceRegistrar.
/// </summary>
public interface IClinicVetReader
{
    Task<Result<List<ClinicVetDto>>> GetVeterinariansForClinic(Guid clinicId, CancellationToken ct);
}

public record ClinicVetDto(Guid Id, string Name);
