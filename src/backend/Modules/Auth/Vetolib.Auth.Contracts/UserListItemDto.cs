namespace Vetolib.Auth.Contracts;

public record UserListItemDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive);
