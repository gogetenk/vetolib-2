namespace Vetolib.Auth.Contracts;

public record UserDto(
    Guid Id,
    string Email,
    UserRole Role,
    Guid ClinicId,
    string? VetLicenseNumber);
