namespace Vetolib.Auth.Contracts;

public record MyOrganizationDto(Guid Id, string Name, Guid ClinicId, IReadOnlyList<string> Roles);
