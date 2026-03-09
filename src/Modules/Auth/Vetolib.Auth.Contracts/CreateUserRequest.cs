namespace Vetolib.Auth.Contracts;

public record CreateUserRequest(
    string Email,
    string Password,
    UserRole Role,
    string? VetLicenseNumber);
