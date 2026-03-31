namespace Vetolib.Auth.Contracts;

public record OwnerAccountDto(
    Guid Id,
    string Email,
    string Phone,
    string FullName,
    bool IsVerified,
    DateTime CreatedAt);
