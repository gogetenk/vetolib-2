namespace Vetolib.Auth.Contracts;

public record InviteUserResponse(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    string TemporaryPassword);
