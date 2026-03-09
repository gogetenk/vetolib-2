namespace Vetolib.Auth.Contracts;

public record InviteUserRequest(
    string Email,
    string FullName,
    UserRole Role);
