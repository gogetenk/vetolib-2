namespace Vetolib.Auth.Contracts;

public record ClinicGroupDto(Guid Id, string Name, Guid OwnerUserId, IReadOnlyList<ClinicGroupMemberDto> Clinics);

public record ClinicGroupMemberDto(Guid ClinicId);
